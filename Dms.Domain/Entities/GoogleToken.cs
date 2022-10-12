using Ubiety.Dns.Core;

namespace Dms.Domain.Entities
{
    public class GoogleToken
    {
        public bool success { get; set; }
        public string access_token { get; set; }
        public Request Request { get; set; }
        public int expires_in { get; set; }
        public Response Response { get; set; }
        public string refresh_token { get; set; }
    }
}
