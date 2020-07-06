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
        // PIZZA

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
            {
                throw new ArgumentNullException("path");
            }
            return Task.FromResult<IStorageFolder>(new StorageFolder(path));
        }

        internal StorageFolder(string path) : base(path) { }

        public StorageFolder(NSUrl url) : base(url) { }

        #region IStorageFolder
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

            return await Task.Run<IStorageFile>(async () =>
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

                string filepath = System.IO.Path.Combine(Path, desiredName);
                Url.StartAccessingSecurityScopedResource();
                File.Create(filepath).Close();
                Url.StopAccessingSecurityScopedResource();
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

            return await Task.Run<IStorageFolder>(async () =>
            {
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

                var newPath = System.IO.Path.Combine(Path, desiredName);
                Url.StartAccessingSecurityScopedResource();
                Directory.CreateDirectory(newPath);
                Url.StopAccessingSecurityScopedResource();
                return new StorageFolder(newPath);
            });
        }

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
            await Task.Delay(5).ConfigureAwait(false);

            return await Task.Run<IReadOnlyList<IStorageFile>>(() =>
            {
                string regex = string.IsNullOrWhiteSpace(pattern)
                    ? null
                    : StringExtensions.WildcardToRegex(pattern);

                List<IStorageFile> files = new List<IStorageFile>();

                Url.StartAccessingSecurityScopedResource();
                //var filePaths = Directory.GetFiles(Path);
                var filePaths = NSFileManager.DefaultManager.GetDirectoryContent(Path, out NSError error);
                if (error != null)
                    return null;
                Url.StopAccessingSecurityScopedResource();
                foreach (string filePath in filePaths)
                {
                    var x = this.RemoveCurrentFolderFromPath(filePath);
                    if (x is string fileName)
                    {
                        if (string.IsNullOrWhiteSpace(regex) || Regex.IsMatch(fileName, regex))
                            files.Add(new StorageFile(filePath));
                    }
                }
                return files.AsReadOnly();
            });
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
            await Task.Delay(5).ConfigureAwait(false);

            return await Task.Run<IReadOnlyList<IStorageFolder>>(() =>
            {
                string regex = string.IsNullOrWhiteSpace(pattern)
                    ? null
                    : StringExtensions.WildcardToRegex(pattern);

                List<IStorageFolder> folders = new List<IStorageFolder>();

                var accessAvailable = Url.StartAccessingSecurityScopedResource();
                //var folderPaths = Directory.GetDirectories(Path);
                var folderPaths = NSFileManager.DefaultManager.GetDirectoryContent(Url, null, NSDirectoryEnumerationOptions.SkipsSubdirectoryDescendants | NSDirectoryEnumerationOptions.ProducesRelativePathUrls, out NSError error);
                if (error != null)
                    return null;
                Url.StopAccessingSecurityScopedResource();

                foreach (var folderPath in folderPaths)
                {
                    //if (this.RemoveCurrentFolderFromPath(folderPath) is string folderName)
                    var folderName = folderPath.AbsoluteString;
                    {
                        if (string.IsNullOrWhiteSpace(regex) || Regex.IsMatch(folderName, regex))
                            folders.Add(new StorageFolder(folderPath));
                    }
                }
                return folders.AsReadOnly();
            });
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

        /// <summary>
        /// Gets the items in the current folder.
        /// </summary>
        /// <returns></returns>
        public async Task<IReadOnlyList<SandboxedStorage.IStorageItem>> GetItemsAsync(string pattern = null)
        {
            await Task.Delay(5).ConfigureAwait(false);

            List<SandboxedStorage.IStorageItem> items = new List<SandboxedStorage.IStorageItem>();

            if (await GetFoldersAsync(pattern) is IReadOnlyList<IStorageFolder> folders && folders.Any())
                items.AddRange(folders);

            if (await GetFilesAsync(pattern) is IReadOnlyList<IStorageFile> files && files.Any())
                items.AddRange(files);

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


        public async Task<bool> ItemExists(string itemName)
        {
            await Task.Delay(5).ConfigureAwait(false);

            if (Url is NSUrl url)
            {
                url.StartAccessingSecurityScopedResource();
                var path = System.IO.Path.Combine(Path, itemName);
                var result = NSFileManager.DefaultManager.FileExists(path);
                url.StopAccessingSecurityScopedResource();
                return result;
            }
            return false;
        }

        public async Task<bool> FileExists(string fileName)
        {
            await Task.Delay(5).ConfigureAwait(false);

            if (Url is NSUrl url)
            {
                url.StartAccessingSecurityScopedResource();
                var path = System.IO.Path.Combine(Path, fileName);
                bool isDirectory = false;
                var result = NSFileManager.DefaultManager.FileExists(path, ref isDirectory);
                url.StopAccessingSecurityScopedResource();
                if (isDirectory)
                    return false;
                return result;
            }
            return false;
        }

        public async Task<bool> FolderExists(string folderName)
        {
            await Task.Delay(5).ConfigureAwait(false);

            if (Url is NSUrl url)
            {
                url.StartAccessingSecurityScopedResource();
                var path = System.IO.Path.Combine(Path, folderName);
                bool isDirectory = false;
                var result = NSFileManager.DefaultManager.FileExists(path, ref isDirectory);
                url.StopAccessingSecurityScopedResource();
                if (!isDirectory)
                    return false;
                return result;
            }
            return false;
        }

    }
}