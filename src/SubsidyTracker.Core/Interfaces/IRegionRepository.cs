namespace SubsidyTracker.Core.Interfaces;

using SubsidyTracker.Core.Models;

public interface IRegionRepository
{
    Task<IEnumerable<Region>> GetAllAsync();
    Task<IEnumerable<Region>> GetTopLevelAsync();
    Task<IEnumerable<Region>> GetChildrenAsync(int parentId);
    Task<Region?> GetByCodeAsync(string code);
    Task<Region> AddAsync(Region region);
}
