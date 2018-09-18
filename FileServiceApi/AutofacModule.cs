//using Autofac;
//using FileServiceApi.FileStorers;
//using FileServiceApi.Models;
//using Microsoft.Extensions.Options;

//namespace FileServiceApi
//{
//    public class AutofacModule : Module
//    {
//        protected override void Load(ContainerBuilder builder)
//        {
//            builder.Register(c => new MinioFileStorer(c.Resolve<IOptions<ConfigurationSettings>>())).As<IMinioFileStorer>();

//        }
//    }
//}