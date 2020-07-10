//-----------------------------------------------------------------------
// <copyright file="StorageFolder.cs" company="In The Hand Ltd">
//     Copyright © 2016-17 In The Hand Ltd. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
// Refactored for cross platform .NetStandard library structure in 2020 by 42ndParallel.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Foundation;

namespace P42.SandboxedStorage.Native
{
    /// <summary>
    /// Manages folders and their contents and provides information about them.
    /// </summary>
    /// <remarks>
    /// <para/><list type="table">
    /// <listheader><term>Platform</term><description>Version supported</description></listheader>
    /// <item><term>Android</term><description>Android 4.4 and later</description></item>
    /// <item><term>iOS</term><description>iOS 9.0 and later</description></item>
    /// <item><term>macOS</term><description>OS X 10.7 and later</description></item>
    /// <item><term>tvOS</term><description>tvOS 9.0 and later</description></item>
    /// <item><term>watchOS</term><description>watchOS 2.0 and later</description></item>
    /// <item><term>Tizen</term><description>Tizen 3.0</description></item>
    /// <item><term>Windows UWP</term><description>Windows 10</description></item>
    /// <item><term>Windows Store</term><description>Windows 8.1 or later</description></item>
    /// <item><term>Windows Phone Store</term><description>Windows Phone 8.1 or later</description></item>
    /// <item><term>Windows Phone Silverlight</term><description>Windows Phone 8.0 or later</description></item>
    /// <item><term>Windows (Desktop Apps)</term><description>Windows 7 or later</description></item></list>
    /// </remarks>
    class StorageFolder : StorageItem, IStorageFolder, IEquatable<StorageFolder>
    {
        /// <summary>
        /// Gets a StorageFile object to represent the file at the specified path.
        /// </summary>
        /// <param name="path">The path of the file to get a StorageFile to represent.
        /// If your path uses slashes, make sure you use backslashes(\).
        /// Forward slashes(/) are not accepted by this method.</param>
        /// <returns>When this method completes, it returns the file as a StorageFile.</returns>
        public static Task<IStorageFolder> GetFolderFromPathAsync(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            bool isDirectory = false;
            if (NSFileManager.DefaultManager.FileExists(path, ref isDirectory))
            {
                if (isDirectory)
                    return Task.FromResult<IStorageFolder>(new StorageFolder(path));
            }
            return null;
        }


        #region Construction
        internal StorageFolder(string path, bool makeBookmark = false) : base(path, makeBookmark) { }

        public StorageFolder(NSUrl url, bool makeBookmark = false) : base(url, makeBookmark) { }
        #endregion


        #region Private Methods
        internal override async Task<bool> RequestAccess(string message)
        {
            if (await base.RequestAccess(message))
            {
                if (await FolderPicker.PickSingleFolderAsync(this) is StorageFolder storageFolder)
                {
                    var bm = storageFolder.Url.GetOrCreateBookmark();
                    Url = bm.NewUrl ?? Url;
                    Bookmark = bm.Bookmark ?? Bookmark;
                    return Bookmark != null;
                }
            }
            return false;
        }
        #endregion


        #region IStorageFolder

        #region Exists
        public Task<bool> ItemExists(string itemName)
        {
            var path = System.IO.Path.Combine(Path, itemName);
            var result = NSFileManager.DefaultManager.FileExists(path);
            return Task.FromResult(result);
        }

        public Task<bool> FileExists(string fileName)
        {
            var path = System.IO.Path.Combine(Path, fileName);
            bool isDirectory = false;
            var result = NSFileManager.DefaultManager.FileExists(path, ref isDirectory);
            if (isDirectory)
                return Task.FromResult(false);
            return Task.FromResult(result);
        }

        public Task<bool> FolderExists(string folderName)
        {
            var path = System.IO.Path.Combine(Path, folderName);
            bool isDirectory = false;
            var result = NSFileManager.DefaultManager.FileExists(path, ref isDirectory);
            if (!isDirectory)
                return Task.FromResult(false);
            return Task.FromResult(result);
        }
        #endregion


        #region Create
        /// <summary>
        /// Creates a new file with the specified name in the current folder.
        /// </summary>
        /// <param name="desiredName">The name of the new file to create in the current folder.</param>
        /// <returns>When this method completes, it returns a StorageFile that represents the new file.</returns>
        public Task<IStorageFile> CreateFileAsync(string desiredName)
            => CreateFileAsync(desiredName, CreationCollisionOption.FailIfExists);

