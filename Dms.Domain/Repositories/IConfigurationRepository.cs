using Dms.Domain.ApiModels;
using Dms.Domain.Entities.Dms_BaseObjects;
using Dms.Domain.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dms.Domain.Repositories
{
    public interface IConfigurationRepository: IDisposable
    {
        Task<UserConfiguration> GetConfiguration();
        Task<ResultObjectDTO<UserConfiguration>> SaveConfiguration(ConfigurationApiModel configurationApiModel);

        ResultObjectDTO<string> CheckConnection(ConfigurationApiModel configurationApiModel);
    }
}
