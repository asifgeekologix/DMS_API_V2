using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Dms.Domain.Repositories;
using Dms.Domain.Entities;
using Dms.Domain.Utilities;
using RestSharp;
using Newtonsoft.Json;
using System.Net;
using Dms.Domain.ApiModels;
using Dms.Domain.Entities.Dms_BaseObjects;
using Dms.DataDapper.Repositories;

namespace Dms.API.Services
{
    public class GoogleDriveFilesRepository : IGoogleDriveFilesRepository
    {
        private static string[] Scopes = { DriveService.Scope.Drive };
        private readonly IConfigurationRepository _IConfigurationRepository;
        private readonly IDataBaseRepository _IDataBaseRepository;
        private readonly IWebHostEnvironment _env;

        public GoogleDriveFilesRepository(IConfigurationRepository IConfigurationRepository, IDataBaseRepository IDataBaseRepository, IWebHostEnvironment env)
        {
            _IConfigurationRepository = IConfigurationRepository;
            _IDataBaseRepository = IDataBaseRepository;
            _env = env;
        }

        public void Dispose()
        {

        }

        public async Task<DriveService> GetService()
        {

            var resultConfig = await _IConfigurationRepository.GetConfiguration();
            var tokenResponse = new TokenResponse
            {
                RefreshToken = resultConfig.refresh_token

            };

            string driveCredPath = Path.Combine(_env.WebRootPath, "DmsDrive");

            var applicationName = "Dms"; // Use the name of the project in Google Cloud
            var username = "user"; // Use your email


            var apiCodeFlow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = resultConfig.client_id,
                    ClientSecret = resultConfig.client_secret

                },

