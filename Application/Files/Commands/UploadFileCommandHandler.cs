using Public;
using Common.Cqrs;
using Domain.Entities;
using SqlKata.Execution;
using Persistence.Interfaces;
using Common.Models.Response;
using Localization.Abstractions;
using Microsoft.Extensions.Logging;

namespace Application.Files.Commands
{
    public class UploadFileCommandHandler : ICommandHandler<UploadFileCommand, BasicResponse>
    {
        private readonly ITextProvider _messages;
        private readonly IWritableRepository _repository;
        private readonly ILogger<UploadFileCommandHandler> _logger;

        private const int _buffer_size = 64 * 1024;
        private readonly string _message_file_created;
        private readonly string _message_file_cannot_create;
        private static readonly string _fileStoragePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UploadedFiles");

        public UploadFileCommandHandler(ITextProvider textProvider, IWritableRepository repository, ILogger<UploadFileCommandHandler> logger)
        {
            _logger = logger;
            _repository = repository;
            _messages = textProvider;
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
                _logger.LogWarning(@"File upload rejected for account {AccountId} because the file name was empty.", request.AccountId);
                return new(false, _message_file_cannot_create);
            }

            var tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.upload");

            _logger.LogDebug(@"File upload temp path created for account {AccountId}. FileName: {FileName}.", request.AccountId, safeFileName);

            try
            {
                var options = new FileStreamOptions
                {
                    Mode = FileMode.CreateNew,
                    Access = FileAccess.Write,
                    Share = FileShare.None,
                    BufferSize = _buffer_size,
                    Options = FileOptions.Asynchronous | FileOptions.SequentialScan
                };

                await using var tempStream = new FileStream(tempFilePath, options);

                await request.FileData.CopyToAsync(tempStream, _buffer_size, cancellationToken);

                await tempStream.FlushAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                TryDelete(tempFilePath);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @"Error uploading file {FileName}", request.FileName);

                TryDelete(tempFilePath);

                return new(false, _message_file_cannot_create);
            }

            var now = DateTime.UtcNow;

            #region setup file storage destination path

            string destination = Path.Combine(_fileStoragePath, now.Year.ToString(), now.Month.ToString("D2"), now.Day.ToString("D2"));

            Directory.CreateDirectory(destination);

            var extension = Path.GetExtension(safeFileName);

            destination = Path.Combine(destination, $"{Guid.NewGuid():N}{extension}");

            _logger.LogDebug(@"File upload destination path generated for account {AccountId}. RelativePath: {RelativePath}.", request.AccountId, Path.GetRelativePath(AppDomain.CurrentDomain.BaseDirectory, destination));

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
                    Size = request.FileData.CanSeek ? request.FileData.Length : 0
                };

                var accountFileId = await _repository.Db.Query($@"{nameof(AccountFile)}")
                    .InsertGetIdAsync<long>(accountFile, cancellationToken: cancellationToken);

                _logger.LogInformation(@"File metadata saved. AccountId: {AccountId}. AccountFileId: {AccountFileId}. FileName: {FileName}. Size: {Size}. ContentType: {ContentType}.", request.AccountId, accountFileId, safeFileName, accountFile.Size, request.ContentType);

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
                _logger.LogError(ex, @"Error creating file {FileName}", request.FileName);

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
