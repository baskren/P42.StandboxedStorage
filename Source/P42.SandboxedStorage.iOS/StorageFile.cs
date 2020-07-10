
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
        public static Task<IStorageFile> GetFileFromPathAsync(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            bool isDirectory = false;
            if (NSFileManager.DefaultManager.FileExists(path, ref isDirectory))
            {
                if (!isDirectory)
                    return Task.FromResult<IStorageFile>(new StorageFile(path));
            }
            return null;
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
        public StorageFile(string path, bool makeBookmark = false) : base(path, makeBookmark) { }

        public StorageFile(NSUrl url, bool makeBookmark = false) : base(url, makeBookmark) { }
        #endregion

        /*
        #region Private Methods
        internal override async Task<bool> RequestAccess(string message)
        {
            if (await base.RequestAccess(message))
            {
                if (await FilePicker.PickSingleFileAsync(this) is IStorageFile iStorageFile && iStorageFile is StorageFile storageFile)
                {
                    Url = storageFile.Url;
                    Bookmark = BookmarkExtensions.GetOrCreateBookmark(Url);
                    return CanWrite();
                }
            }
            return false;
        }
        #endregion
        */


        #region IStorageFile

        #region Check Access
        /*
        public async Task<bool> CanRead()
        {
            var result = NSFileManager.DefaultManager.IsReadableFile(Path);
            return result;
        }

        public bool CanWrite()
        {
            var result = NSFileManager.DefaultManager.IsWritableFile(Path);
            return result;
        }
        */

        bool CanRead => NSFileManager.DefaultManager.IsReadableFile(Path);

        bool CanWrite => NSFileManager.DefaultManager.IsWritableFile(Path);

        internal async Task<bool> StartReadAccess(Action action = null)
        {
            if (!await StartAccess())
                return false;

            var accessDenialResponse = AccessDenialResponse.Value();

            if (!CanRead && accessDenialResponse == AccessDenialResponse.RequestAccess)
                await RequestAccess(Path);
            if (!CanRead)
            {
                if (action != null)
                    action.Invoke();
                StopAccess();
                if (accessDenialResponse == AccessDenialResponse.Exception)
                    ThrowAccessException();
                return false;
            }

            return true;
        }

        internal async Task<bool> StartWriteAccess(Action action = null)
        {
            if (!await StartAccess())
                return false;

            var accessDenialResponse = AccessDenialResponse.Value();

            if (!CanWrite && accessDenialResponse == AccessDenialResponse.RequestAccess)
                await RequestAccess(Path);
            if (!CanWrite)
            {
                if (action != null)
                    action.Invoke();
                StopAccess();
                if (accessDenialResponse == AccessDenialResponse.Exception)
                    ThrowAccessException();
                return false;
            }

            return true;
        }
        #endregion


        #region Copy
        public async Task CopyAndReplaceAsync(IStorageFile fileToReplace)
            //=> await CopyAsync(await fileToReplace.GetParentAsync(), fileToReplace.Name);
            => throw new NotImplementedException();

        public async Task<IStorageFile> CopyAsync(IStorageFolder destinationFolder)
            => await CopyAsync(destinationFolder, Name);

        public async Task<IStorageFile> CopyAsync(IStorageFolder destinationFolder, string desiredNewName)
        {
            return await CopyMoveInner(destinationFolder, desiredNewName, (destinationUrl) =>
            {
                if (NSFileManager.DefaultManager.Copy(Url, destinationUrl, out NSError error))
                    return (new StorageFile(destinationUrl,true), error);
                return (null, error);
            });
        }
        #endregion


        #region Move
        public async Task MoveAndReplaceAsync(IStorageFile fileToReplace)
            => throw new NotImplementedException();

        public async Task MoveAsync(IStorageFolder destinationFolder)
            => await MoveAsync(destinationFolder, Name);

        public async Task MoveAsync(IStorageFolder destinationFolder, string desiredNewName)
        {
            if (await CopyMoveInner(destinationFolder, desiredNewName, (destinationUrl) =>
            {
                if (NSFileManager.DefaultManager.Move(Url, destinationUrl, out NSError error))
                    return (new StorageFile(destinationUrl, true), error);
                return (null, error);
            }) is StorageFile result)
            {
                var bm = result.Url.GetOrCreateBookmark();
                Url = bm.NewUrl ?? Url;
                Bookmark = bm.Bookmark ?? Bookmark;
            }
        }

        async Task<IStorageFile> CopyMoveInner(IStorageFolder destinationFolder, string desiredNewName, Func<NSUrl, (IStorageFile file, NSError error)> func)
        {
            await Task.Delay(5).ConfigureAwait(false);

            // get access
            if (string.IsNullOrWhiteSpace(desiredNewName))
                throw new AccessViolationException("Invalid file name [" + desiredNewName + "]");

            if (!await ((StorageFolder)destinationFolder).StartAccess())
                return null;

            if (await destinationFolder.FileExists(desiredNewName))
            {
                ((StorageFolder)destinationFolder).StopAccess();
                throw new AccessViolationException("File [" + desiredNewName + "] aready exists in folder [" + destinationFolder.Path + "]");
            }

            if (!await StartAccess(()=> ((StorageFolder)destinationFolder).StopAccess()))
                return null;


            // Make the copy
            var destinationUrl = ((StorageFolder)destinationFolder).Url.Append(desiredNewName, false);

            var result = func.Invoke(destinationUrl);

            StopAccess();
            ((StorageFolder)destinationFolder).StopAccess();

            if (result.error != null)
            {
                Console.WriteLine("Cannot copy file [" + Path + "] to destination [" + destinationUrl.Path + "].");
                Console.WriteLine("ERROR: " + result.error);
                if (AccessDenialResponse != AccessDenialResponse.Silent)
                    throw new AccessViolationException(result.error.LocalizedDescription);
                return null;
            }
            return result.file;
        }
        #endregion


        #region Rename
        public async Task RenameAsync(string desiredName)
            => await RenameAsync(desiredName, NameCollisionOption.FailIfExists);

        public async Task RenameAsync(string desiredName, NameCollisionOption option)
        {
            if (string.IsNullOrEmpty(desiredName))
                throw new ArgumentNullException("desiredName");

            await Task.Delay(5).ConfigureAwait(false);

            if (GetParent() is StorageFolder folder)
            {
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
        }
        #endregion

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


        #region Append
        public async Task AppendAllLinesAsync(IEnumerable<string> lines, System.Threading.CancellationToken cancellationToken = default)
        {
            await Task.Delay(5).ConfigureAwait(false);

            var appendText = string.Join("\n", lines);
            await AppendAllTextAsync(appendText);
        }

        public async Task AppendAllTextAsync(string contents, System.Threading.CancellationToken cancellationToken = default)
        {
            await Task.Delay(5).ConfigureAwait(false);

            if (await StartWriteAccess())
            {
                try
                {
                    await File.AppendAllTextAsync(Path, contents, cancellationToken);
                    StopAccess();
                }
                catch (Exception e)
                {
                    AfterActionInvalid(e);
                }
            }
        }
        #endregion


        #region Read
        public async Task<byte[]> ReadAllBytesAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            await Task.Delay(5).ConfigureAwait(false);

            if (await StartReadAccess())
            {
                var data = NSData.FromUrl(Url, NSDataReadingOptions.Mapped, out NSError error);
                if (AfterActionInvalid(error))
                    return null;

                var bytes = data?.ToArray();
                return bytes;
            }
            return null;
        }

        public async Task<string[]> ReadAllLinesAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            await Task.Delay(5).ConfigureAwait(false);

            if (await ReadAllTextAsync(cancellationToken) is string text && text.Length > 0)
            {
                var lines = text.Split('\n');
                return lines;
            }
            return null;
        }

        public async Task<string> ReadAllTextAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            await Task.Delay(5).ConfigureAwait(false);

            if (await StartReadAccess())
            {
                /*
                //var str = await File.ReadAllTextAsync(Path, cancellationToken);
                var data = NSData.FromUrl(Url, NSDataReadingOptions.Mapped, out NSError error);
                if (AfterActionInvalid(error))
                    return null;

                var str = NSString.FromData(data, NSStringEncoding.UTF8);
                return str.ToString();
                */

                try
                {
                    var str = await File.ReadAllTextAsync(Path, cancellationToken);
                    StopAccess();
                    return str;
                }
                catch (Exception e)
                {
                    AfterActionInvalid(e);
                }
            }
            return null;
        }
        #endregion


        #region Write
        public async Task WriteAllBytesAsync(byte[] bytes, System.Threading.CancellationToken cancellationToken = default)
        {
            if (bytes == null || bytes.Length < 0)
                return;

            await Task.Delay(5).ConfigureAwait(false);

            if (await StartWriteAccess())
            {
                try
                {
                    var data = NSData.FromArray(bytes);
                    data.Save(Url, true);
                    StopAccess();
                }
                catch (Exception e)
                {
                    AfterActionInvalid(e);
                }
            }
        }

        public async Task WriteAllLinesAsync(IEnumerable<string> lines, System.Threading.CancellationToken cancellationToken = default)
        {
            if (lines is null || !lines.Any())
                return;

            await Task.Delay(5).ConfigureAwait(false);

            var text = string.Join('\n', lines);
            await WriteAllTextAsync(text);
        }

        public async Task WriteAllTextAsync(string content, System.Threading.CancellationToken cancellationToken = default)
        {
            await Task.Delay(5).ConfigureAwait(false);

            //var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            //await WriteAllBytesAsync(bytes, cancellationToken);
            if (await StartWriteAccess())
            {
                try
                {
                    await File.WriteAllTextAsync(Path, content, cancellationToken);
                    StopAccess();
                }
                catch (Exception e)
                {
                    AfterActionInvalid(e);
                }
            }
        }
        #endregion


        #region Equality
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

        #endregion



    }
}
