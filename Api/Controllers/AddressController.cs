using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.Addresses.Commands;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers
{
    [ApiController, Authorize, Route("[controller]")]
    public class AddressController : Controller
    {
        private readonly ISender _sender;

        public AddressController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost(@"create")]
        public async Task<IActionResult> Create([FromBody] CreateAddressCommand request, CancellationToken cancellationToken = default)
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
