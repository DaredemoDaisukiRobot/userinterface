namespace userinterface.Models
{
    public class MemorySearchResponse
    {
        public int Start { get; set; }
        public int Quantity { get; set; }
        public int Total { get; set; }
        public List<Dictionary<string, object?>> Items { get; set; } = new();
    }
}
