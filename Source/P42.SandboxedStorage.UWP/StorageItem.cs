using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using P42.SandboxedStorage;
using Windows.UI.Xaml.Controls;

namespace P42.SandboxedStorage.Native
{
    class StorageItem : IStorageItem
    {
        #region Properties
        public string Name => _item?.Name;

        public string Path => _item?.Path;

        public ulong Size
        {
            get
            {
                if (GetBasicProperties() is Windows.Storage.FileProperties.BasicProperties properties)
                    return properties.Size;
                return 0;
            }
        }

        public DateTimeOffset DateCreated => _item?.DateCreated ?? DateTimeOffset.MinValue;

        public DateTimeOffset DateModified
        {
            get
            {
                if (GetBasicProperties() is Windows.Storage.FileProperties.BasicProperties properties)
                    return properties.DateModified;
                return DateCreated;
            }
        }

        public FileAttributes Attributes
        {
            get
            {
                if (_item is null)
                    return FileAttributes.ReadOnly | (GetType() == typeof(StorageFolder) ? FileAttributes.Directory : FileAttributes.Normal);
                return (FileAttributes)_item.Attributes;
            }
        }

        public AccessDenialResponse AccessDenialResponse { get; set; }

        protected bool IsBookmarked  => Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.CheckAccess(_item);
        #endregion


        #region Fields
        protected Windows.Storage.IStorageItem _item;
        #endregion


        #region Construction
        public StorageItem(Windows.Storage.IStorageItem item, bool bookmark = false)
        {
            _item = item;
            if (bookmark)
                AddToFutureAccessList();    
        }

        //public StorageItem() { }
        #endregion



        #region Private Methods
        protected static async Task TryNativeMethod<T>(T item, Func<T, Task> nativeMethod,
            [System.Runtime.CompilerServices.CallerFilePath] string callerPath = null, 
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0) 
            where T : StorageItem
        {
            try
            {
                await nativeMethod?.Invoke(item);
            }
            catch (Exception e)
            {
                var accessDenialResponse = item.AccessDenialResponse.Value();
                if (accessDenialResponse == AccessDenialResponse.RequestAccess && await RequestAccess<T>(item.Path) is StorageItem newItem)
                {
                    item._item = newItem._item;
                    await nativeMethod?.Invoke(item);
                }
                else if (accessDenialResponse == AccessDenialResponse.Exception)
                {
                    throw new Exception("Exception at [" + callerPath + ":" + lineNumber + "]", e);
                }
            }
        }
        protected static async Task TryNativeMethod<T>(T item, CancellationToken cancellationToken, Func<T, CancellationToken, Task> nativeMethod,
            [System.Runtime.CompilerServices.CallerFilePath] string callerPath = null,
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0)
            where T : StorageItem
        {
            try
            {
                await nativeMethod?.Invoke(item, cancellationToken);
            }
            catch (Exception e)
            {
                var accessDenialResponse = item.AccessDenialResponse.Value();
                if (accessDenialResponse == AccessDenialResponse.RequestAccess && await RequestAccess<T>(item.Path) is StorageItem newItem)
                {
                    item._item = newItem._item;
                    await nativeMethod?.Invoke(item, cancellationToken);
                }
                else if (accessDenialResponse == AccessDenialResponse.Exception)
                {
                    throw new Exception("Exception at [" + callerPath + ":" + lineNumber + "]", e);
                }
            }
        }

        protected static async Task<object> TryNativeMethod<T>(T item, Func<T, Task<object>> nativeMethod, 
            [System.Runtime.CompilerServices.CallerFilePath] string callerPath = null, 
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0) 
            where T : StorageItem
        {
            object result = null;
            try
            {
                result = await nativeMethod?.Invoke(item);
            }
            catch (Exception e)
            {
                var accessDenialResponse = item.AccessDenialResponse.Value();
                if (accessDenialResponse == AccessDenialResponse.RequestAccess && await RequestAccess<T>(item.Path) is StorageItem newItem)
                {
                    item._item = newItem._item;
                    result = await nativeMethod?.Invoke(item);
                }
                else if (accessDenialResponse == AccessDenialResponse.Exception)
                {
                    throw new Exception("Exception at [" + callerPath + ":" + lineNumber + "]", e);
                }
            }
            return result;
        }

        protected static async Task<object> TryNativeMethod<T>(T item, CancellationToken cancellationToken, Func<T, CancellationToken, Task<object>> nativeMethod,
            [System.Runtime.CompilerServices.CallerFilePath] string callerPath = null,
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0)
            where T : StorageItem
        {
            object result = null;
            try
            {
                result = await nativeMethod?.Invoke(item, cancellationToken);
            }
            catch (Exception e)
            {
                var accessDenialResponse = item.AccessDenialResponse.Value();
                if (accessDenialResponse == AccessDenialResponse.RequestAccess && await RequestAccess<T>(item.Path) is StorageItem newItem)
                {
                    item._item = newItem._item;
                    result = await nativeMethod?.Invoke(item, cancellationToken);
                }
                else if (accessDenialResponse == AccessDenialResponse.Exception)
                {
                    throw new Exception("Exception at [" + callerPath + ":" + lineNumber + "]", e);
                }
            }
            return result;
        }

        protected static async Task<StorageItem> RequestAccess<T>(string path)
        {
            ContentDialog requestAccessDialog = new ContentDialog
            {
                Title = "Access Requested",
                Content =  "Access to "+path+" is needed to continue.  Click [Find] to locate and grant access.",
                PrimaryButtonText = "Find",
                CloseButtonText = "Ok"
            };
            if (ContentDialogResult.Primary == await requestAccessDialog.ShowAsync())
            {
                if (typeof(T) == typeof(StorageFile))
                {
                    if (await FilePicker.PickSingleFileAsync() is StorageFile storageFile)
                        return storageFile;
                }
                else
                {
                    if (await FolderPicker.PickSingleFolderAsync() is StorageFolder storageFolder)
                        return storageFolder;
                }
            }
            return null;
        }

        protected void AddToFutureAccessList()
        {
            var max = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.MaximumItemsAllowed;
            var count = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Entries.Count;
            if (count == max && Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Entries.FirstOrDefault() is Windows.Storage.AccessCache.AccessListEntry firstItem)                
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Remove(firstItem.Token);
            Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(_item);
        }

        Windows.Storage.FileProperties.BasicProperties GetBasicProperties()
        {
            if (_item is null)
                return null;
            var task = Task.Run(async () => await _item.GetBasicPropertiesAsync());
            task.RunSynchronously();
            return task.Result;
        }


        #endregion


        #region Methods
        public virtual Task<IStorageFolder> GetParentAsync()
            =>  throw new NotImplementedException();
        

        public bool IsEqual(IStorageItem item)
            => item?.Path == Path && GetType() == item?.GetType();

        public virtual bool IsOfType(StorageItemTypes type) 
            => type == StorageItemTypes.None;

        public async Task DeleteAsync()
        {
            await Task.Delay(5).ConfigureAwait(false);

            await _item.DeleteAsync();
        }

        public async Task DeleteAsync(StorageDeleteOption option)
        {
            await Task.Delay(5).ConfigureAwait(false);

            await _item.DeleteAsync((Windows.Storage.StorageDeleteOption)((int)option));
        }

        public bool Exists()
        {
            if (_item.IsOfType(Windows.Storage.StorageItemTypes.None))
                return false;
            return true;
        }

        #endregion
    }
}