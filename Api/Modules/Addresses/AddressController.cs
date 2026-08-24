using MediatR;
using Api.Abstractions;
using Api.Constants;
using Api.Extensions;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Api.Modules.Addresses.Requests;
using Application.Modules.Addresses.Queries;
using Application.Modules.Addresses.Commands;
using Application.Modules.Addresses.Dtos;

namespace Api.Modules.Addresses
{
    [ApiController, Authorize, Route(AddressRoute.Root)]
    public sealed class AddressController(ISender mediator, ICurrentAccountAccessor currentAccountAccessor) : ControllerBase
    {
        private readonly ISender _mediator = mediator;

        /// <summary>
        /// Creates a new address using the specified command.
        /// </summary>
        /// <param name="request">The address details supplied by the client.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A 201 response when the address is created; otherwise, a 400 response with the operation result.</returns>
        [HttpPost]
        public async Task<ActionResult<OperationResponse>> Create(
            [FromBody] CreateAddressRequest request,
            CancellationToken cancellationToken = default)
        {
            var currentAccount = await currentAccountAccessor.Get(cancellationToken);

            if (currentAccount is null)
                return this.ApiProblem(StatusCodes.Status403Forbidden, ApiProblemCodes.Forbidden);

            var response = await _mediator.Send(new CreateAddressCommand
            {
                AccountId = currentAccount.AccountId,
                Name = request.Name,
                Description = request.Description,
                Line1 = request.Line1,
                Line2 = request.Line2,
                ExternalNumber = request.ExternalNumber,
                InternalNumber = request.InternalNumber,
                References = request.References,
                City = request.City,
                PostalCode = request.PostalCode,
                Country = request.Country,
                Phone = request.Phone,
                EmailAddress = request.EmailAddress
            }, cancellationToken);

            if (!response.Success)
            {
                return this.ApiProblem(
                    StatusCodes.Status500InternalServerError,
                    ApiProblemCodes.Addresses.CreationFailed,
                    response.Message);
            }

            return CreatedAtAction(nameof(Get), response.ToOperationResponse());
        }

        /// <summary>
        /// Gets the current account's most recently modified address.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<AddressModel>> Get(CancellationToken cancellationToken = default)
        {
            var currentAccount = await currentAccountAccessor.Get(cancellationToken);

            if (currentAccount is null)
                return this.ApiProblem(StatusCodes.Status403Forbidden, ApiProblemCodes.Forbidden);

            var query = new GetAddressByAccountIdQuery
            {
                AccountId = currentAccount.AccountId
            };

            var response = await _mediator.Send(query, cancellationToken);

            return response.Success
                ? response.Detail!
                : this.ApiProblem(
                    StatusCodes.Status404NotFound,
                    ApiProblemCodes.Addresses.NotFound,
                    response.Message);
        }
    }
}
