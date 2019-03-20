using FileServiceApi.Data;
using FileServiceApi.FileStorers;
using FileServiceApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FileServiceApi.Services
{
    public class FileService : IFileService
    {
        private readonly ConfigurationSettings _configurationSettings;
        private readonly ILocalFileStorer _localFileStorer;
        private readonly IMinioFileStorer _minioFileStorer;
        private readonly IMongoDbService _mongoDbService;

        public FileService(ILocalFileStorer localFileStorer, IMinioFileStorer minioFileStorer, IMongoDbService mongoDbService,
            ConfigurationSettings configurationSettings)
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

                if (string.Equals(_configurationSettings.ServiceType, "default", StringComparison.CurrentCultureIgnoreCase))
                {
                    //Use the LocalFileStorer to write to local disk
                    success = _localFileStorer.WriteFile(formFile, fileMetaData.FileGuid, _configurationSettings.LocalStoragePath);
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
            if (_configurationSettings.ServiceType == "default")
            {
                return _localFileStorer.ReturnFile(fileGuid, _configurationSettings.LocalStoragePath);
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

            if (_configurationSettings.ServiceType == "default")
            {
                if (_localFileStorer.DeleteFile(fileGuid, _configurationSettings.LocalStoragePath))
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
                using (var streamReader = new StreamReader($"{currentDirectory}/{_configurationSettings.WhitelistFile}"))
                {
                    var json = streamReader.ReadToEnd();
                    contentTypes = JsonConvert.DeserializeObject<List<FileContentType>>(json);
                }
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Failed to get list of extensions for whitelist.");
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

        /// <inheritdoc />
        /// <summary>
        /// A helper function to make file sizes more readable.
        /// </summary>
        /// <param name="bytesLength">An int representation of the bytes length of a file.</param>
        /// <returns>A string representation of the file size such as 25MB</returns>
        public string GetReadableSize(int bytesLength)
        {
            // Get absolute value 
            long absoluteLength = bytesLength < 0 ? -bytesLength : bytesLength;
            // Determine the suffix and readable value 
            string suffix;
            double readable;
            if (absoluteLength >= 0x1000000000000000) // Exabyte 
            {
                suffix = "E";
                readable = bytesLength >> 50;
            }
            else if (absoluteLength >= 0x4000000000000) // Petabyte 
            {
                suffix = "P";
                readable = bytesLength >> 40;
            }
            else if (absoluteLength >= 0x10000000000) // Terabyte 
            {
                suffix = "T";
                readable = bytesLength >> 30;
            }
            else if (absoluteLength >= 0x40000000) // Gigabyte 
            {
                suffix = "G";
                readable = bytesLength >> 20;
            }
            else if (absoluteLength >= 0x100000) // Megabyte 
            {
                suffix = "M";
                readable = bytesLength >> 10;
            }
            else if (absoluteLength >= 0x400) // Kilobyte 
            {
                suffix = "K";
                readable = bytesLength;
            }
            else
            {
                return bytesLength.ToString("0 B"); // Byte 
            }
            // Divide by 1024 to get fractional value 
            readable = readable / 1024;
            // Return formatted number with suffix 
            return readable.ToString("0.### ") + suffix;
        }
    }
}