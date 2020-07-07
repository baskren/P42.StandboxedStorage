using System;
using System.Collections.Generic;
using Foundation;

namespace P42.SandboxedStorage.Native
{
    public static class BookmarkExtensions
    {

        public static (NSUrl NewUrl, NSData Bookmark) GetBookmark(this NSUrl url)
        {
            var bookmarksObj = NSUserDefaults.StandardUserDefaults.ValueForKey(new NSString("Bookmarks")) as NSArray;
            var nsBookmarks = bookmarksObj?.MutableCopy() as NSMutableArray ?? new NSMutableArray();
            var bookmarks = new List<NSData>();
            for (uint i = 0; i < nsBookmarks.Count; i++)
                bookmarks.Add(nsBookmarks.GetItem<NSData>(i));
            foreach (var bookmark in bookmarks)
            {
                var bookmarkUrl = NSUrl.FromBookmarkData(bookmark,
                    NSUrlBookmarkResolutionOptions.WithoutUI,
                    null,
                    out bool isStale,
                    out NSError error1
                    );
                if (bookmarkUrl != null && error1 == null)
                {
                    if (bookmarkUrl.Path == url.Path)
                    {
                        if (isStale)
                        {
                            if (url.CreateBookmark() is NSData newBookmark)
                            {
                                return (bookmarkUrl, newBookmark);
                            }
                            return (null,null);
                        }
                        return (bookmarkUrl, bookmark);
                    }
                }
                else
                {
                    if (error1 != null)
                        Console.WriteLine("Bookmark error: Bookmark for url [" + url + "] gave error [" + error1.Description + "] when trying to convert to URL.");
                    nsBookmarks.RemoveObject((nint)nsBookmarks.IndexOf(bookmark));
                    NSUserDefaults.StandardUserDefaults.SetValueForKey(nsBookmarks, new NSString("Bookmarks"));
                }
            }
            return (null, null);
        }

        public static NSData CreateBookmark(this NSUrl url)
        {
            var newBookmark = url.CreateBookmarkData((NSUrlBookmarkCreationOptions) 0, new string[] { }, null, out NSError error2);
            if (error2 != null)
            {
                Console.WriteLine("Can not get bookmark for url path [" + url.Path + "].");
                Console.WriteLine("ERROR: " + error2);
                return null;
            }
            var bookmarksObj = NSUserDefaults.StandardUserDefaults.ValueForKey(new NSString("Bookmarks")) as NSArray;
            var nsBookmarks = bookmarksObj?.MutableCopy() as NSMutableArray ?? new NSMutableArray();
            nsBookmarks.Add(newBookmark);
            NSUserDefaults.StandardUserDefaults.SetValueForKey(nsBookmarks, new NSString("Bookmarks"));
            return newBookmark;
        }

        public static (NSUrl NewUrl,NSData Bookmark) GetOrCreateBookmark(this NSUrl url)
        {
            var existingBookmark = url.GetBookmark();
            if (existingBookmark.Bookmark != null)
                return existingBookmark;
            return (url,url.CreateBookmark());
        }

    }
}
