using Dms.Domain.ApiModels;
using Dms.Domain.Entities.Dms_BaseObjects;
using Dms.Domain.Utilities;

namespace Dms.Domain.Repositories
{
    public interface IDataBaseRepository: IDisposable
    {
       
        Task<ResultObjectDTO<List<FileResponse>>> GetFilesAll(string host_connection);

        Task<ResultObjectDTO<List<FileResponse>>> SearchFiles(string host_connection ,FileRequestApiModel requestApiModel);

        Task<ResultObjectDTO<FileResponse>> FileUpload(string host_connection,FileRequestApiModel requestApiModel);

        Task<ResultObjectDTO<FileResponse>> GetFileById(string host_connection,string unique_id);

 
    }
}
