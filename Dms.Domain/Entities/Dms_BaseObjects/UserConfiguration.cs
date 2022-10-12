using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dms.Domain.Entities.Dms_BaseObjects
{
    public class UserConfiguration
    {

        public int cloud_type { get; set; }
        public string refresh_token { get; set; }
        public bool is_connect { get; set; }
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string redirect_uri { get; set; }

        public string host_connection  { get; set; }
    }
}
