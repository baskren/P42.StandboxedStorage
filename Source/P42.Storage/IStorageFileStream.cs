using System;
namespace P42.Storage
{
    public interface IStorageFileStream : IDisposable
    {
        #region Properties
        bool CanRead { get; }

        bool CanSeek { get; }

        bool CanTimeout { get; }

        bool CanWrite { get; }

        long Length { get; }

        long Position { get; set; }

        int ReadTimeout { get; set; }

        int WriteTimeout { get; set; }
        #endregion


        #region Methods
        IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state);

        IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state);

        void Close();

        void CopyTo(System.IO.Stream destination);

        void CopyTo(System.IO.Stream destination, int bufferSize);

        System.Threading.Tasks.Task CopyToAsync(System.IO.Stream destination, int bufferSize, System.Threading.CancellationToken cancellationToken);

        System.Threading.Tasks.Task CopyToAsync(System.IO.Stream destination);

        System.Threading.Tasks.Task CopyToAsync(System.IO.Stream destination, int bufferSize);

        System.Threading.Tasks.Task CopyToAsync(System.IO.Stream destination, System.Threading.CancellationToken cancellationToken);

        System.Threading.Tasks.Task DisposeAsync();

        int EndRead(IAsyncResult asyncResult);

        void EndWrite(IAsyncResult asyncResult);

        void Flush();

        System.Threading.Tasks.Task FlushAsync(System.Threading.CancellationToken cancellationToken);

        System.Threading.Tasks.Task FlushAsync();

        //int Read(Span<byte> buffer);

        int Read(byte[] buffer, int offset, int count);

        //System.Threading.Tasks.ValueTask<int> ReadAsync(Memory<byte> buffer, System.Threading.CancellationToken cancellationToken = default);

        System.Threading.Tasks.Task<int> ReadAsync(byte[] buffer, int offset, int count);

        System.Threading.Tasks.Task<int> ReadAsync(byte[] buffer, int offset, int count, System.Threading.CancellationToken cancellationToken);

        int ReadByte();

        long Seek(long offset, System.IO.SeekOrigin origin);

        void SetLength(long value);

        //System.IO.Stream Synchronized(System.IO.Stream stream);

        //void Write(ReadOnlySpan<byte> buffer);

        void Write(byte[] buffer, int offset, int count);

        //System.Threading.Tasks.ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, System.Threading.CancellationToken cancellationToken = default);

        System.Threading.Tasks.Task WriteAsync(byte[] buffer, int offset, int count);

        System.Threading.Tasks.Task WriteAsync(byte[] buffer, int offset, int count, System.Threading.CancellationToken cancellationToken);

        void WriteByte(byte value);
        #endregion
    }
}
