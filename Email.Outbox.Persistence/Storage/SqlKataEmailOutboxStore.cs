using SqlKata;
using SqlKata.Execution;
using Email.Outbox.Models;
using Email.Outbox.Abstractions;
using Microsoft.Extensions.Logging;
using Email.Outbox.Persistence.Logging;
using Email.Outbox.Persistence.Constants;

namespace Email.Outbox.Persistence.Storage
{
    internal sealed class SqlKataEmailOutboxStore(EmailOutboxRepositoryFactory repositoryFactory, ILogger<SqlKataEmailOutboxStore> logger)
        : IEmailOutboxStore
    {
        public async Task<EmailOutboxEnqueueResult> EnqueueAsync(EmailOutboxRecord record, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(record);

            using var repository = repositoryFactory.Create();

            if (record.IdempotencyKey is not null)
            {
                var existing = await FindByIdempotencyKey(
                    repository.Db,
                    record.IdempotencyKey,
                    cancellationToken);

                if (existing is not null)
                {
                    return Duplicate(existing);
                }
            }

            try
            {
                await repository.Db.Query(EmailOutboxPersistenceConstants.OutboxTable)
                    .InsertAsync(
                        new
                        {
                            Id = record.Id.ToString("D"),
                            record.Payload,
                            record.IdempotencyKey,
                            record.AccountReference,
                            record.CorrelationId,
                            Status = (int)EmailOutboxStatus.Pending,
                            record.AttemptCount,
                            record.CreatedUtc,
                            ModifiedUtc = record.CreatedUtc,
                            record.NextAttemptUtc
                        },
                        cancellationToken: cancellationToken);

                logger.Enqueued(record.Id);
                return new EmailOutboxEnqueueResult(record.Id, false, record.CreatedUtc);
            }
            catch when (record.IdempotencyKey is not null)
            {
                var existing = await FindByIdempotencyKey(
                    repository.Db,
                    record.IdempotencyKey,
                    cancellationToken);

                if (existing is null)
                {
                    throw;
                }

                return Duplicate(existing);
            }
        }

        public async Task<IReadOnlyList<ClaimedEmailOutboxRecord>> ClaimPendingAsync(
            EmailOutboxClaimRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.BatchSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(request), "The email outbox batch size must be greater than zero.");
            }

