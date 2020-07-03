using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace P42.Storage.Native
{
    class StorageFolder : StorageItem, IStorageFolder
    {
        #region Properties
        internal Windows.Storage.StorageFolder _folder
        {
            get => _item as Windows.Storage.StorageFolder;
            set => _item = value;
        }
        #endregion

        #region Construction
        public StorageFolder(Windows.Storage.StorageFolder storageFolder) : base(storageFolder) { }
        #endregion


        #region Conversion Operators
        public static implicit operator StorageFolder(Windows.Storage.StorageFolder f)
            => f is null
                ? null
                : new StorageFolder(f);

        public static implicit operator Windows.Storage.StorageFolder(StorageFolder f)
            => f._folder;

        #endregion

        #region Methods

        public static async Task<IStorageFolder> GetFolderFromPathAsync(string path)
        {
            await Task.Delay(5).ConfigureAwait(false);

            try
            {
                if (await Windows.Storage.StorageFolder.GetFolderFromPathAsync(path) is Windows.Storage.StorageFolder windowsFolder)
                    return new StorageFolder(windowsFolder);
            }
            catch (Exception e)
            {
                // failed (could be permissions problem?)
            }
            return null;
        }

        public override bool IsOfType(StorageItemTypes type)
            => type == StorageItemTypes.Folder;



        public async Task<IStorageFile> CreateFileAsync(string desiredName)
        {
            await Task.Delay(5).ConfigureAwait(false);

            if (_folder != null &&
                !string.IsNullOrWhiteSpace(desiredName) &&
                await _folder.CreateFileAsync(desiredName) is Windows.Storage.StorageFile file)
                return new StorageFile(file);
            return null;
        }

        public async Task<IStorageFile> CreateFileAsync(string desiredName, CreationCollisionOption options)
        {
            await Task.Delay(5).ConfigureAwait(false);

            if (_folder != null &&
                !string.IsNullOrWhiteSpace(desiredName) &&
                await _folder.CreateFileAsync(desiredName, (Windows.Storage.CreationCollisionOption)options) is Windows.Storage.StorageFile file)
                return new StorageFile(file);
            return null;
        }

        public async Task<IStorageFolder> CreateFolderAsync(string desiredName)
        {
            await Task.Delay(5).ConfigureAwait(false);

            if (_folder != null &&
                !string.IsNullOrWhiteSpace(desiredName) &&
                await _folder.CreateFolderAsync(desiredName) is Windows.Storage.StorageFolder folder)
                return new StorageFolder(folder);
            return null;
        }

        public async Task<IStorageFolder> CreateFolderAsync(string desiredName, CreationCollisionOption options)
        {
            await Task.Delay(5).ConfigureAwait(false);

            if (_folder != null &&
                !string.IsNullOrWhiteSpace(desiredName) &&
                await _folder.CreateFolderAsync(desiredName, (Windows.Storage.CreationCollisionOption)options) is Windows.Storage.StorageFolder folder)
                return new StorageFolder(folder);
            return null;
        }

        public async Task<IStorageFile> GetFileAsync(string filename)
        {
            await Task.Delay(5).ConfigureAwait(false);

            if (_folder != null &&
                !string.IsNullOrWhiteSpace(filename) &&
                await _folder.GetFileAsync(filename) is Windows.Storage.StorageFile file)
                return new StorageFile(file);
            return null;
        }

        public async Task<IReadOnlyList<IStorageFile>> GetFilesAsync(string pattern = null)
        {
            await Task.Delay(5).ConfigureAwait(false);

            if (_folder != null &&
                await _folder.GetFilesAsync() is IReadOnlyList<Windows.Storage.StorageFile> windowsFiles)
            {
                List<IStorageFile> files = new List<IStorageFile>();

                string regex = null;
                if (!string.IsNullOrWhiteSpace(pattern))
                {
                    regex = P42.Storage.StringExtensions.WildcardToRegex(pattern);
                }

                foreach (var windowsFile in windowsFiles)
                {
                    if (!string.IsNullOrWhiteSpace(regex))
                    { 
                        if (Regex.IsMatch(windowsFile.Name, regex))
                            files.Add(new StorageFile(windowsFile));
                    }
                    else
                        files.Add(new StorageFile(windowsFile));
                }
                return files.AsReadOnly();
            }
            return null;
        }

        public async Task<IStorageFolder> GetFolderAsync(string name)
        {
            await Task.Delay(5).ConfigureAwait(false);

            if (_folder != null &&
                !string.IsNullOrWhiteSpace(name) &&
                await _folder.GetFolderAsync(name) is Windows.Storage.StorageFolder windowsFoler)
                return new StorageFolder(windowsFoler);
            return null;
        }

        public async Task<IReadOnlyList<IStorageFolder>> GetFoldersAsync()
        {
            await Task.Delay(5).ConfigureAwait(false);

            if (_folder != null &&
                await _folder.GetFoldersAsync() is IReadOnlyList<Windows.Storage.StorageFolder> windowsFolders)
            {
                List<IStorageFolder> folders = new List<IStorageFolder>();
                foreach (var windowsFolder in windowsFolders)
                    folders.Add(new StorageFolder(windowsFolder));
                return folders.AsReadOnly();
            }
            return null;
        }

        public async Task<IStorageItem> GetItemAsync(string name)
        {
            await Task.Delay(5).ConfigureAwait(false);

            if (_folder != null &&
                !string.IsNullOrWhiteSpace(name) &&
                await _folder.GetItemAsync(name) is Windows.Storage.IStorageItem windowsStorageItem)
            {
                if (windowsStorageItem is Windows.Storage.StorageFile windowsFile)
                    return new StorageFile(windowsFile);
                else if (windowsStorageItem is Windows.Storage.StorageFolder windowsFolder)
                    return new StorageFolder(windowsFolder);
            }
            return null;
        }

        public async Task<IReadOnlyList<IStorageItem>> GetItemsAsync()
        {
            await Task.Delay(5).ConfigureAwait(false);

            if (_folder != null &&
                await _folder.GetItemsAsync() is IReadOnlyList<Windows.Storage.IStorageItem> windowsItems)
            {
                List<IStorageItem> items = new List<IStorageItem>();
                foreach (var windowsItem in windowsItems)
                {
                    if (windowsItem is Windows.Storage.StorageFile windowsFile)
                        items.Add(new StorageFile(windowsFile));
                    else if (windowsItem is Windows.Storage.StorageFolder windowsFolder)
                        items.Add(new StorageFolder(windowsFolder));
                }
                return items.AsReadOnly();
            }
            return null;
        }

        public async Task<IStorageItem> TryGetItemAsync(string name)
            => await GetItemAsync(name);
        #endregion
    }
}
