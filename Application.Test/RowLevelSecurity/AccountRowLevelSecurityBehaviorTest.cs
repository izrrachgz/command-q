using SqlKata;
using SqlKata.Execution;
using Cqrs.Abstractions;
using Common.Enumerations;
using Application.Abstractions;
using Persistence.Abstractions;
using Application.RowLevelSecurity;

namespace Application.Test.RowLevelSecurity
{
    public sealed class AccountRowLevelSecurityBehaviorTest
    {
        [Fact]
        public async Task Account_Request_Should_Set_Account_Context_And_Restore_Restrictive_Context()
        {
            var readOnlyRepository = new RecordingRepository();
            var writableRepository = new RecordingWritableRepository();
            var context = new AccountRowLevelSecurityContext();
            var behavior = new AccountRowLevelSecurityBehavior<AccountRequest, bool>(
                context,
                readOnlyRepository,
                writableRepository);
            var accountId = Guid.NewGuid();

            bool handled = await behavior.Handle(
                new AccountRequest(accountId),
                (_, _) => Task.FromResult(true),
                TestContext.Current.CancellationToken);

            Assert.True(handled);
            Assert.Collection(
                readOnlyRepository.Applications,
                values => AssertValues(values, false, accountId),
                values => AssertValues(values, false, null));
            Assert.Empty(writableRepository.Applications);
        }

        [Fact]
        public async Task System_Request_Should_Set_Bypass_And_Restore_Restrictive_Context()
        {
            var readOnlyRepository = new RecordingRepository();
            var writableRepository = new RecordingWritableRepository();
            var context = new AccountRowLevelSecurityContext();
            var behavior = new AccountRowLevelSecurityBehavior<SystemCommand, bool>(
                context,
                readOnlyRepository,
                writableRepository);

            await behavior.Handle(
                new SystemCommand(),
                (_, _) => Task.FromResult(true),
                TestContext.Current.CancellationToken);

            Assert.Collection(
                writableRepository.Applications,
                values => AssertValues(values, true, null),
                values => AssertValues(values, false, null));
            Assert.Empty(readOnlyRepository.Applications);
        }

        [Fact]
        public async Task Unclassified_Request_Should_Use_Restrictive_Context()
        {
            var readOnlyRepository = new RecordingRepository();
            var writableRepository = new RecordingWritableRepository();
            var context = new AccountRowLevelSecurityContext();
            var behavior = new AccountRowLevelSecurityBehavior<UnclassifiedRequest, bool>(
                context,
                readOnlyRepository,
                writableRepository);

            await behavior.Handle(
                new UnclassifiedRequest(),
                (_, _) => Task.FromResult(true),
                TestContext.Current.CancellationToken);

            Assert.All(readOnlyRepository.Applications, values => AssertValues(values, false, null));
            Assert.Empty(writableRepository.Applications);
        }

        [Fact]
        public async Task Nested_Request_Should_Restore_Outer_Account_Context()
        {
            var readOnlyRepository = new RecordingRepository();
            var writableRepository = new RecordingWritableRepository();
            var context = new AccountRowLevelSecurityContext();
            var accountBehavior = new AccountRowLevelSecurityBehavior<AccountRequest, bool>(
                context,
                readOnlyRepository,
                writableRepository);
            var systemBehavior = new AccountRowLevelSecurityBehavior<SystemQuery, bool>(
                context,
                readOnlyRepository,
                writableRepository);
            var accountId = Guid.NewGuid();

            await accountBehavior.Handle(
                new AccountRequest(accountId),
                async (_, cancellationToken) => await systemBehavior.Handle(
                    new SystemQuery(),
                    (_, _) => Task.FromResult(true),
                    cancellationToken),
                TestContext.Current.CancellationToken);

            Assert.Collection(
                readOnlyRepository.Applications,
                values => AssertValues(values, false, accountId),
                values => AssertValues(values, true, null),
                values => AssertValues(values, false, accountId),
                values => AssertValues(values, false, null));
            Assert.Empty(writableRepository.Applications);
        }

        private static void AssertValues(
            IReadOnlyCollection<DatabaseSessionValue> values,
            bool bypass,
            Guid? accountId)
        {
            Assert.Equal(bypass, values.Single(value => value.Key == "command_q.rls_bypass").Value);
            Assert.Equal(accountId, values.Single(value => value.Key == "command_q.account_id").Value);
        }

        private sealed record AccountRequest(Guid AccountId) : IQuery<bool>, IAccountScopedRequest;

        private sealed record SystemCommand : ICommand<bool>, ISystemScopedRequest;

        private sealed record SystemQuery : IQuery<bool>, ISystemScopedRequest;

        private sealed record UnclassifiedRequest : IQuery<bool>;

        private class RecordingRepository : IReadOnlyRepository, IDatabaseSessionContextRepository
        {
            public DatabaseEngine Engine => DatabaseEngine.PgSql;

            public List<IReadOnlyCollection<DatabaseSessionValue>> Applications { get; } = [];

            public Task SetSessionContextAsync(
                IReadOnlyCollection<DatabaseSessionValue> values,
                CancellationToken cancellationToken = default)
            {
                Applications.Add(values.ToArray());
                return Task.CompletedTask;
            }

            public Query Query(string table) => new(table);

            public Task<T?> FirstOrDefaultAsync<T>(Query query, CancellationToken cancellationToken = default) =>
                throw new NotSupportedException();

            public Task<IEnumerable<T>> GetAsync<T>(Query query, CancellationToken cancellationToken = default) =>
                throw new NotSupportedException();

            public Task<PaginationResult<T>> PaginateAsync<T>(
                Query query,
                int page,
                int perPage,
                CancellationToken cancellationToken = default) => throw new NotSupportedException();

            public Task<bool> ExistsAsync(Query query, CancellationToken cancellationToken = default) =>
                throw new NotSupportedException();
        }

        private sealed class RecordingWritableRepository : RecordingRepository, IWritableRepository
        {
            public QueryFactory Db => throw new NotSupportedException();
        }
    }
}
