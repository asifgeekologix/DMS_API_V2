using AutoMapper;
using Dms.API.Services;
using Dms.Domain.ApiModels;
using Dms.Domain.Entities.Dms_BaseObjects;
using Dms.Domain.Entities.Dms_DTO;
using Dms.Domain.Repositories;
using Dms.Domain.Utilities;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using MySqlX.XDevAPI.Common;
using Org.BouncyCastle.Utilities;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using static Google.Apis.Drive.v3.DrivesResource;

namespace Dms.API.Controllers
{
    [Route("api/Storage/[action]")]
    [ApiController]
    public class StorageController : ControllerBase
    {
        private readonly IMapper _IMapper;
        private readonly IConfigurationRepository _IConfigurationRepository;
        private readonly IDataBaseRepository _IDataBaseRepository;
        private readonly IGoogleDriveFilesRepository _IGoogleDriveFilesRepository;

        public StorageController(IMapper IMapper,IConfigurationRepository IConfigurationRepository, IDataBaseRepository IDataBaseRepository, IGoogleDriveFilesRepository IGoogleDriveFilesRepository)
        {
            _IMapper = IMapper;
            _IConfigurationRepository = IConfigurationRepository;
            _IDataBaseRepository = IDataBaseRepository;
            _IGoogleDriveFilesRepository = IGoogleDriveFilesRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetStorageFiles()
        {
            ResultObjectDTO<List<FileResponseDTO>> result = new ResultObjectDTO<List<FileResponseDTO>>();
            var configuration = await _IConfigurationRepository.GetConfiguration();
          
            if (configuration.is_connect)
            {
                result = _IMapper.Map<ResultObjectDTO<List<FileResponseDTO>>>(await _IDataBaseRepository.GetFilesAll(configuration.host_connection));
            }

            else
            {
                result.Result = ResultType.Error;
                result.ResultMessage = "Please Connect Cloud";
            }

            return Ok(result);

        }

        public async Task<IActionResult> SearchFiles(FileRequestApiModel fileRequest)
        {
            ResultObjectDTO<List<FileResponseDTO>> result = new ResultObjectDTO<List<FileResponseDTO>>();
            var configuration = await _IConfigurationRepository.GetConfiguration();
            if (configuration.is_connect)
            {
                result = _IMapper.Map<ResultObjectDTO<List<FileResponseDTO>>>(await _IDataBaseRepository.SearchFiles(configuration.host_connection, fileRequest));
            }

            else
            {
                result.Result = ResultType.Error;
                result.ResultMessage = "Please Connect Cloud";
            }

           
           
            return Ok(result);

        }

        [HttpPost]
        [RequestSizeLimit(100 * 1024 * 1024)]
        public async Task<IActionResult> FileUpload([FromForm] FileRequestApiModel fileRequest)
        {
            ResultObjectDTO<string> postResult = new ResultObjectDTO<string>();
            try
            {
                
                if (fileRequest.file!=null)
                {
                    var configuration = await _IConfigurationRepository.GetConfiguration();
                   
                    if (configuration.is_connect)
                    {

                        if (configuration.cloud_type.Equals(1))
                        {
                            var driveResult = await _IGoogleDriveFilesRepository.FileUpload(fileRequest);
                            if (driveResult.Result == ResultType.Success)
                            {
                                
                                fileRequest.cloud_type = configuration.cloud_type;
                                fileRequest.unique_id = Guid.NewGuid().ToString();
                                fileRequest.file_name = Path.GetFileName(fileRequest.file.FileName);
                                fileRequest.file_extension = Path.GetExtension(fileRequest.file.FileName);
                                fileRequest.file_time_stamp = DateTime.Now;
                                fileRequest.file_content_type = fileRequest.file.ContentType;
                                fileRequest.file_id = driveResult.ResultData.file_id;
                                fileRequest.webViewLink = driveResult.ResultData.webViewLink;

                                var dbResult = await _IDataBaseRepository.FileUpload(configuration.host_connection,fileRequest);
                                postResult.Result = dbResult.Result;
                                postResult.ResultMessage = dbResult.ResultMessage;
                                postResult.ResultData = dbResult.ResultData.unique_id;
                            }
                            else
                            {
                                postResult.Result = driveResult.Result;
                                postResult.ResultMessage = driveResult.ResultMessage;
                            }


                        }
                        else
                        {
                            fileRequest.cloud_type = configuration.cloud_type;
                            fileRequest.unique_id = Guid.NewGuid().ToString();
                            fileRequest.file_name = Path.GetFileName(fileRequest.file.FileName);
                            fileRequest.file_extension = Path.GetExtension(fileRequest.file.FileName);
                            fileRequest.file_time_stamp = DateTime.Now;
                            fileRequest.file_content_type = fileRequest.file.ContentType;
                            
                            var bytes = await fileRequest.file.GetBytes();
                            var hexString = Convert.ToBase64String(bytes);
                            fileRequest.file_data = hexString;
                            var dbResult = await _IDataBaseRepository.FileUpload(configuration.host_connection, fileRequest);
                            postResult.Result = dbResult.Result;
                            postResult.ResultMessage = dbResult.ResultMessage;
                            postResult.ResultData = dbResult.ResultData.unique_id;

                        }
                       

                    }
                    else
                    {
                        postResult.Result = ResultType.Error;
                        postResult.ResultMessage = "Please Connect Cloud";
                    }
                }
                else
                {
                    postResult.Result = ResultType.Error;
                    postResult.ResultMessage ="Please Upload File";
                }
            }
            catch (Exception ex)
            {
                postResult.Result = ResultType.Error;
                postResult.ResultMessage=ex.Message;
            }

            return Ok(postResult);
        }


        [HttpGet]
        public async Task<IActionResult> ViewFile(string id,string email)
        {
            ResultObjectDTO<string> result = new ResultObjectDTO<string>();
            var configuration = await _IConfigurationRepository.GetConfiguration();
            if (configuration.is_connect)
            {
                var file = await _IDataBaseRepository.GetFileById(configuration.host_connection,id);
                if (file.Result == ResultType.Success)
                {
                    if (configuration.cloud_type.Equals(1))
                    {
                        var gFile = await _IGoogleDriveFilesRepository.GetFileInfo(file.ResultData.file_id);
                        if (gFile.Result == ResultType.Success)
                        {
                            if((gFile.ResultData.permissions.Any(d=>d.EmailAddress== email)|| gFile.ResultData.permissions.Any(d => d.Id.Equals("anyoneWithLink"))))
                            {
                                result.Result = ResultType.Success;
                                result.ResultMessage = "Success";
                                result.ResultData = gFile.ResultData.webViewLink;
                            }
                            else
                            {
                                result.Result = ResultType.Error;
                                result.ResultMessage = "You don't have permission to access this resource";

                            }
                            
                        }
                        else
                        {
                            result.Result = ResultType.Error;
                            result.ResultMessage = gFile.ResultMessage;

                        }
                        
                    }
                    else
                    {
                        byte[] bytes = Convert.FromBase64String(file.ResultData.file_data);
                        var memoryStream = new MemoryStream(bytes);
                        Response.Headers.Add("Content-Disposition", "inline; filename=" + file.ResultData.file_name);
                        return new FileStreamResult(memoryStream, file.ResultData.file_content_type);

                    }
                    
                }
                else
                {
                    result.Result = file.Result;
                    result.ResultMessage = file.ResultMessage;
                }


            }
            else
            {
                result.Result = ResultType.Error;
                result.ResultMessage = "Please Connect Cloud";
            }

            return Ok(result);
        }


        [HttpGet]
        public async Task<IActionResult> DownloadFile(string id, string email)
        {
            ResultObjectDTO<string> result = new ResultObjectDTO<string>();
            var configuration = await _IConfigurationRepository.GetConfiguration();
            if (configuration.is_connect)
            {
                var file = await _IDataBaseRepository.GetFileById(configuration.host_connection, id);
                if (file.Result == ResultType.Success)
                {
                    if (configuration.cloud_type.Equals(1))
                    {
                        var gFile = await _IGoogleDriveFilesRepository.GetFileInfo(file.ResultData.file_id);
                        if (gFile.Result == ResultType.Success)
                        {
                            if ((gFile.ResultData.permissions.Any(d => d.EmailAddress == email) || gFile.ResultData.permissions.Any(d => d.Id.Equals("anyoneWithLink"))))
                            {
                                result.Result = ResultType.Success;
                                result.ResultMessage = "Success";
                                result.ResultData= gFile.ResultData.webContentLink;
                            }
                            else
                            {
                                result.Result = ResultType.Error;
                                result.ResultMessage = "You don't have permission to access this resource";

                            }

                        }
                        else
                        {
                            result.Result = ResultType.Error;
                            result.ResultMessage = gFile.ResultMessage;
                        }

                    }
                    else
                    {
                        byte[] bytes = Convert.FromBase64String(file.ResultData.file_data);
                        var memoryStream = new MemoryStream(bytes);
                        Response.Headers.Add("Content-Disposition", "inline; filename=" + file.ResultData.file_name);
                        return File(memoryStream, $"{file.ResultData.file_content_type}", file.ResultData.file_name);

                    }

                }
                else
                {
                    result.Result = file.Result;
                    result.ResultMessage = file.ResultMessage;
                }


            }
            else
            {
                result.Result = ResultType.Error;
                result.ResultMessage = "Please Connect Cloud";
            }

            return Ok(result);
        }
    }

    public static class FormFileExtensions
    {
        public static async Task<byte[]> GetBytes(this IFormFile formFile)
        {
            await using var memoryStream = new MemoryStream();
            await formFile.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
