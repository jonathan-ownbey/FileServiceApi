using FileServiceApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileServiceApi.Data
{
    /// <summary>
    /// A service for writing/reading/deleting file metadata in a Mongo database
    /// </summary>
    public interface IMongoDbService
    {
        /// <summary>
        /// Takes a list of FileMetaData and inserts them as documents in
        /// a MongoDB collection.
        /// </summary>
        /// <param name="fileMetaDatas">A list of FileMetaData to be stored in the db.</param>
        void InsertFileDatas(List<FileMetaData> fileMetaDatas);

        /// <summary>
        /// Takes a list of filenames and returns a Task of type List of
        /// FileMetaData containing file information.
        /// </summary>
        /// <param name="fileNames">A List of string representations of guids
        /// which are unique to a given file in storage.</param>
        /// <returns>Task of type List of FileMetaData containing file info.</returns>
        Task<List<FileMetaData>> GetFileMetaDatas(List<string> fileNames);

        /// <summary>
        /// Gets a Task of type List of FileMetaData containing file information.
        /// </summary>
        /// <returns>Task of type List of FileMetaData containing file info.</returns>
        Task<List<FileMetaData>> GetAllFileMetaDatas();

        /// <summary>
        /// Takes a list of filenames and deletes documents from the
        /// Mongo database associated with them.
        /// </summary>
        /// <param name="fileNames">A List of string representations of guids
        /// which are unique to a given file in storage.</param>
        /// <returns>A Task of type bool representing success.</returns>
        Task<bool> DeleteFileMetaDatas(List<string> fileNames);

        /// <summary>
        /// Gets total records count in the MongoDB collection.
        /// </summary>
        /// <returns>Int representing the total number of records in the MongoDB collection.</returns>
        Task<int> GetTotalNumberOfFiles();
    }
}