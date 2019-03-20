using System.IO;
using System.Threading.Tasks;

namespace FileServiceApi.FileStorers
{
    /// <summary>
    /// A service to upload, retrieve, list, and delete files from a minio server.
    /// </summary>
    public interface IMinioFileStorer
    {
        /// <summary>
        /// Puts a single file in the specified bucket in Minio.
        /// </summary>
        /// <param name="fileName">A string, name of the file to be uploaded.</param>
        /// <param name="data">A stream of the file content to be uploaded.</param>
        /// <param name="contentType">A string, content-type of the file to be uploaded.</param>
        /// <returns>A Task, of type bool which represents success uploading file.</returns>
        Task<bool> UploadFile(string fileName, Stream data, string contentType);
        /// <summary>
        /// Retrieves a single file from the specified bucket in Minio.
        /// </summary>
        /// <param name="fileName">A string, name fo the file which is stored.</param>
        /// <returns>A Task, of type stream which contains the content of the file retrieved.</returns>
        Task<Stream> RetrieveFile(string fileName);
        /// <summary>
        /// Deletes a single file from the given bucket, by ID
        /// </summary>
        /// <param name="fileName">A string, name fo the file which is stored.</param>
        /// <returns>A boolean to represent success.</returns>
        Task<bool> DeleteFile(string fileName);
    }
}
