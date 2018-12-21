using System;

namespace Andoromeda.CleosNet.Client
{
    public class BlockchainInfo
    {
        public string server_version { get; set; }

        public string chain_id { get; set; }

        public ulong head_block_num { get; set; }

        public ulong last_irreversible_block_num { get; set; }

        public string last_irreversible_block_id { get; set; }

        public string head_block_id { get; set; }

        public DateTime head_block_time { get; set; }

        public string head_block_producer { get; set; }

        public ulong virtual_block_cpu_limit { get; set; }

        public ulong virtual_block_net_limit { get; set; }

        public ulong block_cpu_limit { get; set; }

        public ulong block_net_limit { get; set; }

        public string server_version_string { get; set; }
    }
}
