using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MobileCoreServices;
using AppKit;
using Foundation;

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

        internal static async Task<IStorageFile> PickSaveAsFileAsync(string defaultFileExtension = null, IDictionary<string, IList<string>> fileTypeChoices = null)
        {
            return await MainThread.InvokeOnMainThread(async () =>
            {
                var tcs = new TaskCompletionSource<IStorageFile>();
                var panel = new NSSavePanel
                {
                    Title = "Save As ...",
                    CanCreateDirectories = true,
                };
                if (!string.IsNullOrWhiteSpace(defaultFileExtension))
                    panel.AllowedContentTypes = new UniformTypeIdentifiers.UTType[]
                    {
                    //UTType.CreatePreferredIdentifier(UTType.TagClassFilenameExtension, defaultFileExtension, null)
                    UniformTypeIdentifiers.UTType.CreateFromExtension(defaultFileExtension)
                    };

                panel.Begin(button =>
                {
                    if (button == (int)NSPanelButtonType.Ok)
                    {
                        if (panel.Url != null && panel.Url.IsFileUrl)
                        {
                            var result = new StorageFile(panel.Url, true);
                            tcs.SetResult(result);
                            return;
                        }
                    }
                    tcs.SetResult(null);
                });

                return await tcs.Task;
            });
        }

        static void PlatformBeginInvokeOnMainThread(Action action)
        {
            NSRunLoop.Main.BeginInvokeOnMainThread(action.Invoke);
        }

        internal static T InvokeOnMainThread<T>(Func<T> factory)
        {
            T value = default;
            NSRunLoop.Main.InvokeOnMainThread(() => value = factory());
            return value;
        }

    }

}
