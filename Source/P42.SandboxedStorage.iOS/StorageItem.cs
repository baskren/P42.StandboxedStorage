using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Foundation;

namespace P42.SandboxedStorage.Native
{
    class StorageItem : IStorageItem, IEquatable<StorageItem>
    {
        #region Public Properties
        /// <summary>
        /// Gets the name of the file including the file name extension.
        /// </summary>
        public string Name
        //    => System.IO.Path.GetFileName(Path);
        {
            get
            {
                if (Url is NSUrl url)
                    return url.LastPathComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the full file-system path of the current file, if the file has a path.
        /// </summary>
        public string Path
        {
            get
            {
                if (Url is NSUrl url)
                    return url.Path;
                return null;
            }
        }

        /// <summary>
        /// Gets the size of the file in bytes.
        /// </summary>
        public ulong Size
        {
            get
            {
                if (FileAttributes is NSFileAttributes attributes)
                {
                    var size = attributes.Size;
                    return size.Value;
                }
                return 0;
                //FileInfo fi = new FileInfo(Path);
                //return (ulong)fi.Length;
            }
        }

        /// <summary>
        /// Gets the date and time when the current file was created. 
        /// </summary>
        public DateTimeOffset DateCreated
        {
            get
            {
                if (FileAttributes is NSFileAttributes attributes)
                {
                    var appleDate = attributes.CreationDate;
                    var dateTime = (DateTime)appleDate;
                    return (DateTimeOffset)dateTime;
                }
                return DateTimeOffset.MinValue;
                //var utc = File.GetCreationTimeUtc(Path);
                //var local = File.GetCreationTime(Path);
                //var offset = local - utc;
                //return new DateTimeOffset(local, offset);
            }
        }

        /// <summary>
        /// Gets the timestamp of the last time the file was modified.
        /// </summary>
        public DateTimeOffset DateModified
        {
            get
            {
                if (FileAttributes is NSFileAttributes attributes)
                {
                    var appleDate = attributes.ModificationDate;
                    var dateTime = (DateTime)appleDate;
                    return (DateTimeOffset)dateTime;
                }
                return DateTimeOffset.MinValue;
                /*
                DateTime time;
                TimeSpan offset;
                time = File.GetLastWriteTime(Path);
                offset = time.Subtract(File.GetLastWriteTimeUtc(Path));
                return new DateTimeOffset(time, offset);
                */
            }
        }

        /// <summary>
        /// Gets the attributes of a file.
        /// </summary>
        public FileAttributes Attributes
            => FileAttributesHelper.FromIOFileAttributes(File.GetAttributes(Path));


        /// <summary>
        /// What to do if access is denied?
        /// </summary>
        public AccessDenialResponse AccessDenialResponse { get; set; }
        #endregion


        #region Private Properties
        public NSUrl Url { get; protected set; }

        NSData _bookmark;
        protected NSData Bookmark
        {
            get
            {
                _bookmark = _bookmark ?? Url.GetBookmark();
                return _bookmark;
            }
            set => _bookmark = value;
        }

        NSFileAttributes FileAttributes
        {
            get
            {
                Url.StartAccessingSecurityScopedResource();
                var attributes = NSFileManager.DefaultManager.GetAttributes(Path, out NSError error);
                Url.StopAccessingSecurityScopedResource();
                if (error == null)
                    return attributes;
                return null;
            }
        }
        #endregion


        #region Construction
        public StorageItem(string path, bool makeBookmark = false)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Cannot initialize Native.StorageItem for path [" + path + "].");
            Url = NSUrl.CreateFileUrl(path, null);
            if (makeBookmark)
                Bookmark = Url.CreateBookmark();
        }


        public StorageItem(NSUrl url, bool makeBookmark = false)
        {
            Url = url;
            if (Url == null)
                throw new ArgumentException("Cannot initialize Native.StorageItem for URL [" + url + "].");
            if (makeBookmark)
                Bookmark = Url.CreateBookmark();
        }
        #endregion


        #region Private Methods
        internal bool CanAccess()
        {
            //if (Bookmark is null)
            //    Bookmark = Url.GetBookmark();
            return Bookmark != null;
        }

        internal async Task<bool> StartAccess(Action action = null, AccessDenialResponse accessDenialResponse = AccessDenialResponse.Default)
        {
            var canAccess = CanAccess();
            if (!canAccess)
            {
                if (AccessDenialResponse == AccessDenialResponse.Exception || AccessDenialResponse == AccessDenialResponse.Default)
                {
                    if (action != null)
                        action.Invoke();
                    ThrowAccessException();
                }
                else if (AccessDenialResponse == AccessDenialResponse.RequestAccess)
                    return !await RequestAccess(Path);
                return false;
            }
            Url.StartAccessingSecurityScopedResource();
            return true;
        }

        internal void StopAccess()
        {
            Url.StopAccessingSecurityScopedResource();
        }

        internal void ThrowAccessException(string message = null)
        {
            Url.StopAccessingSecurityScopedResource();
            throw new UnauthorizedAccessException(message ?? "Access denied to [" + Path + "].");
        }

        internal async virtual Task<bool> RequestAccess(string message)
        {
            if (AccessDenialResponse == AccessDenialResponse.RequestAccess)
            {
                Xamarin.Forms.Page currentPage = null;
                if (Xamarin.Forms.Application.Current.MainPage.Navigation.NavigationStack.LastOrDefault() is Xamarin.Forms.Page navPage)
                    currentPage = navPage;
                if (currentPage?.Navigation.ModalStack.LastOrDefault() is Xamarin.Forms.Page lastModalPage)
                    currentPage = lastModalPage;
                if (await currentPage.DisplayActionSheet(message + " Do you want to navigate to this file in order to grant access?", "cancel", "ok") is string button)
                {
                    return button == "ok";
                }
            }
            return false;
        }
        #endregion


        #region IStorageItem Methods
        public bool Exists()
        {
            var result = NSFileManager.DefaultManager.FileExists(Path);
            return result;
        }

        public bool CanDelete()
        {
            var result = NSFileManager.DefaultManager.IsDeletableFile(Path);
            return result;
        }

        /// <summary>
        /// Determines whether the current <see cref="StorageFile"/> matches the specified <see cref="StorageItemTypes"/> value.
        /// </summary>
        /// <param name="type">The value to match against.</param>
        /// <returns>True if the <see cref="StorageFile"/> matches the specified value; otherwise false.</returns>
        /// <seealso cref="StorageItemTypes"/>
        public bool IsOfType(StorageItemTypes type)
            => type == StorageItemTypes.File;

        /// <summary>
        /// Indicates whether the current file is equal to the specified file.
        /// </summary>
        /// <param name="item">The <see cref="SandboxedStorage.IStorageItem"/>  object that represents a file to compare against.</param>
        /// <returns>Returns true if the current file is equal to the specified file; otherwise false.</returns>
        public bool IsEqual(SandboxedStorage.IStorageItem item)
            => Path == item.Path;

        /// <summary>
        /// Gets the parent folder of the current file.
        /// </summary>
        /// <returns></returns>
        public async Task<IStorageFolder> GetParentAsync()
        {
            await Task.Delay(5).ConfigureAwait(false);

            return await Task.Run<IStorageFolder>(() =>
            {
                var parentUrl = Url.RemoveLastPathComponent();
                return new StorageFolder(parentUrl);
            });
        }

        /// <summary>
        /// Deletes the current file, optionally deleting the item permanently.
        /// </summary>
        /// <returns></returns>
        public async Task DeleteAsync(StorageDeleteOption option = StorageDeleteOption.Default)
        {
            if (!await StartAccess())
                return;

            if (!CanDelete())
                ThrowAccessException("You do not have ability to delete storage item [" + Path + "]");

            await Task.Delay(5).ConfigureAwait(false);

            if (option == StorageDeleteOption.Default)
            {
                if (NSFileManager.DefaultManager.TrashItem(Url, out NSUrl resultingUrl, out NSError error))
                    Url = resultingUrl;
                else
                {
                    Console.WriteLine("Cannot delete file [" + Url.Path + "].");
                    Console.WriteLine("ERROR: " + error);
                }
            }
            else if (!NSFileManager.DefaultManager.Remove(Url, out NSError error))
            {
                Console.WriteLine("Cannot delete file [" + Url.Path + "].");
                Console.WriteLine("ERROR: " + error);
            }
            StopAccess();
        }

        #endregion


        #region Equality
        public override bool Equals(object obj)
            => Equals(obj as StorageItem);

        public bool Equals(StorageItem other)
            => other != null && Path == other.Path;

        public override int GetHashCode()
            => HashCode.Combine(Path);

        public static bool operator ==(StorageItem left, StorageItem right)
            => EqualityComparer<StorageItem>.Default.Equals(left, right);

        public static bool operator !=(StorageItem left, StorageItem right)
            => !(left == right);
        #endregion
    }
}
