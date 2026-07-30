using MediatR;
using Api.Extensions;
using Microsoft.AspNetCore.Mvc;
using Application.Contacts.Queries;
using Application.Contacts.Commands;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers
{
    [ApiController, Authorize, Route(@"[controller]")]
    public class ContactController(ISender mediator) : Controller
    {
        private readonly ISender _mediator = mediator;

        /// <summary>
        /// Creates a new contact using the specified command.
        /// </summary>
        /// <param name="request">The command containing the details of the contact to create. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>An IActionResult that represents the result of the create operation. Returns 200 OK with the response if
        /// successful; otherwise, returns 400 Bad Request with an error message.</returns>
        [HttpPost(@"create")]
        public async Task<IActionResult> Create([FromBody] CreateContactCommand request, CancellationToken cancellationToken = default)
        {
            if (!User.TryGetAccountId(out var accountId))
                return Unauthorized();

            request.AccountId = accountId;

            var response = await _mediator.Send(request, cancellationToken);

            if (!response.Success)
            {
                return BadRequest(response.Message);
            }

            return Ok(response);
        }

        /// <summary>
        /// Gets the current account's most recently modified contact.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken = default)
        {
            if (!User.TryGetAccountId(out var accountId))
                return Unauthorized();

            var query = new GetContactByAccountIdQuery
            {
                AccountId = accountId
            };

            var response = await _mediator.Send(query, cancellationToken);

            return response.Success ? Ok(response) : NotFound(response);
        }
    }
}
