using MediatR;
using Api.Constants;
using Microsoft.AspNetCore.Mvc;
using Application.Files.Commands;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers
{
    [ApiController, Authorize, Route("[controller]")]
    public sealed class FilesController : ControllerBase
    {
        private readonly ISender _sender;

        public FilesController(ISender sender)
        {
            _sender = sender;
        }

        /// <summary>
        /// Uploads a file to the server.
        /// </summary>
        [HttpPost("upload"), Consumes("multipart/form-data")]
        [EnableRateLimiting(AppSettingsKeys.RateLimitingFixedPolicyName)]
        public async Task<IActionResult> Upload([FromForm] IFormFile file, CancellationToken cancellationToken)
        {
            if (file is null || file.Length == 0)
                return BadRequest("A file is required.");

            await using var fileStream = file.OpenReadStream();

            var command = new UploadFileCommand
            {
                FileName = file.FileName,
                FileData = fileStream
            };

            var response = await _sender.Send(command, cancellationToken);

            return response.Success ? Ok(response) : BadRequest(response);
        }
    }
}