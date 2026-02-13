using Microsoft.EntityFrameworkCore;
using SubsidyTracker.Core.Interfaces;
using SubsidyTracker.Core.Models;

namespace SubsidyTracker.Data.Repositories;

public class SubsidyRepository : ISubsidyRepository
{
    private readonly AppDbContext _context;

    public SubsidyRepository(AppDbContext context) => _context = context;

    public async Task<Subsidy?> GetByIdAsync(int id)
    {
        return await _context.Subsidies
            .Include(s => s.Region)
            .Include(s => s.Category)
            .Include(s => s.SubsidyTargetGroups)
                .ThenInclude(stg => stg.TargetGroup)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Subsidy?> GetByExternalIdAsync(string externalId)
    {
        return await _context.Subsidies
            .FirstOrDefaultAsync(s => s.ExternalId == externalId);
    }

    public async Task<IEnumerable<Subsidy>> GetAllAsync(SubsidyFilter filter)
    {
        var query = BuildQuery(filter);

        query = filter.SortBy?.ToLower() switch
        {
            "title" => filter.SortDescending ? query.OrderByDescending(s => s.Title) : query.OrderBy(s => s.Title),
            "applicationenddate" => filter.SortDescending ? query.OrderByDescending(s => s.ApplicationEndDate) : query.OrderBy(s => s.ApplicationEndDate),
            _ => filter.SortDescending ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt)
        };

        return await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync(SubsidyFilter filter)
    {
        return await BuildQuery(filter).CountAsync();
    }

    public async Task<Subsidy> AddAsync(Subsidy subsidy)
    {
        _context.Subsidies.Add(subsidy);
        await _context.SaveChangesAsync();
        return subsidy;
    }

    public async Task UpdateAsync(Subsidy subsidy)
    {
        subsidy.UpdatedAt = DateTime.UtcNow;
        _context.Subsidies.Update(subsidy);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(string externalId)
    {
        return await _context.Subsidies.AnyAsync(s => s.ExternalId == externalId);
    }

    private IQueryable<Subsidy> BuildQuery(SubsidyFilter filter)
    {
        var query = _context.Subsidies
            .Include(s => s.Region)
            .Include(s => s.Category)
            .Include(s => s.SubsidyTargetGroups)
                .ThenInclude(stg => stg.TargetGroup)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Trim();
            query = query.Where(s =>
                s.Title.Contains(keyword) ||
                s.Description.Contains(keyword) ||
                s.Organization.Contains(keyword));
        }

        if (filter.RegionId.HasValue)
            query = query.Where(s => s.RegionId == filter.RegionId.Value);

        if (filter.CategoryId.HasValue)
            query = query.Where(s => s.CategoryId == filter.CategoryId.Value);

        if (filter.TargetGroupId.HasValue)
            query = query.Where(s => s.SubsidyTargetGroups.Any(stg => stg.TargetGroupId == filter.TargetGroupId.Value));

        if (filter.Status.HasValue)
            query = query.Where(s => s.Status == filter.Status.Value);

        return query;
    }
}
