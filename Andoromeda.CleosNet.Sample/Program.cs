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
        }
    }
}
