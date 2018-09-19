namespace FileServiceApi.Models
{
    /// <summary>
    /// Configuration settings to be used across services/projects.
    /// </summary>
    public class ConfigurationSettings
    {
        /// <summary>
        /// Determines whether to us minio, S3, (currently unavailable - Azure, GCP, etc..)
        /// </summary>
        public string ServiceType { get; set; }
        /// <summary>
        /// The name of the bucket on the storage server where
        /// files are housed.
        /// </summary>
        public string BucketName { get; set; }
        /// <summary>
        /// The address where the minio service will reach out
        /// to upload/download/delete/list files/buckets.
        /// </summary>        
        public string MinioEndpoint { get; set; }
        /// <summary>
        /// The access key used to login to minio server.
        /// </summary>
        public string MinioAccessKey { get; set; }
        /// <summary>
        /// The secret key used to login to minio server.
        /// </summary>
        public string MinioSecretKey { get; set; }
        /// <summary>
        /// The region in which the minio bucket is located.
        /// </summary>
        public string MinioRegion { get; set; }
        /// <summary>
        /// The connection string to the Mongo database.
        /// </summary>
        public string MongoConnectionString { get; set; }
        /// <summary>
        /// The name of the database in Mongo.
        /// </summary>
        public string MongoDatabaseName { get; set; }
        /// <summary>
        /// The name of the collection of documents in the
        /// Mongo database.
        /// </summary>
        public string MongoCollectionName { get; set; }
        /// <summary>
        /// The AWS address where the minio service will reach out
        /// to upload/download/delete/list files/buckets.
        /// </summary>
        public string AwsEndpoint { get; set; }
        /// <summary>
        /// The access key used to login to AWS server.
        /// </summary>
        public string AwsAccessKey { get; set; }
        /// <summary>
        /// The secret key used to login to AWS server.
        /// </summary>
        public string AwsSecretKey { get; set; }
        /// <summary>
        /// The account ID associated with AWS.
        /// </summary>
        public string AwsAccountId { get; set; }
        /// <summary>
        /// The name of the bucket on AWS server where
        /// files are housed.
        /// </summary>
        public string AwsBucketName { get; set; }
        /// <summary>
        /// The Azure address where the file service will reach
        /// out to upload/download/delete/list files/buckets
        /// </summary>
        public string AzureEndpoint { get; set; }
        /// <summary>
        /// The access key used to login to Azure server.
        /// </summary>
        public string AzureAccessKey { get; set; }
        /// <summary>
        /// The secret key used to login to Azure server.
        /// </summary>
        public string AzureSecretKey { get; set; }
        /// <summary>
        /// The maximum number of files allowed to be uploaded.
        /// </summary>
        public string MaxFileUploadLimit { get; set; }
        /// <summary>
        /// The maximum file size allowed to be uploaded.
        /// </summary>
        public int MaxUploadFileSize { get; set; }
        /// <summary>
        /// A local storage path for the DefaultFileStorer.
        /// </summary>
        public string LocalStoragePath { get; set; }
        /// <summary>
        /// Name of the file used for whitelisting files.
        /// </summary>
        public string WhitelistFile { get; set; }
    }
}