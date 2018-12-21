using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Andoromeda.CleosNet.Agent.Models;

namespace Andoromeda.CleosNet.Agent.Controllers
{
    [Route("api/[controller]")]
    public class FileController : BaseController
    {
        [HttpGet("directory")]
        public object GetDirectory(string path, bool? subfolder)
        {
            if (!Directory.Exists(path))
            {
                return ApiResult(404, "Directory not found");
            }

            var ret = new Dictionary<string, string>();

            foreach (var x in Directory.EnumerateDirectories(path, "*", subfolder.HasValue && subfolder.Value ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
            {
                ret.Add(x, "folder");
            }

            foreach (var x in Directory.EnumerateFiles(path, "*", subfolder.HasValue && subfolder.Value ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
            {
                ret.Add(x, "file");
            }

            return ApiResult(ret);
        }

        [HttpPut("directory")]
        [HttpPost("directory")]
        [HttpPatch("directory")]
        public object CreateDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                return ApiResult(400, "Directory already exists");
            }

            Directory.CreateDirectory(path);
            return ApiResult(200, "ok");
        }

        [HttpDelete("directory")]
        public object DeleteDirectory(string path, bool? recursive)
        {
            if (!Directory.Exists(path))
            {
                return ApiResult(404, "Directory not found");
            }

            Directory.Delete(path, recursive.HasValue && recursive.Value);

            return ApiResult(200, "ok");
        }

        [HttpGet("file")]
        public object GetFile(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                return ApiResult(404, "File not found");
            }

            var fileInfo = new FileInfo(path);
            var base64 = Convert.ToBase64String(System.IO.File.ReadAllBytes(path));
            return ApiResult(new
            {
                base64,
                filename = Path.GetFileName(path),
                lastWrite = fileInfo.LastWriteTimeUtc,
                lastRead = fileInfo.LastAccessTimeUtc,
                createdAt = fileInfo.CreationTimeUtc
            });
        }

        [HttpPut("file")]
        [HttpPost("file")]
        [HttpPatch("file")]
        public object CreateFile(string path, string base64)
        {
            if (System.IO.File.Exists(path))
            {
                return ApiResult(400, "File already exists");
            }

            var bytes = Convert.FromBase64String(base64);
            System.IO.File.WriteAllBytes(path, bytes);
            return ApiResult(200, "ok");
        }

        [HttpDelete("file")]
        public object DeleteFile(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                return ApiResult(404, "File not found");
            }

            System.IO.File.Delete(path);
            return ApiResult(200, "ok");
        }
    }
}
