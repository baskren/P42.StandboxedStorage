using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using P42.SandboxedStorage;
using Xamarin.Forms;

namespace SandboxedStorageSample
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        const string testText = "The quick brown fox jumped over the lazy dog.";
        const string appendedText = "\nAPPENDED TEXT";
        const string textFileName1 = "textFile.txt";
        const string textFileName2 = "renamedTextFile.txt";

        const string contentFolderPathKey = "ContentFolderPathKey";
        const string textFilePathKey = "TextFilePathKey";

        public MainPage()
        {
            InitializeComponent();
        }



        protected override void OnAppearing()
        {
            base.OnAppearing();

            Device.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(1000);
                await GetExistingTextfile();
                await GetExistingFolder();
            });
        }

        async Task GetExistingFolder()
        {
            //if (Xamarin.Essentials.Preferences.Get(contentFolderPathKey, null) is string contentFolderPath)
            if (Preferences.Get(contentFolderPathKey, null) is string contentFolderPath)
            {
                if (await StorageFolderExtensions.GetFolderFromPathAsync(contentFolderPath) is IStorageFolder folder)
                    ContentFolderPicker.StorageFolder = folder;
                else
                    await DisplayError("Could not get folder from cached path [" + contentFolderPath + "]");
            }
        }

        async Task GetExistingTextfile()
        {
            //if (Xamarin.Essentials.Preferences.Get(textFilePathKey, null) is string textFilePath)
            if (Preferences.Get(textFilePathKey, null) is string textFilePath)
            {
                if (await StorageFileExtensions.GetFileFromPathAsync(textFilePath) is IStorageFile textFile)
                    TextFilePicker.StorageFile = textFile;
                else
                    await DisplayError("Could not get file from cached path [" + textFilePath + "]");
            }
        }

        async void FolderPicker_StorageFolderChanged(System.Object sender, P42.SandboxedStorage.StorageFolderChangedEventArgs e)
        {
            ShowSpinner();
            if (e.StorageFolder != null)
            {
                await DisplayFolderContents(e.StorageFolder);

                var results = "";
                var test = "e.StorageFolder.GetOrCreateFolderAsync(\"testFiles\")";
                if (await e.StorageFolder.GetOrCreateFolderAsync("testFiles") is IStorageFolder testFilesFolder)
                {
                    results += test + ": OK\n";
                    test = "testFilesFolder.GetOrCreateFileAsync(textFileName1)";
                    if (await testFilesFolder.GetOrCreateFileAsync(textFileName1) is IStorageFile textFile)
                    {
                        results += test + ": OK\n";
                        await textFile.WriteAllTextAsync(testText);
                        results += "textFile.WriteAllTextAsync(testText): OK";
                        test = "textFile.ReadAllTextAsync()";
                        if (await textFile.ReadAllTextAsync() is string contentText)
                        {
                            results += test + ": OK\n";
                            test = "contentText == testText";
                            if (contentText == testText)
                            {
                                results += test + ": OK\n";
                                await textFile.AppendAllTextAsync(appendedText);
                                results += "textFile.AppendAllTextAsync(appendedText): OK";
                                test = "textFile.ReadAllTextAsync()";
                                if (await textFile.ReadAllTextAsync() is string contentText1)
                                {
                                    results += test + ": OK\n";
                                    test = "contentText1 == testText";
                                    if (contentText1 == testText + appendedText)
                                    {
                                        results += test + ": OK\n";
                                        test = "textFile.RenameAsync(textFileName2)";
                                        await textFile.RenameAsync(textFileName2);
                                        results += test + ": OK\n";
                                        if (textFile.Name == textFileName2)
                                        {
                                            results += test + ": OK\n";
                                            test = "textFile.ReadAllTextAsync()";
                                            if (await textFile.ReadAllTextAsync() is string contentText2)
                                            {
                                                results += test + ": OK\n";
                                                test = "contentText2 == testText";
                                                if (contentText2 == testText + appendedText)
                                                {
                                                    results += test + ": OK\n";
                                                    test = "textFile.MoveAsync(e.StorageFolder)";
                                                    await textFile.MoveAsync(e.StorageFolder);
                                                    results += test + ": OK\n";
                                                    test += "textFile.GetParentAsync()";
                                                    if (await textFile.GetParentAsync() is IStorageFolder parentFolder)
                                                    {
                                                        results += test + ": OK\n";
                                                        test = "parentFolder.Path == e.StorageFolder.Path";
                                                        if (parentFolder.Path == e.StorageFolder.Path)
                                                        {
                                                            results += test + ": OK\n";
                                                            test = "parentFolder.IsEqual(e.StorageFolder)";
                                                            if (parentFolder.IsEqual(e.StorageFolder))
                                                            {
                                                                results += test + ": OK\n";
                                                                test = "StorageFolderExtensions.FileExists(e.StorageFolder,textFileName2)";
                                                                if (await StorageFolderExtensions.FileExists(e.StorageFolder, textFileName2))
                                                                {
                                                                    results += test + ": OK\n";
                                                                    test += "!await StorageFolderExtensions.FileExists(testFilesFolder, textFileName2)";
                                                                    if (!await StorageFolderExtensions.FileExists(testFilesFolder, textFileName2))
                                                                    {
                                                                        results += test + ": OK\n";
                                                                        //TODO: Add test to try to delete a folder that still contains files.  Need to see what happens with UWP!
                                                                        test += "await testFilesFolder.DeleteAsync();";
                                                                        await testFilesFolder.DeleteAsync();
                                                                        results += test + ": OK\n";
                                                                        test += "!await StorageFolderExtensions.FolderExists(e.StorageFolder, testFilesFolder.Name)";
                                                                        if (!await StorageFolderExtensions.FolderExists(e.StorageFolder, testFilesFolder.Name))
                                                                        {
                                                                            results += test + ": OK\n";
                                                                            test = "textFile.DeleteAsync()";
                                                                            await textFile.DeleteAsync();
                                                                            results += test + ": OK\n";
                                                                            test = "!await StorageFolderExtensions.FileExists(e.StorageFolder, textFileName2)";
                                                                            if (!await StorageFolderExtensions.FileExists(e.StorageFolder, textFileName2))
                                                                            {
                                                                                results += test + ": OK\n";
                                                                                await DisplayAlert("Success", results, "OK");
                                                                                HideSpinner();
                                                                                return;
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                await DisplayError(test + ": FAIL");
            }
            HideSpinner();
        }

        async void ContentFolderPicker_StorageFolderChanged(System.Object sender, P42.SandboxedStorage.StorageFolderChangedEventArgs e)
        {
            if (e.StorageFolder != null)
            {
                //Xamarin.Essentials.Preferences.Set(contentFolderPathKey, e.StorageFolder.Path);
                Preferences.Set(contentFolderPathKey, e.StorageFolder.Path);
                await DisplayFolderContents(e.StorageFolder);
            }
            else
                //Xamarin.Essentials.Preferences.Clear(contentFolderPathKey);
                Preferences.Clear(contentFolderPathKey);
        }

        async void TextFilePicker_StorageFileChanged(System.Object sender, P42.SandboxedStorage.StorageFileChangedEventArgs e)
        {
            if (e.StorageFile != null)
            {
                //Xamarin.Essentials.Preferences.Set(textFilePathKey, e.StorageFile.Path);
                Preferences.Set(textFilePathKey, e.StorageFile.Path);
                await DisplaySomeTextFileContents(e.StorageFile);
            }
            else
                //Xamarin.Essentials.Preferences.Set(textFilePathKey, e.StorageFile.Path);
                Preferences.Set(textFilePathKey, e.StorageFile.Path);
        }

        async Task DisplayFolderContents(IStorageFolder folder)
        {
            if (folder is null)
                return;
            ShowSpinner();
            if (await folder.GetItemsAsync() is IReadOnlyList<IStorageItem> items)
            {
                var result = $@"Path {folder.Path}
Attributes: {folder.Attributes}
DateCreated:  {folder.DateCreated}
DateModified: {folder.DateModified}
Size: {folder.Size}
Name: {folder.Name}

CONTENT:
";
                foreach (var item in items)
                {
                    result += item.Path;
                    if (item is IStorageFolder)
                        result += "/";
                    result += "\n";
                }
                await DisplayAlert("Folder Data:", result, "ok");
            }
            else
                await DisplayError("Failed to GetItemsAsync for folder [" + folder?.Path + "]");
            HideSpinner();
        }

        async Task DisplaySomeTextFileContents(IStorageFile textFile)
        {
            if (textFile is null)
                return;
            ShowSpinner();
            if (await textFile.ReadAllTextAsync() is string text)
            {
                var result = $@"Path {textFile.Path}
ContentType: {textFile.ContentType}
Attributes: {textFile.Attributes}
FileType: {textFile.FileType}
DateCreated:  {textFile.DateCreated}
DateModified: {textFile.DateModified}
Size: {textFile.Size}
Name: {textFile.Name}

CONTENT:
";
                await DisplayAlert("Text File Data", result + text.Substring(0, Math.Min(text.Length,200)) + " ...", "ok");
            }
            else
                await DisplayError("Failed to get text from file [" + textFile?.Path + "]");
            HideSpinner();
        }

        async Task DisplayError(string message)
        {
            await DisplayAlert("Error", message, "ok");
        }

        ContentView spinnerContentView;
        void ShowSpinner()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (spinnerContentView != null)
                    return;

                if (this.Content is Grid grid)
                {
                    var rows = grid.RowDefinitions.Count;
                    var columns = grid.ColumnDefinitions.Count;

                    var spinner = new Xamarin.Forms.ActivityIndicator
                    {
                        IsEnabled = true,
                        IsRunning = true,
                        Color = Color.White,
                        HorizontalOptions = LayoutOptions.Center,
                        VerticalOptions = LayoutOptions.Center,
                    };

                    spinnerContentView = new ContentView
                    {
                        BackgroundColor = Color.FromRgba(0, 0, 0, 0.5),
                        HorizontalOptions = LayoutOptions.Fill,
                        VerticalOptions = LayoutOptions.Fill,
                        Content = spinner
                    };

                    if (rows > 0)
                        Grid.SetRowSpan(spinnerContentView, rows);
                    if (columns > 0)
                        Grid.SetColumnSpan(spinnerContentView, columns);
                    grid.Children.Add(spinnerContentView);
                }
            });
        }

        void HideSpinner()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (this.Content is Grid grid && spinnerContentView != null)
                {
                    grid.Children.Remove(spinnerContentView);
                    spinnerContentView = null;
                }
            });
        }
    }
}
