using System;
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

            // Interpret 'start' as starting ID (inclusive), not skip count
            var page = await _db.FinreflectkgFull
                .AsNoTracking()
                .Where(x => x.Id >= start)            // <- 使用 Id >= start 作為起點
                .OrderBy(x => x.Id)
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

            // 計算最後一筆 id（若 items 為空則為 null）
            int? lastId = null;
            if (items.Count > 0)
            {
                var val = items.Last()["id"];
                if (val is int i) lastId = i;
                else if (val != null)
                {
                    try { lastId = Convert.ToInt32(val); } catch { lastId = null; }
                }
            }

            return new MemorySearchResponse
            {
                Start = start,
                Quantity = items.Count,
                Items = items,
                LastId = lastId
            };
        }

        public async Task<MemorySearchResponse> FilterAsync(int start, int quantity, string criteria)
        {
            if (start < 0) start = 0;
            if (quantity <= 0) quantity = 10;
            var pattern = $"%{criteria}%";

            var query = _db.FinreflectkgFull.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(criteria))
            {
                // 新增 starts-with 檢查 (criteria + "%")，並保留 contains (%criteria%)
                var startPattern = $"{criteria}%";
                query = query.Where(x =>
                    EF.Functions.Like(x.TripletId ?? "", pattern) ||
                    EF.Functions.Like(x.Entity ?? "", pattern) ||
                    EF.Functions.Like(x.EntityType ?? "", pattern) ||
                    EF.Functions.Like(x.Relationship ?? "", pattern) ||
                    // 對 Target 同時做 contains 與 starts-with 比對
                    EF.Functions.Like(x.Target ?? "", pattern) ||
                    EF.Functions.Like(x.Target ?? "", startPattern) ||
                    EF.Functions.Like(x.TargetType ?? "", pattern) ||
                    EF.Functions.Like(x.StartDate ?? "", pattern) ||
                    EF.Functions.Like(x.EndDate ?? "", pattern) ||
                    EF.Functions.Like(x.ExtractionType ?? "", pattern) ||
                    EF.Functions.Like(x.Ticker ?? "", pattern) ||
                    EF.Functions.Like(x.SourceFile ?? "", pattern) ||
                    EF.Functions.Like(x.PageId ?? "", pattern) ||
                    EF.Functions.Like(x.ChunkId ?? "", pattern) ||
                    EF.Functions.Like(x.ChunkText ?? "", pattern)
                );
            }

            // Interpret 'start' as starting ID (inclusive), not skip count
            var page = await query
                .Where(x => x.Id >= start)           // <- 使用 Id >= start 作為起點
                .OrderBy(x => x.Id)
                .Take(quantity)
                .Select(x => new
                {
                    x.Id,
                    TripletId = x.TripletId != null && x.TripletId.Length > 10 ? x.TripletId.Substring(0, 10) : x.TripletId,
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
                    ChunkText = x.ChunkText != null && x.ChunkText.Length > 10 ? x.ChunkText.Substring(0, 10) : x.ChunkText,
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

            // 計算最後一筆 id（若 items 為空則為 null）
            int? lastId = null;
            if (items.Count > 0)
            {
                var val = items.Last()["id"];
                if (val is int i) lastId = i;
                else if (val != null)
                {
                    try { lastId = Convert.ToInt32(val); } catch { lastId = null; }
                }
            }

            return new MemorySearchResponse
            {
                Start = start,
                Quantity = items.Count,
                Items = items,
                LastId = lastId + 1
            };
        }
    }
}
