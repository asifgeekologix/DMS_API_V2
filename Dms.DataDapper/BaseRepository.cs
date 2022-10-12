using Dms.Domain.DbInfo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dms.DataDapper
{
    public class BaseRepository
    {
        public readonly DbInfo _dbInfo;
        public BaseRepository(DbInfo dbInfo)
        {
            this._dbInfo = dbInfo;
        }
        public IDbConnection conn;
     
    }
}
