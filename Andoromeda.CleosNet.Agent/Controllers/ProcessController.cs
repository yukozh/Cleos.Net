using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Andoromeda.CleosNet.Agent.Models;
using Andoromeda.CleosNet.Agent.Services;

namespace Andoromeda.CleosNet.Agent.Controllers
{
    [Route("api/[controller]")]
    public class ProcessController : BaseController
    {
        private static LaunchStatus _status = LaunchStatus.NotLaunched;

        public enum LaunchStatus
        {
            NotLaunched,
            Launching,
            Active,
            LaunchFailed
        }

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

        [HttpPut("onebox/init")]
        [HttpPost("onebox/init")]
        [HttpPatch("onebox/init")]
        public async Task<ApiResult> Init(bool? safeMode)
        {
            if (_status != LaunchStatus.NotLaunched && _status != LaunchStatus.LaunchFailed)
            {
                return ApiResult(409, $"The EOS is under {_status} status.");
            }

            Task.Factory.StartNew(() => {
                using (var serviceScope = HttpContext.RequestServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var eos = serviceScope.ServiceProvider.GetService<OneBoxService>();
                    _status = LaunchStatus.Launching;
                    if (eos.Launch(safeMode.HasValue ? safeMode.Value : false))
                    {
                        _status = LaunchStatus.Active;
                    }
                    else
                    {
                        _status = LaunchStatus.LaunchFailed;
                    }
                }
            });
            
            return ApiResult(201, "Lauching...");
        }

        [HttpPut("onebox/stop")]
        [HttpPost("onebox/stop")]
        [HttpPatch("onebox/stop")]
        public ApiResult Stop(bool? safeMode, [FromServices] OneBoxService eos)
        {
            if (_status != LaunchStatus.Active && _status != LaunchStatus.Launching)
            {
                return ApiResult(409, $"The EOS is under {_status} status.");
            }

            if (safeMode.HasValue && safeMode.Value)
            {
                eos.GracefulShutdown();
            }
            else
            {
                eos.ForceShutdown();
            }
            _status = LaunchStatus.NotLaunched;
            return ApiResult(200, "Succeeded");
        }

        [HttpGet("onebox/status")]
        public async Task<ApiResult<object>> Status([FromServices] OneBoxService eos)
        {
            string chainId = null;
            if (_status == LaunchStatus.Active)
            {
                chainId = await eos.RetriveChainIdAsync();
            }
            return ApiResult<object>(new
            {
                Status = _status.ToString(),
                ChainId = chainId,
                LogStreamId = eos.GetOneBoxProcId()
            });
        }
    }
}
