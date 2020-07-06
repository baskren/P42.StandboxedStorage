using AppKit;
using Foundation;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;
using SandboxedStorageSample;

namespace SandboxedStorageSample.MacOS
{
    [Register("AppDelegate")]
    public class AppDelegate : FormsApplicationDelegate
    {
        NSWindow window;
        public override NSWindow MainWindow => window;

        public AppDelegate()
        {
            var style = NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Titled;

            var rect = new CoreGraphics.CGRect(200, 1000, 1024, 768);
            window = new NSWindow(rect, style, NSBackingStore.Buffered, false);
            window.Title = "SandboxedStorageSample"; 
            //window.TitleVisibility = NSWindowTitleVisibility.Hidden;
            window.TitleVisibility = NSWindowTitleVisibility.Visible;

        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            P42.Utils.MacOS.Settings.Init();
            Forms.Init();
            P42.SandboxedStorage.Platform.Init();
            LoadApplication(new App());
            base.DidFinishLaunching(notification);
        }

        public override void WillTerminate(NSNotification notification)
        {
            // Insert code here to tear down your application
        }

    }
}
