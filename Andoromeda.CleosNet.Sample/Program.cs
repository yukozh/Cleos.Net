using System;
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
        }
    }
}
