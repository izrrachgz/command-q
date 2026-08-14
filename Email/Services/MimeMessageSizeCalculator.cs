using MimeKit;

namespace Email.Services
{
    /// <summary>
    /// Measures the serialized size of a complete MIME message without buffering its bytes.
    /// </summary>
    internal sealed class MimeMessageSizeCalculator
    {
        /// <summary>
        /// Measures the exact number of bytes written by MimeKit for the supplied message.
        /// </summary>
        /// <param name="message">The MIME message to measure.</param>
        /// <param name="cancellationToken">The cancellation token for serialization.</param>
        /// <returns>The serialized MIME size in bytes.</returns>
        public async Task<long> MeasureAsync(MimeMessage message, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(message);

            var stream = new CountingStream();
            await message.WriteToAsync(stream, cancellationToken);
            return stream.Length;
        }

        private sealed class CountingStream : Stream
        {
            private long _length;

            public override bool CanRead => false;
            public override bool CanSeek => false;
            public override bool CanWrite => true;
            public override long Length => _length;

            public override long Position
            {
                get => _length;
                set => throw new NotSupportedException();
            }

            public override void Flush()
            {
            }

            public override Task FlushAsync(CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                _length += count;
            }

            public override void WriteByte(byte value)
            {
                _length++;
            }

            public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                _length += count;
                return Task.CompletedTask;
            }

            public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
            {
                _length += buffer.Length;
                return ValueTask.CompletedTask;
            }
        }
    }
}
