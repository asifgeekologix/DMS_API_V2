using Dms.Domain.ApiModels;
using Dms.Domain.Entities.Dms_BaseObjects;
using Dms.Domain.Utilities;
using Google.Apis.Drive.v3;

namespace Dms.API.Services
{
    public interface IGoogleDriveFilesRepository : IDisposable
    {
        Task<DriveService> GetService();

        Task<ResultObjectDTO<List<FileResponse>>> GetDriveFiles(string fileId = null);

        Task<ResultObjectDTO<string>> GenerateRefeshToken(string authorization_code);

        Task<ResultObjectDTO<FileResponse>> FileUpload(FileRequestApiModel fileRequest);

        Task<ResultObjectDTO<string>> FileDownload(string fileId);

        Task<ResultObjectDTO<FileResponse>> GetFileInfo(string fileId);

    }
}
