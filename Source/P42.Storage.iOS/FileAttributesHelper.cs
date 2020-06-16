//-----------------------------------------------------------------------
// <copyright file="FileAttributesHelper.cs" company="In The Hand Ltd">
//     Copyright © 2016-17 In The Hand Ltd. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using Foundation;

namespace P42.Storage
{
    internal static class FileAttributesHelper
    {
        public static FileAttributes FromIOFileAttributes(System.IO.FileAttributes attrs)
        {
            FileAttributes outvalue = FileAttributes.Normal;

            if(attrs.HasFlag(System.IO.FileAttributes.ReadOnly))
                outvalue |= FileAttributes.ReadOnly;
            if (attrs.HasFlag(System.IO.FileAttributes.Directory))
                outvalue |= FileAttributes.Directory;
            if (attrs.HasFlag(System.IO.FileAttributes.Archive))
                outvalue |= FileAttributes.Archive;
            if (attrs.HasFlag(System.IO.FileAttributes.Temporary))
                outvalue |= FileAttributes.Temporary;

            return outvalue;
        }

    }
}