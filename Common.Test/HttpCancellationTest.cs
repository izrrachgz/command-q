using Common.Transfer.Http;

namespace Common.Test.Transfer.Http;

public sealed class HttpCancellationTest
{
    [Fact]
    public async Task HttpCall_Should_Propagate_Caller_Cancellation()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new HttpCall().Get(
            "http://127.0.0.1:1",
            cancellationToken: cancellationTokenSource.Token));
    }

    [Fact]
    public async Task FileUploader_Should_Propagate_Caller_Cancellation()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => new FileUploader().UploadBytes(
            "http://127.0.0.1:1",
            [1, 2, 3],
            "payload.bin",
            cancellationToken: cancellationTokenSource.Token));
    }
}
