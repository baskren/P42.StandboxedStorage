using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using P42.Storage;
using Xamarin.Forms;

namespace StorageSample
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        async void FilePicker_Clicked(System.Object sender, System.EventArgs e)
        {
            if (await Pickers.PickSingleFileAsync() is IStorageFile storageFile)
            {
                await DisplayAlert("File Data", $@"Path {storageFile.Path}
ContentType: {storageFile.ContentType}
Attributes: {storageFile.Attributes}
FileType: {storageFile.FileType}
DateCreated:  {storageFile.DateCreated}
DateModified: {storageFile.DateModified}
Size: {storageFile.Size}
Name: {storageFile.Name}"
                   , "ok");
            }
                //System.Diagnostics.Debug.WriteLine("File: " + storageFile.Path);

        }

        async void FolderPicker_Clicked(System.Object sender, System.EventArgs e)
        {
            if (await Pickers.PickSingleFolderAsync() is IStorageFolder storageFolder)
            {
                if (await storageFolder.GetItemsAsync() is IReadOnlyList<IStorageItem> items)
                {
                    var paths = items.Select(i => i.Path);
                    await DisplayAlert("Folder Items", string.Join("\n", paths), "ok");
                }
            }
        }

        async void SaveFile_Clicked(System.Object sender, System.EventArgs e)
        {
            if (await Pickers.PickSaveFileAsync() is IStorageFile saveFile)
            {

            }
        }
    }
}
