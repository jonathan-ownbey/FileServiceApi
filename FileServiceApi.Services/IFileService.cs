using FileServiceApi.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FileServiceApi.Services
{
    /// <summary>
    /// Service which handles the files between storage and API endpoints.
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// Takes a list of IFormFile. Calls to create a string representation of a guid
        /// for each file and writes that as well as other file-related data to an instance
        /// of FileMetaData. Calls the minio service to upload the files, then writes the
        /// metadata to a collection in MongoDB.
        /// </summary>
        /// <param name="filesIn">A list of IFormFile which contains the files to be uploaded.</param>
        /// <returns>A string representation of the GUID which is the filename in storage</returns>
        Task<List<string>> StoreFiles(List<IFormFile> filesIn);
        /// <summary>
        /// Calls the minio service to retrieve a file with a string representation
        /// of guid as its name, then returns a Task of type Stream containing the
        /// file data.
        /// </summary>
        /// <param name="fileGuid">A string representation of a guid which is the filename in storage.</param>
        /// <returns>A Task of type Stream containing the file data.</returns>
        Task<Stream> GetFile(string fileGuid);
        /// <summary>
        /// Makes a call to the minio service to delete the file from storage.
        /// </summary>
        /// <param name="fileGuid">A string representation of a guid which is the filename in storage.</param>
        Task<bool> DeleteFile(string fileGuid);
        /// <summary>
        /// Makes a call to the MongoDB service to get data associated with the given file id.
        /// </summary>
        /// <param name="fileGuids">A string representation of a guid which is the filename in storage.</param>
        /// <returns>An instance of FileMetaData which contains information about the uploaded file.</returns>
        Task<List<FileMetaData>> GetFileMetaData(List<string> fileGuids);
        /// <summary>
        /// Makes a call to the MongoDB service to get data associated with all files in storage.
        /// </summary>
        /// <returns>Returns a List of type FileMetaData which contains information about a file.</returns>
        Task<List<FileMetaData>> GetAllFileMetaData();
        /// <summary>
        /// Gets a list of allowed/whitelisted file content-types and extensions
        /// </summary>
        /// <returns>A list of FileContentType which contains the content-types and extensions</returns>
        List<FileContentType> GetAllowedFileTypes();
        /// <summary>
        /// Gets the number of files previously uploaded.
        /// </summary>
        /// <returns>An int which represents the count of files uploaded.</returns>
        Task<int> GetNumberOfFilesInStorage();
    }
}