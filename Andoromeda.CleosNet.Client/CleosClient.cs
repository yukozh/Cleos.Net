﻿using System;
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
        private string _node;
        private readonly string _wallet;
        private HttpClient _client;

        public CleosClient() : this("http://eos.greymass.com", "http://localhost:8900")
        {
        }

        public CleosClient(string node, string wallet)
        {
            _node = node;
            _wallet = wallet;
            _client = new HttpClient { BaseAddress = new Uri("http://localhost:5500") };
        }

        public enum EosNet
        {
            Mainnet,
            Onebox
        }

        public void ChangeNet(EosNet net)
        {
            if (net == EosNet.Mainnet)
            {
                _node = "http://eos.greymass.com";
            }
            else
            {
                _node = "http://localhost:8888";
            }
        }

        public void ChangeNet(string node)
        {
            _node = node;
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

        public async Task<ClientResult> OpenWalletAsync(string name, CancellationToken cancellationToken = default)
        {
            using (var result = await _client.PostAsync("/api/process", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "file", "cleos" },
                { "args", $"wallet open -n {name}" }
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

        public async Task<ClientResult> LockWalletAsync(string name, CancellationToken cancellationToken = default)
        {
            using (var result = await _client.PostAsync("/api/process", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "file", "cleos" },
                { "args", $"wallet lock -n {name}" }
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

        public async Task<ClientResult> LockAllWalletAsync(CancellationToken cancellationToken = default)
        {
            using (var result = await _client.PostAsync("/api/process", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "file", "cleos" },
                { "args", $"wallet lock_all" }
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
                var commandResult = JsonConvert.DeserializeObject<ApiResult<CommandResult>>(text).data;

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
                var commandResult = JsonConvert.DeserializeObject<ApiResult<CommandResult>>(text).data;

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
                var commandResult = JsonConvert.DeserializeObject<ApiResult<CommandResult>>(text).data;

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
                var commandResult = JsonConvert.DeserializeObject<ApiResult<CommandResult>>(text).data;

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

        public async Task<OneBoxStatusResult> WaitForOneBoxReadyAsync(CancellationToken cancellationToken = default)
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

                    if (cancellationToken.CanBeCanceled)
                    {
                        throw new TaskCanceledException();
                    }

                    return await WaitForOneBoxReadyAsync(cancellationToken);
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
                var commandResult = JsonConvert.DeserializeObject<ApiResult<CommandResult>>(text).data;

                return new ClientResult<BlockchainInfo>
                {
                    Error = commandResult.Stderr,
                    IsSucceeded = commandResult.ExitCode == 0,
                    Output = commandResult.Stdout,
                    Result = JsonConvert.DeserializeObject<BlockchainInfo>(commandResult.Stdout)
                };
            }
        }

        public async Task<ClientResult> GenerateKeyValuePair(string path, CancellationToken cancellationToken = default)
        {
            using (var result = await _client.PostAsync("/api/process", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "file", "cleos" },
                { "args", $"create key --file {path}" }
            }), cancellationToken))
            {
                var text = await result.Content.ReadAsStringAsync();
                var commandResult = JsonConvert.DeserializeObject<ApiResult<CommandResult>>(text).data;

                return new ClientResult<BlockchainInfo>
                {
                    Error = commandResult.Stderr,
                    IsSucceeded = commandResult.ExitCode == 0,
                    Output = commandResult.Stdout,
                    Result = JsonConvert.DeserializeObject<BlockchainInfo>(commandResult.Stdout)
                };
            }
        }

        public async Task<ClientResult> CreateAccountAsync(string creator, string account, string activeKey, string ownerKey, CancellationToken cancellationToken = default)
        {
            using (var result = await _client.PostAsync("/api/process", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "file", "cleos" },
                { "args", $"create account {creator} {account} {activeKey} {ownerKey} -p {creator}" }
            }), cancellationToken))
            {
                var text = await result.Content.ReadAsStringAsync();
                var commandResult = JsonConvert.DeserializeObject<ApiResult<CommandResult>>(text).data;

                return new ClientResult<BlockchainInfo>
                {
                    Error = commandResult.Stderr,
                    IsSucceeded = commandResult.ExitCode == 0,
                    Output = commandResult.Stdout,
                    Result = JsonConvert.DeserializeObject<BlockchainInfo>(commandResult.Stdout)
                };
            }
        }

        public async Task<ClientResult> CreateAccountAsync(string creator, string account, string activeKey, string ownerKey, float net, float cpu, float ram, CancellationToken cancellationToken = default)
        {
            using (var result = await _client.PostAsync("/api/process", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "file", "cleos" },
                { "args", $"create account {creator} {account} {activeKey} {ownerKey} --staked-net \"{net.ToString("0.0000")} EOS\" --stake-cpu \"{cpu.ToString("0.0000")} EOS\" --buy-ram \"{ram.ToString("0.0000")} EOS\" -p {creator}" }
            }), cancellationToken))
            {
                var text = await result.Content.ReadAsStringAsync();
                var commandResult = JsonConvert.DeserializeObject<ApiResult<CommandResult>>(text).data;

                return new ClientResult<BlockchainInfo>
                {
                    Error = commandResult.Stderr,
                    IsSucceeded = commandResult.ExitCode == 0,
                    Output = commandResult.Stdout,
                    Result = JsonConvert.DeserializeObject<BlockchainInfo>(commandResult.Stdout)
                };
            }
        }

        public async Task<ClientResult> CreateFolderAsync(string path, CancellationToken cancellationToken = default)
        {
            using (var result = await _client.PostAsync("/api/process", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "file", "mkdir" },
                { "args", path }
            }), cancellationToken))
            {
                var text = await result.Content.ReadAsStringAsync();
                var commandResult = JsonConvert.DeserializeObject<ApiResult<CommandResult>>(text).data;

                return new ClientResult<BlockchainInfo>
                {
                    Error = commandResult.Stderr,
                    IsSucceeded = commandResult.ExitCode == 0,
                    Output = commandResult.Stdout,
                    Result = JsonConvert.DeserializeObject<BlockchainInfo>(commandResult.Stdout)
                };
            }
        }

        public async Task<ClientResult> CreateFileAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
        {
            using (var result = await _client.PostAsync("/api/file/file", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "base64", Convert.ToBase64String(bytes) },
                { "path", path }
            }), cancellationToken))
            {
                var text = await result.Content.ReadAsStringAsync();
                var commandResult = JsonConvert.DeserializeObject<ApiResult<CommandResult>>(text).data;

                return new ClientResult<BlockchainInfo>
                {
                    Error = commandResult.Stderr,
                    IsSucceeded = commandResult.ExitCode == 0,
                    Output = commandResult.Stdout,
                    Result = JsonConvert.DeserializeObject<BlockchainInfo>(commandResult.Stdout)
                };
            }
        }

        public async Task<byte[]> GetFileAsync(string path, CancellationToken cancellationToken = default)
        {
            using (var result = await _client.PostAsync("/api/file/file", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "path", path }
            }), cancellationToken))
            {
                var text = await result.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<ApiResult<GetFileResult>>(text).data;

                return Convert.FromBase64String(response.Base64);
            }
        }

        public async Task DownloadAndCompileEosioTokenAsync(CancellationToken cancellationToken = default)
        {
            await CreateFolderAsync("/home/cleos-net/contracts/eosio.token", cancellationToken);
            await CreateFileAsync("/home/cleos-net/contracts/eosio.token/CMakeLists.txt", 
                await HttpGetAsync("https://raw.githubusercontent.com/EOSIO/eos/master/contracts/eosio.token/CMakeLists.txt"), 
                cancellationToken);
            await CreateFileAsync("/home/cleos-net/contracts/eosio.token/eosio.token.cpp",
                await HttpGetAsync("https://raw.githubusercontent.com/EOSIO/eos/master/contracts/eosio.token/eosio.token.cpp"),
                cancellationToken);
            await CreateFileAsync("/home/cleos-net/contracts/eosio.token/eosio.token.hpp",
                await HttpGetAsync("https://raw.githubusercontent.com/EOSIO/eos/master/contracts/eosio.token/eosio.token.hpp"),
                cancellationToken);
            await CreateFileAsync("/home/cleos-net/contracts/eosio.token/eosio.token.abi",
                await HttpGetAsync("https://raw.githubusercontent.com/EOSIO/eos/master/contracts/eosio.token/eosio.token.abi"),
                cancellationToken);
            await CompileSmartContractAsync("/home/cleos-net/contracts/eosio.token");
        }

        public async Task<(string PrivateKey, string PublicKey)> RetriveKeyPairsAsync(string path, CancellationToken cancellationToken = default)
        {
            var file = await GetFileAsync(path, cancellationToken);
            var text = Encoding.Default.GetString(file);
            var splited = text.Split('\n');
            var privateKey = splited[0].Split(' ').Last();
            var publicKey = splited[1].Split(' ').Last();
            return (privateKey, publicKey);
        }

        public async Task QuickLaunchOneBoxAsync(CancellationToken cancellationToken = default)
        {
            if (_node != "http://localhost:8888")
            {
                throw new InvalidOperationException("Node is not onebox.");
            }

            await LaunchOneBoxAsync(cancellationToken);
            await WaitForOneBoxReadyAsync(cancellationToken);
            await CreateWalletAsync("eosio.token", "/home/cleos-net/wallet/eosio.token.txt", cancellationToken);
            await GenerateKeyValuePair("/home/cleos-net/wallet/eosio.token.key.txt", cancellationToken);
            var keys = await RetriveKeyPairsAsync("/home/cleos-net/wallet/eosio.token.key.txt", cancellationToken);
            await CreateAccountAsync("eosio", "eosio.token", keys.PublicKey, keys.PublicKey, cancellationToken);
            await DownloadAndCompileEosioTokenAsync(cancellationToken);
            await SetContractAsync("/home/cleos-net/contracts/eosio.token/build", "eosio.token", "eosio.token", "active", cancellationToken);
            await PushActionAsync("eosio.token", "create", "eosio.token", "active", new[] { "eosio.token", "1000000000.0000 EOS" });
        }

        private async Task<byte[]> HttpGetAsync(string url, CancellationToken cancellationToken = default)
        {
            var hostIndex = url.IndexOf('/', "https://".Length);
            var host = url.Substring(0, hostIndex);
            using (var client = new HttpClient() { BaseAddress = new Uri(host) })
            using (var result = await client.GetAsync(url.Substring(host.Length)))
            {
                return await result.Content.ReadAsByteArrayAsync();
            }
        }
    }
}
