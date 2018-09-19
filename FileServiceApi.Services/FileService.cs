using FileServiceApi.Data;
using FileServiceApi.FileStorers;
using FileServiceApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FileServiceApi.Services
{
    public class FileService : IFileService
    {
        private readonly IOptions<ConfigurationSettings> _configurationSettings;
        private readonly ILocalFileStorer _localFileStorer;
        private readonly IMinioFileStorer _minioFileStorer;
        private readonly IMongoDbService _mongoDbService;

        public FileService(ILocalFileStorer localFileStorer, IMinioFileStorer minioFileStorer, IMongoDbService mongoDbService,
            IOptions<ConfigurationSettings> configurationSettings)
        {
            _localFileStorer = localFileStorer;
            _minioFileStorer = minioFileStorer;
            _mongoDbService = mongoDbService;
            _configurationSettings = configurationSettings;
        }
        /// <inheritdoc />
        /// <summary>
        /// Takes a list of IFormFile. Calls to create a string representation of a guid
        /// for each file and writes that as well as other file-related data to an instance
        /// of FileMetaData. Calls the minio service to upload the files, then writes the
        /// metadata to a collection in MongoDB.
        /// </summary>
        /// <param name="filesIn">A list of IFormFile which contains the files to be uploaded.</param>
        /// <returns>A string representation of the GUID which is the filename in storage</returns>
        public async Task<List<string>> StoreFiles(List<IFormFile> filesIn)
        {
            var success = false;
            var fileMetaDatas = new List<FileMetaData>();
            try
            {
                foreach (var formFile in filesIn)
                {
                    if (formFile.Length <= 0) continue;

                    Log.Information($"Uploading file: {formFile.FileName}.");

                    var fileMetaData = new FileMetaData
                    {
                        FileName = formFile.FileName,
                        FileType = formFile.ContentType,
                        FileGuid = CreateFileGuid(),
                        UploadDate = DateTime.Now
                    };

                    fileMetaDatas.Add(fileMetaData);

                    if (_configurationSettings.Value.ServiceType == "default")
                    {
                        //Use the LocalFileStorer to write to local disk
                        success = _localFileStorer.WriteFile(formFile, fileMetaData.FileGuid, _configurationSettings.Value.LocalStoragePath);
                    }
                    else
                    {
                        success = await _minioFileStorer.UploadFile(fileMetaData.FileGuid, formFile.OpenReadStream(), fileMetaData.FileType);
                    }
                }

                if (!success) return null;
                {
                    _mongoDbService.InsertFileDatas(fileMetaDatas);

                    var returnGuids = new List<string>();
                    foreach (var fileMetaData in fileMetaDatas)
                    {
                        returnGuids.Add(fileMetaData.FileGuid);
                    }

                    return returnGuids;
                }

            }
            catch (Exception exception)
            {
                Log.Error(exception, "Failed to store file externally.");
                return null;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Calls the minio service to retrieve a file with a string representation
        /// of guid as its name, then returns a Task of type Stream containing the
        /// file data.
        /// </summary>
        /// <param name="fileGuid">A string representation of a guid which is the filename in storage.</param>
        /// <returns>A Task of type Stream containing the file data.</returns>
        public async Task<Stream> GetFile(string fileGuid)
        {
            if (_configurationSettings.Value.ServiceType == "default")
            {
                return _localFileStorer.ReturnFile(fileGuid, _configurationSettings.Value.LocalStoragePath);
            }

            return await _minioFileStorer.RetrieveFile(fileGuid);
        }
        /// <inheritdoc />
        /// <summary>
        /// Makes a call to the minio service to delete the file from storage.
        /// </summary>
        /// <param name="fileGuid">A string representation of a guid which is the filename in storage.</param>
        public async Task<bool> DeleteFile(string fileGuid)
        {
            var success = false;

            if (_configurationSettings.Value.ServiceType == "default")
            {
                if (_localFileStorer.DeleteFile(fileGuid, _configurationSettings.Value.LocalStoragePath))
                    return true;
            }
            else
            {
                if (await _minioFileStorer.DeleteFile(fileGuid))
                {
                    var fileGuids = new List<string> { fileGuid };
                    if (await _mongoDbService.DeleteFileMetaDatas(fileGuids))
                        success = true;
                }
            }

            return success;
        }
        /// <inheritdoc />
        /// <summary>
        /// Makes a call to the MongoDB service to get data associated with the given file id.
        /// </summary>
        /// <param name="fileGuids">A string representation of a guid which is the filename in storage.</param>
        /// <returns>An instance of FileMetaData which contains information about the uploaded file.</returns>
        public async Task<List<FileMetaData>> GetFileMetaData(List<string> fileGuids)
        {
            return await _mongoDbService.GetFileMetaDatas(fileGuids);
        }
        /// <inheritdoc />
        /// <summary>
        /// Makes a call to the MongoDB service to get data associated with all files in storage.
        /// </summary>
        /// <returns>Returns a List of type FileMetaData which contains information about a file.</returns>
        public async Task<List<FileMetaData>> GetAllFileMetaData()
        {
            return await _mongoDbService.GetAllFileMetaDatas();
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets a list of allowed/whitelisted file content-types and extensions
        /// </summary>
        /// <returns>A list of FileContentType which contains the content-types and extensions</returns>
        public List<FileContentType> GetAllowedFileTypes()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            List<FileContentType> contentTypes;

            try
            {
                using (var streamReader = new StreamReader($"{currentDirectory}/{_configurationSettings.Value.WhitelistFile}"))
                {
                    var json = streamReader.ReadToEnd();
                    contentTypes = JsonConvert.DeserializeObject<List<FileContentType>>(json);
                }
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Failed to get list of content-types for whitelist.");
                return null;
            }

            return contentTypes;
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets the number of files previously uploaded.
        /// </summary>
        /// <returns>An int which represents the count of files uploaded.</returns>
        public async Task<int> GetNumberOfFilesInStorage()
        {
            return await _mongoDbService.GetTotalNumberOfFiles();
        }

        /// <summary>
        /// Creates and returns a string representation of a new Guid..
        /// </summary>
        public string CreateFileGuid()
        {
            return Guid.NewGuid().ToString();
        }
    }
}