using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using P42.Storage;
using Windows.Storage;

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

        public async Task MoveAndReplaceAsync(IStorageFile iFileToReplace)
        {
            if (_file != null &&
                iFileToReplace is StorageFile fileToReplace &&
                fileToReplace._file is Windows.Storage.StorageFile windowsFileToReplace)
                await _file.MoveAndReplaceAsync(windowsFileToReplace);
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






    }
}
