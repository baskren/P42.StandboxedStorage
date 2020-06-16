using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MobileCoreServices;
using AppKit;

namespace P42.Storage.Native
{
    public static class FilePicker
    {
        public static Task<IStorageFile> PickSingleFileAsync(IList<string> fileTypes)
        {
            var panel = new NSOpenPanel
            {
                CanChooseDirectories = false,
                CanChooseFiles = true,
                FloatingPanel = true,
                AllowsMultipleSelection = false,
            };

            panel.RunModal(fileTypes?.ToArray() ?? new string[] { UTType.Content, UTType.Item, "public.data" });

            System.Diagnostics.Debug.WriteLine("panel.Url.Path: " + panel.Url.Path);

            return Task.FromResult<IStorageFile>(new StorageFile(panel.Url));
        }

    }
}
