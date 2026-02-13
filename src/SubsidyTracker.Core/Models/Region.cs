namespace SubsidyTracker.Core.Models;

public class Region
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;     // 지역명 (서울, 부산 등)
    public string Code { get; set; } = string.Empty;     // 지역코드
    public int? ParentId { get; set; }                    // 상위지역 (시도→시군구)
    public Region? Parent { get; set; }
    public ICollection<Region> Children { get; set; } = new List<Region>();
    public ICollection<Subsidy> Subsidies { get; set; } = new List<Subsidy>();
}
