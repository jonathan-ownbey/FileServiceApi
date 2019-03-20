using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FileServiceApi.Data;
using FileServiceApi.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;


namespace FileServiceApi.Tests
{
    [TestClass]
    public class MongoDbServiceTests
    {
        private readonly Mock<List<FileMetaData>> _metaDatas = new Mock<List<FileMetaData>>();
        private readonly Mock<List<string>> _fileNameList = new Mock<List<string>>();

        [TestMethod]
        public async Task GetFileMetaDatas_Successfully_Returns_List_Of_MetaData()
        {
            var mockService = new Mock<IMongoDbService>();
            mockService.Setup(x => x.GetFileMetaDatas(It.IsAny<List<string>>())).Returns(Task.FromResult(_metaDatas.Object));

            var mongoService = mockService.Object;

            var result = await mongoService.GetFileMetaDatas(_fileNameList.Object);

            Assert.IsInstanceOfType(result, typeof(List<FileMetaData>));
        }

        [TestMethod]
        public async Task GetAllFileMetaDatas_Returns_List_Of_MetaData()
        {
            var mockService = new Mock<IMongoDbService>();
            mockService.Setup(x => x.GetAllFileMetaDatas()).Returns(Task.FromResult(_metaDatas.Object));

            var mongoService = mockService.Object;

            var result = await mongoService.GetAllFileMetaDatas();

            Assert.IsInstanceOfType(result, typeof(List<FileMetaData>));
        }

        [TestMethod]
        public async Task DeleteFileMetaDatas_Successfully_Deletes_And_Returns_True()
        {
            var mockService = new Mock<IMongoDbService>();
            mockService.Setup(x => x.DeleteFileMetaDatas(_fileNameList.Object)).Returns(Task.FromResult(true));

            var mongoService = mockService.Object;

            var result = await mongoService.DeleteFileMetaDatas(_fileNameList.Object);

            Assert.IsTrue(result);
        }
    }
}
