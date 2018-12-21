using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Net.Http;
using System.Linq;
using Newtonsoft.Json;

namespace Andoromeda.CleosNet.Client
{
    public class CleosClient
    {
        private readonly string _node;
        private readonly string _wallet;
        private HttpClient _client;

        public CleosClient() : this("http://eos.greymass.com", "http://localhost:8900")
        {
            _client = new HttpClient { BaseAddress = new Uri(_node) };
        }

        public CleosClient(string node, string wallet)
        {
            _node = node;
            _wallet = wallet;
        }

        public async Task<ClientResult> CreateWalletAsync(string name, string privateKeyPath, CancellationToken cancellationToken = default)
        {
            using (var result = await _client.PostAsync("/api/process", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "file", "cleos" },
                { "args", $"wallet create -n {name} -f {privateKeyPath}" }
            }), cancellationToken))
            {
                var text = await result.Content.ReadAsStringAsync();
                var commandResult = JsonConvert.DeserializeObject<ApiResult<CommandResult>>(text).data;

                return new ClientResult
                {
                    Error = commandResult.Stderr,
                    IsSucceeded = commandResult.ExitCode == 0,
                    Output = commandResult.Stdout
                };
            }
        }

        public async Task<ClientResult<string>> GetWalletPrivateKeyAsync(string privateKeyPath, CancellationToken cancellationToken = default)
        {
            using (var result = await _client.GetAsync("/api/file?path=" + privateKeyPath, cancellationToken))
            {
                var text = await result.Content.ReadAsStringAsync();
                var fileResult = JsonConvert.DeserializeObject<ApiResult<GetFileResult>>(text);

                return new ClientResult<string>
                {
                    IsSucceeded = fileResult.code == 200,
                    Result = Encoding.Default.GetString(Convert.FromBase64String(fileResult.data.Base64))
                };
            }
        }

        public async Task<ClientResult> ImportPrivateKeyToWalletAsync(string privateKey, CancellationToken cancellationToken = default)
        {
            using (var result = await _client.PostAsync("/api/process", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "file", "cleos" },
                { "args", $"-u {_node} --wallet-url http://localhost:8888 wallet import --private-key " + privateKey }
            }), cancellationToken))
            {
                var text = await result.Content.ReadAsStringAsync();
                var commandResult = JsonConvert.DeserializeObject<ApiResult<CommandResult>>(text).data;

                return new ClientResult
                {
                    Error = commandResult.Stderr,
                    IsSucceeded = commandResult.ExitCode == 0,
                    Output = commandResult.Stdout
                };
            }
        }

        public async Task<ClientResult> PushActionAsync(string code, string method, string account, string permission, IEnumerable<object> args, CancellationToken cancellationToken = default)
        {
            using (var result = await _client.PostAsync("/api/process", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "file", "bash" },
                { "stdin", $"cleos -u {_node} --wallet-url {_wallet} push action {code} {method} '{JsonConvert.SerializeObject(args)}' -p{account}@{permission}" }
            }), cancellationToken))
            {
                var text = await result.Content.ReadAsStringAsync();
                var commandResult = JsonConvert.DeserializeObject<ApiResult<CommandResult>>(text).data;

                return new ClientResult
                {
                    Error = commandResult.Stderr,
                    IsSucceeded = commandResult.ExitCode == 0,
                    Output = commandResult.Stdout
                };
            }
        }

        public async Task<ClientResult> UnlockWalletAsync(string password, CancellationToken cancellationToken = default)
        {
            using (var result = await _client.PostAsync("/api/process", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "file", "cleos" },
                { "args", $"--wallet-url {_wallet} wallet unlock --password " + password }
            }), cancellationToken))
            {
                var text = await result.Content.ReadAsStringAsync();
                var commandResult = JsonConvert.DeserializeObject<ApiResult<CommandResult>>(text).data;

                return new ClientResult
                {
                    Error = commandResult.Stderr,
                    IsSucceeded = commandResult.ExitCode == 0,
                    Output = commandResult.Stdout
                };
            }
        }

        public async Task<ClientResult> SetContractAsync(string contractPath, string contractName, string account, string permission = "active", CancellationToken cancellationToken = default)
        {
            using (var result = await _client.PostAsync("/api/process", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "file", "cleos" },
                { "args", $"-u {_node} --wallet-url {_wallet} set contract {contractName} {contractPath} -p{account}@{permission}" }
            }), cancellationToken))
            {
                var text = await result.Content.ReadAsStringAsync();
                var commandResult = JsonConvert.DeserializeObject<CommandResult>(text);

                return new ClientResult
                {
                    Error = commandResult.Stderr,
                    IsSucceeded = commandResult.ExitCode == 0,
                    Output = commandResult.Stdout
                };
            }
        }

        public async Task<ClientResult<IEnumerable<Asset>>> GetCurrencyBalanceAsync(string code, string account, CancellationToken cancellationToken = default)
        {
            using (var result = await _client.PostAsync("/api/process", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "file", "cleos" },
                { "args", $"-u {_node} get currency balance {code} {account}" }
            }), cancellationToken))
            {
                var text = await result.Content.ReadAsStringAsync();
                var commandResult = JsonConvert.DeserializeObject<CommandResult>(text);

                return new ClientResult<IEnumerable<Asset>>
                {
                    Error = commandResult.Stderr,
                    IsSucceeded = commandResult.ExitCode == 0,
                    Output = commandResult.Stdout,
                    Result = commandResult.Stdout
                        .Split('\n')
                        .Where(x => !string.IsNullOrEmpty(x))
                        .Select(x => new Asset
                        {
                            Symbol = x.Split(' ')[1],
                            Amount = Convert.ToSingle(x.Split(' ')[0])
                        })
                        .ToList()
                };
            }
        }

        public async Task<ClientResult<Asset>> GetCurrencyBalanceAsync(string code, string account, string symbol, CancellationToken cancellationToken = default)
        {
            using (var result = await _client.PostAsync("/api/process", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "file", "cleos" },
                { "args", $"-u {_node} get currency balance {code} {account} {symbol}" }
            }), cancellationToken))
            {
                var text = await result.Content.ReadAsStringAsync();
                var commandResult = JsonConvert.DeserializeObject<CommandResult>(text);

                return new ClientResult<Asset>
                {
                    Error = commandResult.Stderr,
                    IsSucceeded = commandResult.ExitCode == 0,
                    Output = commandResult.Stdout,
                    Result = commandResult.Stdout
                        .Split('\n')
                        .Where(x => !string.IsNullOrEmpty(x))
                        .Select(x => new Asset
                        {
                            Symbol = x.Split(' ')[1],
                            Amount = Convert.ToSingle(x.Split(' ')[0])
                        })
                        .FirstOrDefault()
                };
            }
        }

        public async Task<ClientResult<BlockchainInfo>> GetBlockchainInfoAsync(string code, string account, string symbol, CancellationToken cancellationToken = default)
        {
            using (var result = await _client.PostAsync("/api/process", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "file", "cleos" },
                { "args", $"-u {_node} get currency balance {code} {account} {symbol}" }
            }), cancellationToken))
            {
                var text = await result.Content.ReadAsStringAsync();
                var commandResult = JsonConvert.DeserializeObject<CommandResult>(text);

                return new ClientResult<BlockchainInfo>
                {
                    Error = commandResult.Stderr,
                    IsSucceeded = commandResult.ExitCode == 0,
                    Output = commandResult.Stdout,
                    Result = JsonConvert.DeserializeObject<BlockchainInfo>(commandResult.Stdout)
                };
            }
        }

        public async Task<ClientResult> LaunchOneBoxAsync(CancellationToken cancellationToken = default)
        {
            using (var result = await _client.PostAsync("/api/process/onebox/init", new FormUrlEncodedContent(new Dictionary<string, string>
            {
            }), cancellationToken))
            {
                var text = await result.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<ApiResult<object>>(text);

                return new ClientResult
                {
                    IsSucceeded = response.code == 201,
                };
            }
        }

        public async Task<OneBoxStatusResult> WaitForOneBoxReadyAsync()
        {
            using (var result = await _client.GetAsync("/api/process/onebox/status"))
            {
                var text = await result.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<ApiResult<OneBoxStatusResult>>(text);
                if (response.data.Status == "NotLaunched")
                {
                    throw new InvalidOperationException("You should launch onebox first.");
                }

                if (response.data.Status == "Launching")
                {
                    await Task.Delay(1000);
                    return await WaitForOneBoxReadyAsync();
                }

                return response.data;
            }
        }

        public async Task<ClientResult> CompileSmartContractAsync(string path, CancellationToken cancellationToken = default)
        {
            using (var result = await _client.PostAsync("/api/file/file", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "base64", "mkdir build\r\ncd build\r\ncmake ..\r\nmake\r\n" },
                { "path", System.IO.Path.Combine(path, "build.sh")}
            })))
            {
            }

            using (var result = await _client.PostAsync("/api/process", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "file", "build.sh" },
                { "workDir", System.IO.Path.Combine(path, "build.sh") }
            }), cancellationToken))
            {
                var text = await result.Content.ReadAsStringAsync();
                var commandResult = JsonConvert.DeserializeObject<CommandResult>(text);

                return new ClientResult<BlockchainInfo>
                {
                    Error = commandResult.Stderr,
                    IsSucceeded = commandResult.ExitCode == 0,
                    Output = commandResult.Stdout,
                    Result = JsonConvert.DeserializeObject<BlockchainInfo>(commandResult.Stdout)
                };
            }
        }
    }
}
