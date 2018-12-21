using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Andoromeda.CleosNet.Agent.Models;

namespace Andoromeda.CleosNet.Agent.Services
{
    public class OneBoxService
    {
        internal const string _eosioToken = "eosio.token";
        internal const string _dasdaqRootPath = "/home/cleos-net";
        internal const string _privateKeyFilePath = "/home/cleos-net/wallet/privatekey.txt";
        internal const string _walletPath = "/mnt/dev/data/default.wallet";

        private static HttpClient _client = new HttpClient() { BaseAddress = new Uri("http://127.0.0.1:8888") };
        private static OneBoxProcess _oneboxProc;
        private ProcessService _proc;

        public OneBoxService(ProcessService proc)
        {
            if (!Directory.Exists(_dasdaqRootPath))
            {
                Directory.CreateDirectory(_dasdaqRootPath);
            }
            
            _proc = proc;
        }

        public Guid? GetOneBoxProcId()
        {
            if (_oneboxProc != null)
            {
                return _oneboxProc.Id;
            }
            else
            {
                return null;
            }
        }

        public bool Launch(bool safeMode = false)
        {
            try
            {
                if (Process.GetProcessesByName("nodeos").Length > 0)
                {
                    return false;
                }
                if (safeMode)
                {
                    Console.WriteLine("[Agent] Starting in safe mode, force removing existed wallet.");
                    EnsureRemoveDefaultWallet();
                }
                var id = StartEosNode();

                Console.WriteLine("[Agent] Starting EOS node, OneBox Proc Id = " + id);
                WaitEosNodeAsync().Wait();
                Console.WriteLine("[Agent] EOS node web API is ready.");
                if (!File.Exists(_walletPath))
                {
                    Console.WriteLine("[Agent] Wallet is not found, generating...");
                    GenerateWallet();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Agent] An error occurred while launching EOS: \r\n" + ex.ToString());
                return false;
            }

            return true;
        }

        public void GracefulShutdown()
        {
            ExecuteCommand("kill -15 " + _oneboxProc.Process.Id);
            _oneboxProc = null;
        }

        public void ForceShutdown()
        {
            ExecuteCommand("kill -15 " + _oneboxProc.Process.Id);
            _oneboxProc = null;
        }

        public CommandResult ExecuteCommand(string command, string workDir = null)
        {
            var startInfo = new ProcessStartInfo("bash");
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardInput = true;
            if (workDir != null)
            {
                startInfo.WorkingDirectory = workDir;
            }
            var process = Process.Start(startInfo);
            process.StandardInput.WriteLine(command);
            process.StandardInput.Close();
            process.WaitForExit();
            var result = new CommandResult
            {
                ErrorOutput = process.StandardError.ReadToEnd(),
                StandardOutput = process.StandardOutput.ReadToEnd(),
                ExitCode = process.ExitCode,
                IsSucceeded = process.ExitCode == 0
            };

            if (!string.IsNullOrEmpty(result.ErrorOutput))
            {
                PushCleosLogsToEosChannel(result.ErrorOutput, true, process.Id);
            }

            if (!string.IsNullOrEmpty(result.StandardOutput))
            {
                PushCleosLogsToEosChannel(result.StandardOutput, false, process.Id);
            }

            return result;
        }

        public CommandResult ExecuteCleosCommand(string command)
        {
            return ExecuteCommand($"cleos -u http://0.0.0.0:8888 --wallet-url http://0.0.0.0:8888 {command}");
        }

        public CommandResult ExecuteEosioCppCommand(string command, string workDir)
        {
            return ExecuteCommand($"eosiocpp {command}", workDir);
        }

        public CommandResult ExecuteGitCommand(string command, string workDir)
        {
            return ExecuteCommand($"git {command}", workDir);
        }
        
