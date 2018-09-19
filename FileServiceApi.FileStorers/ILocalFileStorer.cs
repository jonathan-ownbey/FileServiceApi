using Microsoft.AspNetCore.Http;
using System.IO;

namespace FileServiceApi.FileStorers
{
    /// <summary>
    /// A file storer for local file storage.
    /// </summary>
    public interface ILocalFileStorer
    {
        /// <summary>
        /// Takes a filename and filepath and retrieves the file stored there.
        /// </summary>
        /// <param name="fileGuid">A string representation of a guid,
        /// which is the file name in storage.</param>
        /// <param name="filePath">A string representation of the path
        /// where the file is stored.</param>
        /// <returns>A FileStream containing the contents of the file.</returns>
        FileStream ReturnFile(string fileGuid, string filePath);
        /// <summary>
        /// Takes an IFormFile, file name, and file path and writes the file
        /// to local storage.
        /// </summary>
        /// <param name="file">An IFileForm to be stored locally.</param>
        /// <param name="fileGuid">A string representation of a guid,
        /// which will be the file name in storage.</param>
        /// <param name="filePath">A string representation of the path
        /// where the file will be stored.</param>
        bool WriteFile(IFormFile file, string fileGuid, string filePath);
        /// <summary>
        /// Takes a filename and filepath and deletes the file stored there.
        /// </summary>
        /// <param name="fileGuid">A string representation of a guid,
        /// which is the file name in storage.</param>
        /// <param name="filePath">A string representation of the path
        /// where the file is stored.</param>
        bool DeleteFile(string fileGuid, string filePath);
    }
}