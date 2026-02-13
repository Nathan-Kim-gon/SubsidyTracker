namespace SubsidyTracker.Core.Models;

public class CollectionLog
{
    public int Id { get; set; }
    public string Source { get; set; } = string.Empty;    // 수집 소스명
    public SourceType SourceType { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int ItemsCollected { get; set; }
    public int ItemsUpdated { get; set; }
    public int ItemsSkipped { get; set; }
    public CollectionStatus Status { get; set; } = CollectionStatus.Running;
    public string? ErrorMessage { get; set; }
}
