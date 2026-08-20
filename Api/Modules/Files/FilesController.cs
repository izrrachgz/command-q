using Public;
using MediatR;
using Api.Constants;
using Api.Abstractions;
using Common.Models.Request;
using Microsoft.AspNetCore.Mvc;
using Localization.Abstractions;
using Application.Modules.Files.Queries;
using Microsoft.AspNetCore.RateLimiting;
using Application.Modules.Files.Commands;
using Microsoft.AspNetCore.Authorization;

namespace Api.Modules.Files
{
    [ApiController, Authorize, Route(FilesRouteConstants.Root)]
    public sealed class FilesController : ControllerBase
    {
        private readonly ISender _mediator;
        private readonly ITextProvider _messages;
        private readonly ICurrentAccountAccessor _currentAccountAccessor;

        public FilesController(ISender mediator, ITextProvider messages, ICurrentAccountAccessor currentAccountAccessor)
        {
            _mediator = mediator;
            _messages = messages;
            _currentAccountAccessor = currentAccountAccessor;
        }

        /// <summary>
        /// Uploads a file to the server.
        /// </summary>
        [HttpPost(FilesRouteConstants.Upload), Consumes(ContentTypes.MultipartFormData)]
        [EnableRateLimiting(ApiConfigKeys.RateLimiting.FixedPolicyName)]
        public async Task<IActionResult> Upload([FromForm] IFormFile file, CancellationToken cancellationToken)
        {
            if (file is null || file.Length == 0)
                return BadRequest(_messages.Get(nameof(Messages.File_Is_Required)));

            var currentAccount = await _currentAccountAccessor.Get(cancellationToken);

            if (currentAccount is null)
                return Forbid();

            await using var fileStream = file.OpenReadStream();

            var command = new UploadFileCommand
            {
                AccountId = currentAccount.AccountId,
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
        [HttpGet(FilesRouteConstants.Get)]
        public async Task<IActionResult> Get([FromQuery] SearchCriteria searchCriteria, CancellationToken cancellationToken)
        {
            var currentAccount = await _currentAccountAccessor.Get(cancellationToken);

            if (currentAccount is null)
                return Forbid();

            var query = new GetAccountFilesQuery
            {
                AccountId = currentAccount.AccountId,
                SearchCriteria = searchCriteria
            };

            var response = await _mediator.Send(query, cancellationToken);

            return Ok(response);
        }
    }
}
