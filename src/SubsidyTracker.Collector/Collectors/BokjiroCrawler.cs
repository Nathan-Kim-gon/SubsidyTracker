using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using SubsidyTracker.Core.Interfaces;
using SubsidyTracker.Core.Models;

namespace SubsidyTracker.Collector.Collectors;

public class BokjiroCrawler : IDataCollector
{
    public string SourceName => "Bokjiro";

    private readonly HttpClient _httpClient;
    private readonly ISubsidyRepository _subsidyRepository;
    private readonly IRegionRepository _regionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICollectionLogRepository _logRepository;
    private readonly ILogger<BokjiroCrawler> _logger;

    public BokjiroCrawler(
        HttpClient httpClient,
        ISubsidyRepository subsidyRepository,
        IRegionRepository regionRepository,
        ICategoryRepository categoryRepository,
        ICollectionLogRepository logRepository,
        ILogger<BokjiroCrawler> logger)
    {
        _httpClient = httpClient;
        _subsidyRepository = subsidyRepository;
        _regionRepository = regionRepository;
        _categoryRepository = categoryRepository;
        _logRepository = logRepository;
        _logger = logger;
    }

    public async Task<int> CollectAsync(CancellationToken cancellationToken = default)
    {
        var log = new CollectionLog
        {
            Source = SourceName,
            SourceType = SourceType.Bokjiro,
            StartedAt = DateTime.UtcNow,
            Status = CollectionStatus.Running
        };
        await _logRepository.AddAsync(log);

        try
        {
            var collected = 0;
            var skipped = 0;

            // 복지로 서비스 목록 페이지 크롤링
            var baseUrl = "https://www.bokjiro.go.kr/ssis-tbu/twataa/wlfareInfo/moveTWAT52011M.do";

            _logger.LogInformation("복지로 크롤링 시작");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
            _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml");
            _httpClient.DefaultRequestHeaders.Add("Accept-Language", "ko-KR,ko;q=0.9");

            try
            {
                var response = await _httpClient.GetAsync(baseUrl, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var html = await response.Content.ReadAsStringAsync(cancellationToken);
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    // 서비스 목록 파싱
                    var serviceNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'service-list')]//li")
                                      ?? doc.DocumentNode.SelectNodes("//ul[contains(@class, 'list')]//li")
                                      ?? doc.DocumentNode.SelectNodes("//div[contains(@class, 'cont')]//a");

                    if (serviceNodes != null)
                    {
                        foreach (var node in serviceNodes)
                        {
                            if (cancellationToken.IsCancellationRequested) break;

                            try
                            {
                                var titleNode = node.SelectSingleNode(".//a") ?? node.SelectSingleNode(".//strong") ?? node;
                                var title = titleNode?.InnerText?.Trim();

                                if (string.IsNullOrWhiteSpace(title)) continue;

                                var link = titleNode?.GetAttributeValue("href", "");
                                var externalId = $"bokjiro_{title.GetHashCode():X8}";

                                if (await _subsidyRepository.ExistsAsync(externalId))
                                {
                                    skipped++;
                                    continue;
                                }

                                var descNode = node.SelectSingleNode(".//p") ?? node.SelectSingleNode(".//span[contains(@class,'desc')]");
                                var description = descNode?.InnerText?.Trim() ?? "";

                                var defaultRegion = (await _regionRepository.GetByCodeAsync("ALL"))!;
                                var defaultCategory = (await _categoryRepository.GetByCodeAsync("LIVING"))!;

                                var subsidy = new Subsidy
                                {
                                    Title = System.Net.WebUtility.HtmlDecode(title),
                                    Description = System.Net.WebUtility.HtmlDecode(description),
                                    Organization = "보건복지부",
                                    SourceUrl = string.IsNullOrEmpty(link) ? baseUrl : (link.StartsWith("http") ? link : $"https://www.bokjiro.go.kr{link}"),
                                    ExternalId = externalId,
                                    SourceType = SourceType.Bokjiro,
                                    Status = SubsidyStatus.Active,
                                    RegionId = defaultRegion.Id,
                                    CategoryId = defaultCategory.Id
                                };

                                await _subsidyRepository.AddAsync(subsidy);
                                collected++;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "복지로 항목 파싱 중 오류");
                                skipped++;
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning("복지로 서비스 목록 노드를 찾을 수 없습니다. HTML 구조가 변경되었을 수 있습니다.");
                    }
                }
                else
                {
                    _logger.LogWarning("복지로 페이지 요청 실패: {StatusCode}", response.StatusCode);
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "복지로 접속 실패 - 네트워크 오류");
            }

            log.ItemsCollected = collected;
            log.ItemsSkipped = skipped;
            log.Status = collected > 0 ? CollectionStatus.Completed : CollectionStatus.PartiallyCompleted;
            log.CompletedAt = DateTime.UtcNow;
            await _logRepository.UpdateAsync(log);

            _logger.LogInformation("복지로 크롤링 완료: {Collected}건 수집, {Skipped}건 건너뜀", collected, skipped);
            return collected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "복지로 크롤링 중 오류 발생");
            log.Status = CollectionStatus.Failed;
            log.ErrorMessage = ex.Message;
            log.CompletedAt = DateTime.UtcNow;
            await _logRepository.UpdateAsync(log);
            return 0;
        }
    }
}