                Scopes = Scopes,
                DataStore = new FileDataStore(driveCredPath, true)

            });


            var credential = new UserCredential(apiCodeFlow, username, tokenResponse);


            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = applicationName
            });

            return service;
        }

        public async Task<ResultObjectDTO<List<FileResponse>>> GetDriveFiles(string fileId = null)
        {
            ResultObjectDTO<List<FileResponse>> FileList = new ResultObjectDTO<List<FileResponse>>();
            try
            {
                DriveService service = await GetService();
                var request = service.Files.List();

                if (!string.IsNullOrEmpty(fileId))
                    request.Q = $"parents in '{fileId}' ";
                else
                    request.Q = "parents in 'root' or (sharedWithMe = true)";

                request.Fields = "nextPageToken, files(id, name, createdTime, mimeType, description, fileExtension, webViewLink, capabilities, parents)";
               
                var requestResult = await request.ExecuteAsync();

                FileList.ResultData = new List<FileResponse>();
                if (requestResult.Files != null && requestResult.Files.Count > 0)
                {
                    FileList.Result = ResultType.Success;
                    FileList.ResultMessage = ResultType.Success.ToString();
                    foreach (var file in requestResult.Files)
                    {
                        FileResponse File = new FileResponse
                        {
                            unique_id = file.Id,
                            file_name = file.Name,
                            file_attribute = file.Description,
                            file_extension = file.FileExtension,
                            webViewLink = file.WebViewLink,
                            file_content_type = file.MimeType,
                            file_time_stamp = Convert.ToDateTime(file.CreatedTime),
                          
                        };
                        FileList.ResultData.Add(File);
                    }
                }
                else
                {
                    FileList.Result = ResultType.Error;
                    FileList.ResultMessage = "No Record Found";
                }
            }
            catch (Exception ex)
            {

                FileList.Result = ResultType.Error;
                FileList.ResultMessage = ex.Message;
            }

            return FileList;
        }

        public async Task<ResultObjectDTO<string>> GenerateRefeshToken(string authorization_code)
        {

            ResultObjectDTO<string> resultDrive = new ResultObjectDTO<string>();

            var resultConfig = await _IConfigurationRepository.GetConfiguration();
            try
            {

                var client = new RestClient($"https://accounts.google.com/o/oauth2");
                var request = new RestRequest($"/token", RestSharp.Method.Post);
                var postData = new
                {
                    code = authorization_code,
                    resultConfig.client_id,
                    resultConfig.client_secret,
                    resultConfig.redirect_uri,
                    grant_type = "authorization_code"
                };
                var body = JsonConvert.SerializeObject(postData);
                request.AddBody(body, "application/json");
                var response = client.Post(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var content = response.Content;
                    var resData = JsonConvert.DeserializeObject<GoogleToken>(content);
                    if (!string.IsNullOrEmpty(resData.refresh_token))
                    {
                        resultDrive.Result = ResultType.Success;
                        resultDrive.ResultMessage = "Success";
                        resultDrive.ResultData = resData.refresh_token;
                    }
                    else
                    {
                        resultDrive.Result = ResultType.Error;
                        resultDrive.ResultMessage = "Refresh Token Not Valid";
                    }
                }
                else
                {
                    resultDrive.Result = ResultType.Error;
                    resultDrive.ResultMessage = response.Content;
                }
            }
            catch (Exception ex)
            {
                resultDrive.Result = ResultType.Error;
                resultDrive.ResultMessage = ex.Message;
            }


            return resultDrive;
        }

        public async Task<ResultObjectDTO<FileResponse>> FileUpload(FileRequestApiModel fileRequest)
        {
            ResultObjectDTO<FileResponse> resultUpload = new ResultObjectDTO<FileResponse>();
            resultUpload.ResultData = new FileResponse();
            try
            {
                DriveService service = await GetService();

                var FileMetaData = new Google.Apis.Drive.v3.Data.File();
                FileMetaData.Name = Path.GetFileName(fileRequest.file.FileName);
                FileMetaData.MimeType = fileRequest.file.ContentType;
                FileMetaData.Description = fileRequest.file_attribute;
                if (!string.IsNullOrEmpty(fileRequest.unique_id))
                {
                    FileMetaData.Parents = new List<string>
                    {
                        fileRequest.unique_id
                    };
                }

                FilesResource.CreateMediaUpload request;
                using (var ms = new MemoryStream())
                {
                    await fileRequest.file.CopyToAsync(ms);
                    request = service.Files.Create(FileMetaData, ms, FileMetaData.MimeType);
                    request.Fields = "id, webViewLink";
                    await request.UploadAsync();

                    var file1 = request.ResponseBody;
                    resultUpload.Result = ResultType.Success;
                    resultUpload.ResultMessage = ResultType.Success.ToString();
                    resultUpload.ResultData.file_id = file1.Id;
                    resultUpload.ResultData.webViewLink = file1.WebViewLink;
                }
            }
            catch (Exception ex)
            {

                resultUpload.Result = ResultType.Error;
                resultUpload.ResultMessage = ex.Message;
            }

            return resultUpload;
        }


        public async Task<ResultObjectDTO<string>> FileDownload(string fileId)
        {
            ResultObjectDTO<string> fileResult = new ResultObjectDTO<string>();
            try
            {
                DriveService service = await GetService();

                FilesResource.GetRequest request = service.Files.Get(fileId);
                string FileName = request.Execute().Name;
                string MimeType = request.Execute().MimeType;


                string FilePath = Path.Combine(_env.WebRootPath, "Downloads", FileName);
                var stream = new MemoryStream();
                if (MimeType.Contains("google-apps.spreadsheet"))
                {
                    FilePath = $"{FilePath}.xlsx";
                    var exportResult = service.Files.Export(fileId, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                    await exportResult.DownloadAsync(stream);

                }
                else
                {
                    await request.DownloadAsync(stream);

                }
                SaveStream(stream, FilePath);


                if (System.IO.File.Exists(FilePath))
                {
                    fileResult.Result = ResultType.Success;
                    fileResult.ResultMessage = ResultType.Success.ToString();
                    fileResult.ResultData = FilePath;
                }
                else
                {
                    fileResult.Result = ResultType.Error;
                    fileResult.ResultMessage = "File Not Found";

                }


            }
            catch (Exception ex)
            {
                fileResult.Result = ResultType.Error;
                fileResult.ResultMessage = ex.Message.ToString();
            }
            return fileResult;
        }
        private static void SaveStream(MemoryStream stream, string saveTo)
        {
            using (FileStream file = new FileStream(saveTo, FileMode.Create, FileAccess.Write))
            {
                stream.WriteTo(file);
            }
        }

        public async Task<ResultObjectDTO<FileResponse>> GetFileInfo(string fileId)
        {
            ResultObjectDTO<FileResponse> fileResult = new ResultObjectDTO<FileResponse>();
            fileResult.ResultData = new FileResponse();
            try
            {
                DriveService service = await GetService();
                var request = service.Files;

                var fileRequest = request.Get(fileId);
                fileRequest.Fields = "webViewLink, capabilities, permissions, webContentLink, copyRequiresWriterPermission";
                var  file=await fileRequest.ExecuteAsync();

                if (file != null)
                {
                    fileResult.Result = ResultType.Success;
                    fileResult.ResultMessage = ResultType.Success.ToString();
                    fileResult.ResultData.webViewLink = file.WebViewLink;
                    fileResult.ResultData.permissions = file.Permissions;
                    fileResult.ResultData.webContentLink = file.WebContentLink;
                }
                else
                {
                    fileResult.Result = ResultType.Error;
                    fileResult.ResultMessage = "The file you are trying to access is not found in your drive.";
                }

            }
            catch (Exception ex)
            {
                //fileResult.Result = ResultType.Error;
                fileResult.Result = ResultType.Error;
                fileResult.ResultMessage = "The file you are trying to access is not found in your drive.";
            }
            return fileResult;
        }
        
    }
}

