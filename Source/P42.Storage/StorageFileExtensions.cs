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

        public static void AppendAllLines(IStorageFile storageFile, IEnumerable<string> lines)
            => storageFile?.AppendAllLines(lines);

        public static Task AppendAllLinesAsync(IStorageFile storageFile, IEnumerable<string> lines, System.Threading.CancellationToken cancellationToken = default)
            => storageFile?.AppendAllLinesAsync(lines, cancellationToken);

        public static void AppendAllText(IStorageFile storageFile, string contents)
            => storageFile?.AppendAllText(contents);

        public static Task AppendAllTextAsync(IStorageFile storageFile, string contents, System.Threading.CancellationToken cancellationToken = default)
            => storageFile?.AppendAllTextAsync(contents, cancellationToken);

        public static System.IO.FileStream Open(IStorageFile storageFile, System.IO.FileMode mode, System.IO.FileAccess access = System.IO.FileAccess.ReadWrite, System.IO.FileShare share = System.IO.FileShare.None)
            => storageFile?.Open(mode, access, share);

        public static System.IO.FileStream OpenRead(IStorageFile storageFile)
            => storageFile?.OpenRead();

        public static System.IO.StreamReader OpenText(IStorageFile storageFile)
            => storageFile?.OpenText();

        public static System.IO.FileStream OpenWrite(IStorageFile storageFile)
            => storageFile?.OpenWrite();

        public static byte[] ReadAllBytes(IStorageFile storageFile)
            => storageFile?.ReadAllBytes();

        public static Task<byte[]> ReadAllBytesAsync(IStorageFile storageFile, System.Threading.CancellationToken cancellationToken = default)
            => storageFile?.ReadAllBytesAsync(cancellationToken);

        public static string[] ReadAllLines(IStorageFile storageFile)
            => storageFile?.ReadAllLines();

        public static Task<string[]> ReadAllLinesAsync(IStorageFile storageFile, System.Threading.CancellationToken cancellationToken = default)
            => storageFile?.ReadAllLinesAsync(cancellationToken);

        public static string ReadAllText(IStorageFile storageFile)
            => storageFile?.ReadAllText();

        public static Task<string> ReadAllTextAsync(IStorageFile storageFile, System.Threading.CancellationToken cancellationToken = default)
            => storageFile?.ReadAllTextAsync(cancellationToken);

        public static void WriteAllBytes(IStorageFile storageFile, byte[] bytes)
            => storageFile?.WriteAllBytes(bytes);

        public static Task WriteAllBytesAsync(IStorageFile storageFile, byte[] bytes, System.Threading.CancellationToken cancellationToken = default)
            => storageFile?.WriteAllBytesAsync(bytes, cancellationToken);

        public static void WriteAllLines(IStorageFile storageFile, IEnumerable<string> content)
            => storageFile?.WriteAllLines(content);

        public static Task WriteAllLinesAsync(IStorageFile storageFile, IEnumerable<string> content, System.Threading.CancellationToken cancellationToken = default)
            => storageFile?.WriteAllLinesAsync(content, cancellationToken);

        public static void WriteAllText(IStorageFile storageFile, string content)
            => storageFile?.WriteAllText(content);

        public static Task WriteAllTextAsync(IStorageFile storageFile, string content, System.Threading.CancellationToken cancellationToken = default)
            => storageFile?.WriteAllTextAsync(content, cancellationToken);

    }
}
