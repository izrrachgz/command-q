using MediatR;
using Api.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Application.Modules.Addresses.Queries;
using Application.Modules.Addresses.Commands;

namespace Api.Modules.Addresses
{
    [ApiController, Authorize, Route(AddressRouteConstants.Root)]
    public sealed class AddressController(ISender mediator, ICurrentAccountAccessor currentAccountAccessor) : Controller
    {
        private readonly ISender _mediator = mediator;

        /// <summary>
        /// Creates a new address using the specified command.
        /// </summary>
        /// <param name="request">The command containing the details of the address to create. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>An IActionResult that represents the result of the create operation. Returns an HTTP 200 response with the
        /// created address information if successful; otherwise, returns an HTTP 400 response with an error message.</returns>
        [HttpPost(AddressRouteConstants.Create)]
        public async Task<IActionResult> Create([FromBody] CreateAddressCommand request, CancellationToken cancellationToken = default)
        {
            var currentAccount = await currentAccountAccessor.Get(cancellationToken);

            if (currentAccount is null)
                return Forbid();

            request.AccountId = currentAccount.AccountId;

            var response = await _mediator.Send(request, cancellationToken);

            if (!response.Success)
            {
                return BadRequest(response.Message);
            }

            return Ok(response);
        }

        /// <summary>
        /// Gets the current account's most recently modified address.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken = default)
        {
            var currentAccount = await currentAccountAccessor.Get(cancellationToken);

            if (currentAccount is null)
                return Forbid();

            var query = new GetAddressByAccountIdQuery
            {
                AccountId = currentAccount.AccountId
            };

            var response = await _mediator.Send(query, cancellationToken);

            return response.Success ? Ok(response) : NotFound(response);
        }
    }
}
