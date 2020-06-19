using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using P42.Storage;
using Windows.Storage;
using Windows.UI.Xaml;

namespace P42.Storage.Native
{
    class StorageFile : StorageItem, IStorageFile
    {
        #region Properties
        public string ContentType => _file?.ContentType;

        public string FileType => _file?.FileType;

        internal Windows.Storage.StorageFile _file
        {
            get => _item as Windows.Storage.StorageFile;
            set => _item = value;
        }
        #endregion


        #region Construction
        public StorageFile(string path)
        {
            var task = Task.Run(async () => _file = await Windows.Storage.StorageFile.GetFileFromPathAsync(path));
            task.RunSynchronously();
        }

        public StorageFile(Windows.Storage.StorageFile file) : base(file) { }
        #endregion


        #region Type Conversion Operators
        public static implicit operator StorageFile(Windows.Storage.StorageFile f)
            => f is null
                ? null
                : new StorageFile(f);
        

        public static implicit operator Windows.Storage.StorageFile(StorageFile f)
            => f._file;
        #endregion


        #region Methods
        public override bool IsOfType(StorageItemTypes type)
            => type == StorageItemTypes.File;

        internal async static Task<IStorageFile> GetFileFromPathAsync(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }
            if (await Windows.Storage.StorageFile.GetFileFromPathAsync(path) is Windows.Storage.StorageFile nativeStorageFile)
                return new StorageFile(nativeStorageFile);
            return null;
        }

        public async Task CopyAndReplaceAsync(IStorageFile iStorageFileToReplace)
        {
            if (_file != null && 
                iStorageFileToReplace is StorageFile storageFileToReplace && 
                storageFileToReplace._file is Windows.Storage.StorageFile fileToReplace)
            {
                await _file.CopyAndReplaceAsync(fileToReplace);
            }
        }

        public async Task<IStorageFile> CopyAsync(IStorageFolder iDestinationFolder)
        {
            if (_file != null &&
                iDestinationFolder is StorageFolder destinationFolder &&
                destinationFolder?._folder is Windows.Storage.StorageFolder windowsDestinationFolder &&
                await _file.CopyAsync(windowsDestinationFolder) is Windows.Storage.StorageFile newFile)
                return new StorageFile(newFile);
            return null;
        }

        public async Task<IStorageFile> CopyAsync(IStorageFolder iDestinationFolder, string desiredNewName)
        {
            if (_file != null &&
                iDestinationFolder is StorageFolder destinationFolder &&
                destinationFolder?._folder is Windows.Storage.StorageFolder windowsDestinationFolder &&
                !string.IsNullOrWhiteSpace(desiredNewName) &&
                await _file.CopyAsync(windowsDestinationFolder, desiredNewName) is Windows.Storage.StorageFile newFile)
                return new StorageFile(newFile);
            return null;
        }

        public override async Task<IStorageFolder> GetParentAsync()
        {
            if (_file is null)
                return null;
            if (_file != null && await _file.GetParentAsync() is Windows.Storage.StorageFolder folder)
            {
                return new StorageFolder(folder);
            }
            return null;
        }

        public async Task MoveAndReplaceAsync(IStorageFile iFileToReplace)
        {
            if (_file != null &&
                iFileToReplace is StorageFile fileToReplace &&
                fileToReplace._file is Windows.Storage.StorageFile windowsFileToReplace)
                await _file.MoveAndReplaceAsync(windowsFileToReplace);
        }

        public async Task MoveAsync(IStorageFolder iDestinationFolder)
        {
            if (_file != null &&
                iDestinationFolder is StorageFolder destinationFolder &&
                destinationFolder._folder is Windows.Storage.StorageFolder windowsDestintationFolder)
                await _file.MoveAsync(windowsDestintationFolder);
        }

        public async Task MoveAsync(IStorageFolder iDestinationFolder, string desiredNewName)
        {
            if (_file != null &&
                !string.IsNullOrWhiteSpace(desiredNewName) &&
                iDestinationFolder is StorageFolder destinationFolder &&
                destinationFolder._folder is Windows.Storage.StorageFolder windowsDestintationFolder)
                await _file.MoveAsync(windowsDestintationFolder, desiredNewName);

        }

