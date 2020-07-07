using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MobileCoreServices;
using AppKit;

namespace P42.SandboxedStorage.Native
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
                ResolvesAliases = true,
            };

            panel.RunModal(fileTypes?.ToArray() ?? new string[] { UTType.Content, UTType.Item, "public.data" });

            if (panel.Url is null)
                return Task.FromResult<IStorageFile>(null);

            System.Diagnostics.Debug.WriteLine("panel.Url.Path: " + panel.Url.Path);
            return Task.FromResult<IStorageFile>(new StorageFile(panel.Url, true));
        }

        internal static Task<IStorageFile> PickSingleFileAsync(StorageFile storageFile, string message = null)
        {
            /*
            var folderTask = storageFile.GetParentAsync();
            folderTask.Wait();
            var folder = folderTask.Result as StorageFolder;
            */
            var folderUrl = storageFile.Url.RemoveLastPathComponent();

            var panel = new NSOpenPanel
            //var paen = new NSSavePanel
            {
                CanCreateDirectories = true,
                CanChooseDirectories = false,
                CanChooseFiles = true,
                FloatingPanel = true,
                AllowsMultipleSelection = false,
                ResolvesAliases = true,
                DirectoryUrl = folderUrl,
                Prompt = "THIS IS THE PROMPT!",
                Title = "TITLE!",
            };

            if (!string.IsNullOrWhiteSpace(message))
                panel.Message = message;

            string utType = storageFile.FileType; // UTType.CreatePreferredIdentifier(UTType.TagClassMIMEType, storageFile.ContentType, null);

            panel.RunModal(folderUrl.Path, storageFile.Name,  new string[] { utType });

            if (panel.Url is null)
                return Task.FromResult<IStorageFile>(null);

            System.Diagnostics.Debug.WriteLine("panel.Url.Path: " + panel.Url.Path);
            return Task.FromResult<IStorageFile>(new StorageFile(panel.Url, true));
        }
    }
}
