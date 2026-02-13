using Microsoft.EntityFrameworkCore;
using SubsidyTracker.Core.Interfaces;
using SubsidyTracker.Core.Models;

namespace SubsidyTracker.Data.Repositories;

public class RegionRepository : IRegionRepository
{
    private readonly AppDbContext _context;

    public RegionRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<Region>> GetAllAsync()
    {
        return await _context.Regions.Include(r => r.Children).OrderBy(r => r.Id).ToListAsync();
    }

    public async Task<IEnumerable<Region>> GetTopLevelAsync()
    {
        return await _context.Regions
            .Where(r => r.ParentId == null)
            .Include(r => r.Children)
            .OrderBy(r => r.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<Region>> GetChildrenAsync(int parentId)
    {
        return await _context.Regions.Where(r => r.ParentId == parentId).OrderBy(r => r.Name).ToListAsync();
    }

    public async Task<Region?> GetByCodeAsync(string code)
    {
        return await _context.Regions.FirstOrDefaultAsync(r => r.Code == code);
    }

    public async Task<Region> AddAsync(Region region)
    {
        _context.Regions.Add(region);
        await _context.SaveChangesAsync();
        return region;
    }
}
