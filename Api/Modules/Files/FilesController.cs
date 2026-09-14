using Public;
using Cqrs.Abstractions;
using Api.Constants;
using Api.Abstractions;
using Api.Extensions;
using Api.Models;
using Common.Models.Request;
using Microsoft.AspNetCore.Mvc;
using Localization.Abstractions;
using Api.Modules.Files.Policies;
using Api.Modules.Files.Requests;
using Application.Modules.Files.Queries;
using Microsoft.AspNetCore.RateLimiting;
using Application.Modules.Files.Commands;
using Application.Modules.Files.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace Api.Modules.Files
{
    [ApiController, Authorize, Route(FilesRoute.Root)]
    public sealed class FilesController : ControllerBase
    {
        private readonly IRequestDispatcher _dispatcher;
        private readonly ITextProvider _messages;
        private readonly ICurrentAccountAccessor _currentAccountAccessor;

        public FilesController(IRequestDispatcher dispatcher, ITextProvider messages, ICurrentAccountAccessor currentAccountAccessor)
        {
            _dispatcher = dispatcher;
            _messages = messages;
            _currentAccountAccessor = currentAccountAccessor;
        }

        /// <summary>
        /// Uploads a supported document, image, audio, video or ZIP file no larger than 200 MB.
        /// </summary>
        /// <param name="request">The multipart file supplied by the client.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the upload.</param>
        /// <returns>A 201 response when the file is stored; otherwise, a 400 response describing the rejected upload.</returns>
        [HttpPost, Consumes(ContentTypes.MultipartFormData)]
        [RequestSizeLimit(FileUploadPolicies.MaximumRequestSizeBytes)]
        [RequestFormLimits(MultipartBodyLengthLimit = FileUploadPolicies.MaximumFileSizeBytes)]
        [EnableRateLimiting(ApiConfigKeys.RateLimiting.ConcurrencyPolicyName)]
        public async Task<ActionResult<OperationResponse>> Upload(
            [FromForm] UploadFileRequest request,
            CancellationToken cancellationToken)
        {
            var validationFailure = FileUploadPolicies.General.Validate(request.File);

            if (validationFailure != FileUploadValidationFailure.None)
            {
                return this.ApiProblem(
                    StatusCodes.Status400BadRequest,
                    GetUploadProblemCode(validationFailure),
                    GetUploadValidationMessage(validationFailure));
            }

            var currentAccount = await _currentAccountAccessor.Get(cancellationToken);

            if (currentAccount is null)
                return this.ApiProblem(StatusCodes.Status403Forbidden, ApiProblemCodes.Forbidden);

            IFormFile file = request.File!;
            await using var fileStream = file.OpenReadStream();

            var command = new UploadFileCommand
            {
                AccountId = currentAccount.AccountId,
                FileName = file.FileName,
                FileData = fileStream,
                ContentType = file.ContentType
            };

            var response = await _dispatcher.Send(command, cancellationToken);

            return response.Success
                ? CreatedAtAction(nameof(Get), response.ToOperationResponse())
                : this.ApiProblem(
                    StatusCodes.Status500InternalServerError,
                    ApiProblemCodes.Files.UploadFailed,
                    response.Message);
        }

        /// <summary>
        /// Gets file metadata for the authenticated account without returning file content.
        /// </summary>
        /// <param name="searchCriteria">Paging, sorting and optional filtering criteria.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>The authenticated account's paged file metadata.</returns>
        [HttpGet]
        public async Task<ActionResult<PageResponse<AccountFileModel>>> Get(
            [FromQuery] SearchCriteria searchCriteria,
            CancellationToken cancellationToken)
        {
            var currentAccount = await _currentAccountAccessor.Get(cancellationToken);

            if (currentAccount is null)
                return this.ApiProblem(StatusCodes.Status403Forbidden, ApiProblemCodes.Forbidden);

            var query = new GetAccountFilesQuery
            {
                AccountId = currentAccount.AccountId,
                SearchCriteria = searchCriteria
            };

            var response = await _dispatcher.Send(query, cancellationToken);

            return response.ToPageResponse();
        }

        private static string GetUploadProblemCode(FileUploadValidationFailure failure)
        {
            return failure switch
            {
                FileUploadValidationFailure.FileRequired => ApiProblemCodes.Files.Required,
                FileUploadValidationFailure.FileTooLarge => ApiProblemCodes.Files.TooLarge,
                _ => ApiProblemCodes.Files.TypeNotAllowed
            };
        }

        private string GetUploadValidationMessage(FileUploadValidationFailure failure)
        {
            return failure switch
            {
                FileUploadValidationFailure.FileRequired => _messages.Get(nameof(Messages.File_Is_Required)),
                FileUploadValidationFailure.FileTooLarge => _messages.Get(
                    nameof(Messages.File_Exceeds_Maximum_Size),
                    FileUploadPolicies.MaximumFileSizeMegabytes),
                _ => _messages.Get(nameof(Messages.File_Type_Not_Allowed))
            };
        }
    }
}
