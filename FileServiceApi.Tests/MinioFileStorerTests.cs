using FileServiceApi.FileStorers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IO;
using System.Threading.Tasks;

namespace FileServiceApi.Tests
{
    [TestClass]
    public class MinioFileStorerTests
    {
        private const string FileName = "00000000-0000-0000-0000-000000000000";
        private readonly Mock<Stream> _fileStream = new Mock<Stream>();

        [TestMethod]
        public async Task UploadFile_Succeeds_And_Returns_True()
        {
            const string contentType = "application/pdf";

            var mockService = new Mock<IMinioFileStorer>();
            mockService.Setup(x => x.UploadFile(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>())).Returns(Task.FromResult(true));

            var minioService = mockService.Object;

            var result = await minioService.UploadFile(FileName, _fileStream.Object, contentType);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task RetrieveFile_Successfully_Returns_Stream()
        {
            var mockService = new Mock<IMinioFileStorer>();
            mockService.Setup(x => x.RetrieveFile(It.IsAny<string>())).Returns(Task.FromResult(_fileStream.Object));

            var minioService = mockService.Object;

            var result = await minioService.RetrieveFile(FileName);

            Assert.IsInstanceOfType(result, typeof(Stream));
        }

        [TestMethod]
        public async Task DeleteFile_Returns_True_On_Success()
        {
            var mockService = new Mock<IMinioFileStorer>();
            mockService.Setup(x => x.DeleteFile(It.IsAny<string>())).Returns(Task.FromResult(true));

            var minioService = mockService.Object;

            var result = await minioService.DeleteFile(FileName);

            Assert.IsTrue(result);
        }
    }
}