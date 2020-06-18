//-----------------------------------------------------------------------
// <copyright file="IStorageFolder.cs" company="In The Hand Ltd">
//     Copyright © 2016 In The Hand Ltd. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
// Refactored for cross platform .NetStandard library structure in 2020 by 42ndParallel.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace P42.Storage
{
    /// <summary>
    /// Manipulates folders and their contents, and provides information about them.
    /// </summary>
    public interface IStorageFolder : IStorageItem
    {
        /* Waiting for UWP to support .NetStandard 2.1 + C# 8
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
        */

        #region Methods

        /// <summary>
        /// Creates a new file in the current folder.
        /// </summary>
        /// <param name="desiredName">The desired name of the file to create.</param>
        /// <returns></returns>
        Task<IStorageFile> CreateFileAsync(string desiredName);

        /// <summary>
        /// Creates a new file in the current folder, and specifies what to do if a file with the same name already exists in the current folder.
        /// </summary>
        /// <param name="desiredName">The desired name of the file to create.</param>
        /// <param name="options">The enum value that determines what to do if the desiredName is the same as the name of an existing file in the current folder.</param>
        /// <returns></returns>
        Task<IStorageFile> CreateFileAsync(string desiredName, CreationCollisionOption options);

        /// <summary>
        /// Creates a new folder in the current folder.
        /// </summary>
        /// <param name="desiredName">The desired name of the folder to create.</param>
        /// <returns></returns>
        Task<IStorageFolder> CreateFolderAsync(string desiredName);

        /// <summary>
        /// Creates a new folder in the current folder, and specifies what to do if a folder with the same name already exists in the current folder.
        /// </summary>
        /// <param name="desiredName">The desired name of the folder to create.</param>
        /// <param name="options">The enum value that determines what to do if the desiredName is the same as the name of an existing folder in the current folder.</param>
        /// <returns></returns>
        Task<IStorageFolder> CreateFolderAsync(string desiredName, CreationCollisionOption options);

        /// <summary>
        /// Gets the specified file from the current folder.
        /// </summary>
        /// <param name="filename">The name (or path relative to the current folder) of the file to retrieve.</param>
        /// <returns></returns>
        Task<IStorageFile> GetFileAsync(string filename);

        /// <summary>
        /// Gets the files from the current folder.
        /// </summary>
        /// <returns></returns>
        Task<IReadOnlyList<IStorageFile>> GetFilesAsync();

        /// <summary>
        /// Gets the specified folder from the current folder.
        /// </summary>
        /// <param name="name">The name of the child folder to retrieve.</param>
        /// <returns></returns>
        Task<IStorageFolder> GetFolderAsync(string name);

        /// <summary>
        /// Gets the folders in the current folder.
        /// </summary>
        /// <returns></returns>
        Task<IReadOnlyList<IStorageFolder>> GetFoldersAsync();

        /// <summary>
        /// Gets the specified item from the <see cref="IStorageFolder"/>.
        /// </summary>
        /// <param name="name">The name of the item to retrieve.</param>
        /// <returns></returns>
        Task<IStorageItem> GetItemAsync(string name);

        /// <summary>
        /// Gets the items from the current folder.
        /// </summary>
        /// <returns></returns>
        Task<IReadOnlyList<IStorageItem>> GetItemsAsync();

        /// <summary>
        /// Try to get a single file or sub-folder from the current folder by using the name of the item.
        /// </summary>
        /// <param name="name">The name (or path relative to the current folder) of the file or sub-folder to try to retrieve.</param>
        /// <returns>When this method completes successfully, it returns the file or folder (type <see cref="IStorageItem"/>).</returns>
        Task<IStorageItem> TryGetItemAsync(string name);
        #endregion

    }
}