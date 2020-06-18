using System;
using System.Threading.Tasks;

namespace P42.Storage
{
    public static class StorageFolder
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
    }
}
