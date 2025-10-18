using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using userinterface.Models;
// 新增：DataAnnotations 對應資料表/欄位
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace userinterface.Services
{
    public class MemoryService : IMemoryService
    {
        // 改為使用 ORM 的 DbContext（memory）
        private readonly MemoryDbContext _db;
        public MemoryService(MemoryDbContext db)
        {
            _db = db;
        }

        public async Task<MemorySearchResponse> SearchAsync(int start, int quantity)
        {
            // ORM 分頁查詢
            var query = _db.FinreflectkgFull.AsNoTracking();

            var total = await query.CountAsync();

            var page = await query
                .OrderBy(x => x.Id) // 確保分頁穩定性
                .Skip(start)
                .Take(1) // 固定只抓一筆
                .Select(x => new
                {
                    x.Id,
                    x.TripletId,
                    x.Entity,
                    x.EntityType,
                    x.Relationship,
                    x.Target,
                    x.TargetType,
                    x.StartDate,
                    x.EndDate,
                    x.ExtractionType,
                    x.Ticker,
                    x.Year,
                    x.SourceFile,
                    x.PageId,
                    x.ChunkId,
                    x.ChunkText,
                    x.TripletLength,
                    x.ChunkTextLength,
                    x.HasContext
                })
                .ToListAsync();

            static string? Left10(string? s) => string.IsNullOrEmpty(s) ? s : (s!.Length <= 10 ? s : s.Substring(0, 10));

            var items = page.Select(d => new Dictionary<string, object?>
            {
                ["id"] = d.Id,
                ["triplet_id"] = Left10(d.TripletId),
                ["entity"] = Left10(d.Entity),
                ["entity_type"] = Left10(d.EntityType),
                ["relationship"] = Left10(d.Relationship),
                ["target"] = Left10(d.Target),
                ["target_type"] = Left10(d.TargetType),
                ["start_date"] = Left10(d.StartDate),
                ["end_date"] = Left10(d.EndDate),
                ["extraction_type"] = Left10(d.ExtractionType),
                ["ticker"] = Left10(d.Ticker),
                ["year"] = d.Year,
                ["source_file"] = Left10(d.SourceFile),
                ["page_id"] = Left10(d.PageId),
                ["chunk_id"] = Left10(d.ChunkId),
                ["chunk_text"] = Left10(d.ChunkText),
                ["triplet_length"] = d.TripletLength,
                ["chunk_text_length"] = d.ChunkTextLength,
                ["has_context"] = d.HasContext
            }).ToList();

            return new MemorySearchResponse
            {
                Start = start,
                Quantity = 1, // 固定 1
                Total = total,
                Items = items
            };
        }
    }
}

// ORM 實體：對應 memory.finreflectkg_full
namespace userinterface.Models
{
    [Table("finreflectkg_full")] // Database 已由 MemoryDbContext 連線字串指定為 memory
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

    // 新增：MemoryDbContext（指向 Database=memory）
    public class MemoryDbContext : DbContext
    {
        public MemoryDbContext(DbContextOptions<MemoryDbContext> options) : base(options) { }

        public DbSet<FinreflectkgFull> FinreflectkgFull { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // finreflectkg_full 設定（欄位由 DataAnnotations 指定）
            modelBuilder.Entity<FinreflectkgFull>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.ToTable("finreflectkg_full");
            });
        }
    }
}
