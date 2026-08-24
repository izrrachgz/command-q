using MediatR;
using Api.Abstractions;
using Api.Constants;
using Api.Extensions;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Api.Modules.Contacts.Requests;
using Application.Modules.Contacts.Queries;
using Application.Modules.Contacts.Commands;
using Application.Modules.Contacts.Dtos;

namespace Api.Modules.Contacts
{
    [ApiController, Authorize, Route(ContactRoute.Root)]
    public sealed class ContactController(ISender mediator, ICurrentAccountAccessor currentAccountAccessor) : ControllerBase
    {
        private readonly ISender _mediator = mediator;

        /// <summary>
        /// Creates a new contact using the specified command.
        /// </summary>
        /// <param name="request">The contact details supplied by the client.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A 201 response when the contact is created; otherwise, a 400 response with the operation result.</returns>
        [HttpPost]
        public async Task<ActionResult<OperationResponse>> Create(
            [FromBody] CreateContactRequest request,
            CancellationToken cancellationToken = default)
        {
            var currentAccount = await currentAccountAccessor.Get(cancellationToken);

            if (currentAccount is null)
                return this.ApiProblem(StatusCodes.Status403Forbidden, ApiProblemCodes.Forbidden);

            var response = await _mediator.Send(new CreateContactCommand
            {
                AccountId = currentAccount.AccountId,
                Name = request.Name,
                Description = request.Description,
                EmailAddress = request.EmailAddress,
                PhoneNumber = request.PhoneNumber
            }, cancellationToken);

            if (!response.Success)
            {
                return this.ApiProblem(
                    StatusCodes.Status500InternalServerError,
                    ApiProblemCodes.Contacts.CreationFailed,
                    response.Message);
            }

            return CreatedAtAction(nameof(Get), response.ToOperationResponse());
        }

        /// <summary>
        /// Gets the current account's most recently modified contact.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ContactModel>> Get(CancellationToken cancellationToken = default)
        {
            var currentAccount = await currentAccountAccessor.Get(cancellationToken);

            if (currentAccount is null)
                return this.ApiProblem(StatusCodes.Status403Forbidden, ApiProblemCodes.Forbidden);

            var query = new GetContactByAccountIdQuery
            {
                AccountId = currentAccount.AccountId
            };

            var response = await _mediator.Send(query, cancellationToken);

            return response.Success
                ? response.Detail!
                : this.ApiProblem(
                    StatusCodes.Status404NotFound,
                    ApiProblemCodes.Contacts.NotFound,
                    response.Message);
        }
    }
}
