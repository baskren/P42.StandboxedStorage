//-----------------------------------------------------------------------
// <copyright file="INativePickers.cs" company="42nd Parallel">
//     Copyright © 2020 42nd Parallel, LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------


using System.Collections.Generic;
using System.Threading.Tasks;

namespace P42.SandboxedStorage
{
    internal interface IFileSavePicker

    {
        bool CanPickSaveFile { get; }

        Task<IStorageFile> PickSaveFileAsync(string defaultExtension, IDictionary<string, IList<string>> FileTypeChoices);

        IDictionary<string, IList<string>> SaveFileTypeChoices { get; }
    }
}
