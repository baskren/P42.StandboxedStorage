using System;
namespace P42.Storage.Native
{
    class StorageFileStream : IStorageFileStream
    {
        #region Properties
        public bool CanRead => _fileStream.CanRead;

        public bool CanSeek => _fileStream.CanSeek;

        public bool CanTimeout => _fileStream.CanTimeout;

        public bool CanWrite => _fileStream.CanWrite;

        public long Length => _fileStream.Length;

        public long Position
        {
            get => _fileStream.Position;
            set => _fileStream.Position = value;
        }

        public int ReadTimeout
        {
            get => _fileStream.ReadTimeout;
            set => _fileStream.ReadTimeout = value;
        }

        public int WriteTimeout
        {
            get => _fileStream.WriteTimeout;
            set => _fileStream.WriteTimeout = value;
        }
        #endregion


        #region Fields
        readonly StorageFile _storageFile;
        readonly System.IO.FileStream _fileStream;
        #endregion


        #region Construction / Disposal
        public StorageFileStream(StorageFile storageFile, System.IO.FileStream fileStream)
        {
            _storageFile = storageFile;
            _fileStream = fileStream;

            if (_storageFile == null)
                throw new ArgumentException("Null argument", nameof(storageFile));
            if (_fileStream == null)
                throw new ArgumentException("Null argument", nameof(fileStream));
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _disposed = true;
                _fileStream.Dispose();
                _storageFile.Url.StopAccessingSecurityScopedResource();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async System.Threading.Tasks.Task DisposeAsync()
        {
            if (!_disposed)
            {
                _disposed = true;
                await _fileStream.DisposeAsync();
                _storageFile.Url.StopAccessingSecurityScopedResource();
            }
            GC.SuppressFinalize(this);
        }

        #endregion


        #region Methods
        public IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            => _fileStream.BeginRead(buffer, offset, count, callback, state);

        public IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
            => _fileStream.BeginWrite(buffer, offset, count, callback, state);

        public void Close()
        {
            _fileStream.Close();
            Dispose();
        }

        public void CopyTo(System.IO.Stream destination)
            => _fileStream.CopyTo(destination);

        public void CopyTo(System.IO.Stream destination, int bufferSize)
            => _fileStream.CopyTo(destination, bufferSize);

        public System.Threading.Tasks.Task CopyToAsync(System.IO.Stream destination, int bufferSize, System.Threading.CancellationToken cancellationToken)
            => _fileStream.CopyToAsync(destination, bufferSize, cancellationToken);

        public System.Threading.Tasks.Task CopyToAsync(System.IO.Stream destination)
            => _fileStream.CopyToAsync(destination);

        public System.Threading.Tasks.Task CopyToAsync(System.IO.Stream destination, int bufferSize)
            => _fileStream.CopyToAsync(destination, bufferSize);

        public System.Threading.Tasks.Task CopyToAsync(System.IO.Stream destination, System.Threading.CancellationToken cancellationToken)
            => _fileStream.CopyToAsync(destination, cancellationToken);


        public int EndRead(IAsyncResult asyncResult)
            => _fileStream.EndRead(asyncResult);

        public void EndWrite(IAsyncResult asyncResult)
            => _fileStream.EndWrite(asyncResult);

        public void Flush()
            => _fileStream.Flush();

        public System.Threading.Tasks.Task FlushAsync(System.Threading.CancellationToken cancellationToken)
            => _fileStream.FlushAsync(cancellationToken);

        public System.Threading.Tasks.Task FlushAsync()
            => _fileStream.FlushAsync();

        //int Read(Span<byte> buffer);

        public int Read(byte[] buffer, int offset, int count)
            => _fileStream.Read(buffer, offset, count);

        //System.Threading.Tasks.ValueTask<int> ReadAsync(Memory<byte> buffer, System.Threading.CancellationToken cancellationToken = default);

        public System.Threading.Tasks.Task<int> ReadAsync(byte[] buffer, int offset, int count)
            => _fileStream.ReadAsync(buffer, offset, count);

        public System.Threading.Tasks.Task<int> ReadAsync(byte[] buffer, int offset, int count, System.Threading.CancellationToken cancellationToken)
            => _fileStream.ReadAsync(buffer, offset, count, cancellationToken);

        public int ReadByte()
            => _fileStream.ReadByte();

        public long Seek(long offset, System.IO.SeekOrigin origin)
            => _fileStream.Seek(offset, origin);

        public void SetLength(long value)
            => _fileStream.SetLength(value);

        //System.IO.Stream Synchronized(System.IO.Stream stream);

        //void Write(ReadOnlySpan<byte> buffer);

        public void Write(byte[] buffer, int offset, int count)
            => _fileStream.Write(buffer, offset, count);

        //System.Threading.Tasks.ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, System.Threading.CancellationToken cancellationToken = default);

        public System.Threading.Tasks.Task WriteAsync(byte[] buffer, int offset, int count)
            => _fileStream.WriteAsync(buffer, offset, count);

        public System.Threading.Tasks.Task WriteAsync(byte[] buffer, int offset, int count, System.Threading.CancellationToken cancellationToken)
            => _fileStream.WriteAsync(buffer, offset, count, cancellationToken);


        public void WriteByte(byte value)
            => _fileStream.WriteByte(value);
        #endregion
    }
}
