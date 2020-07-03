using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace P42.Storage
{
    public static class StorageFileExtensions
    {
        /// <summary>
        /// Gets a StorageFile object to represent the file at the specified path.
        /// </summary>
        /// <param name="path">The path of the file to get a StorageFile to represent.
        /// If your path uses slashes, make sure you use backslashes(\).
        /// Forward slashes(/) are not accepted by this method.</param>
        /// <returns>When this method completes, it returns the file as a StorageFile.</returns>
        public static Task<IStorageFile> GetFileFromPathAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException("path");

            return PlatformDelegate.GetFileFromPathAsync?.Invoke(path) ?? Task.FromResult<IStorageFile>(default);
        }

        /// <summary>
        /// Asynchronously append a list of lines to an IStorageFile
        /// </summary>
        /// <param name="storageFile"></param>
        /// <param name="lines"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task AppendAllLinesAsync(IStorageFile storageFile, IEnumerable<string> lines, System.Threading.CancellationToken cancellationToken = default)
            => storageFile?.AppendAllLinesAsync(lines, cancellationToken);

        public static Task AppendAllTextAsync(IStorageFile storageFile, string contents, System.Threading.CancellationToken cancellationToken = default)
            => storageFile?.AppendAllTextAsync(contents, cancellationToken);

        /// <summary>
        /// Asynchronously read all bytes (as a byte array) from an IStorageFile
        /// </summary>
        /// <param name="storageFile"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<byte[]> ReadAllBytesAsync(IStorageFile storageFile, System.Threading.CancellationToken cancellationToken = default)
            => storageFile?.ReadAllBytesAsync(cancellationToken);

        /// <summary>
        /// Asynchronously read all lines from an IStorageFile
        /// </summary>
        /// <param name="storageFile"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<string[]> ReadAllLinesAsync(IStorageFile storageFile, System.Threading.CancellationToken cancellationToken = default)
            => storageFile?.ReadAllLinesAsync(cancellationToken);

        /// <summary>
        /// Asynchronously read all text from an IStorageFile
        /// </summary>
        /// <param name="storageFile"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<string> ReadAllTextAsync(IStorageFile storageFile, System.Threading.CancellationToken cancellationToken = default)
            => storageFile?.ReadAllTextAsync(cancellationToken);

        /// <summary>
        /// Asynchronously write an array of bytes to an IStorageFile
        /// </summary>
        /// <param name="storageFile"></param>
        /// <param name="bytes"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task WriteAllBytesAsync(IStorageFile storageFile, byte[] bytes, System.Threading.CancellationToken cancellationToken = default)
            => storageFile?.WriteAllBytesAsync(bytes, cancellationToken);

        /// <summary>
        /// Asynchronously write a list of lines (strings) to an IStorageFile
        /// </summary>
        /// <param name="storageFile"></param>
        /// <param name="content"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task WriteAllLinesAsync(IStorageFile storageFile, IEnumerable<string> content, System.Threading.CancellationToken cancellationToken = default)
            => storageFile?.WriteAllLinesAsync(content, cancellationToken);

        /// <summary>
        /// Asynchronously write a string to an IStorageFile
        /// </summary>
        /// <param name="storageFile"></param>
        /// <param name="content"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task WriteAllTextAsync(IStorageFile storageFile, string content, System.Threading.CancellationToken cancellationToken = default)
            => storageFile?.WriteAllTextAsync(content, cancellationToken);

    }
}
