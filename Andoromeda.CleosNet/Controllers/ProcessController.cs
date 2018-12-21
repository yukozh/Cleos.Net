using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Andoromeda.CleosNet.Controllers
{
    [Route("api/[controller]")]
    public class ProcessController : BaseController
    {
        [HttpPut]
        [HttpPost]
        [HttpPatch]
        public object Run(string file, string args, string workDir, string stdin = null, int timeout = 30000)
        {
            var startInfo = new ProcessStartInfo(file, args);

            if (!string.IsNullOrEmpty(workDir))
            {
                startInfo.WorkingDirectory = workDir;
            }

            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            if (!string.IsNullOrEmpty(args))
            {
                startInfo.Arguments = args;
            }

            if (!string.IsNullOrEmpty(stdin))
            {
                startInfo.RedirectStandardInput = true;
            }

            var process = Process.Start(startInfo);

            if (!string.IsNullOrEmpty(stdin))
            {
                process.StandardInput.WriteLine(stdin);
                process.StandardInput.Close();
            }

            if (!process.WaitForExit(timeout))
            {
                process.Kill();
                process.Dispose();
                return ApiResult(400, "Process exceeded the time limitation");
            }

            return ApiResult(new
            {
                exitCode = process.ExitCode,
                stdout = process.StandardOutput.ReadToEnd(),
                stderr = process.StandardError.ReadToEnd()
            });
        }
    }
}
