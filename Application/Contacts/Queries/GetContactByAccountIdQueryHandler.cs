using Public;
using Common.Cqrs;
using Domain.Entities;
using SqlKata.Execution;
using Domain.Interfaces;
using Application.Interfaces;
using Common.Models.Response;
using Localization.Abstractions;
using Application.Contacts.Dtos;

namespace Application.Contacts.Queries
{
    public class GetContactByAccountIdQueryHandler : IQueryHandler<GetContactByAccountIdQuery, DetailResponse<ContactModel>>
    {
        private readonly IMapper<Contact, ContactModel> _mapper;
        private readonly ITextProvider _messages;
        private readonly IReadOnlyRepository _repository;

        private readonly string _message_record_found;
        private readonly string _message_record_not_found;

        public GetContactByAccountIdQueryHandler(IMapper<Contact, ContactModel> mapper, IReadOnlyRepository repository, ITextProvider textProvider)
        {
            _mapper = mapper;
            _repository = repository;
            _messages = textProvider;
            _message_record_found = _messages.Get(nameof(Messages.Record_Found));
            _message_record_not_found = _messages.Get(nameof(Messages.Record_Not_Found));
        }

        /// <summary>
        /// Handles the retrieval of the most recently modified contact associated with the specified account
        /// identifier.
        /// </summary>
        /// <param name="request">The query containing the account identifier for which to retrieve the contact.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a DetailResponse<ContactModel>
        /// with the contact details if found; otherwise, the response indicates that no contact was found.</returns>
        public async Task<DetailResponse<ContactModel>> Handle(GetContactByAccountIdQuery request, CancellationToken cancellationToken)
        {
            var query = _repository.Db.Query($@"{nameof(Contact)}")
                .WhereNull($@"{nameof(Contact.Deleted)}")
                .Where($@"{nameof(Contact.AccountId)}", request.AccountId)
                .OrderByDesc($@"{nameof(Contact.Modified)}")
                .Limit(1);

            var record = await query.FirstOrDefaultAsync<Contact>(cancellationToken: cancellationToken);

            if (record is null)
                return new(false, _message_record_not_found, null);

            return new(true, _message_record_found, _mapper.Map(record));
        }
    }
}
