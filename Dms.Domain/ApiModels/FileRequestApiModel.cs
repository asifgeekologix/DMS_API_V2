using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dms.Domain.ApiModels
{
    public class FileRequestApiModel
    {
        public string unique_id { get; set; }
        public int cloud_type { get; set; }
        public string file_name { get; set; }
        public DateTime file_time_stamp { get; set; }
        public string file_attribute { get; set; }
        public string file_extension { get; set; }
        public string file_content_type { get; set; }

        public IFormFile file { get; set; }

        public string file_data { get; set; }

        public string file_id { get; set; }

        public string webViewLink { get; set; }
    }
}