        /// <summary>
        /// Creates a new file with the specified name in the current folder.
        /// </summary>
        /// <param name="desiredName">The name of the new file to create in the current folder.</param>
        /// <param name="options">One of the enumeration values that determines how to handle the collision if a file with the specified desiredName already exists in the current folder.</param>
        /// <returns>When this method completes, it returns a StorageFile that represents the new file.</returns>
        public async Task<IStorageFile> CreateFileAsync(string desiredName, CreationCollisionOption options)
        {
            await Task.Delay(5).ConfigureAwait(false);

            return await Task.Run(async () =>
            {
                if (await GetFileAsync(desiredName) is IStorageFile existingFile)
                {
                    switch (options)
                    {
                        case CreationCollisionOption.OpenIfExists:
                            return existingFile;

                        case CreationCollisionOption.ReplaceExisting:
                            await existingFile.DeleteAsync();
                            break;

                        case CreationCollisionOption.GenerateUniqueName:
                            for (int i = 1; i < 100; i++)
                            {
                                string newName = string.Format(desiredName.Substring(0, desiredName.LastIndexOf('.')) + " ({0})" + desiredName.Substring(desiredName.LastIndexOf('.')), i);
                                if (!(await GetFileAsync(newName) is IStorageFile))
                                {
                                    desiredName = newName;
                                    break;
                                }
                            }
                            break;

                        default:
                            throw new IOException();
                    }
                }

                if (!await StartAccess())
                    return null;


                string filepath = System.IO.Path.Combine(Path, desiredName);
                File.Create(filepath).Close();
                //var result = NSFileManager.DefaultManager.CreateFile(filepath, null, new NSDictionary {  });
                StopAccess();
                /*
                if (!result)
                {
                    if (AccessDenialResponse != AccessDenialResponse.Silent)
                        throw new AccessViolationException("Unable to create file [" + filepath + "]");
                    return null;
                }
                */
                return new StorageFile(filepath);
            });
        }

        /// <summary>
        /// Creates a new subfolder with the specified name in the current folder.
        /// </summary>
        /// <param name="desiredName">The name of the new subfolder to create in the current folder.</param>
        /// <returns>When this method completes, it returns a StorageFolder that represents the new subfolder.</returns>
        public Task<IStorageFolder> CreateFolderAsync(string desiredName)
            => CreateFolderAsync(desiredName, CreationCollisionOption.FailIfExists);

        /// <summary>
        /// Creates a new subfolder with the specified name in the current folder.
        /// This method also specifies what to do if a subfolder with the same name already exists in the current folder. 
        /// </summary>
        /// <param name="desiredName">The name of the new subfolder to create in the current folder.</param>
        /// <param name="options">One of the enumeration values that determines how to handle the collision if a subfolder with the specified desiredName already exists in the current folder.</param>
        /// <returns>When this method completes, it returns a StorageFolder that represents the new subfolder.</returns>
        public async Task<IStorageFolder> CreateFolderAsync(string desiredName, CreationCollisionOption options)
        {
            await Task.Delay(5).ConfigureAwait(false);

            if (await GetFolderAsync(desiredName) is IStorageFolder existingFolder)
            {
                switch (options)
                {
                    case CreationCollisionOption.OpenIfExists:
                        return existingFolder;

                    case CreationCollisionOption.ReplaceExisting:
                        await existingFolder.DeleteAsync();
                        break;

                    case CreationCollisionOption.GenerateUniqueName:
                        for (int i = 1; i < 100; i++)
                        {
                            string uniqueName = string.Format(desiredName.Substring(0, desiredName.LastIndexOf('.')) + " ({0})" + desiredName.Substring(desiredName.LastIndexOf('.')), i);
                            if (!(await GetItemAsync(uniqueName) is SandboxedStorage.IStorageItem))
                            {
                                desiredName = uniqueName;
                                break;
                            }
                        }
                        break;

                    default:
                        throw new IOException();
                }
            }

            if (!await StartAccess())
                return null;

            var newPath = System.IO.Path.Combine(Path, desiredName);
            Directory.CreateDirectory(newPath);
            // the above does work but the below does not?!?!
            //NSFileManager.DefaultManager.CreateDirectory(desiredName, false, new NSDictionary { }, out NSError error);
            StopAccess();
            return new StorageFolder(newPath);
        }
        #endregion


        #region Get
        /// <summary>
        /// Gets the file with the specified name from the current folder. 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public async Task<IStorageFile> GetFileAsync(string filename)
        {
            await Task.Delay(5).ConfigureAwait(false);

            return (await GetFilesAsync(filename)).FirstOrDefault();
        }

        /// <summary>
        /// Gets the files in the current folder.
        /// </summary>
        /// <returns></returns>
        public async Task<IReadOnlyList<IStorageFile>> GetFilesAsync(string pattern = null)
        {
            if (await GetItemsAsync(pattern, false, true) is IReadOnlyList<IStorageItem> items)
            {
                var result = new List<IStorageFile>();
                foreach (var item in items)
                    if (item is IStorageFile folder)
                        result.Add(folder);
                return result.AsReadOnly();
            }
            return null;
        }

