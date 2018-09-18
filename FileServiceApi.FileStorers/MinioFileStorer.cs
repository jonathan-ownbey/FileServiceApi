﻿using FileServiceApi.Models;
using Microsoft.Extensions.Options;
using Minio;
using Minio.Exceptions;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FileServiceApi.FileStorers
{
    /// <summary>
    /// A service to upload, retrieve, list, and delete files from a minio server.
    /// </summary>
    public class MinioFileStorer : IMinioFileStorer
    {
        private readonly IOptions<ConfigurationSettings> _configurationSettings;
        private readonly MinioClient _minioClient;

        public MinioFileStorer(IOptions<ConfigurationSettings> configurationSettings)
        {
            _configurationSettings = configurationSettings;

            switch (_configurationSettings.Value.ServiceType)
            {
                case "S3":
                    _minioClient = new MinioClient(_configurationSettings.Value.AwsEndpoint, _configurationSettings.Value.AwsAccessKey,
                        _configurationSettings.Value.AwsSecretKey).WithSSL();
                    break;
                case "minio":
#if DEBUG
                    // use the if DEBUG if you aren't connecting to https locally.
                    _minioClient = new MinioClient(_configurationSettings.Value.MinioEndpoint,
                        _configurationSettings.Value.MinioAccessKey, _configurationSettings.Value.MinioSecretKey,
                        _configurationSettings.Value.MinioRegion);
#else
                _minioClient = new MinioClient(_configurationSettings.Value.MinioEndpoint,
                    _configurationSettings.Value.MinioAccessKey, _configurationSettings.Value.MinioSecretKey,
                    _configurationSettings.Value.MinioRegion).WithSSL();
#endif
                    break;
                default:
                    Log.Information("Incorrect or no service type defined in appsettings.json.");
                    break;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Puts a single file in the specified bucket in Minio.
        /// </summary>
        /// <param name="fileName">A string, name of the file to be uploaded.</param>
        /// <param name="data">A stream of the file content to be uploaded.</param>
        /// <param name="contentType">A string, content-type of the file to be uploaded.</param>
        /// <returns>A Task, of type bool which represents success uploading file.</returns>
        public async Task<bool> UploadFile(string fileName, Stream data, string contentType)
        {
            try
            {
                Log.Information($"Uploading file: {fileName} to bucket: {_configurationSettings.Value.BucketName}.");
                if (await CreateBucket(_configurationSettings.Value.BucketName, _configurationSettings.Value.MinioRegion))
                    await _minioClient.PutObjectAsync(_configurationSettings.Value.BucketName, fileName, data, data.Length, contentType);


                Log.Information("Upload succeeded.");
                return true;
            }
            catch (Exception exception)
            {
                Log.Error(exception, $"Failed to upload file: {fileName} to bucket: {_configurationSettings.Value.BucketName}.");
                return false;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Retrieves a single file from the specified bucket in Minio.
        /// </summary>
        /// <param name="fileName">A string, name fo the file which is stored.</param>
        /// <returns>A Task, of type stream which contains the content of the file retrieved.</returns>
        public async Task<Stream> RetrieveFile(string fileName)
        {
            try
            {
                Log.Information($"Retrieving file: {fileName}, from bucket: {_configurationSettings.Value.BucketName}.");
                Stream returnStream = new MemoryStream();
                // Check whether the object exists using statObject().
                // If the object is not found, statObject() throws an exception,
                // else it means that the object exists.
                // Execution is successful.
                await _minioClient.StatObjectAsync(_configurationSettings.Value.BucketName, fileName);

                // Get input stream to have content of 'fileName' from 'bucketName'
                await _minioClient.GetObjectAsync(_configurationSettings.Value.BucketName, fileName,
                    (stream) =>
                    {
                        stream.CopyTo(returnStream);
                    });
                returnStream.Seek(0, SeekOrigin.Begin);

                Log.Information("Successfully retrieved file.");
                return returnStream;
            }
            catch (MinioException exception)
            {
                Log.Error(exception, $"Failed to retrieve file: {fileName} from bucket: {_configurationSettings.Value.BucketName}.");
                return null;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Deletes a single file from the given bucket, by ID
        /// </summary>
        /// <param name="fileName">A string, name fo the file which is stored.</param>
        /// <returns>A boolean to represent success.</returns>
        public async Task<bool> DeleteFile(string fileName)
        {
            try
            {
                Log.Information($"Deleting file: {fileName} from bucket: {_configurationSettings.Value.BucketName}.");
                await _minioClient.RemoveObjectAsync(_configurationSettings.Value.BucketName, fileName);
                return true;
            }
            catch (Exception exception)
            {
                Log.Error(exception, $"Failed to delete file: {fileName} from bucket: {_configurationSettings.Value.BucketName}.");
                return false;
            }
        }

        /// <summary>
        /// Makes a bucket for storing files in Minio.
        /// </summary>
        /// <param name="bucketName">A string, name of the bucket to be made.</param>
        /// <param name="location">A string, location/region of the bucket to be made.</param>
        /// <returns></returns>
        private async Task<bool> CreateBucket(string bucketName, string location)
        {
            try
            {
                Log.Information($"Creating bucket: {bucketName}.");
                // Make a bucket on the server, if not already present.
                var found = await _minioClient.BucketExistsAsync(bucketName);
                if (!found)
                    await _minioClient.MakeBucketAsync(bucketName, location);

                Log.Information("Successfully created bucket.");
                return true;
            }
            catch (Exception exception)
            {
                Log.Error(exception, $"Failed to create bucket: {bucketName}.");
                return false;
            }
        }
    }
}