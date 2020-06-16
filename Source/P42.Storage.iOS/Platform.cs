using System;

namespace P42.Storage
{
    public static class Platform
    {
        public static void Init()
        {
            
            PlatformDelegate.GetFileFromPathAsync =  Native.StorageFile.GetFileFromPathAsync;

            PlatformDelegate.GetFolderFromPathAsync = Native.StorageFolder.GetFolderFromPathAsync;

            PlatformDelegate.PickFileAsync = Native.FilePicker.PickSingleFileAsync;

            PlatformDelegate.PickFolderAsync = Native.FolderPicker.PickSingleFolderAsync;

            PlatformDelegate.Initiated = true;
        }

    }
}
