using System;
using System.IO;
using System.Threading.Tasks;
using Andoromeda.CleosNet.Client;

namespace Andoromeda.CleosNet.Sample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var client = new CleosClient("http://localhost:8888");
            await client.QuickLaunchOneBoxAsync();
            var keys = await client.RetriveKeyPairsAsync("/home/cleos-net/wallet/eosio.token.key.txt");
            await client.CreateAccountAsync("eosio", "yuko", keys.PublicKey, keys.PublicKey);
            await client.PushActionAsync("eosio.token", "transfer", "eosio.token", "active", new object[] { "eosio.token", "yuko", "1000.0000 EOS", "" });

            var keys2 = await client.RetriveKeyPairsAsync("/home/cleos-net/wallet/eosio.token.key.txt");
            await client.ImportPrivateKeyToWalletAsync(keys2.PrivateKey, "eosio.token");
            await client.CreateAccountAsync("eosio", "counter", keys2.PublicKey, keys2.PublicKey);
            await client.PushActionAsync("eosio.token", "transfer", "eosio.token", "active", new object[] { "eosio.token", "counter", "1000.0000 EOS", "" });

            await client.CreateFolderAsync("/opt/eosio/contracts/counter");
            await client.UploadFileAsync("/opt/eosio/contracts/counter/counter.cpp", File.ReadAllBytes("counter.cpp"));
            await client.UploadFileAsync("/opt/eosio/contracts/counter/CMakeLists.txt", File.ReadAllBytes("CMakeLists.txt"));
            await client.CompileSmartContractAsync("/opt/eosio/contracts/counter");
            await client.SetContractAsync("/opt/eosio/contracts/counter/build", "counter", "pomelo");

            await client.PushActionAsync("counter", "init", "counter", "active", new object[] { });
            await client.PushActionAsync("counter", "add", "counter", "active", new object[] { "counter" });
        }
    }
}
