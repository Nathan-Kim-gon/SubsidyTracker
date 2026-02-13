using Microsoft.EntityFrameworkCore;
using SubsidyTracker.Core.Interfaces;
using SubsidyTracker.Core.Models;

namespace SubsidyTracker.Data.Repositories;

public class CollectionLogRepository : ICollectionLogRepository
{
    private readonly AppDbContext _context;

    public CollectionLogRepository(AppDbContext context) => _context = context;

    public async Task<CollectionLog> AddAsync(CollectionLog log)
    {
        _context.CollectionLogs.Add(log);
        await _context.SaveChangesAsync();
        return log;
    }

    public async Task UpdateAsync(CollectionLog log)
    {
        _context.CollectionLogs.Update(log);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<CollectionLog>> GetRecentAsync(int count = 10)
    {
        return await _context.CollectionLogs
            .OrderByDescending(l => l.StartedAt)
            .Take(count)
            .ToListAsync();
    }
}
