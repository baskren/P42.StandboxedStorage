//-----------------------------------------------------------------------
// <copyright file="INativePickers.cs" company="42nd Parallel">
//     Copyright © 2020 42nd Parallel, LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------


using System.Collections.Generic;
using System.Threading.Tasks;

namespace P42.Storage
{
    internal interface IFilePicker
    {
        bool CanPickSingleFile { get; }

        Task<IStorageFile> PickSingleFileAsync(IList<string> FileTypeFilter);

        IList<string> FileTypeFilter { get;  }
    }
}
