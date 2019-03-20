using Autofac;
using FileServiceApi.Data;
using FileServiceApi.FileStorers;
using FileServiceApi.Models;
using FileServiceApi.Services;
using Microsoft.Extensions.Options;

namespace FileServiceApi
{
    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance<ILocalFileStorer>(LocalFileStorer.Instance);
            builder.Register(c => new MinioFileStorer(c.Resolve<IOptions<ConfigurationSettings>>())).As<IMinioFileStorer>();
            builder.Register(c => new MongoDbService(c.Resolve<IOptions<ConfigurationSettings>>())).As<IMongoDbService>();
            builder.Register(c => new FileService(c.Resolve<ILocalFileStorer>(), c.Resolve<IMinioFileStorer>(),
                c.Resolve<IMongoDbService>(), c.Resolve<ConfigurationSettings>())).As<IFileService>();
        }
    }
}