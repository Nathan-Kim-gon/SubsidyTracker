namespace SubsidyTracker.Core.Models;

public class Subsidy
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;           // 지원금명
    public string Description { get; set; } = string.Empty;     // 설명
    public string Organization { get; set; } = string.Empty;    // 주관기관
    public string? Amount { get; set; }                          // 지원금액 (텍스트, "월 50만원" 등)
    public string? EligibilityCriteria { get; set; }            // 자격요건
    public string? ApplicationMethod { get; set; }              // 신청방법
    public string? ApplicationUrl { get; set; }                 // 신청 URL
    public string? ContactInfo { get; set; }                    // 문의처
    public string? SourceUrl { get; set; }                      // 출처 URL
    public string? ExternalId { get; set; }                     // 외부 API ID (중복방지)

    public DateTime? ApplicationStartDate { get; set; }         // 신청시작일
    public DateTime? ApplicationEndDate { get; set; }           // 신청마감일
    public SubsidyStatus Status { get; set; } = SubsidyStatus.Active;
    public SourceType SourceType { get; set; }                  // API or Crawling

    public int RegionId { get; set; }
    public Region Region { get; set; } = null!;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public ICollection<SubsidyTargetGroup> SubsidyTargetGroups { get; set; } = new List<SubsidyTargetGroup>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
