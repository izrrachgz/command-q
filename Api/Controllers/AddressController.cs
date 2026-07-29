using MediatR;
using Api.Extensions;
using Microsoft.AspNetCore.Mvc;
using Application.Addresses.Queries;
using Application.Addresses.Commands;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers
{
    [ApiController, Authorize, Route(@"[controller]")]
    public class AddressController(ISender sender) : Controller
    {
        private readonly ISender _sender = sender;

        /// <summary>
        /// Creates a new address using the specified command.
        /// </summary>
        /// <param name="request">The command containing the details of the address to create. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>An IActionResult that represents the result of the create operation. Returns an HTTP 200 response with the
        /// created address information if successful; otherwise, returns an HTTP 400 response with an error message.</returns>
        [HttpPost(@"create")]
        public async Task<IActionResult> Create([FromBody] CreateAddressCommand request, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetAccountId(out var accountId))
                return Unauthorized();

            request.AccountId = accountId;

            var response = await _sender.Send(request, cancellationToken);

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
            if (!User.TryGetAccountId(out var accountId))
                return Unauthorized();

            var query = new GetAddressByAccountIdQuery
            {
                AccountId = accountId
            };

            var response = await _sender.Send(query, cancellationToken);

            return response.Success ? Ok(response) : NotFound(response);
        }
    }
}
