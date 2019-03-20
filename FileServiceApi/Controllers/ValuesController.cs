using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using FileServiceApi.Models;
using FileServiceApi.Services;
using Newtonsoft.Json;

namespace FileServiceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ConfigurationSettings _configurationSettings;
        private readonly IFileService _fileService;

        public ValuesController(IFileService fileService, ConfigurationSettings configurationSettings)
        {
            _configurationSettings = configurationSettings;
            _fileService = fileService;
        }

        [HttpGet]
        public string Get()
        {
            var uploadWhiteList = new UploadWhiteList
            {
                MaxFileUploadLimit = _configurationSettings.MaxFileUploadLimit,
                MaxFileUploadSize = _configurationSettings.MaxUploadFileSize.ToString(),
                ContentTypes = _fileService.GetAllowedFileTypes()
            };

            return JsonConvert.SerializeObject(uploadWhiteList);
        }
    }

    internal class UploadWhiteList
    {
        public List<FileContentType> ContentTypes;
        public string MaxFileUploadSize;
        public string MaxFileUploadLimit;
    }
}
