using System;
using Foundation;

namespace P42.SandboxedStorage.Native
{
    public static class MainThread
    {
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
