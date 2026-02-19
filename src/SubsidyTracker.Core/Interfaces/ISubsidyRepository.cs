namespace SubsidyTracker.Core.Interfaces;

using SubsidyTracker.Core.Models;

public interface ISubsidyRepository
{
    Task<Subsidy?> GetByIdAsync(int id);
    Task<Subsidy?> GetByExternalIdAsync(string externalId);
    Task<IEnumerable<Subsidy>> GetAllAsync(SubsidyFilter filter);
    Task<int> GetCountAsync(SubsidyFilter filter);
    Task<Subsidy> AddAsync(Subsidy subsidy);
    Task UpdateAsync(Subsidy subsidy);
    Task<bool> ExistsAsync(string externalId);
    Task<int> CloseMissingAsync(SourceType sourceType, IEnumerable<string> activeExternalIds);
}

public class SubsidyFilter
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Keyword { get; set; }
    public int? RegionId { get; set; }
    public int? CategoryId { get; set; }
    public int? TargetGroupId { get; set; }
    public SubsidyStatus? Status { get; set; }
    public string? SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}
