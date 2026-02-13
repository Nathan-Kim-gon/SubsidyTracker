namespace SubsidyTracker.Core.Models;

public class TargetGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;     // 청년, 노인, 장애인, 저소득층 등
    public string Code { get; set; } = string.Empty;
    public ICollection<SubsidyTargetGroup> SubsidyTargetGroups { get; set; } = new List<SubsidyTargetGroup>();
}
