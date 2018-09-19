using FileServiceApi.Models;
using FileServiceApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileServiceApi.Controllers
{
    [Route("api/[controller]")]
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

            _maxFileSize = _configurationSettings.Value.MaxUploadFileSize * 1024;
        }

        // GET api/files
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

        // GET api/files/5
        [HttpGet("{id}")]
        public async Task<FileResult> Get(string id)
        {
            var stream = await _fileService.GetFile(id);
            var fileMetaDatas = await _fileService.GetFileMetaData(new List<string> { id });
            var fileData = fileMetaDatas[0];

            // Note: In Postman, the dialog for downloading a file will not respect the filename.
            // If you just paste the same request into a browser, it will download the file
            // with the appropriate name.
            return stream == null ? null : File(stream, fileData.FileType, fileData.FileName, null, EntityTagHeaderValue.Any, true);
        }

        // POST api/files/upload
        [HttpPost("upload")]
        public async Task<IActionResult> Post([FromForm]List<IFormFile> files)
        {
            if (files.Count == 0)
                if (Request.HasFormContentType)
                    files = Request.Form.Files.ToList();

            if (files.Count > 0)
            {
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
                        Log.Error(exception, "The MaxFileUploadLimit configuration setting must be either NO_UPLOAD_LIMIT or a number");
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

                    if (!allowedFileTypes.Any(x => x.ContentType == file.ContentType && file.FileName.EndsWith(x.Extension)))
                    {
                        var errorString = $"File with content-type: {file.ContentType} is not allowed.";
                        Log.Error(errorString);
                        return StatusCode(415, errorString);
                    }
                }
                var guids = await _fileService.StoreFiles(files);
                if (guids != null)
                    return Ok(string.Join(",", guids));
            }
            else
            {
                return NoContent();
            }

            return StatusCode(500);
        }

        // DELETE api/files/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (await _fileService.DeleteFile(id))
                return Ok();

            return StatusCode(500);
        }
    }
}