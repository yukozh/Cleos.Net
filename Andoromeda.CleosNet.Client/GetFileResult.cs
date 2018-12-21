using System;

namespace Andoromeda.CleosNet.Client
{
    public class GetFileResult
    {
        public string Base64 { get; set; }

        public string Filename { get; set; }

        public DateTime LastWrite { get; set; }

        public DateTime LastRead { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
