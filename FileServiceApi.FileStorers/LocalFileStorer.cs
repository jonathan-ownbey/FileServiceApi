using Microsoft.AspNetCore.Http;
using Serilog;
using System.IO;

namespace FileServiceApi.FileStorers
{
    /// <inheritdoc />
    /// <summary>
    /// A file storer for local file storage.
    /// </summary>
    public sealed class LocalFileStorer : ILocalFileStorer
    {
        private LocalFileStorer()
        { }

        public static LocalFileStorer Instance => Nested.instance;

        private class Nested
        {
            // Explicit static constructor to tell compiler not 
            // to mark type as beforefieldinit
            static Nested()
            { }

            internal static readonly LocalFileStorer instance = new LocalFileStorer();
        }

        /// <inheritdoc />
        /// <summary>
        /// Takes an IFormFile, file name, and file path and writes the file
        /// to local storage.
        /// </summary>
        /// <param name="file">An IFileForm to be stored locally.</param>
        /// <param name="fileGuid">A string representation of a guid,
        /// which will be the file name in storage.</param>
        /// <param name="filePath">A string representation of the path
        /// where the file will be stored.</param>
        public bool WriteFile(IFormFile file, string fileGuid, string filePath)
        {
            if (File.Exists(filePath + fileGuid))
            {
                var exception = new IOException("File already exists"); //TODO: to throw or not to throw?
                Log.Error(exception, $"Cannot save file: {fileGuid}, because it already exists in storage location.");
                return false;
            }

            using (var fileStream = new FileStream(filePath + fileGuid, FileMode.OpenOrCreate))
            {
                Log.Information($"Saving file: {fileGuid} to local storage.");
                file.CopyTo(fileStream);
                return true;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Takes a filename and filepath and retrieves the file stored there.
        /// </summary>
        /// <param name="fileGuid">A string representation of a guid,
        /// which is the file name in storage.</param>
        /// <param name="filePath">A string representation of the path
        /// where the file is stored.</param>
        /// <returns>A FileStream containing the contents of the file.</returns>
        public FileStream ReturnFile(string fileGuid, string filePath)
        {
            Log.Information($"Retrieving file: {fileGuid} from local storage.");
            return new FileStream(filePath + fileGuid, FileMode.Open);
        }

        /// <inheritdoc />
        /// <summary>
        /// Takes a filename and filepath and deletes the file stored there.
        /// </summary>
        /// <param name="fileGuid">A string representation of a guid,
        /// which is the file name in storage.</param>
        /// <param name="filePath">A string representation of the path
        /// where the file is stored.</param>
        public bool DeleteFile(string fileGuid, string filePath)
        {
            Log.Information($"Deleting file: {fileGuid} from local storage.");
            File.Delete(filePath + fileGuid);
            return true;
        }
    }
}