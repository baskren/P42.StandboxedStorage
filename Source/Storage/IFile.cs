//-----------------------------------------------------------------------
// <copyright file="IStorageFile.cs" company="In The Hand Ltd">
//     Copyright © 2016-17 In The Hand Ltd. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
// Refactored for cross platform .NetStandard library structure in 2020 by 42ndParallel.

using System;
using System.Threading.Tasks;

namespace P42.Storage
{
    /// <summary>
    /// Represents a file.
    /// Provides information about the file and its contents, and ways to manipulate them.
    /// </summary>
    public interface IStorageFile : IStorageItem
    {
        /// <summary>
        /// Gets a StorageFile object to represent the file at the specified path.
        /// </summary>
        /// <param name="path">The path of the file to get a StorageFile to represent.
        /// If your path uses slashes, make sure you use backslashes(\).
        /// Forward slashes(/) are not accepted by this method.</param>
        /// <returns>When this method completes, it returns the file as a StorageFile.</returns>
        public static Task<IStorageFile> GetFileFromPathAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException("path");

            return PlatformDelegate.GetFileFromPathAsync?.Invoke(path) ?? Task.FromResult<IStorageFile>(null);
        }

        /*
        public static Task<bool> Exists(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException("path");

            return Platform.FileExists?.Invoke()

        }
        */

        /// <summary>
        /// Replaces the specified file with a copy of the current file.
        /// </summary>
        /// <param name="fileToReplace">The file to replace.</param>
        /// <returns>No object or value is returned when this method completes.</returns>
        Task CopyAndReplaceAsync(IStorageFile fileToReplace);

        /// <summary>
        /// Creates a copy of the file in the specified folder.
        /// </summary>
        /// <param name="destinationFolder"></param>
        /// <returns></returns>
        Task<IStorageFile> CopyAsync(IStorageFolder destinationFolder);

        /// <summary>
        /// Creates a copy of the file in the specified folder, using the desired name.
        /// </summary>
        /// <param name="destinationFolder"></param>
        /// <param name="desiredNewName"></param>
        /// <returns></returns>
        Task<IStorageFile> CopyAsync(IStorageFolder destinationFolder, string desiredNewName);

        /// <summary>
        /// Moves the current file to the location of the specified file and replaces the specified file in that location.
        /// </summary>
        /// <param name="fileToReplace"></param>
        /// <returns></returns>
        Task MoveAndReplaceAsync(IStorageFile fileToReplace);

        /// <summary>
        /// Moves the current file to the specified folder.
        /// </summary>
        /// <param name="destinationFolder"></param>
        /// <returns></returns>
        Task MoveAsync(IStorageFolder destinationFolder);

        /// <summary>
        /// Moves the current file to the specified folder and renames the file according to the desired name.
        /// </summary>
        /// <param name="destinationFolder"></param>
        /// <param name="desiredNewName"></param>
        /// <returns></returns>
        Task MoveAsync(IStorageFolder destinationFolder, string desiredNewName);


        /// <summary>
        /// Renames the current file.
        /// </summary>
        /// <param name="desiredName">The desired, new name of the current item.</param>
        /// <returns>No object or value is returned by this method when it completes.</returns>
        Task RenameAsync(string desiredName);

        /// <summary>
        /// Renames the current file.
        /// This method also specifies what to do if an existing item in the current file's location has the same name.
        /// </summary>
        /// <param name="desiredName">The desired, new name of the current file.
        /// <para>If there is an existing item in the current file's location that already has the specified desiredName, the specified <see cref="NameCollisionOption"/>  determines how the system responds to the conflict.</para></param>
        /// <param name="option">The enum value that determines how the system responds if the desiredName is the same as the name of an existing item in the current file's location.</param>
        /// <returns>No object or value is returned by this method when it completes.</returns>
        Task RenameAsync(string desiredName, NameCollisionOption option);

        /// <summary>
        /// Gets the MIME type of the contents of the file.
        /// </summary>
        /// <value>The MIME type of the file contents.
        /// For example, a music file might have the "audio/mpeg" MIME type.</value>
        string ContentType { get; }

        /// <summary>
        /// Gets the type (file name extension) of the file.
        /// </summary>
        string FileType { get; }

        FileAttributes Attributes { get; }


    }
}
