//-----------------------------------------------------------------------
// <copyright file="FileOpenPicker.cs" company="In The Hand Ltd">
//     Copyright © 2016-17 In The Hand Ltd. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
// Refactored for cross platform .NetStandard library structure in 2020 by 42ndParallel.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace P42.SandboxedStorage
{
    /// <summary>
    /// Represents a UI element that lets the user choose and open files. 
    /// </summary>
    /// <remarks>
    /// <para/><list type="table">
    /// <listheader><term>Platform</term><description>Version supported</description></listheader>
    /// <item><term>Windows UWP</term><description>Windows 10</description></item>
    /// </remarks>
    public static class Pickers
    {

        /// <summary>
        /// Shows the file picker so that the user can pick one file.
        /// </summary>
        /// <returns>When the call to this method completes successfully, it returns a <see cref="StorageFileExtensions"/> object that represents the file that the user picked.</returns>
        public static Task<IStorageFile> PickSingleFileAsync(IList<string> fileTypes = null)
        {
            if (PlatformDelegate.PickFileAsync != null)
                return PlatformDelegate.PickFileAsync.Invoke(fileTypes);
            throw new PlatformNotSupportedException();
        }


        /// <summary>
        /// Shows the file picker so that the user can save a file and set the file name, extension, and location of the file to be saved.
        /// </summary>
        /// <returns>When the call to this method completes successfully, it returns a storageFile object that was created to represent the saved file.
        /// The file name, extension, and location of this storageFile match those specified by the user, but the file has no content.</returns>
        public static Task<IStorageFile> PickSaveFileAsync(string defaultFileExtension = null, IDictionary<string, IList<string>> fileTypeChoices = null)
        {
            if (PlatformDelegate.PickSaveAsFileAsync != null)
                return PlatformDelegate.PickSaveAsFileAsync?.Invoke(defaultFileExtension, fileTypeChoices);
            throw new PlatformNotSupportedException();
        }


        /// <summary>
        /// Shows the folderPicker object so that the user can pick a folder.
        /// </summary>
        /// <returns>When the call to this method completes successfully, it returns a <see cref="StorageFolderExtensions"/> object that represents the folder that the user picked.</returns>
        public static Task<IStorageFolder> PickSingleFolderAsync()
        {
            if (PlatformDelegate.PickFolderAsync != null)
                return PlatformDelegate.PickFolderAsync?.Invoke();
            throw new PlatformNotSupportedException();
        }



    }
}