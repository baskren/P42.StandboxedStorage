//-----------------------------------------------------------------------
// <copyright file="Platform.cs" company="42nd Parallel">
//     Copyright © 2020 42nd Parallel, LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace P42.Storage
{
    internal static class PlatformDelegate
    {
        public static Func<string, Task<IStorageFile>> GetFileFromPathAsync;

        public static Func<string, Task<IStorageFolder>> GetFolderFromPathAsync;

        public static Func<IList<string>, Task<IStorageFile>> PickFileAsync;

        public static Func<Task<IStorageFolder>> PickFolderAsync;

        public static Func<string, IDictionary<string, IList<string>>, Task<IStorageFile>> PickSaveAsFileAsync;

        public static bool Initiated;

        internal static void AssessInitiated()
        {
            if (!Initiated)
            {
                var text = "P42.Storage not initiated.  ";
                if (Xamarin.Essentials.DeviceInfo.Platform == Xamarin.Essentials.DevicePlatform.iOS)
                    throw new Exception(text + "Call P42.Storage.iOS.Platform.Init() in your iOS project's AppDelegate.FinishedLaunching method.");
                if (Xamarin.Essentials.DeviceInfo.Platform == Xamarin.Essentials.DevicePlatform.Android)
                    throw new Exception(text + "Call P42.Storage.Android.Platform.Init() in your iOS project's MainActivity.OnCreate method.");
                throw new PlatformNotSupportedException();
            }
        }
    }
}