            if (request.LeaseDuration <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(request), "The email outbox lease duration must be greater than zero.");
            }

            using var repository = repositoryFactory.Create();
            int candidateLimit = request.BatchSize > int.MaxValue / EmailOutboxPersistenceConstants.CandidateMultiplier
                ? int.MaxValue
                : request.BatchSize * EmailOutboxPersistenceConstants.CandidateMultiplier;
            Query candidatesQuery = ApplyClaimableFilter(
                    repository.Db.Query(EmailOutboxPersistenceConstants.OutboxTable),
                    request.UtcNow)
                .OrderBy(nameof(EmailOutboxStorageRecord.NextAttemptUtc))
                .OrderBy(nameof(EmailOutboxStorageRecord.CreatedUtc))
                .Limit(candidateLimit);
            var candidates = await candidatesQuery.GetAsync<EmailOutboxStorageRecord>(
                cancellationToken: cancellationToken);
            var claimed = new List<ClaimedEmailOutboxRecord>(request.BatchSize);

            foreach (var candidate in candidates)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (claimed.Count >= request.BatchSize)
                {
                    break;
                }

                string lockId = Guid.NewGuid().ToString("N");
                DateTime lockedUntilUtc = request.UtcNow.Add(request.LeaseDuration);
                Query claimQuery = ApplyClaimableFilter(
                    repository.Db.Query(EmailOutboxPersistenceConstants.OutboxTable)
                        .Where(nameof(EmailOutboxStorageRecord.Id), candidate.Id),
                    request.UtcNow);
                int affected = await claimQuery.UpdateAsync(
                    new
                    {
                        Status = (int)EmailOutboxStatus.Processing,
                        LockId = lockId,
                        LockedUntilUtc = lockedUntilUtc,
                        ModifiedUtc = request.UtcNow
                    },
                    cancellationToken: cancellationToken);

                if (affected == 1 && Guid.TryParse(candidate.Id, out Guid id))
                {
                    claimed.Add(new ClaimedEmailOutboxRecord(
                        id,
                        candidate.Payload,
                        lockId,
                        candidate.AttemptCount,
                        candidate.CorrelationId));
                }
            }

            logger.Claimed(claimed.Count, request.WorkerId);
            return claimed;
        }

        public async Task<bool> MarkSentAsync(EmailOutboxSentUpdate update, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(update);

            using var repository = repositoryFactory.Create();
            int affected = await LockedRecordQuery(repository.Db, update.Id, update.LockId)
                .UpdateAsync(
                    new
                    {
                        Status = (int)EmailOutboxStatus.Sent,
                        update.AttemptCount,
                        update.MessageId,
                        update.SmtpResponse,
                        update.SentUtc,
                        ModifiedUtc = update.SentUtc,
                        LockedUntilUtc = (DateTime?)null,
                        LockId = (string?)null,
                        LastFailureKind = (int?)null,
                        LastError = (string?)null
                    },
                    cancellationToken: cancellationToken);

            if (affected == 1)
            {
                logger.MarkedSent(update.Id);
            }

            return affected == 1;
        }

        public async Task<bool> MarkFailedAsync(EmailOutboxFailedUpdate update, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(update);

            using var repository = repositoryFactory.Create();
            int affected = await LockedRecordQuery(repository.Db, update.Id, update.LockId)
                .UpdateAsync(
                    new
                    {
                        Status = (int)(update.IsDeadLetter
                            ? EmailOutboxStatus.DeadLetter
                            : EmailOutboxStatus.Pending),
                        update.AttemptCount,
                        LastFailureKind = (int)update.FailureKind,
                        LastError = update.Error,
                        update.NextAttemptUtc,
                        DeadLetterUtc = update.IsDeadLetter ? update.FailedUtc : (DateTime?)null,
                        ModifiedUtc = update.FailedUtc,
                        LockedUntilUtc = (DateTime?)null,
                        LockId = (string?)null
                    },
                    cancellationToken: cancellationToken);

            if (affected == 1)
            {
                logger.MarkedFailed(update.Id, update.IsDeadLetter);
            }

            return affected == 1;
        }

        public async Task<int> ReleaseAsync(
            IReadOnlyCollection<ClaimedEmailOutboxRecord> records,
            DateTime nextAttemptUtc,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(records);

            using var repository = repositoryFactory.Create();
            var released = 0;

            foreach (var record in records)
            {
                int affected = await LockedRecordQuery(repository.Db, record.Id, record.LockId)
                    .UpdateAsync(
                        new
                        {
                            Status = (int)EmailOutboxStatus.Pending,
                            NextAttemptUtc = nextAttemptUtc,
                            ModifiedUtc = nextAttemptUtc,
                            LockedUntilUtc = (DateTime?)null,
                            LockId = (string?)null
                        },
                        cancellationToken: cancellationToken);

                released += affected;
            }

            logger.Released(released);
            return released;
        }

        private EmailOutboxEnqueueResult Duplicate(EmailOutboxStorageRecord existing)
        {
            var id = Guid.Parse(existing.Id);
            logger.DuplicateFound(id);
            return new EmailOutboxEnqueueResult(id, true, existing.CreatedUtc);
        }

        private static async Task<EmailOutboxStorageRecord?> FindByIdempotencyKey(
            QueryFactory database,
            string idempotencyKey,
            CancellationToken cancellationToken)
        {
            var records = await database.Query(EmailOutboxPersistenceConstants.OutboxTable)
                .Where(nameof(EmailOutboxStorageRecord.IdempotencyKey), idempotencyKey)
                .Limit(1)
                .GetAsync<EmailOutboxStorageRecord>(cancellationToken: cancellationToken);

            return records.FirstOrDefault();
        }

        private static Query LockedRecordQuery(QueryFactory database, Guid id, string lockId)
        {
            return database.Query(EmailOutboxPersistenceConstants.OutboxTable)
                .Where(nameof(EmailOutboxStorageRecord.Id), id.ToString("D"))
                .Where(nameof(EmailOutboxStorageRecord.Status), (int)EmailOutboxStatus.Processing)
                .Where(nameof(EmailOutboxStorageRecord.LockId), lockId);
        }

        private static Query ApplyClaimableFilter(Query query, DateTime utcNow)
        {
            return query.Where(eligible => eligible
                .Where(pending => pending
                    .Where(nameof(EmailOutboxStorageRecord.Status), (int)EmailOutboxStatus.Pending)
                    .Where(nameof(EmailOutboxStorageRecord.NextAttemptUtc), "<=", utcNow))
                .OrWhere(processing => processing
                    .Where(nameof(EmailOutboxStorageRecord.Status), (int)EmailOutboxStatus.Processing)
                    .Where(expired => expired
                        .WhereNull(nameof(EmailOutboxStorageRecord.LockedUntilUtc))
                        .OrWhere(nameof(EmailOutboxStorageRecord.LockedUntilUtc), "<=", utcNow))));
        }
    }
}
