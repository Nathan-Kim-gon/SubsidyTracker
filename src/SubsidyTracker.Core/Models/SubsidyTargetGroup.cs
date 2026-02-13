namespace SubsidyTracker.Core.Models;

public class SubsidyTargetGroup
{
    public int SubsidyId { get; set; }
    public Subsidy Subsidy { get; set; } = null!;
    public int TargetGroupId { get; set; }
    public TargetGroup TargetGroup { get; set; } = null!;
}
