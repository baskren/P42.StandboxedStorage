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
            var picker = new Windows.Storage.Pickers.FolderPicker
            {
                CommitButtonText = "Grant Access",
            };
            picker.FileTypeFilter.Add("*");
            if (await picker.PickSingleFolderAsync() is Windows.Storage.StorageFolder windowsFolder)
            {
                return new StorageFolder(windowsFolder, true);
            }
            return null;
        }
    }
}
