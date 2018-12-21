using System;
using System.Collections.Generic;
using System.Text;

namespace Andoromeda.CleosNet.Client
{
    public class ApiResult<T>
    {
        public int code { get; set; }

        public string msg { get; set; }

        public T data { get; set; }
    }
}
