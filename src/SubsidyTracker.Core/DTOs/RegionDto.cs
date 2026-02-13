namespace SubsidyTracker.Core.DTOs;

public class RegionDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public List<RegionDto> Children { get; set; } = new();
}
