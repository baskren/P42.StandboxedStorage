using System;
using System.Threading.Tasks;

namespace P42.Storage
{
    public static class StorageFolderExtensions
    {
        /// <summary>
        /// Gets an IStorageFile object to represent the file at the specified path.
        /// </summary>
        /// <param name="path">The path of the file to get a StorageFile to represent.
        /// If your path uses slashes, make sure you use backslashes(\).
        /// Forward slashes(/) are not accepted by this method.</param>
        /// <returns>When this method completes, it returns the file as a StorageFile.</returns>
        public static Task<IStorageFolder> GetFolderFromPathAsync(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            return PlatformDelegate.GetFolderFromPathAsync?.Invoke(path) ?? Task.FromResult<IStorageFolder>(null);
        }

        public static async Task<bool> FileExists(this IStorageFolder storageFolder, string fileName)
        {
            if (storageFolder is null || string.IsNullOrWhiteSpace(fileName))
                return false;
            return await storageFolder.GetFileAsync(fileName) != null;
        }

        public static async Task<bool> FolderExists(this IStorageFolder storageFolder, string folderName)
        {
            if (storageFolder is null || string.IsNullOrWhiteSpace(folderName))
                return false;
            return await storageFolder.GetFolderAsync(folderName) != null;
        }

        public static async Task<bool> ItemExists(this IStorageFolder storageFolder, string itemName)
        {
            if (storageFolder is null || string.IsNullOrWhiteSpace(itemName))
                return false;
            return await storageFolder.GetItemAsync(itemName) != null;
        }

        public static async Task<IStorageFile> GetOrCreateFileAsync(this IStorageFolder storageFolder, string fileName)
        {
            if (storageFolder is null)
                return null;
            if (await storageFolder.GetFileAsync(fileName) is IStorageFile existingStorageFile)
                return existingStorageFile;
            if (await storageFolder.CreateFileAsync(fileName) is IStorageFile newStorageFile)
                return newStorageFile;
            return null;
        }

        public static async Task<IStorageFolder> GetOrCreateFolderAsync(this IStorageFolder storageFolder, string folderName)
        {
            if (storageFolder is null)
                return null;
            if (await storageFolder.GetFolderAsync(folderName) is IStorageFolder existingStorageFolder)
                return existingStorageFolder;
            if (await storageFolder.CreateFolderAsync(folderName) is IStorageFolder newStorageFolder)
                return newStorageFolder;
            return null;
        }
    }
}
