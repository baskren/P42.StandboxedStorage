using P42.SandboxedStorage.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P42.SandboxedStorage.Native
{
    static class FolderPicker 
    {
        public static async Task<IStorageFolder> PickSingleFolderAsync()
        {
            var picker = new Windows.Storage.Pickers.FolderPicker();
            picker.FileTypeFilter.Add("*");
            if (await picker.PickSingleFolderAsync() is Windows.Storage.StorageFolder windowsFolder)
            {
                Windows.Storage.AccessCache.StorageApplicationPermissions.MostRecentlyUsedList.Add(windowsFolder, windowsFolder.Path);
                return new StorageFolder(windowsFolder);
            }
            return null;
        }
    }
}
