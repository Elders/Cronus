namespace Elders.Cronus.EventStore
{
    public class PagingOptions
    {
        PagingOptions() { }

        public PagingOptions(int pageSize, byte[] token)
        {
            Take = pageSize;
            PaginationToken = token;
        }

        public int Take { get; init; }

        public byte[] PaginationToken { get; init; }

        public static PagingOptions Empty() => new PagingOptions();

        public override string ToString()
        {
            return $"\n Records taken: {Take}\n Token: {PaginationToken}";
        }
    }
}
