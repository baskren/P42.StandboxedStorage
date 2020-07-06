﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using P42.SandboxedStorage;
using Windows.Storage;
using Windows.UI.Xaml;

namespace P42.SandboxedStorage.Native
{
    class StorageFile : StorageItem, IStorageFile
    {
        #region Properties
        public string ContentType => _file?.ContentType;

        public string FileType => _file?.FileType;

        internal Windows.Storage.StorageFile _file
        {
            get => _item as Windows.Storage.StorageFile;
            set => _item = value;
        }
        #endregion


        #region Construction
        public StorageFile(Windows.Storage.StorageFile file) : base(file) 
        {
        }
        #endregion


        #region Type Conversion Operators
        public static implicit operator StorageFile(Windows.Storage.StorageFile f)
            => f is null
                ? null
                : new StorageFile(f);
        

        public static implicit operator Windows.Storage.StorageFile(StorageFile f)
            => f._file;
        #endregion


        #region Methods
        public override bool IsOfType(StorageItemTypes type)
            => type == StorageItemTypes.File;

        internal async static Task<IStorageFile> GetFileFromPathAsync(string path)
        {
            await Task.Delay(5).ConfigureAwait(false);

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }
            try
            {
                if (await Windows.Storage.StorageFile.GetFileFromPathAsync(path) is Windows.Storage.StorageFile nativeStorageFile)
                    return new StorageFile(nativeStorageFile);
            }
            catch (Exception e)
            {
                // failed (could be permissions problem?)
            }
            return null;
        }

        public async Task CopyAndReplaceAsync(IStorageFile iStorageFileToReplace)
        {
            await Task.Delay(5).ConfigureAwait(false);

            if (_file != null && 
                iStorageFileToReplace is StorageFile storageFileToReplace && 
                storageFileToReplace._file is Windows.Storage.StorageFile fileToReplace)
            {
                await _file.CopyAndReplaceAsync(fileToReplace);
            }
        }

        public async Task<IStorageFile> CopyAsync(IStorageFolder iDestinationFolder)
        {
            await Task.Delay(5).ConfigureAwait(false);

            if (_file != null &&
                iDestinationFolder is StorageFolder destinationFolder &&
                destinationFolder?._folder is Windows.Storage.StorageFolder windowsDestinationFolder &&
                await _file.CopyAsync(windowsDestinationFolder) is Windows.Storage.StorageFile newFile)
                return new StorageFile(newFile);
            return null;
        }

        public async Task<IStorageFile> CopyAsync(IStorageFolder iDestinationFolder, string desiredNewName)
        {
            await Task.Delay(5).ConfigureAwait(false);

            if (_file != null &&
                iDestinationFolder is StorageFolder destinationFolder &&
                destinationFolder?._folder is Windows.Storage.StorageFolder windowsDestinationFolder &&
                !string.IsNullOrWhiteSpace(desiredNewName) &&
                await _file.CopyAsync(windowsDestinationFolder, desiredNewName) is Windows.Storage.StorageFile newFile)
                return new StorageFile(newFile);
            return null;
        }

        public override async Task<IStorageFolder> GetParentAsync()
        {
            await Task.Delay(5).ConfigureAwait(false);

            if (_file is null)
                return null;
            if (_file != null && await _file.GetParentAsync() is Windows.Storage.StorageFolder folder)
            {
                return new StorageFolder(folder);
            }
            return null;
        }

        public async Task MoveAndReplaceAsync(IStorageFile iFileToReplace)
        {
            await Task.Delay(5).ConfigureAwait(false);

            if (_file != null &&
                iFileToReplace is StorageFile fileToReplace &&
                fileToReplace._file is Windows.Storage.StorageFile windowsFileToReplace)
                await _file.MoveAndReplaceAsync(windowsFileToReplace);
        }

        public async Task MoveAsync(IStorageFolder iDestinationFolder)
        {
            await Task.Delay(5).ConfigureAwait(false);

            if (_file != null &&
                iDestinationFolder is StorageFolder destinationFolder &&
                destinationFolder._folder is Windows.Storage.StorageFolder windowsDestintationFolder)
                await _file.MoveAsync(windowsDestintationFolder);
        }

