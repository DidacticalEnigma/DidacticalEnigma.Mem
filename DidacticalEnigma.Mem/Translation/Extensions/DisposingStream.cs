using System;
using System.IO;

namespace DidacticalEnigma.Mem.Translation.Extensions
{
    // Wraps a stream, but also disposes another object afterwards
    public class DisposingStream : Stream
    {
        private Stream wrappedStream;
        
        private readonly IDisposable disposable;

        public DisposingStream(Stream wrappedStream, IDisposable disposable)
        {
            this.wrappedStream = wrappedStream;
            this.disposable = disposable;
        }

        public override void Flush()
        {
            wrappedStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return wrappedStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return wrappedStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            wrappedStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            wrappedStream.Write(buffer, offset, count);
        }

        public override bool CanRead => wrappedStream.CanRead;

        public override bool CanSeek => wrappedStream.CanSeek;

        public override bool CanWrite => wrappedStream.CanWrite;

        public override long Length => wrappedStream.Length;

        public override long Position
        {
            get => wrappedStream.Position;
            set => wrappedStream.Position = value;
        }

        protected override void Dispose(bool disposing)
        {
            this.wrappedStream.Dispose();
            this.disposable.Dispose();
        }
    }
}