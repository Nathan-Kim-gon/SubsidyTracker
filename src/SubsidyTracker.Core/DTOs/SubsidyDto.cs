namespace SubsidyTracker.Core.DTOs;

public class SubsidyListDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;
    public string? Amount { get; set; }
    public string RegionName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public List<string> TargetGroups { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public DateTime? ApplicationEndDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SubsidyDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;
    public string? Amount { get; set; }
    public string? EligibilityCriteria { get; set; }
    public string? ApplicationMethod { get; set; }
    public string? ApplicationUrl { get; set; }
    public string? ContactInfo { get; set; }
    public string? SourceUrl { get; set; }
    public DateTime? ApplicationStartDate { get; set; }
    public DateTime? ApplicationEndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string RegionName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public List<string> TargetGroups { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
