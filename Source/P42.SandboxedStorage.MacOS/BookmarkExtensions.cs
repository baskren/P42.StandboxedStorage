using System;
using System.Collections.Generic;
using Foundation;

namespace P42.SandboxedStorage.Native
{
    public static class BookmarkExtensions
    {
        //public static NSUrl LastUrl;

        public static (NSUrl NewUrl, NSData Bookmark) GetBookmark(this NSUrl url)
        {
            var bookmarksObj = NSUserDefaults.StandardUserDefaults.ValueForKey(new NSString("Bookmarks")) as NSDictionary;
            var nsBookmarks = bookmarksObj?.MutableCopy() as NSMutableDictionary ?? new NSMutableDictionary();
            //var bookmarks = new List<NSData>();
            foreach (var key in nsBookmarks.Keys)
            //for (uint i = 0; i < nsBookmarks.Count; i++)
            //    bookmarks.Add(nsBookmarks.GetItem<NSData>(i));
            //foreach (var bookmark in bookmarks)
            {
                var bookmark = nsBookmarks[key] as NSData;
                var bookmarkUrl = NSUrl.FromBookmarkData(bookmark,
                    NSUrlBookmarkResolutionOptions.WithoutUI |
                    NSUrlBookmarkResolutionOptions.WithSecurityScope,
                    null,
                    out bool isStale,
                    out NSError error1
                    );
                if (bookmarkUrl != null && error1 == null)
                {
                    if (bookmarkUrl.Path.TrimEnd('/') == url.Path.TrimEnd('/'))
                    {
                        if (isStale)
                        {
                            if (url.CreateBookmark() is NSData freshBookmark)
                            {
                                //LastUrl = bookmarkUrl;
                                return (bookmarkUrl, freshBookmark);
                            }
                            return (null,null);
                        }
                        //LastUrl = bookmarkUrl;
                        return (bookmarkUrl, bookmark);
                    }
                }
                else
                {
                    if (error1 != null)
                        Console.WriteLine("Bookmark error: Bookmark for url [" + url + "] gave error [" + error1.Description + "] when trying to convert to URL.");
                    //nsBookmarks.RemoveObject((nint)nsBookmarks.IndexOf(bookmark));
                    nsBookmarks.Remove(key);
                    NSUserDefaults.StandardUserDefaults.SetValueForKey(nsBookmarks, new NSString("Bookmarks"));
                }
            }
            return (null, null);
        }

        public static NSData CreateBookmark(this NSUrl url)
        {
            //var newBookmark = url.CreateBookmarkData((NSUrlBookmarkCreationOptions) 0, new string[] { }, null, out NSError error2);
            var newBookmark = url.CreateBookmarkData(
                NSUrlBookmarkCreationOptions.WithSecurityScope
                //| NSUrlBookmarkCreationOptions.PreferFileIDResolution
                //| NSUrlBookmarkCreationOptions.MinimalBookmark (won't allow bookmark to be created)
                //| NSUrlBookmarkCreationOptions.SuitableForBookmarkFile  (won't allow bookmark to be created)
                , new string[] { }, null, out NSError error2);
            if (error2 != null)
            {
                Console.WriteLine("Can not get bookmark for url path [" + url.Path + "].");
                Console.WriteLine("ERROR: " + error2);
                return null;
            }
            var bookmarksObj = NSUserDefaults.StandardUserDefaults.ValueForKey(new NSString("Bookmarks")) as NSDictionary;
            var nsBookmarks = bookmarksObj?.MutableCopy() as NSMutableDictionary ?? new NSMutableDictionary();
            nsBookmarks[url.Path] = newBookmark;
            NSUserDefaults.StandardUserDefaults.SetValueForKey(nsBookmarks, new NSString("Bookmarks"));
            //LastUrl = url;
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
