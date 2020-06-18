using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P42.Storage.Native
{
    static class FilePicker
    {
        public static async Task<IStorageFile> PickSingleFileAsync(IList<string> fileTypes = null)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            if (fileTypes != null && fileTypes.Count > 0)
            {
                foreach (var fileType in fileTypes)
                    picker.FileTypeFilter.Add(fileType);
            }
            else
                picker.FileTypeFilter.Add("*");
            if (await picker.PickSingleFileAsync() is Windows.Storage.StorageFile windowsFile)
                return new StorageFile(windowsFile);

            return null;
        }

        public static async Task<IStorageFile> PickSaveFileAsync(string defaultFileExtension = null, IDictionary<string, IList<string>> fileTypeChoices = null)
        {
            var picker = new Windows.Storage.Pickers.FileSavePicker();
            if (fileTypeChoices != null && fileTypeChoices.Count > 0)
            {
                foreach (var kvp in fileTypeChoices)
                    picker.FileTypeChoices.Add(kvp);
            }
            //else
            //    picker.FileTypeChoices.Add()
            if (await picker.PickSaveFileAsync() is Windows.Storage.StorageFile windowsFile)
                return new StorageFile(windowsFile);

            return null;
        }
    }
}
