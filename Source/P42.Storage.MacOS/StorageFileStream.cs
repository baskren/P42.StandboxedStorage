using System;
using System.Threading.Tasks;

namespace P42.Storage.Native
{
    class StorageFileStream : System.IO.FileStream
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

        public StorageFileStream(StorageFile storageFile, string path, System.IO.FileMode mode, System.IO.FileAccess access, System.IO.FileShare share, int bufferSize, bool useAsync) :
            base(path, mode, access, share, bufferSize, useAsync)
            => Init(storageFile);

        public StorageFileStream(StorageFile storageFile, string path, System.IO.FileMode mode, System.IO.FileAccess access, System.IO.FileShare share, int bufferSize)
            : base(path, mode, access, share, bufferSize)
            => Init(storageFile);

        public StorageFileStream(StorageFile storageFile, string path, System.IO.FileMode mode, System.IO.FileAccess access, System.IO.FileShare share)
            : base(path, mode, access, share)
            => Init(storageFile);

        public StorageFileStream(StorageFile storageFile, string path, System.IO.FileMode mode, System.IO.FileAccess access, System.IO.FileShare share, int bufferSize, System.IO.FileOptions options)
            : base(path, mode, access, share, bufferSize, options)
            => Init(storageFile);

        public StorageFileStream(StorageFile storageFile, string path, System.IO.FileMode mode, System.IO.FileAccess access)
            : base(path, mode, access)
            => Init(storageFile);

        public StorageFileStream(StorageFile storageFile, string path, System.IO.FileMode mode)
            : base(path, mode)
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

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            if (!_disposed)
            {
                _disposed = true;
                _storageFile.Url.StopAccessingSecurityScopedResource();
            }
        }
        #endregion

    }
}
