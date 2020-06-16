
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Foundation;

namespace P42.Storage.Native
{
    public sealed class StorageFile : StorageItem, IStorageFile
    {
        /// <summary>
        /// Gets a StorageFile object to represent the file at the specified path.
        /// </summary>
        /// <param name="path">The path of the file to get a StorageFile to represent.
        /// If your path uses slashes, make sure you use backslashes(\).
        /// Forward slashes(/) are not accepted by this method.</param>
        /// <returns>When this method completes, it returns the file as a StorageFile.</returns>
        internal static Task<IStorageFile> GetFileFromPathAsync(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }
            var url = NSUrl.CreateFileUrl(path, false, null);
            //return Task.FromResult<IStorageFile>(new StorageFile(path));
            return Task.FromResult<IStorageFile>(new StorageFile(url));
        }




        public StorageFile(string path) : base(path) { }

        public StorageFile(NSUrl url) : base(url) { }


        #region IStorageFile
        /// <summary>
        /// Replaces the specified file with a copy of the current file.
        /// </summary>
        /// <param name="fileToReplace"></param>
        /// <returns></returns>
        public Task CopyAndReplaceAsync(IStorageFile fileToReplace)
        //    => Task.Run(() => { File.Replace(Path, fileToReplace.Path, null); });
            => CopyAsync(fileToReplace.GetParentAsync().Result, fileToReplace.Name);


        /// <summary>
        /// Creates a copy of the file in the specified folder.
        /// </summary>
        /// <param name="destinationFolder">The destination folder where the copy of the file is created.</param>
        /// <returns></returns>
        public Task<IStorageFile> CopyAsync(IStorageFolder destinationFolder)
        //  => CopyAsync(destinationFolder, System.IO.Path.GetFileName(Path));
            => CopyAsync(destinationFolder, Name);



        /// <summary>
        /// Creates a copy of the file in the specified folder and renames the copy.
        /// </summary>
        /// <param name="destinationFolder">The destination folder where the copy of the file is created.</param>
        /// <param name="desiredNewName">The new name for the copy of the file created in the destinationFolder.</param>
        /// <returns></returns>
        public Task<IStorageFile> CopyAsync(IStorageFolder destinationFolder, string desiredNewName)

        => Task.Run<IStorageFile>(() =>
        {
            if (destinationFolder is StorageFolder folder &&
                folder.Url is NSUrl folderUrl &&
                !string.IsNullOrWhiteSpace(desiredNewName) &&
                Url is NSUrl url
                )
            {
                var destination = folderUrl.Append(desiredNewName, false);
                url.StartAccessingSecurityScopedResource();
                destination.StartAccessingSecurityScopedResource();
                if (NSFileManager.DefaultManager.Copy(url, destination, out NSError error))
                {
                    return Task.FromResult<IStorageFile>(new StorageFile(destination));
                }
                else
                {
                    Console.WriteLine("Cannot copy file [" + url.Path + "] to destination [" + folder.Url.Path + "].");
                    Console.WriteLine("ERROR: " + error);
                }
                destination.StopAccessingSecurityScopedResource();
                url.StopAccessingSecurityScopedResource();
            }
            return null;
        });


        /// <summary>
        /// Moves the current file to the location of the specified file and replaces the specified file in that location.
        /// </summary>
        /// <param name="fileToReplace">The file to replace.</param>
        /// <returns>No object or value is returned by this method.</returns>
        public Task MoveAndReplaceAsync(IStorageFile fileToReplace)
            => Task.Run(async () =>
            {
                if (fileToReplace is StorageFile file &&
                    file.Url is NSUrl fileUrl &&
                    Url is NSUrl url)
                {
                    url.StartAccessingSecurityScopedResource();
                    fileUrl.StartAccessingSecurityScopedResource();
                    if (NSFileManager.DefaultManager.Replace(fileUrl, url,
                        fileUrl.AppendPathExtension(".bak").LastPathComponent,
                        NSFileManagerItemReplacementOptions.WithoutDeletingBackupItem,
                        out NSUrl resultingUrl, out NSError error))
                    {
                        Url = resultingUrl;
                    }
                    else
                    {
                        Console.WriteLine("Cannot replace file at destination [" + fileUrl.Path + "] with [" + url.Path + "].");
                        Console.WriteLine("ERROR: " + error);
                    }
                    fileUrl.StopAccessingSecurityScopedResource();
                    url.StopAccessingSecurityScopedResource();
                }
                return Task.CompletedTask;
            });

        /// <summary>
        /// Moves the current file to the specified folder and renames the file according to the desired name.
        /// </summary>
        /// <param name="destinationFolder">The destination folder where the file is moved.</param>
        /// <returns></returns>
        public Task MoveAsync(IStorageFolder destinationFolder)
            => MoveAsync(destinationFolder, Name);


        /// <summary>
        /// Moves the current file to the specified folder and renames the file according to the desired name.
        /// </summary>
        /// <param name="destinationFolder">The destination folder where the file is moved.</param>
        /// <param name="desiredNewName">The desired name of the file after it is moved.</param>
        /// <returns></returns>
        public Task MoveAsync(IStorageFolder destinationFolder, string desiredNewName)
            => Task.Run(() =>
            { 
                if (destinationFolder is StorageFolder folder &&
                    folder.Url is NSUrl folderUrl &&
                    !string.IsNullOrWhiteSpace(desiredNewName) &&
                    Url is NSUrl url
                    )
                {
                    var destination = folderUrl.Append(desiredNewName, false);
                    url.StartAccessingSecurityScopedResource();
                    destination.StartAccessingSecurityScopedResource();
                    if (NSFileManager.DefaultManager.Move(url, destination, out NSError error))
                    {
                        return Task.FromResult<IStorageFile>(new StorageFile(destination));
                    }
                    else
                    {
                        Console.WriteLine("Cannot move file [" + url.Path + "] to destination [" + folder.Url.Path + "].");
                        Console.WriteLine("ERROR: " + error);
                    }
                    destination.StopAccessingSecurityScopedResource();
                    url.StopAccessingSecurityScopedResource();
                }
                return null;
            });

