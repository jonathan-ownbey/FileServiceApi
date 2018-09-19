namespace FileServiceApi.Models
{
    /// <summary>
    /// A class which contains properties for validating
    /// uploaded files by type/extension.
    /// </summary>
    public class FileContentType
    {
        /// <summary>
        /// The file MIME type
        /// </summary>
        public string ContentType { get; set; }
        /// <summary>
        /// The file extension
        /// </summary>
        public string Extension { get; set; }
    }
}