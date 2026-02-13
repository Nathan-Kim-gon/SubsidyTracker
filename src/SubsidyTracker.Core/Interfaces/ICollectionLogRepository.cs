namespace SubsidyTracker.Core.Interfaces;

using SubsidyTracker.Core.Models;

public interface ICollectionLogRepository
{
    Task<CollectionLog> AddAsync(CollectionLog log);
    Task UpdateAsync(CollectionLog log);
    Task<IEnumerable<CollectionLog>> GetRecentAsync(int count = 10);
}
