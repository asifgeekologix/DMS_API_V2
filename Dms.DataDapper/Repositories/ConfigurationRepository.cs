using Dapper;
using Dms.Domain.ApiModels;
using Dms.Domain.DbInfo;
using Dms.Domain.Entities.Dms_BaseObjects;
using Dms.Domain.Repositories;
using Dms.Domain.Utilities;
using MySql.Data.MySqlClient;

namespace Dms.DataDapper.Repositories
{
    public class ConfigurationRepository :BaseRepository, IConfigurationRepository
    {
        public ConfigurationRepository(DbInfo dbInfo) : base(dbInfo)
        {
        }

        public void Dispose()
        {
           
        }
        public async Task<UserConfiguration> GetConfiguration()
        {
            UserConfiguration resQuote = new UserConfiguration();
            try
            {
                using (conn = new MySqlConnection(_dbInfo.ConnectionStrings))
                {
                    resQuote = await conn.QueryFirstOrDefaultAsync<UserConfiguration>("SELECT  * FROM user_configuration limit 1;");
                }

            }
            catch (Exception ex)
            {
                throw;

            }
            return resQuote;
        }

        public async Task<ResultObjectDTO<UserConfiguration>> SaveConfiguration(ConfigurationApiModel configurationApiModel)
        {
            ResultObjectDTO<UserConfiguration> resQuote = new ResultObjectDTO<UserConfiguration>();
            try
            {
                using (conn = new MySqlConnection(_dbInfo.ConnectionStrings))
                {
                    string sql = $"Update user_configuration set cloud_type=@cloud_type,refresh_token=@refresh_token,is_connect=@is_connect,host_connection=@host_connection";
                    await conn.ExecuteAsync(sql, configurationApiModel);
                    resQuote.ResultData = await conn.QueryFirstOrDefaultAsync<UserConfiguration>("SELECT  * FROM user_configuration limit 1;");
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

        public ResultObjectDTO<string> CheckConnection(ConfigurationApiModel configurationApiModel)
        {
            ResultObjectDTO<string> resQuote = new ResultObjectDTO<string>();
            try
            {
                string ConnectionString = $"Server={configurationApiModel.host_name};database={configurationApiModel.host_database};uid={configurationApiModel.host_user};pwd={configurationApiModel.host_password};Port={configurationApiModel.host_port};";
                using (conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    resQuote.Result = ResultType.Success;
                    resQuote.ResultMessage = "Success";
                    resQuote.ResultData = ConnectionString;
                }

            }
            catch (Exception ex)
            {

                resQuote.ResultMessage = "Connection Not Connected Please Check  Credentials First";
                resQuote.Result = ResultType.Error;

            }
            return resQuote;
        }

    }
}
