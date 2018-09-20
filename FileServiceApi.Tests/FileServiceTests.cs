using FileServiceApi.Data;
using FileServiceApi.FileStorers;
using FileServiceApi.Models;
using FileServiceApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FileServiceApi.Tests
{
    [TestClass]
    public class FileServiceTests
    {
        private Mock<IFormFile> _mockFormFile = new Mock<IFormFile>();
        private Mock<IOptions<ConfigurationSettings>> _mockConfigurationSettings;
        private Mock<ILocalFileStorer> _mockLocalFileStorer;
        private Mock<IMinioFileStorer> _mockMinioStorer;
        private Mock<IMongoDbService> _mockMongoDbService;

        [TestInitialize]
        public void SetupTests()
        {
            _mockConfigurationSettings = new Mock<IOptions<ConfigurationSettings>>();
            _mockLocalFileStorer = new Mock<ILocalFileStorer>();
            _mockMinioStorer = new Mock<IMinioFileStorer>();
            _mockMongoDbService = new Mock<IMongoDbService>();
            SetupFile();
        }

        [TestMethod]
        public async Task StoreFiles_Successfully_Returns_List()
        {
            var fileList = new List<IFormFile> {_mockFormFile.Object};
            _mockLocalFileStorer.Setup(x => x.WriteFile(It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);
            _mockMinioStorer.Setup(x => x.UploadFile(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>())).Returns(Task.FromResult(true));
            _mockMongoDbService.Setup(x => x.InsertFileDatas(It.IsAny<List<FileMetaData>>()));

            var fileService = new FileService(_mockLocalFileStorer.Object, _mockMinioStorer.Object, _mockMongoDbService.Object, _mockConfigurationSettings.Object);

            var result = await fileService.StoreFiles(fileList);

            Assert.IsInstanceOfType(result, typeof(List<string>));
        }

        [TestMethod]
        public async Task GetFiles_Successfully_Returns_File()
        {
            const string fileName = "00000000-0000-0000-0000-000000000000";
            var mockStream = new Mock<Stream>();
            var mockService = new Mock<IFileService>();
            mockService.Setup(x => x.GetFile(fileName)).Returns(Task.FromResult<Stream>(mockStream.Object));

            var fileService = mockService.Object;

            var result = await fileService.GetFile(fileName);

            Assert.IsInstanceOfType(result, typeof(Stream));
        }

        [TestMethod]
        public async Task DeleteFile_Successfully_Deletes_File()
        {
            const string fileName = "00000000-0000-0000-0000-000000000000";
            var mockService = new Mock<IFileService>();
            mockService.Setup(x => x.DeleteFile(fileName)).Returns(Task.FromResult(true));

            var fileService = mockService.Object;

            var result = await fileService.DeleteFile(fileName);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task GetFileMetaData_Successfully_Returns_List()
        {
            var guidList = new Mock<List<string>>();
            var metaDataList = new Mock<List<FileMetaData>>();
            var mockService = new Mock<IFileService>();
            mockService.Setup(x => x.GetFileMetaData(It.IsAny<List<string>>())).Returns(Task.FromResult<List<FileMetaData>>(metaDataList.Object));

            var fileService = mockService.Object;

            var result = await fileService.GetFileMetaData(guidList.Object);

            Assert.IsInstanceOfType(result, typeof(List<FileMetaData>));
        }

        [TestMethod]
        public async Task GetAllFileMetaData_Successfully_Returns_List()
        {
            var metaDataList = new Mock<List<FileMetaData>>();
            var mockService = new Mock<IFileService>();
            mockService.Setup(x => x.GetAllFileMetaData()).Returns(Task.FromResult<List<FileMetaData>>(metaDataList.Object));

            var fileService = mockService.Object;

            var result = await fileService.GetAllFileMetaData();

            Assert.IsInstanceOfType(result, typeof(List<FileMetaData>));
        }

        private void SetupFile()
        {
            const string content = "Hello World from a Fake File";
            const string fileName = "test.pdf";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;
            _mockFormFile.Setup(_ => _.OpenReadStream()).Returns(ms);
            _mockFormFile.Setup(_ => _.FileName).Returns(fileName);
            _mockFormFile.Setup(_ => _.Length).Returns(ms.Length);
        }
    }
}
