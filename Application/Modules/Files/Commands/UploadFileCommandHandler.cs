using Public;
using Common.Cqrs;
using SqlKata.Execution;
using System.Globalization;
using System.Text.Json;
using Common.Models.Response;
using Persistence.Interfaces;
using Localization.Abstractions;
using Microsoft.Extensions.Logging;
using Application.Modules.Files.Logging;
using Application.Modules.Identity.AccountActivity;
using Application.Modules.Identity.AccountActivity.Commands;
using Application.Modules.Identity.AccountActivity.Services;

namespace Application.Modules.Files.Commands
{
    public class UploadFileCommandHandler : ICommandHandler<UploadFileCommand, BasicResponse>
    {
        private readonly ITextProvider _messages;
        private readonly IWritableRepository _repository;
        private readonly IAccountActivityRecorder _activityRecorder;
        private readonly ILogger<UploadFileCommandHandler> _logger;

        private readonly string _message_file_created;
        private readonly string _message_file_cannot_create;
        private static readonly string _fileStoragePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileUploadConstants.StorageRootFolderName);

        public UploadFileCommandHandler(ITextProvider textProvider, IWritableRepository repository, IAccountActivityRecorder activityRecorder, ILogger<UploadFileCommandHandler> logger)
        {
            _logger = logger;
            _repository = repository;
            _messages = textProvider;
            _activityRecorder = activityRecorder;
            _message_file_created = _messages.Get(nameof(Messages.Created), nameof(AccountFile));
            _message_file_cannot_create = _messages.Get(nameof(Messages.Cannot_Create), nameof(AccountFile));
        }

        /// <summary>
        /// Handles the file upload command by saving the uploaded file to a temporary location,
        /// then moving it to a structured storage path based on the current date. Returns a BasicResponse indicating success or failure.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<BasicResponse> Handle(UploadFileCommand request, CancellationToken cancellationToken)
        {
            var safeFileName = Path.GetFileName(request.FileName);

            if (string.IsNullOrWhiteSpace(safeFileName))
            {
                _logger.FileUploadEmptyName(request.AccountId);
                return new(false, _message_file_cannot_create);
            }

            var tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}{FileUploadConstants.TempFileExtension}");

            _logger.FileUploadTempPathCreated(request.AccountId, safeFileName);

            try
            {
                var options = new FileStreamOptions
                {
                    Mode = FileMode.CreateNew,
                    Access = FileAccess.Write,
                    Share = FileShare.None,
                    BufferSize = FileUploadConstants.BufferSizeBytes,
                    Options = FileOptions.Asynchronous | FileOptions.SequentialScan
                };

                await using var tempStream = new FileStream(tempFilePath, options);

                await request.FileData.CopyToAsync(tempStream, FileUploadConstants.BufferSizeBytes, cancellationToken);

                await tempStream.FlushAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                TryDelete(tempFilePath);
                throw;
            }
            catch (Exception ex)
            {
                _logger.FileUploadFailed(ex, request.FileName);

                TryDelete(tempFilePath);

                return new(false, _message_file_cannot_create);
            }

            var now = DateTime.UtcNow;

            #region setup file storage destination path

            string destination = Path.Combine(
                _fileStoragePath,
                now.Year.ToString(CultureInfo.InvariantCulture),
                now.Month.ToString(FileUploadConstants.DatePathSegmentFormat, CultureInfo.InvariantCulture),
                now.Day.ToString(FileUploadConstants.DatePathSegmentFormat, CultureInfo.InvariantCulture));

            Directory.CreateDirectory(destination);

            var extension = Path.GetExtension(safeFileName);

            destination = Path.Combine(destination, $"{Guid.NewGuid():N}{extension}");

            _logger.FileUploadDestinationGenerated(request.AccountId, Path.GetRelativePath(AppDomain.CurrentDomain.BaseDirectory, destination));

            #endregion

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                File.Move(tempFilePath, destination, overwrite: false);

                var accountFile = new AccountFile
                {
                    AccountId = request.AccountId,
                    FileName = safeFileName,
                    Path = Path.GetRelativePath(AppDomain.CurrentDomain.BaseDirectory, destination),
                    ContentType = request.ContentType,
                    Size = request.FileData.CanSeek ? request.FileData.Length : 0,
                    Created = now,
                    Modified = now
                };

                var accountFileId = await _repository.Db.Query($@"{nameof(AccountFile)}")
                    .InsertGetIdAsync<long>(new
                    {
                        accountFile.AccountId,
                        accountFile.FileName,
                        accountFile.Path,
                        accountFile.ContentType,
                        accountFile.Size,
                        accountFile.Created,
                        accountFile.Modified
                    }, cancellationToken: cancellationToken);

                _logger.FileMetadataSaved(request.AccountId, accountFileId, safeFileName, accountFile.Size, request.ContentType);

                await _activityRecorder.RecordAsync(new RecordAccountActivityCommand
                {
                    AccountId = request.AccountId,
                    Module = AccountActivityConstants.Modules.Files,
                    Action = AccountActivityConstants.Actions.FileUploaded,
                    EntityType = nameof(AccountFile),
                    EntityId = accountFileId.ToString(),
                    Message = string.Format(CultureInfo.InvariantCulture, AccountActivityConstants.Messages.FileUploadedFormat, safeFileName),
                    MetadataJson = JsonSerializer.Serialize(new
                    {
                        FileName = safeFileName,
                        request.ContentType,
                        accountFile.Size
                    })
                }, cancellationToken);

                return new(true, _message_file_created);
            }
            catch (OperationCanceledException)
            {
                TryDelete(tempFilePath);
                TryDelete(destination);
                throw;
            }
            catch (Exception ex)
            {
                _logger.FileMetadataCreationFailed(ex, request.FileName);

                TryDelete(destination);

                return new(false, _message_file_cannot_create);
            }
            finally
            {
                TryDelete(tempFilePath);
            }
        }

        /// <summary>
        /// Tries to delete a file at the specified path. If the file does not exist, it does nothing.
        /// </summary>
        /// <param name="path"></param>
        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch { }
        }
    }
}
