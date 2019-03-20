using FileServiceApi.Models;
using FileServiceApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileServiceApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly IOptions<ConfigurationSettings> _configurationSettings;
        private readonly int _maxFileSize;
        private const string MaxNumberOfFiles = "NO_UPLOAD_LIMIT";

        public FilesController(IFileService fileService, IOptions<ConfigurationSettings> configurationSettings)
        {
            _fileService = fileService;
            _configurationSettings = configurationSettings;

            _maxFileSize = _configurationSettings.Value.MaxUploadFileSize * 1048576;
        }

        // GET files
        [HttpGet]
        public async Task<string> Get()
        {
            var fileMetaDatas = await _fileService.GetAllFileMetaData();
            if (!fileMetaDatas.Any()) return "No files in storage currently!!";

            var returnText = string.Empty;

            fileMetaDatas.ForEach(x =>
            {
                returnText += $"{x.FileGuid}, {x.FileName}, {x.FileType}, {x.UploadDate}, {x.IsDeleted}\n";
            });

            return returnText;
        }

        // GET files/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var stream = await _fileService.GetFile(id);

            // Short circuit and return 401 if the file is not found.
            if (stream == null) return Unauthorized();

            var fileMetaDatas = await _fileService.GetFileMetaData(new List<string> { id });
            var fileData = fileMetaDatas[0];

            // Note: In Postman, the dialog for downloading a file will not respect the filename.
            // If you just paste the same request into a browser, it will download the file
            // with the appropriate name.
            return stream == null ? null : File(stream, fileData.FileType, fileData.FileName, null, EntityTagHeaderValue.Any, true);
        }

        // POST files/upload
        [HttpPost("upload")]
        public async Task<IActionResult> Post([FromForm]List<IFormFile> files)
        {
            // In some instances I've seen inconsistent behavior. This is a fix for that.
            if (files.Count == 0)
                if (Request.HasFormContentType)
                    files = Request.Form.Files.ToList();

            if (files.Count <= 0) return NoContent();

            var allowedFileTypes = _fileService.GetAllowedFileTypes();

            if (_configurationSettings.Value.MaxFileUploadLimit != MaxNumberOfFiles)
            {
                var uploadedFileCount = _fileService.GetNumberOfFilesInStorage();

                try
                {
                    var maxFiles = int.Parse(_configurationSettings.Value.MaxFileUploadLimit);

                    if (uploadedFileCount.Result >= maxFiles)
                    {
                        var errorString = $"File upload would exceed the maximum number of files: {_configurationSettings.Value.MaxFileUploadLimit}";
                        Log.Error(errorString);
                        return StatusCode(406, errorString);
                    }
                }
                catch (Exception exception)
                {
                    Log.Error(exception, "The MaxFileUploadLimit configuration setting must be either NO_UPLOAD_LIMIT or a positive number.");
                    throw;
                }
            }

            foreach (var file in files)
            {
                if (file.Length > _maxFileSize)
                {
                    var errorString = $"File failed to upload, max file size is {_maxFileSize}MB";
                    Log.Error(errorString);
                    return StatusCode(413, errorString);
                }

                // Note: This could check content type also, but presents a different set of issues wherein 
                //       some files will read as application/octet-stream which can be misleading.
                if (!allowedFileTypes.Any(x => x.Extension.Equals(Path.GetExtension(file.FileName))))
                {
                    var errorString = $"File with content-type: {file.ContentType} is not allowed.";
                    Log.Error(errorString);
                    return StatusCode(415, errorString);
                }
            }
            var guids = await _fileService.StoreFiles(files);
            if (guids == null) return NoContent();

            var returnObjects = guids.Select(x => new { id = x }).ToList();

            return Ok(JsonConvert.SerializeObject(returnObjects));
        }

        // DELETE files/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (await _fileService.DeleteFile(id))
                return Ok();

            return Unauthorized();
        }
    }
}