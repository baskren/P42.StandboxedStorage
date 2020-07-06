using System;
using System.Threading.Tasks;
using AppKit;

namespace P42.SandboxedStorage.Native
{
    public static class FolderPicker
    {
        public static Task<IStorageFolder> PickSingleFolderAsync()
        {
            var panel = new NSOpenPanel
            {
                CanChooseDirectories = true,
                CanChooseFiles = false,
                FloatingPanel = true,
                AllowsMultipleSelection = false,
            };

            panel.RunModal();

            if (panel.Url is null)
                return Task.FromResult<IStorageFolder>(null);

            System.Diagnostics.Debug.WriteLine("panel.Url.Path: " + panel.Url.Path);
            return Task.FromResult<IStorageFolder>(new StorageFolder(panel.Url, true));
        }

        internal static Task<IStorageFolder> PickSingleFolderAsync(IStorageFolder storageFolder)
        {
            return PickSingleFolderAsync(storageFolder.Path);
        }

        public static Task<IStorageFolder> PickSingleFolderAsync(string folderPath)
        {
            var panel = new NSOpenPanel
            {
                CanChooseDirectories = true,
                CanChooseFiles = false,
                FloatingPanel = true,
                AllowsMultipleSelection = false,
            };

            panel.RunModal(folderPath, null);

            if (panel.Url is null)
                return Task.FromResult<IStorageFolder>(null);

            System.Diagnostics.Debug.WriteLine("panel.Url.Path: " + panel.Url.Path);
            return Task.FromResult<IStorageFolder>(new StorageFolder(panel.Url, true));
        }

    }
}
