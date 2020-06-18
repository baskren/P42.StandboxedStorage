using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using P42.Storage;

namespace P42.Storage.Native
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
        #endregion


        #region Fields
        protected Windows.Storage.IStorageItem _item;
        #endregion


        #region Construction
        public StorageItem(Windows.Storage.IStorageItem item) 
            => _item = item;

        public StorageItem() { }
        #endregion


        #region Methods
        Windows.Storage.FileProperties.BasicProperties GetBasicProperties()
        {
            if (_item is null)
                return null;
            var task = Task.Run(async () => await _item.GetBasicPropertiesAsync());
            task.RunSynchronously();
            return task.Result;
        }

        public virtual Task<IStorageFolder> GetParentAsync()
            =>  throw new NotImplementedException();
        

        public bool IsEqual(IStorageItem item)
            => item?.Path == Path && GetType() == item?.GetType();

        public virtual bool IsOfType(StorageItemTypes type) 
            => type == StorageItemTypes.None;

        public async Task DeleteAsync()
            => await _item.DeleteAsync();

        public async Task DeleteAsync(StorageDeleteOption option)
            => await _item.DeleteAsync((Windows.Storage.StorageDeleteOption)((int)option));
        #endregion
    }
}