        /// <summary>
        /// Gets the specified folder from the current folder. 
        /// </summary>
        /// <param name="name">The name of the child folder to retrieve.</param>
        /// <returns>When this method completes successfully, it returns a StorageFolder that represents the child folder.</returns>
        public async Task<IStorageFolder> GetFolderAsync(string name)
        {
            await Task.Delay(5).ConfigureAwait(false);

            return (await GetFoldersAsync(name)).FirstOrDefault();
        }

        /// <summary>
        /// Gets the folders in the current folder.
        /// </summary>
        /// <returns></returns>
        public async Task<IReadOnlyList<IStorageFolder>> GetFoldersAsync(string pattern = null)
        {
            if (await GetItemsAsync(pattern, true, false) is IReadOnlyList<IStorageItem> items)
            {
                var result = new List<IStorageFolder>();
                foreach (var item in items)
                    if (item is IStorageFolder folder)
                        result.Add(folder);
                return result.AsReadOnly();
            }
            return null;
        }


        /// <summary>
        /// Gets the file or folder with the specified name from the current folder.
        /// </summary>
        /// <param name="name">The name (or path relative to the current folder) of the file or folder to get.</param>
        /// <returns></returns>
        public async Task<SandboxedStorage.IStorageItem> GetItemAsync(string name)
        {
            await Task.Delay(5).ConfigureAwait(false);

            return (await GetItemsAsync(name)).FirstOrDefault();
        }

        public async Task<IReadOnlyList<SandboxedStorage.IStorageItem>> GetItemsAsync(string pattern = null)
            => await GetItemsAsync(pattern, true, true);

        /// <summary>
        /// Gets the items in the current folder.
        /// </summary>
        /// <returns></returns>
        public async Task<IReadOnlyList<SandboxedStorage.IStorageItem>> GetItemsAsync(string pattern, bool folders, bool files)
        {
            await Task.Delay(5).ConfigureAwait(false);

            List<SandboxedStorage.IStorageItem> items = new List<SandboxedStorage.IStorageItem>();
            string regex = string.IsNullOrWhiteSpace(pattern)
                ? null
                : StringExtensions.WildcardToRegex(pattern);


            if (!await StartAccess())
                return null;

            var itemUrls = NSFileManager.DefaultManager.GetDirectoryContent(Url, null, NSDirectoryEnumerationOptions.SkipsSubdirectoryDescendants | NSDirectoryEnumerationOptions.ProducesRelativePathUrls, out NSError error);
            if (AfterActionInvalid(error))
                return null;
            

            foreach (var itemUrl in itemUrls ?? new NSUrl[] { })
            {
                if (this.RemoveCurrentFolderFromPath(itemUrl.Path) is string itemName)
                //var folderName = folderPath.AbsoluteString;
                {
                    if (string.IsNullOrWhiteSpace(regex) || Regex.IsMatch(itemName, regex))
                    {
                        
                        if(itemUrl.TryGetResource(NSUrl.IsDirectoryKey, out NSObject value) && value is NSNumber isDirectory)
                        {
                            if (isDirectory.BoolValue)
                            {
                                if (folders)
                                    items.Add(new StorageFolder(itemUrl));
                            }
                            else if (files)
                                items.Add(new StorageFile(itemUrl));
                        }
                    }
                }
            }


            var result = items.AsReadOnly();
            return result;
        }


        /// <summary>
        /// Tries to get the file or folder with the specified name from the current folder.
        /// Returns null instead of raising a FileNotFoundException if the specified file or folder is not found.
        /// </summary>
        /// <param name="name">The name (or path relative to the current folder) of the file or folder to get.</param>
        /// <returns>When this method completes successfully, it returns an IStorageItem that represents the specified file or folder.
        /// If the specified file or folder is not found, this method returns null instead of raising an exception.</returns>
        public async Task<SandboxedStorage.IStorageItem> TryGetItemAsync(string name)
        {
            await Task.Delay(5).ConfigureAwait(false);

            return await GetItemAsync(name);
        }
        #endregion


        #region Equality
        public override bool Equals(object obj)
        {
            return Equals(obj as StorageFolder);
        }

        public bool Equals(StorageFolder other)
        {
            return other != null &&
                   base.Equals(other) &&
                   Path == other.Path;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Path);
        }

        public static bool operator ==(StorageFolder left, StorageFolder right)
        {
            return EqualityComparer<StorageFolder>.Default.Equals(left, right);
        }

        public static bool operator !=(StorageFolder left, StorageFolder right)
        {
            return !(left == right);
        }
        #endregion


        #endregion



    }
}