using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dms.Domain.Utilities
{
    public class ResultObjectDTO<T>
    {

        public ResultType Result;

        public string ResultMessage;

        public T ResultData;
    }
}
