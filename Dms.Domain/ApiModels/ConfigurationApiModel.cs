using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dms.Domain.ApiModels
{
    public class ConfigurationApiModel
    {
        public int cloud_type { get; set; }

        public string auth_code { get; set; }

        public string refresh_token { get; set; }
      
        public bool is_connect { get; set; }

        public string host_name { get; set; }
        public string host_database { get; set; }
        public string host_user { get; set; }
        public string host_password { get; set; }
        public int host_port { get; set; }
        public string host_connection { get; set; }

    }
}
