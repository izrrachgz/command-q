using Public;
using MediatR;
using Api.Constants;
using Api.Extensions;
using Common.Models.Request;
using Microsoft.AspNetCore.Mvc;
using Localization.Abstractions;
using Application.Modules.Files.Queries;
using Microsoft.AspNetCore.RateLimiting;
using Application.Modules.Files.Commands;
using Microsoft.AspNetCore.Authorization;

namespace Api.Modules.Files
{
    [ApiController, Authorize, Route(@"[controller]")]
    public sealed class FilesController : ControllerBase
    {
        private readonly ISender _mediator;
        private readonly ITextProvider _messages;

        public FilesController(ISender mediator, ITextProvider messages)
        {
            _mediator = mediator;
            _messages = messages;
        }

        /// <summary>
        /// Uploads a file to the server.
        /// </summary>
        [HttpPost(@"upload"), Consumes(ContentTypes.MultipartFormData)]
        [EnableRateLimiting(ConfigKeys.RateLimiting.FixedPolicyName)]
        public async Task<IActionResult> Upload([FromForm] IFormFile file, CancellationToken cancellationToken)
        {
            if (file is null || file.Length == 0)
                return BadRequest(_messages.Get(nameof(Messages.File_Is_Required)));

            if (!User.TryGetAccountId(out var accountId))
                return Unauthorized(_messages.Get(nameof(Messages.Unauthorized)));

            await using var fileStream = file.OpenReadStream();

            var command = new UploadFileCommand
            {
                AccountId = accountId,
                FileName = file.FileName,
                FileData = fileStream,
                ContentType = file.ContentType
            };

            var response = await _mediator.Send(command, cancellationToken);

            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Gets file metadata for the authenticated account without returning file content.
        /// </summary>
        [HttpGet(@"get")]
        public async Task<IActionResult> Get([FromQuery] SearchCriteria searchCriteria, CancellationToken cancellationToken)
        {
            if (!User.TryGetAccountId(out var accountId))
                return Unauthorized(_messages.Get(nameof(Messages.Unauthorized)));

            var query = new GetAccountFilesQuery
            {
                AccountId = accountId,
                SearchCriteria = searchCriteria
            };

            var response = await _mediator.Send(query, cancellationToken);

            return Ok(response);
        }
    }
}
