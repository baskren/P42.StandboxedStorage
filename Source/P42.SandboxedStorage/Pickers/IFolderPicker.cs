//-----------------------------------------------------------------------
// <copyright file="INativePickers.cs" company="42nd Parallel">
//     Copyright © 2020 42nd Parallel, LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------


using System.Collections.Generic;
using System.Threading.Tasks;

namespace P42.SandboxedStorage
{
    internal interface IFolderPicker
    {
        bool CanPickSingleFolder { get; }

        Task<IStorageFolder> PickSingleFolderAsync();
    }
}
