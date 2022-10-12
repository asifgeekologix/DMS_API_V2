using AutoMapper;
using Dms.Domain.Entities.Dms_BaseObjects;
using Dms.Domain.Entities.Dms_DTO;
using Dms.Domain.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dms.Domain.Mapper
{
    public class AutoMapperProfile: Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<FileResponse,FileResponseDTO>();
            CreateMap<ResultObjectDTO<FileResponse>, ResultObjectDTO<FileResponseDTO>>();
            CreateMap<ResultObjectDTO<List<FileResponse>>, ResultObjectDTO<List<FileResponseDTO>>>();

            CreateMap<UserConfiguration, UserConfigurationDTO>();
            CreateMap<ResultObjectDTO<UserConfiguration>, ResultObjectDTO<UserConfigurationDTO>>();
            
        }
    }
}
