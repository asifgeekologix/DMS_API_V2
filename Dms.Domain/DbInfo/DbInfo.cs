using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dms.Domain.DbInfo
{
    public class DbInfo
    {
        public string ConnectionStrings { get; }
        public DbInfo(string connectionStrings) => ConnectionStrings = connectionStrings;
    }
}
