using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
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
            throw new NotImplementedException();
        }

        [TestMethod]
        public async Task RetrieveFile_Successfully_Returns_Stream()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public async Task DeleteFile_Returns_True_On_Success()
        {
            throw new NotImplementedException();
        }
    }
}