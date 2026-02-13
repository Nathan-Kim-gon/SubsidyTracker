namespace SubsidyTracker.Core.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;     // 카테고리명
    public string Code { get; set; } = string.Empty;     // 카테고리 코드
    public string? Description { get; set; }
    public ICollection<Subsidy> Subsidies { get; set; } = new List<Subsidy>();
}
