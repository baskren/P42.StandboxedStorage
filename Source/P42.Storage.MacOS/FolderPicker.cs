using System;
using System.Threading.Tasks;
using AppKit;

namespace P42.Storage.Native
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
            return Task.FromResult<IStorageFolder>(new StorageFolder(panel.Url));
        }
    }
}
