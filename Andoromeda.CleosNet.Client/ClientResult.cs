namespace Andoromeda.CleosNet.Client
{
    public class ClientResult
    {
        public bool IsSucceeded { get; set; }

        public string Error { get; set; }

        public string Output { get; set; }
    }

    public class ClientResult<T> : ClientResult
    {
        public T Result { get; set; }
    }
}