        public Guid StartEosNode()
        {
            var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
            var pluginsCommand = string.Join(' ', config.plugins.Select(x => $"--plugin {x}"));
            var startInfo = new ProcessStartInfo("/opt/eosio/bin/nodeos", $"-e -p eosio {pluginsCommand} -d /mnt/dev/data --config-dir /mnt/dev/config --http-server-address=0.0.0.0:8888 --access-control-allow-origin=* --contracts-console --http-validate-host=false --delete-all-blocks");
            _oneboxProc = _proc.StartProcess(startInfo, async (id, x) => {
                try
                {
                    //await _hub.Clients.All.SendAsync("onLogReceived", id, x.IsError, x.Text);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[Agent] " + ex.ToString());
                }
            }, "nodeos");
            Task.Factory.StartNew(() => {
                // Start bash to launch nodeos
                _oneboxProc.Process.Start();
                _oneboxProc.Process.WaitForExit();
            }).ConfigureAwait(false);
            return _oneboxProc.Id;
        }

        public async Task WaitEosNodeAsync()
        {
            while (true)
            {
                try
                {
                    using (var response = await _client.GetAsync("/"))
                    {
                        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            break;
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    await Task.Delay(1000);
                }
            }
        }

        public (string publicKey, string privateKey) RetriveSignatureProviderKey()
        {
            const string configPath = "/config.ini";
            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException(configPath);
            }

            var lines = File.ReadAllLines(configPath);
            var signatureLine = lines.SingleOrDefault(x => x.StartsWith("signature-provider"));
            if (signatureLine == null)
            {
                throw new Exception("Line signature-provider was not found");
            }

            var splitedStrings = signatureLine.Split('=');
            var publicKey = splitedStrings[1].Trim();
            var privateKey = splitedStrings[2].Trim().Substring(4);

            return (publicKey, privateKey);
        }

        public async Task<string> RetriveChainIdAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var response = await _client.GetAsync("/v1/chain/get_info"))
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var getChainInfoResponse = JsonConvert.DeserializeObject<GetChainInfoResponseBody>(jsonString);
                return getChainInfoResponse.chain_id;
            }
        }

        public string GenerateWallet()
        {
            // Start cleos to create a wallet
            var result = ExecuteCleosCommand("wallet create");
            var output = result.StandardOutput;

            // Find out the private key string
            var regex = new Regex("(?<=\").*?(?=\")");
            var matchResult = regex.Match(output);
            if (!matchResult.Success)
            {
                var error = "Wallet create failed. \r\n Output: \r\n" + output;
                Console.Error.WriteLine(error);
                throw new Exception(error);
            }

            StoreWalletPrivateKey(matchResult.Value);
            return matchResult.Value;
        }

        public void StoreWalletPrivateKey(string privateKey)
        {
            File.WriteAllText(_privateKeyFilePath, privateKey);
        }

        public string GetPrivateKey()
        {
            if (!File.Exists(_privateKeyFilePath))
            {
                throw new FileNotFoundException(_privateKeyFilePath);
            }

            return File.ReadAllText(_privateKeyFilePath);
        }

        public bool UnlockWallet()
        {
            // Start cleos to unlock the wallet
            return ExecuteCleosCommand($"wallet unlock --password {GetPrivateKey()}").IsSucceeded;
        }

        private void PushCleosLogsToEosChannel(string text, bool isError, int processId)
        {
            if (_oneboxProc == null)
            {
                return;
            }

            _oneboxProc.Logs.Add(new Log
            {
                IsError = isError,
                ProcessId = processId,
                Text = text,
                Time = DateTime.Now
            });

            try
            {
                //_hub.Clients.All.SendAsync("onLogReceived", _oneboxProc.Id, isError, text);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Agent] " + ex.ToString());
            }
        }
        
        private class GetChainInfoResponseBody
        {
            public string chain_id { get; set; }
        }

        private bool EnsureRemoveDefaultWallet()
        {
            var walletPath = _walletPath;
            var result = ExecuteCommand("rm -rf " + walletPath);
            return result.IsSucceeded;
        }
    }
}