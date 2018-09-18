using System;
using FileServiceApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Serilog;

namespace FileServiceApi.Data
{
    public class MongoDbService : IMongoDbService
    {
        private readonly IMongoCollection<FileMetaData> _collection;

        public MongoDbService(IOptions<ConfigurationSettings> configurationSettings)
        {
            Log.Information("Creating MongoDB service.");
            try
            {
                var mongoClient = new MongoClient(configurationSettings.Value.MongoConnectionString);
                var database = mongoClient.GetDatabase(configurationSettings.Value.MongoDatabaseName);
                _collection = database.GetCollection<FileMetaData>(configurationSettings.Value.MongoCollectionName);

                Log.Information("Successfully created MongoDB service.");
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Failed to create MongoDB service.");
                throw;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Takes a list of FileMetaData and inserts them as documents in
        /// a MongoDB collection.
        /// </summary>
        /// <param name="fileMetaDatas">A list of FileMetaData to be stored in the db.</param>
        public void InsertFileDatas(List<FileMetaData> fileMetaDatas)
        {
            try
            {
                Log.Information($"Inserting list of FileMetaData: {fileMetaDatas} in MongoDB.");

                _collection.InsertManyAsync(fileMetaDatas);

                Log.Information("List of FileMetaData inserted successfully.");
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Failed to insert List of FileMetaData.");
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Takes a list of filenames and returns a Task of type List of
        /// FileMetaData containing file information.
        /// </summary>
        /// <param name="fileNames">A List of string representations of guids
        /// which are unique to a given file in storage.</param>
        /// <returns>Task of type List of FileMetaData containing file info.</returns>
        public async Task<List<FileMetaData>> GetFileMetaDatas(List<string> fileNames)
        {
            var fileMetaDatas = new List<FileMetaData>();
            var filterDef = new FilterDefinitionBuilder<FileMetaData>();
            var filter = filterDef.In(x => x.FileGuid, fileNames);

            try
            {
                Log.Information($"Retrieving List of FileMetaData for List of guids: {fileNames}.");

                using (var cursor = await _collection.FindAsync(filter))
                {
                    while (await cursor.MoveNextAsync())
                        fileMetaDatas.AddRange(cursor.Current);
                }

                Log.Information("Successfully retrieved list of FileMetaData.");
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Failed to retrieve List of FileMetaData.");
            }

            return fileMetaDatas;
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets a Task of type List of FileMetaData containing file information.
        /// </summary>
        /// <returns>Task of type List of FileMetaData containing file info.</returns>
        public async Task<List<FileMetaData>> GetAllFileMetaDatas()
        {
            var fileMetaDatas = new List<FileMetaData>();
            try
            {
                Log.Information($"Retrieving List of FileMetaData for all files.");
                using (var cursor = await _collection.FindAsync(FilterDefinition<FileMetaData>.Empty))
                {
                    while (await cursor.MoveNextAsync())
                        fileMetaDatas.AddRange(cursor.Current);
                }

                return fileMetaDatas;
            }
            catch (Exception exception)
            {
                Log.Error(exception, "An error occurred while retrieving FileMetaData for all files.");
                return null;
            }
        }
        /// <inheritdoc />
        /// <summary>
        /// Takes a list of filenames and deletes documents from the
        /// Mongo database associated with them.
        /// </summary>
        /// <param name="fileNames">A List of string representations of guids
        /// which are unique to a given file in storage.</param>
        /// <returns>A Task of type bool representing success.</returns>
        public async Task<bool> DeleteFileMetaDatas(List<string> fileNames)
        {
            var success = false;
            foreach (var fileGuid in fileNames)
            {
                try
                {
                    Log.Information($"Setting file: {fileGuid} to deleted in MongoDB.");
                    var updateDef = Builders<FileMetaData>.Update.Set(x => x.IsDeleted, true);
                    await _collection.UpdateOneAsync(x => x.FileGuid == fileGuid, updateDef);
                    success = true;
                }
                catch (Exception exception)
                {
                    Log.Error(exception, $"An error occurred when setting file: {fileGuid} to deleted in MongoDB.");
                    return false;
                }
            }
            return success;
        }
        /// <inheritdoc />
        /// <summary>
        /// Gets total records count in the MongoDB collection.
        /// </summary>
        /// <returns>Int representing the total number of records in the MongoDB collection.</returns>
        public async Task<int> GetTotalNumberOfFiles()
        {
            try
            {
                Log.Information("Getting file count from MongoDB.");
                var filterDef = new FilterDefinitionBuilder<FileMetaData>();
                var filter = filterDef.Where(x => x.IsDeleted == false);

                return (int)await _collection.CountDocumentsAsync(filter);
            }
            catch (Exception exception)
            {
                Log.Error(exception, "An error occurred while attempting to get file count in MongoDB.");
                return -1;
            }
        }
    }
}