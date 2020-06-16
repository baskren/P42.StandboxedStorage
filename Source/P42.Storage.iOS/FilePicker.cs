using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MobileCoreServices;
using UIKit;

namespace P42.Storage.Native
{
    public static class FilePicker
    {
        static UIDocumentPickerViewController pvc;

        static TaskCompletionSource<IStorageFile> tcs;

        internal static UIViewController GetActiveViewController()
        {
            UIWindow window = UIApplication.SharedApplication.KeyWindow;
            UIViewController viewController = window.RootViewController;

            while (viewController.PresentedViewController != null)
            {
                viewController = viewController.PresentedViewController;
            }

            return viewController;
        }

        public static Task<IStorageFile> PickSingleFileAsync(IList<string> fileTypes)
        {
            tcs = new TaskCompletionSource<IStorageFile>();

            pvc = new UIDocumentPickerViewController(fileTypes?.ToArray() ?? new string[] { UTType.Content, UTType.Item, "public.data" }, UIDocumentPickerMode.Open)
            {
                AllowsMultipleSelection = false
            };
            pvc.DidPickDocument += Pvc_DidPickDocument;
            pvc.WasCancelled += Pvc_WasCancelled;
            pvc.DidPickDocumentAtUrls += Pvc_DidPickDocumentAtUrls;

            UIViewController viewController = GetActiveViewController();
            viewController.PresentViewController(pvc, true, null);

            return tcs.Task; //Task.FromResult<IStorageFile>(new StorageFile(_uri));
        }


        /// <summary>
        /// Callback method called by document picker when file has been picked; this is called
        /// starting from iOS 11.
        /// </summary>
        /// <param name="sender">sender object (document picker)</param>
        /// <param name="args">event args</param>
        private static void Pvc_DidPickDocumentAtUrls(object sender, UIDocumentPickedAtUrlsEventArgs args)
        {
            var control = (UIDocumentPickerViewController)sender;
            var count = args.Urls.Count();

            if (count == 0)
                Pvc_WasCancelled(sender, null);
            else
                Pvc_DidPickDocument(control, new UIDocumentPickedEventArgs(args.Urls[0]));

            control.Dispose();
        }

        private static void Pvc_WasCancelled(object sender, EventArgs e)
        {
            tcs.TrySetResult(null);
            tcs = null;
            pvc.Dispose();
            pvc = null;
        }

        /// <summary>
        /// Callback method called by document picker when file has been picked; this is called
        /// up to iOS 10.
        /// </summary>
        /// <param name="sender">sender object (document picker)</param>
        /// <param name="args">event args</param>
        private static void Pvc_DidPickDocument(object sender, UIDocumentPickedEventArgs args)
        {
            try
            {
                /*
                var securityEnabled = args.Url.StartAccessingSecurityScopedResource();
                var doc = new UIDocument(args.Url);

                string filename = doc.LocalizedName;
                string pathname = doc.FileUrl?.Path;

                args.Url.StopAccessingSecurityScopedResource();
                if (!string.IsNullOrWhiteSpace(pathname))
                {
                    // iCloud drive can return null for LocalizedName.
                    if (filename == null && pathname != null)
                        filename = System.IO.Path.GetFileName(pathname);

                    tcs.TrySetResult(new StorageFile(pathname));
                }
                else
                    tcs.TrySetResult(null);
                */
                tcs.TrySetResult(new StorageFile(args.Url));
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
            tcs = null;
            pvc.Dispose();
            pvc = null;
        }


    }
}
