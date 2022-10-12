using Dapper;
using Dms.Domain.ApiModels;
using Dms.Domain.DbInfo;
using Dms.Domain.Entities;
using Dms.Domain.Entities.Dms_BaseObjects;
using Dms.Domain.Repositories;
using Dms.Domain.Utilities;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Dms.DataDapper.Repositories
{
    public class DataBaseRepository:BaseRepository,IDataBaseRepository
    {
        public DataBaseRepository(DbInfo dbInfo) : base(dbInfo)
        {

        }

        public void Dispose()
        {

        }

        public async Task<ResultObjectDTO<List<FileResponse>>> GetFilesAll(string host_connection)
        {
            ResultObjectDTO<List<FileResponse>> resQuote = new ResultObjectDTO<List<FileResponse>>();
            try
            {
                string sql = $"SELECT df.unique_id,df.cloud_type,df.file_name,df.file_time_stamp,df.file_attribute,df.file_extension,df.file_content_type FROM drive_files df order by df.file_time_stamp desc;";
                using (conn = new MySqlConnection(host_connection))
                {
                    var dbResult = await conn.QueryAsync<FileResponse>(sql);

                    if (dbResult.Count() > 0)
                    {
                        resQuote.Result = ResultType.Success;
                        resQuote.ResultMessage = "Success";
                        resQuote.ResultData = dbResult.ToList();
                    }
                    else
                    {
                        resQuote.Result = ResultType.Error;
                        resQuote.ResultMessage = "No Record Found";
                    }
                }
                
                
            }
            catch (Exception ex)
            {
                resQuote.ResultMessage = ex.Message.ToString();
                resQuote.Result = ResultType.Error;

            }
            return resQuote;
        }

        public async Task<ResultObjectDTO<List<FileResponse>>> SearchFiles(string host_connection, FileRequestApiModel requestApiModel)
        {
            ResultObjectDTO<List<FileResponse>> resQuote = new ResultObjectDTO<List<FileResponse>>();
            try
            {
                string sql = $"SELECT df.unique_id,df.cloud_type,df.file_name,df.file_time_stamp,df.file_attribute,df.file_extension,df.file_content_type FROM drive_files df WHERE df.file_attribute LIKE '%{requestApiModel.file_attribute}%' OR df.file_name LIKE '%{requestApiModel.file_attribute}%' order by df.file_time_stamp desc;";
                using (conn = new MySqlConnection(host_connection))
                {
                    var dbResult = await conn.QueryAsync<FileResponse>(sql);

                    if (dbResult.Count() > 0)
                    {
                        resQuote.Result = ResultType.Success;
                        resQuote.ResultMessage = "Success";
                        resQuote.ResultData = dbResult.ToList();
                    }
                    else
                    {
                        resQuote.Result = ResultType.Error;
                        resQuote.ResultMessage = "No Record Found";
                    }
                }


            }
            catch (Exception ex)
            {
                resQuote.ResultMessage = ex.Message.ToString();
                resQuote.Result = ResultType.Error;

            }
            return resQuote;
        }

        public async Task<ResultObjectDTO<FileResponse>> FileUpload(string host_connection,FileRequestApiModel requestApiModel)
        {
            ResultObjectDTO<FileResponse> resQuote = new ResultObjectDTO<FileResponse>();
            try
            {
                using (conn = new MySqlConnection(host_connection))
                {
                    string sql = $"Insert Into drive_files (unique_id,cloud_type,file_name,file_time_stamp,file_attribute,file_extension,file_content_type,file_data,file_id,webViewLink) Values (@unique_id,@cloud_type,@file_name,@file_time_stamp,@file_attribute,@file_extension,@file_content_type,@file_data,@file_id,@webViewLink)";
                    await conn.ExecuteAsync(sql, requestApiModel);
                    resQuote.ResultData = await conn.QueryFirstOrDefaultAsync<FileResponse>("SELECT  unique_id,cloud_type,file_name,file_time_stamp,file_attribute,file_extension,file_content_type,file_id,webViewLink FROM drive_files where unique_id=@unique_id;", new { unique_id = requestApiModel.unique_id });
                }

                if (resQuote.ResultData != null)
                {
                    resQuote.Result = ResultType.Success;
                    resQuote.ResultMessage = "Suceess";
                }

            }
            catch (Exception ex)
            {
                resQuote.ResultMessage = ex.Message.ToString();
                resQuote.Result = ResultType.Error;

            }
            return resQuote;
        }
        public async Task<ResultObjectDTO<FileResponse>> GetFileById(string host_connection,string unique_id)
        {
            ResultObjectDTO<FileResponse> resQuote = new ResultObjectDTO<FileResponse>();
            try
            {
                using (conn = new MySqlConnection(host_connection))
                {
                     resQuote.ResultData = await conn.QueryFirstOrDefaultAsync<FileResponse>("SELECT * FROM drive_files where unique_id=@unique_id;", new { unique_id });
                }

                if (resQuote.ResultData != null)
                {
                    resQuote.Result = ResultType.Success;
                    resQuote.ResultMessage = "Suceess";
                }
                else
                {
                    resQuote.Result = ResultType.Error;
                    resQuote.ResultMessage = "No Record Found";
                }

            }
            catch (Exception ex)
            {
                resQuote.ResultMessage = ex.Message.ToString();
                resQuote.Result = ResultType.Error;

            }
            return resQuote;
        }

    
    }
}
