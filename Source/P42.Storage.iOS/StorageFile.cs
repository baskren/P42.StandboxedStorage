
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Foundation;


namespace P42.Storage.Native
{
    class StorageFile : StorageItem, IStorageFile
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
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }
            var url = NSUrl.CreateFileUrl(path, false, null);
            //return Task.FromResult<IStorageFile>(new StorageFile(path));
            return Task.FromResult<IStorageFile>(new StorageFile(url));
        }

        #region Properties
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

        public string FileType
            => Url?.PathExtension;
        #endregion


        #region Constructors
        public StorageFile(string path) : base(path) { }

        public StorageFile(NSUrl url) : base(url) { }
        #endregion


        #region IStorageFile
        public Task CopyAndReplaceAsync(IStorageFile fileToReplace)
        //    => Task.Run(() => { File.Replace(Path, fileToReplace.Path, null); });
            => CopyAsync(fileToReplace.GetParentAsync().Result, fileToReplace.Name);


        public Task<IStorageFile> CopyAsync(IStorageFolder destinationFolder)
            => CopyAsync(destinationFolder, Name);

        public Task<IStorageFile> CopyAsync(IStorageFolder destinationFolder, string desiredNewName)
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
        }


        public Task MoveAndReplaceAsync(IStorageFile fileToReplace)
            => Task.Run(() =>
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

        public Task MoveAsync(IStorageFolder destinationFolder)
            => MoveAsync(destinationFolder, Name);

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

        public Task RenameAsync(string desiredName)
            => RenameAsync(desiredName, NameCollisionOption.FailIfExists);

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


        public void AppendAllLines(IEnumerable<string> lines)
        {
            if (Url is NSUrl url)
            {
                url.StartAccessingSecurityScopedResource();
                File.AppendAllLines(url.Path, lines);
                url.StopAccessingSecurityScopedResource();
            }
        }

        public async Task AppendAllLinesAsync(IEnumerable<string> lines, System.Threading.CancellationToken cancellationToken = default)
        {
            if (Url is NSUrl url)
            {
                url.StartAccessingSecurityScopedResource();
                await File.AppendAllLinesAsync(url.Path, lines, cancellationToken);
                url.StopAccessingSecurityScopedResource();
            }
        }

        public void AppendAllText(string contents)
        {
            if (Url is NSUrl url)
            {
                url.StartAccessingSecurityScopedResource();
                File.AppendAllText(url.Path, contents);
                url.StopAccessingSecurityScopedResource();
            }
        }

        public async Task AppendAllTextAsync(string contents, System.Threading.CancellationToken cancellationToken = default)
        {
            if (Url is NSUrl url)
            {
                url.StartAccessingSecurityScopedResource();
                await File.AppendAllTextAsync(url.Path, contents, cancellationToken);
                url.StopAccessingSecurityScopedResource();
            }
        }


        public System.IO.FileStream Open(System.IO.FileMode mode, System.IO.FileAccess access = FileAccess.ReadWrite, System.IO.FileShare share = FileShare.None)
        {
            if (Url is NSUrl url)
            {
                url.StartAccessingSecurityScopedResource();
                return new StorageFileStream(this, Url.Path, mode, access, share);
            }
            return null;
        }

        public System.IO.FileStream OpenRead()
        {
            if (Url is NSUrl url)
            {
                url.StartAccessingSecurityScopedResource();
                return new StorageFileStream(this, Url.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            return null;
        }

        public System.IO.StreamReader OpenText()
        {
            if (OpenRead() is System.IO.FileStream fileStream)
            {
                return new StorageFileStreamReader(this, fileStream);
            }
            return null;
        }

        public System.IO.FileStream OpenWrite()
        {
            if (Url is NSUrl url)
            {
                url.StartAccessingSecurityScopedResource();
                return new StorageFileStream(this, Url.Path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            }
            return null;
        }



        public byte[] ReadAllBytes()
        {
            if (Url is NSUrl url)
            {
                url.StartAccessingSecurityScopedResource();
                var bytes = File.ReadAllBytes(url.Path);
                url.StopAccessingSecurityScopedResource();
                return bytes;
            }
            return null;
        }

        public async Task<byte[]> ReadAllBytesAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            if (Url is NSUrl url)
            {
                url.StartAccessingSecurityScopedResource();
                var bytes = await File.ReadAllBytesAsync(url.Path, cancellationToken);
                url.StopAccessingSecurityScopedResource();
                return bytes;
            }
            return null;
        }

        public string[] ReadAllLines()
        {
            if (Url is NSUrl url)
            {
                url.StartAccessingSecurityScopedResource();
                var lines = File.ReadAllLines(url.Path);
                url.StopAccessingSecurityScopedResource();
                return lines;
            }
            return null;
        }

        public async Task<string[]> ReadAllLinesAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            if (Url is NSUrl url)
            {
                url.StartAccessingSecurityScopedResource();
                var lines = await File.ReadAllLinesAsync(url.Path, cancellationToken);
                url.StopAccessingSecurityScopedResource();
                return lines;
            }
            return null;
        }

        public string ReadAllText()
        {
            if (Url is NSUrl url)
            {
                url.StartAccessingSecurityScopedResource();
                var content = File.ReadAllText(url.Path);
                url.StopAccessingSecurityScopedResource();
                return content;
            }
            return null;
        }

        public async Task<string> ReadAllTextAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            if (Url is NSUrl url)
            {
                url.StartAccessingSecurityScopedResource();
                var content = await File.ReadAllTextAsync(url.Path, cancellationToken);
                url.StopAccessingSecurityScopedResource();
                return content;
            }
            return null;
        }

        public void WriteAllBytes(byte[] bytes)
        {
            if (Url is NSUrl url)
            {
                url.StartAccessingSecurityScopedResource();
                File.WriteAllBytes(url.Path, bytes);
                url.StopAccessingSecurityScopedResource();
            }
        }

        public async Task WriteAllBytesAsync(byte[] bytes, System.Threading.CancellationToken cancellationToken = default)
        {
            if (Url is NSUrl url)
            {
                url.StartAccessingSecurityScopedResource();
                await File.WriteAllBytesAsync(url.Path, bytes, cancellationToken);
                url.StopAccessingSecurityScopedResource();
            }
        }

        public void WriteAllLines(IEnumerable<string> content)
        {
            if (Url is NSUrl url)
            {
                url.StartAccessingSecurityScopedResource();
                File.WriteAllLines(url.Path, content);
                url.StopAccessingSecurityScopedResource();
            }
        }

        public async Task WriteAllLinesAsync(IEnumerable<string> content, System.Threading.CancellationToken cancellationToken = default)
        {
            if (Url is NSUrl url)
            {
                url.StartAccessingSecurityScopedResource();
                await File.WriteAllLinesAsync(url.Path, content, cancellationToken);
                url.StopAccessingSecurityScopedResource();
            }
        }

        public void WriteAllText(string content)
        {
            if (Url is NSUrl url)
            {
                url.StartAccessingSecurityScopedResource();
                File.WriteAllText(url.Path, content);
                url.StopAccessingSecurityScopedResource();
            }
        }

        public async Task WriteAllTextAsync(string content, System.Threading.CancellationToken cancellationToken = default)
        {
            if (Url is NSUrl url)
            {
                url.StartAccessingSecurityScopedResource();
                await File.WriteAllTextAsync(url.Path, content, cancellationToken);
                url.StopAccessingSecurityScopedResource();
            }
        }

    }
}
