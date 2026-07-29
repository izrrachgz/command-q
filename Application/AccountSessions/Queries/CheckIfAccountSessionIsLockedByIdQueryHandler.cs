using Public;
using Common.Cqrs;
using Domain.Entities;
using Domain.Interfaces;
using Common.Models.Response;
using Localization.Abstractions;

namespace Application.AccountSessions.Queries
{
    public class CheckIfAccountSessionIsLockedByIdQueryHandler : IQueryHandler<CheckIfAccountSessionIsLockedByIdQuery, BasicResponse>
    {
        private readonly ITextProvider _messages;
        private readonly IReadOnlyRepository _repository;

        private readonly string _message_account_locked;
        private readonly string _message_record_not_found;

        public CheckIfAccountSessionIsLockedByIdQueryHandler(IReadOnlyRepository repository, ITextProvider textProvider)
        {
            _repository = repository;
            _messages = textProvider;
            _message_account_locked = _messages.Get(nameof(Messages.Account_Locked));
            _message_record_not_found = _messages.Get(nameof(Messages.Record_Not_Found));
        }

        /// <summary>
        /// Handles the retrieval of an account by username and returns detailed information about the account.
        /// </summary>
        /// <param name="request">The query containing the username of the account to retrieve.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a DetailResponse<AccountModel>
        /// with the account details if found; otherwise, the result indicates that no matching account was found.</returns>
        public async Task<BasicResponse> Handle(CheckIfAccountSessionIsLockedByIdQuery request, CancellationToken cancellationToken)
        {
            var query = _repository.Query($@"{nameof(AccountSession)}")
                .WhereNull($@"{nameof(AccountSession.Deleted)}")
                .Where($@"{nameof(AccountSession.AccountId)}", request.AccountId)
                .Where($@"{nameof(AccountSession.FailedLoginAttempts)}", ">=", request.MaxFailedAttempts)
                .Where($@"{nameof(AccountSession.LockedUntilUtc)}", ">=", DateTime.UtcNow);

            bool exists = await _repository.ExistsAsync(query, cancellationToken);

            string message = exists ? _message_account_locked : _message_record_not_found;

            return new(exists, message);
        }
    }
}
