using MongoDB.Bson;
using System;

namespace FileServiceApi.Models
{
    /// <summary>
    /// A class which contains data about uploaded files
    /// which are stored with a different, obscured name
    /// on the server. The orginal file data is written
    /// to a collection in MongoDB to be retrieved and
    /// reunited with the file at a later time.
    /// </summary>
    public class FileMetaData
    {
        /// <summary>
        /// An ID field necessary for MongoDB to translate to a type
        /// when retrieving records.
        /// </summary>
        public ObjectId Id { get; set; }
        /// <summary>
        /// The original name of the file pre-upload.
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// The content-type of the file.
        /// </summary>
        public string FileType { get; set; }
        /// <summary>
        /// The string representation of a guid which is
        /// used as the file name in storage.
        /// </summary>
        public string FileGuid { get; set; }
        /// <summary>
        /// The original upload date of the file.
        /// </summary>
        public DateTime UploadDate { get; set; }
        /// <summary>
        /// A boolean flag to determine if the file has been deleted.
        /// </summary>
        public bool IsDeleted { get; set; }
    }
}
