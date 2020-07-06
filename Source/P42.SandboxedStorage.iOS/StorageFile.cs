
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Foundation;


namespace P42.SandboxedStorage.Native
{
    class StorageFile : StorageItem, IStorageFile, IEquatable<StorageFile>
    {
        /// <summary>
        /// Gets a StorageFile object to represent the file at the specified path.
        /// </summary>
        /// <param name="path">The path of the file to get a StorageFile to represent.
        /// If your path uses slashes, make sure you use backslashes(\).
        /// Forward slashes(/) are not accepted by this method.</param>
        /// <returns>When this method completes, it returns the file as a StorageFile.</returns>
        public async static Task<IStorageFile> GetFileFromPathAsync(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            await Task.Delay(5).ConfigureAwait(false);

            return await Task.Run<IStorageFile>(() =>
            {
                var url = NSUrl.CreateFileUrl(path, false, null);
                return new StorageFile(url);
            });
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
        {
            get
            {
                var extension = Url?.PathExtension.ToLower();
                return string.IsNullOrEmpty(extension)
                    ? extension
                    : "." + extension;
            }
        }
        #endregion


        #region Constructors
        public StorageFile(string path) : base(path) { }

        public StorageFile(NSUrl url) : base(url) { }
        #endregion


        #region IStorageFile
        public async Task CopyAndReplaceAsync(IStorageFile fileToReplace)
            => await CopyAsync(await fileToReplace.GetParentAsync(), fileToReplace.Name);


        public async Task<IStorageFile> CopyAsync(IStorageFolder destinationFolder)
            => await CopyAsync(destinationFolder, Name);

        public async Task<IStorageFile> CopyAsync(IStorageFolder destinationFolder, string desiredNewName)
        {
            await Task.Delay(5).ConfigureAwait(false);

            return await Task.Run<IStorageFile>(() =>
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
                        return new StorageFile(destination);
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
        }


        public async Task MoveAndReplaceAsync(IStorageFile fileToReplace)
        {
            await Task.Delay(5).ConfigureAwait(false);

            await Task.Run(() =>
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
            });
        }

        public async Task MoveAsync(IStorageFolder destinationFolder)
            => await MoveAsync(destinationFolder, Name);

        public async Task MoveAsync(IStorageFolder destinationFolder, string desiredNewName)
        {
            await Task.Delay(5).ConfigureAwait(false);

            await Task.Run(() =>
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
                        var newFile = new StorageFile(destination);
                        Url = newFile.Url;
                    }
                    else
                    {
                        Console.WriteLine("Cannot move file [" + url.Path + "] to destination [" + folder.Url.Path + "].");
                        Console.WriteLine("ERROR: " + error);
                    }
                    destination.StopAccessingSecurityScopedResource();
                    url.StopAccessingSecurityScopedResource();
                }
            });
        }

        public async Task RenameAsync(string desiredName)
            => await RenameAsync(desiredName, NameCollisionOption.FailIfExists);

        public async Task RenameAsync(string desiredName, NameCollisionOption option)
        {
            if (string.IsNullOrEmpty(desiredName))
                throw new ArgumentNullException("desiredName");

            await Task.Delay(5).ConfigureAwait(false);

            var folder = await GetParentAsync();

            switch (option)
            {
                case NameCollisionOption.GenerateUniqueName:
                    int i = 0;
                    string uniqueName = desiredName;
                    while (await folder.GetFileAsync(uniqueName) is IStorageFile)
                    {
                        uniqueName = string.Format(desiredName.Substring(0, desiredName.LastIndexOf('.')) + " ({0})" + desiredName.Substring(desiredName.LastIndexOf('.')), ++i);
                    }
                    await MoveAsync(folder, uniqueName);
                    break;

                case NameCollisionOption.ReplaceExisting:
                    if (await folder.GetFileAsync(desiredName) is IStorageFile existingFile)
                        await existingFile.DeleteAsync();
                    await MoveAsync(folder, desiredName);
                    break;

                default:
                    if (!(await folder.GetFileAsync(desiredName) is IStorageFile))
                        await MoveAsync(folder, desiredName);
                    break;
            }
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

        #region System.IO.File methods
        public async Task AppendAllLinesAsync(IEnumerable<string> lines, System.Threading.CancellationToken cancellationToken = default)
        {
            await Task.Delay(5).ConfigureAwait(false);

            if (Url is NSUrl url)
            {
                var appendText = string.Join("\n", lines);
                await AppendAllTextAsync(appendText);
            }
        }

        public async Task AppendAllTextAsync(string contents, System.Threading.CancellationToken cancellationToken = default)
        {
            await Task.Delay(5).ConfigureAwait(false);

            if (Url is NSUrl url)
            {
                url.StartAccessingSecurityScopedResource();
                await File.AppendAllTextAsync(url.Path, contents, cancellationToken);
                url.StopAccessingSecurityScopedResource();
            }
        }

        public async Task<byte[]> ReadAllBytesAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            await Task.Delay(5).ConfigureAwait(false);

            if (Url is NSUrl url)
            {
                url.StartAccessingSecurityScopedResource();
                //var bytes = await File.ReadAllBytesAsync(url.Path, cancellationToken);

                var readable = NSFileManager.DefaultManager.IsReadableFile(url.Path);

                var data = NSData.FromUrl(url, NSDataReadingOptions.Mapped, out NSError error);
                var bytes = data.ToArray();



                url.StopAccessingSecurityScopedResource();
                return bytes;
            }
            return null;
        }

        public async Task<string[]> ReadAllLinesAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            await Task.Delay(5).ConfigureAwait(false);

            if (Url is NSUrl url)
            {
                /*
                url.StartAccessingSecurityScopedResource();
                var lines = await File.ReadAllLinesAsync(url.Path, cancellationToken);
                url.StopAccessingSecurityScopedResource();
                return lines;
                */
                if (await ReadAllTextAsync(cancellationToken) is string text && text.Length > 0)
                {
                    var lines = text.Split('\n');
                    return lines;
                }
            }
            return null;
        }

        public async Task<string> ReadAllTextAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            await Task.Delay(5).ConfigureAwait(false);

            if (Url is NSUrl url)
            {
                /*
                url.StartAccessingSecurityScopedResource();
                //var content = await File.ReadAllTextAsync(url.Path, cancellationToken);
                var data = NSData.FromUrl(url);
                var content = NSString.FromData(data, NSStringEncoding.UTF8);
                url.StopAccessingSecurityScopedResource();
                return content;
                */
                if (await ReadAllBytesAsync(cancellationToken) is byte[] bytes && bytes.Length > 0)
                {
                    var text = System.Text.Encoding.UTF8.GetString(bytes);
                    return text;
                }
            }
            return null;
        }

        public async Task WriteAllBytesAsync(byte[] bytes, System.Threading.CancellationToken cancellationToken = default)
        {
            if (bytes == null || bytes.Length < 0)
                return;

            await Task.Delay(5).ConfigureAwait(false);

            if (Url is NSUrl url)
            {
                url.StartAccessingSecurityScopedResource();
                //await File.WriteAllBytesAsync(url.Path, bytes, cancellationToken);
                var data = NSData.FromArray(bytes);
                data.Save(url, true);
                url.StopAccessingSecurityScopedResource();
            }
        }

        public async Task WriteAllLinesAsync(IEnumerable<string> lines, System.Threading.CancellationToken cancellationToken = default)
        {
            if (lines is null || !lines.Any())
                return;

            await Task.Delay(5).ConfigureAwait(false);

            /*
            if (Url is NSUrl url)
            {
                url.StartAccessingSecurityScopedResource();
                await File.WriteAllLinesAsync(url.Path, lines, cancellationToken);
                url.StopAccessingSecurityScopedResource();
            }
            */
            var text = string.Join('\n', lines);
            await WriteAllTextAsync(text);
        }

        public async Task WriteAllTextAsync(string content, System.Threading.CancellationToken cancellationToken = default)
        {
            await Task.Delay(5).ConfigureAwait(false);

            /*
            if (Url is NSUrl url)
            {
                url.StartAccessingSecurityScopedResource();
                await File.WriteAllTextAsync(url.Path, content, cancellationToken);
                url.StopAccessingSecurityScopedResource();
            }
            */
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            await WriteAllBytesAsync(bytes, cancellationToken);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as StorageFile);
        }

        public bool Equals(StorageFile other)
        {
            return other != null &&
                   base.Equals(other) &&
                   Path == other.Path;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Path);
        }

        public static bool operator ==(StorageFile left, StorageFile right)
        {
            return EqualityComparer<StorageFile>.Default.Equals(left, right);
        }

        public static bool operator !=(StorageFile left, StorageFile right)
        {
            return !(left == right);
        }
        #endregion


        public async Task<bool> CanRead()
        {
            if (Url is NSUrl url)
            {
                url.StartAccessingSecurityScopedResource();
                var result = NSFileManager.DefaultManager.IsReadableFile(Path);
                url.StopAccessingSecurityScopedResource();
                return await Task.FromResult<bool>(result);
            }
            return await Task.FromResult<bool>(false);
        }

        public async Task<bool> CanWrite()
        {
            if (Url is NSUrl url)
            {
                url.StartAccessingSecurityScopedResource();
                var result = NSFileManager.DefaultManager.IsWritableFile(Path);
                url.StopAccessingSecurityScopedResource();
                return await Task.FromResult<bool>(result);
            }
            return await Task.FromResult<bool>(false);
        }

        public async Task<bool> CanDelete()
        {
            if (Url is NSUrl url)
            {
                url.StartAccessingSecurityScopedResource();
                var result = NSFileManager.DefaultManager.IsDeletableFile(Path);
                url.StopAccessingSecurityScopedResource();
                return await Task.FromResult<bool>(result);
            }
            return await Task.FromResult<bool>(false);
        }

    }
}