        public async Task RenameAsync(string desiredName)
        {
            if (_file != null && !string.IsNullOrWhiteSpace(desiredName))
                await _file.RenameAsync(desiredName);
        }

        public async Task RenameAsync(string desiredName, NameCollisionOption option)
        {
            if (_file != null && !string.IsNullOrWhiteSpace(desiredName))
                await _file.RenameAsync(desiredName, (Windows.Storage.NameCollisionOption)option);
        }
        #endregion


        #region System.IO.File methods
        public void AppendAllLines(IEnumerable<string> lines)
            =>AppendAllLinesAsync(lines).RunSynchronously();

        public Task AppendAllLinesAsync(IEnumerable<string> lines, System.Threading.CancellationToken cancellationToken = default)
            => Windows.Storage.PathIO.AppendLinesAsync(_file.Path, lines, Windows.Storage.Streams.UnicodeEncoding.Utf8).AsTask(cancellationToken);

        public void AppendAllText(string contents)
            => AppendAllTextAsync(contents).RunSynchronously();

        public Task AppendAllTextAsync(string contents, System.Threading.CancellationToken cancellationToken = default)
            => Windows.Storage.PathIO.AppendTextAsync(_file.Path, contents, Windows.Storage.Streams.UnicodeEncoding.Utf8).AsTask(cancellationToken);

        public FileStream Open(FileMode mode, FileAccess access = FileAccess.ReadWrite, FileShare share = FileShare.None)
            => new FileStream(_file.Path, mode, access, share);

        public FileStream OpenRead()
            => new FileStream(_file.Path, FileMode.Open, FileAccess.Read, FileShare.Read);

        public StreamReader OpenText()
        {
            if (OpenRead() is System.IO.FileStream fileStream)
            {
                return new StreamReader(fileStream);
            }
            return null;
        }

        public FileStream OpenWrite()
            => new FileStream(_file.Path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);

        public byte[] ReadAllBytes()
        {
            var task = ReadAllBytesAsync();
            task.RunSynchronously();
            return task.Result;
        }

        public async Task<byte[]> ReadAllBytesAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            var buffer = await Windows.Storage.PathIO.ReadBufferAsync(_file.Path);
            return buffer?.ToArray();
        }

        public string[] ReadAllLines()
        {
            var task = ReadAllLinesAsync();
            task.RunSynchronously();
            return task.Result;
        }

        public async Task<string[]> ReadAllLinesAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            var result = await Windows.Storage.PathIO.ReadLinesAsync(_file.Path, Windows.Storage.Streams.UnicodeEncoding.Utf8).AsTask(cancellationToken);
            return result.ToArray();
        }

        public string ReadAllText()
        {
            var task = ReadAllTextAsync();
            task.RunSynchronously();
            return task.Result;
        }

        public Task<string> ReadAllTextAsync(System.Threading.CancellationToken cancellationToken = default)
            => Windows.Storage.PathIO.ReadTextAsync(_file.Path, Windows.Storage.Streams.UnicodeEncoding.Utf8).AsTask(cancellationToken);

        public void WriteAllBytes(byte[] bytes)
            => WriteAllBytesAsync(bytes).RunSynchronously();

        public Task WriteAllBytesAsync(byte[] bytes, System.Threading.CancellationToken cancellationToken = default)
            => Windows.Storage.PathIO.WriteBytesAsync(_file.Path, bytes).AsTask(cancellationToken);

        public void WriteAllLines(IEnumerable<string> content)
            => WriteAllLinesAsync(content).RunSynchronously();

        public Task WriteAllLinesAsync(IEnumerable<string> lines, System.Threading.CancellationToken cancellationToken = default)
            => Windows.Storage.PathIO.WriteLinesAsync(_file.Path, lines, Windows.Storage.Streams.UnicodeEncoding.Utf8).AsTask(cancellationToken);

        public void WriteAllText(string content)
            => WriteAllTextAsync(content).RunSynchronously();

        public Task WriteAllTextAsync(string content, System.Threading.CancellationToken cancellationToken = default)
            => Windows.Storage.PathIO.WriteTextAsync(_file.Path, content, Windows.Storage.Streams.UnicodeEncoding.Utf8).AsTask(cancellationToken);

        #endregion



    }
}
