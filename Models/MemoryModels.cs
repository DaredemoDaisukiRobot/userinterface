using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace userinterface.Models
{
    public class MemorySearchResponse
    {
        public int Start { get; set; }
        public int Quantity { get; set; }
        public int Total { get; set; }
        public List<Dictionary<string, object?>> Items { get; set; } = new();
    }

    [Table("finreflectkg_full")]
    public class FinreflectkgFull
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("triplet_id")] public string? TripletId { get; set; }
        [Column("entity")] public string? Entity { get; set; }
        [Column("entity_type")] public string? EntityType { get; set; }
        [Column("relationship")] public string? Relationship { get; set; }
        [Column("target")] public string? Target { get; set; }
        [Column("target_type")] public string? TargetType { get; set; }
        [Column("start_date")] public string? StartDate { get; set; }
        [Column("end_date")] public string? EndDate { get; set; }
        [Column("extraction_type")] public string? ExtractionType { get; set; }
        [Column("ticker")] public string? Ticker { get; set; }
        [Column("year")] public int? Year { get; set; }
        [Column("source_file")] public string? SourceFile { get; set; }
        [Column("page_id")] public string? PageId { get; set; }
        [Column("chunk_id")] public string? ChunkId { get; set; }
        [Column("chunk_text")] public string? ChunkText { get; set; }
        [Column("triplet_length")] public int? TripletLength { get; set; }
        [Column("chunk_text_length")] public int? ChunkTextLength { get; set; }
        [Column("has_context")] public bool? HasContext { get; set; }
    }
}
