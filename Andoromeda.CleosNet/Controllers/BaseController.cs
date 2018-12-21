using Microsoft.AspNetCore.Mvc;
using Andoromeda.CleosNet.Models;

namespace Andoromeda.CleosNet.Controllers
{
    public class BaseController : Controller
    {
        protected ApiResult<T> ApiResult<T>(T data, int code = 200)
        {
            Response.StatusCode = code;
            return new ApiResult<T>
            {
                code = code,
                data = data,
                msg = "ok"
            };
        }

        protected ApiResult ApiResult(int code, string msg)
        {
            Response.StatusCode = code;
            return new ApiResult
            {
                code = code,
                msg = msg
            };
        }
    }
}
