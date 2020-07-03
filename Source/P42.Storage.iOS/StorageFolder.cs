//-----------------------------------------------------------------------
// <copyright file="StorageFolder.cs" company="In The Hand Ltd">
//     Copyright © 2016-17 In The Hand Ltd. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
// Refactored for cross platform .NetStandard library structure in 2020 by 42ndParallel.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Foundation;

namespace P42.Storage.Native
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
    class StorageFolder : StorageItem, IStorageFolder
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
        public Task<IStorageFile> CreateFileAsync(string desiredName, CreationCollisionOption options)
        {
            return Task.Run<IStorageFile>(() =>
            {
                string filepath = System.IO.Path.Combine(Path, desiredName);

                if (File.Exists(filepath))
                {
                    switch (options)
                    {
                        case CreationCollisionOption.OpenIfExists:
                            return new StorageFile(filepath);

                        case CreationCollisionOption.ReplaceExisting:
                            File.Delete(filepath);
                            break;

                        case CreationCollisionOption.GenerateUniqueName:
                            for (int i = 1; i < 100; i++)
                            {
                                string newPath = string.Format(filepath.Substring(0, filepath.LastIndexOf('.')) + " ({0})" + filepath.Substring(filepath.LastIndexOf('.')), i);
                                if (!File.Exists(newPath))
                                {
                                    filepath = newPath;
                                    break;
                                }
                            }
                            break;

                        default:
                            throw new IOException();
                    }
                }

                File.Create(filepath).Close();

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
        public Task<IStorageFolder> CreateFolderAsync(string desiredName, CreationCollisionOption options)
        {
            return Task.Run<IStorageFolder>(() =>
            {
                string newpath = System.IO.Path.Combine(Path, desiredName);

                if (Directory.Exists(newpath))
                {
                    switch (options)
                    {
                        case CreationCollisionOption.OpenIfExists:
                            return new StorageFolder(newpath);

                        case CreationCollisionOption.ReplaceExisting:
                            Directory.Delete(newpath);
                            break;

                        case CreationCollisionOption.GenerateUniqueName:
                            for (int i = 1; i < 100; i++)
                            {
                                string uniquePath = string.Format(newpath.Substring(0, newpath.LastIndexOf('.')) + " ({0})" + newpath.Substring(newpath.LastIndexOf('.')), i);
                                if (!File.Exists(uniquePath))
                                {
                                    newpath = uniquePath;
                                    break;
                                }
                            }
                            break;

                        default:
                            throw new IOException();
                    }
                }

                Directory.CreateDirectory(newpath);

                return new StorageFolder(newpath);
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

            return await Task.Run<IStorageFile>(() =>
            {
                string filepath = System.IO.Path.Combine(Path, filename);

                if (!File.Exists(filepath))
                {
                    throw new FileNotFoundException();
                }

                return new StorageFile(filepath);
            });
        }

        /// <summary>
        /// Gets the files in the current folder.
        /// </summary>
        /// <returns></returns>
        public async Task<IReadOnlyList<IStorageFile>> GetFilesAsync(string pattern = null)
        {
            await Task.Delay(5).ConfigureAwait(false);

            List<IStorageFile> files = new List<IStorageFile>();

            string regex = string.IsNullOrWhiteSpace(pattern)
                ? null
                : StringExtensions.WildcardToRegex(pattern);

            foreach (string filename in Directory.GetFiles(Path))
                if (string.IsNullOrWhiteSpace(regex) || Regex.IsMatch(filename, regex))
                    files.Add(new StorageFile(System.IO.Path.Combine(Path, filename)));

            var result = files.AsReadOnly();
            return await Task.FromResult<IReadOnlyList<IStorageFile>>(result);
        }

        /// <summary>
        /// Gets the specified folder from the current folder. 
        /// </summary>
        /// <param name="name">The name of the child folder to retrieve.</param>
        /// <returns>When this method completes successfully, it returns a StorageFolder that represents the child folder.</returns>
        public async Task<IStorageFolder> GetFolderAsync(string name)
        {
            await Task.Delay(5).ConfigureAwait(false);

            return await Task.Run<IStorageFolder>(() =>
            {
                string folderpath = System.IO.Path.Combine(Path, name);

                if (!Directory.Exists(folderpath))
                {
                    throw new FileNotFoundException();
                }
                return new StorageFolder(folderpath);
            });
        }

        /// <summary>
        /// Gets the folders in the current folder.
        /// </summary>
        /// <returns></returns>
        public async Task<IReadOnlyList<IStorageFolder>> GetFoldersAsync(string pattern = null)
        {
            await Task.Delay(5).ConfigureAwait(false);

            List<IStorageFolder> folders = new List<IStorageFolder>();

            string regex = string.IsNullOrWhiteSpace(pattern)
                ? null
                : StringExtensions.WildcardToRegex(pattern);

            return await Task.Run<IReadOnlyList<IStorageFolder>>(() =>
            {
                foreach (string foldername in Directory.GetDirectories(Path))
                    if (string.IsNullOrWhiteSpace(regex) || Regex.IsMatch(foldername, regex))
                        folders.Add(new StorageFolder(System.IO.Path.Combine(Path, foldername)));
                
                return folders.AsReadOnly();
            });
        }

        /// <summary>
        /// Gets the file or folder with the specified name from the current folder.
        /// </summary>
        /// <param name="name">The name (or path relative to the current folder) of the file or folder to get.</param>
        /// <returns></returns>
        public async Task<IStorageItem> GetItemAsync(string name)
        {
            await Task.Delay(5).ConfigureAwait(false);

            return await Task.Run<IStorageItem>(() =>
            {
                foreach (string foldername in Directory.GetDirectories(Path))
                {
                    if (foldername == name)
                        return new StorageFolder(System.IO.Path.Combine(Path, foldername));
                }

                foreach (string filename in Directory.GetFiles(Path))
                {
                    if(filename == name)
                        return new StorageFile(System.IO.Path.Combine(Path, filename));
                }

                return null;
            });
        }

        /// <summary>
        /// Gets the items in the current folder.
        /// </summary>
        /// <returns></returns>
        public async Task<IReadOnlyList<IStorageItem>> GetItemsAsync(string pattern = null)
        {
            await Task.Delay(5);

            List<IStorageItem> items = new List<IStorageItem>();

            string regex = string.IsNullOrWhiteSpace(pattern)
                ? null
                : StringExtensions.WildcardToRegex(pattern);

            foreach (string foldername in Directory.GetDirectories(Path))
                if (string.IsNullOrWhiteSpace(regex) || Regex.IsMatch(foldername, regex))
                    items.Add(new StorageFolder(System.IO.Path.Combine(Path, foldername)));

            foreach (string filename in Directory.GetFiles(Path))
                if (string.IsNullOrWhiteSpace(regex) || Regex.IsMatch(filename, regex))
                    items.Add(new StorageFile(System.IO.Path.Combine(Path, filename)));

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
        public Task<IStorageItem> TryGetItemAsync(string name)
        {
            return Task.Run<IStorageItem>(() =>
            {
                string itempath = System.IO.Path.Combine(Path, name);

                if (File.Exists(itempath))
                    return new StorageFile(itempath);
                else if (Directory.Exists(itempath))
                    return new StorageFolder(itempath);

                return null;
            });
        }

        #endregion


    }
}