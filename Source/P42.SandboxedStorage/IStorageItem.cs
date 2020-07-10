//-----------------------------------------------------------------------
// <copyright file="IStorageItem.cs" company="In The Hand Ltd">
//     Copyright © 2016-17 In The Hand Ltd. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
// Refactored for cross platform .NetStandard library structure in 2020 by 42ndParallel.

using System;
using System.Threading.Tasks;

namespace P42.SandboxedStorage
{
    /// <summary>
    /// Manipulates storage items (files and folders) and their contents, and provides information about them.
    /// </summary>
    public interface IStorageItem 
    {
        #region Properties
        /// <summary>
        /// Gets the name of the item including the file name extension if there is one.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the full file-system path of the item, if the item has a path.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets the size of the file in bytes.
        /// </summary>
        ulong Size { get; }

        /// <summary>
        /// Gets the date and time when the current item was created. 
        /// </summary>
        DateTimeOffset DateCreated { get; }

        /// <summary>
        /// Gets the timestamp of the last time the file was modified.
        /// </summary>
        DateTimeOffset DateModified { get; }

        /// <summary>
        /// Gets the attributes of a storage item.
        /// </summary>
        FileAttributes Attributes { get; }

        /// <summary>
        /// What to do if access is denied?
        /// </summary>
        AccessDenialResponse AccessDenialResponse { get; set; }
        #endregion


        #region Methods
        /// <summary>
        /// Does the storage item exist?
        /// </summary>
        /// <returns></returns>
        bool Exists();

        /// <summary>
        /// Gets the parent folder of the current storage item.
        /// </summary>
        /// <returns></returns>
        Task<IStorageFolder> GetParentAsync();

        /// <summary>
        /// Indicates whether the current item is the same as the specified item.
        /// </summary>
        /// <param name="item">The <see cref="IStorageItem"/> object that represents a storage item to compare against.</param>
        /// <returns>Returns true if the current storage item is the same as the specified storage item; otherwise false.</returns>
        bool IsEqual(IStorageItem item);

        /// <summary>
        /// Determines whether the current IStorageItem matches the specified StorageItemTypes value.
        /// </summary>
        /// <param name="type">The value to match against.</param>
        /// <returns></returns>
        /// <seealso cref="StorageItemTypes"/>
        bool IsOfType(StorageItemTypes type);

        /// <summary>
        /// Delete the item
        /// </summary>
        /// <param name="option">Optional: put into TRASH</param>
        /// <returns>true on success</returns>
        Task DeleteAsync(StorageDeleteOption option = StorageDeleteOption.Default);

        #endregion
    }
}
