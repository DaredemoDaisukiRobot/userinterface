using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using userinterface.Models;

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
            if (start < 0) start = 0;
            if (quantity <= 0) quantity = 10;

            var page = await _db.FinreflectkgFull
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Skip(start)
                .Take(quantity)
                .Select(x => new
                {
                    x.Id,
                    TripletId = x.TripletId != null ? x.TripletId.Substring(0,10) : null,
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
                    ChunkText = x.ChunkText != null ? x.ChunkText.Substring(0,10) : null,
                    x.TripletLength,
                    x.ChunkTextLength,
                    x.HasContext
                })
                .ToListAsync();

            var items = page.Select(d => new Dictionary<string, object?>
            {
                ["id"] = d.Id,
                ["triplet_id"] = d.TripletId,
                ["entity"] = d.Entity,
                ["entity_type"] = d.EntityType,
                ["relationship"] = d.Relationship,
                ["target"] = d.Target,
                ["target_type"] = d.TargetType,
                ["start_date"] = d.StartDate,
                ["end_date"] = d.EndDate,
                ["extraction_type"] = d.ExtractionType,
                ["ticker"] = d.Ticker,
                ["year"] = d.Year,
                ["source_file"] = d.SourceFile,
                ["page_id"] = d.PageId,
                ["chunk_id"] = d.ChunkId,
                ["chunk_text"] = d.ChunkText,
                ["triplet_length"] = d.TripletLength,
                ["chunk_text_length"] = d.ChunkTextLength,
                ["has_context"] = d.HasContext
            }).ToList();

            return new MemorySearchResponse
            {
                Start = start,
                Quantity = items.Count,
                Items = items
            };
        }
    }
}
