using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.Contacts.Commands;

namespace Api.Controllers
{
    [ApiController, Route("[controller]")]
    public class ContactController : Controller
    {
        private readonly ISender _sender;

        public ContactController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost(@"create")]
        public async Task<IActionResult> Create([FromBody] CreateContactCommand request, CancellationToken cancellationToken = default)
        {
            var response = await _sender.Send(request, cancellationToken);

            if (!response.Success)
            {
                return BadRequest(response.Message);
            }

            return Ok(response);
        }
    }
}
