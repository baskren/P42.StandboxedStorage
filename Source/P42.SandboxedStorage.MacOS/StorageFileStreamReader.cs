using System;
namespace P42.SandboxedStorage.Native
{
    class StorageFileStreamReader : System.IO.StreamReader
    {
        #region Fields
        StorageFile _storageFile;
        #endregion



        #region Construction / Disposal
        void Init(StorageFile storageFile)
        {
            if (storageFile is null)
                throw new ArgumentException("Null argument", nameof(storageFile));
            _storageFile = storageFile;
        }

        public StorageFileStreamReader(StorageFile storageFile, System.IO.Stream stream) : base(stream)
            => Init(storageFile);

        public StorageFileStreamReader(StorageFile storageFile, string path) : base(path)
            => Init(storageFile);

        public StorageFileStreamReader(StorageFile storageFile, System.IO.Stream stream, bool detectEncodingFromByteOrderMarks)
            : base(stream, detectEncodingFromByteOrderMarks)
            => Init(storageFile);

        public StorageFileStreamReader(StorageFile storageFile, System.IO.Stream stream, System.Text.Encoding encoding)
            : base(stream, encoding)
            => Init(storageFile);

        public StorageFileStreamReader(StorageFile storageFile, string path, bool detectEncodingFromByteOrderMarks) : base(path, detectEncodingFromByteOrderMarks)
            => Init(storageFile);

        public StorageFileStreamReader(StorageFile storageFile, string path, System.Text.Encoding encoding) : base(path, encoding)
            => Init(storageFile);

        public StorageFileStreamReader(StorageFile storageFile, System.IO.Stream stream, System.Text.Encoding encoding, bool detectEncodingFromByteOrderMarks) : base(stream, encoding, detectEncodingFromByteOrderMarks)
            => Init(storageFile);

        public StorageFileStreamReader(StorageFile storageFile, string path, System.Text.Encoding encoding, bool detectEncodingFromByteOrderMarks) : base(path, encoding, detectEncodingFromByteOrderMarks)
            => Init(storageFile);

        public StorageFileStreamReader(StorageFile storageFile, System.IO.Stream stream, System.Text.Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
            : base( stream, encoding, detectEncodingFromByteOrderMarks, bufferSize)
            => Init(storageFile);

        public StorageFileStreamReader(StorageFile storageFile, string path, System.Text.Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
            : base(path, encoding, detectEncodingFromByteOrderMarks, bufferSize)
            => Init(storageFile);

        public StorageFileStreamReader(StorageFile storageFile, System.IO.Stream stream, System.Text.Encoding encoding = default, bool detectEncodingFromByteOrderMarks = true, int bufferSize = -1, bool leaveOpen = false)
            : base(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize, leaveOpen)
            => Init(storageFile);


        bool _disposed;
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!_disposed && disposing)
            {
                _disposed = true;
                _storageFile.Url.StopAccessingSecurityScopedResource();
            }
        }

        #endregion
        /*
        #region Properties
        public System.IO.Stream BaseStream => _streamReader.BaseStream;

        public System.Text.Encoding CurrentEncoding => _streamReader.CurrentEncoding;

        public bool EndOfStream => _streamReader.EndOfStream;
        #endregion


        #region Fields
        readonly StorageFile _storageFile;
        readonly System.IO.StreamReader _streamReader;
        #endregion


        #region Construction / Disposal
        public StorageFileStreamReader(StorageFile storageFile, System.IO.StreamReader streamReader)
        {
            if (storageFile is null)
                throw new ArgumentException("Null argument", nameof(storageFile));
            if (streamReader is null)
                throw new ArgumentException("Null argument", nameof(streamReader));
            _storageFile = storageFile;
            _streamReader = streamReader;
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _disposed = true;
                _streamReader.Dispose();
                _storageFile.Url.StopAccessingSecurityScopedResource();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion


        #region Methods
        public void Close()
            => Dispose();

        public void DiscardBufferedData()
            => _streamReader.DiscardBufferedData();

        public int Peek()
            => _streamReader.Peek();

        public int Read()
            => _streamReader.Read();

        //int Read(Span<char> buffer);

        public int Read(char[] buffer, int index, int count)
            => _streamReader.Read(buffer, index, count);

        public System.Threading.Tasks.Task<int> ReadAsync(char[] buffer, int index, int count)
            => _streamReader.ReadAsync(buffer, index, count);

        //System.Threading.Tasks.ValueTask<int> ReadAsync(Memory<char> buffer, System.Threading.CancellationToken cancellationToken = default);

        //int ReadBlock(Span<char> buffer);

        public int ReadBlock(char[] buffer, int index, int count)
            => _streamReader.ReadBlock(buffer, index, count);

        //System.Threading.Tasks.ValueTask<int> ReadBlockAsync(Memory<char> buffer, System.Threading.CancellationToken cancellationToken = default);

        public System.Threading.Tasks.Task<int> ReadBlockAsync(char[] buffer, int index, int count)
            => _streamReader.ReadBlockAsync(buffer, index, count);

        public string ReadLine()
            => _streamReader.ReadLine();

        public System.Threading.Tasks.Task<string> ReadLineAsync()
            => _streamReader.ReadLineAsync();

        public string ReadToEnd()
            => _streamReader.ReadToEnd();

        public System.Threading.Tasks.Task<string> ReadToEndAsync()
            => _streamReader.ReadToEndAsync();

        #endregion
*/
    }
}