        /// <summary>
        /// Gets the MIME type of the contents of the file.
        /// </summary>
        /// <remarks>
        /// <para/><list type="table">
        /// <listheader><term>Platform</term><description>Version supported</description></listheader>
        /// <item><term>iOS</term><description>iOS 9.0 and later</description></item>
        /// <item><term>tvOS</term><description>tvOS 9.0 and later</description></item>
        /// <item><term>Tizen</term><description>Tizen 3.0</description></item>
        /// <item><term>Windows UWP</term><description>Windows 10</description></item>
        /// <item><term>Windows Store</term><description>Windows 8.1 or later</description></item>
        /// <item><term>Windows Phone Store</term><description>Windows Phone 8.1 or later</description></item>
        /// <item><term>Windows Phone Silverlight</term><description>Windows Phone 8.0 or later</description></item>
        /// </list>
        /// </remarks>
        public string ContentType
        {
            get
            {
                string mime = string.Empty;

                var tag = FileType.ToLower();

                string utref = MobileCoreServices.UTType.CreatePreferredIdentifier(MobileCoreServices.UTType.TagClassFilenameExtension, tag, null);
                if (!string.IsNullOrEmpty(utref))
                    mime = MobileCoreServices.UTType.GetPreferredTag(utref, MobileCoreServices.UTType.TagClassMIMEType);
                return mime;
            }
        }

        /// <summary>
        /// Gets the type (file name extension) of the file.
        /// </summary>
        public string FileType
            => Url?.PathExtension;

        #endregion


        #region IStorageItem
        #endregion


        /*

        /// <summary>
        /// Retrieves an adjusted thumbnail image for the file, determined by the purpose of the thumbnail.
        /// </summary>
        /// <param name="mode">The enum value that describes the purpose of the thumbnail and determines how the thumbnail image is adjusted.</param>
        /// <returns>When this method completes successfully, it returns a <see cref="StorageItemThumbnail"/> that represents the thumbnail image or null if there is no thumbnail image associated with the file.</returns>
        public Task<StorageItemThumbnail> GetThumbnailAsync(ThumbnailMode mode)
        {
#if WINDOWS_UWP || WINDOWS_APP || WINDOWS_PHONE_APP || WINDOWS_PHONE
            return Task.Run<StorageItemThumbnail>(async () =>
            {
                return await _file.GetThumbnailAsync((Windows.Storage.FileProperties.ThumbnailMode)mode);
            });

#else
            if (ContentType.StartsWith("video"))
            {
                return StorageItemThumbnail.CreateVideoThumbnailAsync(this);
            }

            else if (ContentType.StartsWith("image"))
            {
                return StorageItemThumbnail.CreatePhotoThumbnailAsync(this);
            }

            return Task.FromResult<StorageItemThumbnail>(null);
#endif
        }
        */



        /// <summary>
        /// Renames the current file.
        /// </summary>
        /// <param name="desiredName">The desired, new name of the current item.</param>
        /// <returns>No object or value is returned by this method when it completes.</returns>
        public Task RenameAsync(string desiredName)
            => RenameAsync(desiredName, NameCollisionOption.FailIfExists);
        

        /// <summary>
        /// Renames the current file.
        /// This method also specifies what to do if an existing item in the current file's location has the same name.
        /// </summary>
        /// <param name="desiredName">The desired, new name of the current file.
        /// <para>If there is an existing item in the current file's location that already has the specified desiredName, the specified <see cref="NameCollisionOption"/>  determines how the system responds to the conflict.</para></param>
        /// <param name="option">The enum value that determines how the system responds if the desiredName is the same as the name of an existing item in the current file's location.</param>
        /// <returns>No object or value is returned by this method when it completes.</returns>
        public async Task RenameAsync(string desiredName, NameCollisionOption option)
        {
            if (string.IsNullOrEmpty(desiredName))
            {
                throw new ArgumentNullException("desiredName");
            }
            string folder = System.IO.Path.GetDirectoryName(Path);
            string newPath = System.IO.Path.Combine(folder, desiredName);
            var newUrl = NSUrl.CreateFileUrl(newPath, null);

            switch (option)
            {

                case NameCollisionOption.GenerateUniqueName:
                    string generatedPath = newPath;
                    int num = 2;
                    while (File.Exists(generatedPath))
                    {
                        generatedPath = System.IO.Path.Combine(folder, System.IO.Path.GetFileNameWithoutExtension(desiredName), string.Format("({0})", num), System.IO.Path.GetExtension(desiredName));
                        num++;
                    }
                    newPath = generatedPath;
                    break;

                case NameCollisionOption.ReplaceExisting:
                    if (NSFileManager.DefaultManager.FileExists(newPath))
                    {
                        if (!NSFileManager.DefaultManager.Remove(newPath, out NSError error))
                        {
                            Console.WriteLine("Cannot delete file [" + newUrl.Path + "].");
                            Console.WriteLine("ERROR: " + error);
                        }
                    }                        
                    break;

                default:
                    break;
            }

            await MoveAsync(await GetParentAsync(), System.IO.Path.GetFileName(newPath));
        }






    }
}