        public async Task MoveAsync(IStorageFolder iDestinationFolder, string desiredNewName)
        {
            await Task.Delay(5).ConfigureAwait(false);

            if (_file != null &&
                !string.IsNullOrWhiteSpace(desiredNewName) &&
                iDestinationFolder is StorageFolder destinationFolder &&
                destinationFolder._folder is Windows.Storage.StorageFolder windowsDestintationFolder)
                await _file.MoveAsync(windowsDestintationFolder, desiredNewName);

        }

        public async Task RenameAsync(string desiredName)
        {
            await Task.Delay(5).ConfigureAwait(false);

            if (_file != null && !string.IsNullOrWhiteSpace(desiredName))
                await _file.RenameAsync(desiredName);
        }

        public async Task RenameAsync(string desiredName, NameCollisionOption option)
        {
            await Task.Delay(5).ConfigureAwait(false);

            if (_file != null && !string.IsNullOrWhiteSpace(desiredName))
                await _file.RenameAsync(desiredName, (Windows.Storage.NameCollisionOption)option);
        }
        #endregion


        #region System.IO.File methods
        public async Task AppendAllLinesAsync(IEnumerable<string> lines, System.Threading.CancellationToken cancellationToken = default)
        {
            await Task.Delay(5).ConfigureAwait(false);

            await Windows.Storage.FileIO.AppendLinesAsync(_file, lines, Windows.Storage.Streams.UnicodeEncoding.Utf8).AsTask(cancellationToken);
        }

        public async Task AppendAllTextAsync(string contents, System.Threading.CancellationToken cancellationToken = default)
        {
            await Task.Delay(5).ConfigureAwait(false);

            await Windows.Storage.FileIO.AppendTextAsync(_file, contents, Windows.Storage.Streams.UnicodeEncoding.Utf8).AsTask(cancellationToken);
        }

        public async Task<byte[]> ReadAllBytesAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            await Task.Delay(5).ConfigureAwait(false);

            var buffer = await Windows.Storage.FileIO.ReadBufferAsync(_file);
            return buffer?.ToArray();
        }

        public async Task<string[]> ReadAllLinesAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            await Task.Delay(5).ConfigureAwait(false);

            var result = await Windows.Storage.FileIO.ReadLinesAsync(_file, Windows.Storage.Streams.UnicodeEncoding.Utf8).AsTask(cancellationToken);
            return result.ToArray();
        }

        public async Task<string> ReadAllTextAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            await Task.Delay(5).ConfigureAwait(false);

            var text = await Windows.Storage.FileIO.ReadTextAsync(_file, Windows.Storage.Streams.UnicodeEncoding.Utf8).AsTask(cancellationToken);
            //var text = await Windows.Storage.FileIO.ReadTextAsync(_file.Path, Windows.Storage.Streams.UnicodeEncoding.Utf8).AsTask(cancellationToken);
            return text;
        }

        public async Task WriteAllBytesAsync(byte[] bytes, System.Threading.CancellationToken cancellationToken = default)
        {
            await Task.Delay(5).ConfigureAwait(false);

            await Windows.Storage.FileIO.WriteBytesAsync(_file, bytes).AsTask(cancellationToken);
        }

        public async Task WriteAllLinesAsync(IEnumerable<string> lines, System.Threading.CancellationToken cancellationToken = default)
        {
            await Task.Delay(5).ConfigureAwait(false);

            await Windows.Storage.FileIO.WriteLinesAsync(_file, lines, Windows.Storage.Streams.UnicodeEncoding.Utf8).AsTask(cancellationToken);
        }

        public async Task WriteAllTextAsync(string content, System.Threading.CancellationToken cancellationToken = default)
        {
            await Task.Delay(5).ConfigureAwait(false);

            await Windows.Storage.FileIO.WriteTextAsync(_file, content, Windows.Storage.Streams.UnicodeEncoding.Utf8).AsTask(cancellationToken);
        }

        #endregion



    }
}