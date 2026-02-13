using System.Text.Json;
using Microsoft.Extensions.Logging;
using SubsidyTracker.Core.Interfaces;
using SubsidyTracker.Core.Models;

namespace SubsidyTracker.Collector.Collectors;

public class YouthCenterCollector : IDataCollector
{
    public string SourceName => "YouthCenter";

    private readonly HttpClient _httpClient;
    private readonly ISubsidyRepository _subsidyRepository;
    private readonly IRegionRepository _regionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICollectionLogRepository _logRepository;
    private readonly ILogger<YouthCenterCollector> _logger;

    public YouthCenterCollector(
        HttpClient httpClient,
        ISubsidyRepository subsidyRepository,
        IRegionRepository regionRepository,
        ICategoryRepository categoryRepository,
        ICollectionLogRepository logRepository,
        ILogger<YouthCenterCollector> logger)
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
            SourceType = SourceType.YouthCenter,
            StartedAt = DateTime.UtcNow,
            Status = CollectionStatus.Running
        };
        await _logRepository.AddAsync(log);

        try
        {
            var collected = 0;
            var skipped = 0;

            _logger.LogInformation("온통청년 수집 시작");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

            // 온통청년 정책 API
            var url = "https://www.youthcenter.go.kr/youngPlcyUnif/youngPlcyUnifList.do";

            try
            {
                var requestContent = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "pageIndex", "1" },
                    { "pageUnit", "100" },
                    { "srchWord", "" },
                    { "bizTycdSel", "" },    // 정책유형
                    { "srchRegion", "" },     // 지역
                });

                var response = await _httpClient.PostAsync(url, requestContent, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);

                    // Try to parse as JSON first
                    try
                    {
                        var json = JsonDocument.Parse(content);
                        var root = json.RootElement;

                        JsonElement dataArray;
                        if (root.TryGetProperty("youthPolicyList", out dataArray) ||
                            root.TryGetProperty("resultList", out dataArray) ||
                            root.TryGetProperty("data", out dataArray))
                        {
                            foreach (var item in dataArray.EnumerateArray())
                            {
                                if (cancellationToken.IsCancellationRequested) break;

                                try
                                {
                                    var externalId = GetStringProp(item, "bizId") ?? GetStringProp(item, "policyId") ?? Guid.NewGuid().ToString();
                                    var fullId = $"youth_{externalId}";

                                    if (await _subsidyRepository.ExistsAsync(fullId))
                                    {
                                        skipped++;
                                        continue;
                                    }

                                    var defaultRegion = (await _regionRepository.GetByCodeAsync("ALL"))!;
                                    var category = (await _categoryRepository.GetByCodeAsync("EMPLOYMENT"))!;

                                    var subsidy = new Subsidy
                                    {
                                        Title = GetStringProp(item, "polyBizSjnm") ?? GetStringProp(item, "policyName") ?? "제목 없음",
                                        Description = GetStringProp(item, "polyItcnCn") ?? GetStringProp(item, "policyDescription") ?? "",
                                        Organization = GetStringProp(item, "cnsgNmor") ?? GetStringProp(item, "organizationName") ?? "청년정책",
                                        Amount = GetStringProp(item, "sporCn") ?? GetStringProp(item, "supportContent"),
                                        EligibilityCriteria = GetStringProp(item, "ageInfo") ?? GetStringProp(item, "eligibility"),
                                        ApplicationMethod = GetStringProp(item, "rqutProcCn") ?? GetStringProp(item, "applicationMethod"),
                                        ApplicationUrl = GetStringProp(item, "rqutUrla") ?? GetStringProp(item, "applicationUrl"),
                                        SourceUrl = $"https://www.youthcenter.go.kr/youngPlcyUnif/youngPlcyUnifDtl.do?bizId={externalId}",
                                        ExternalId = fullId,
                                        SourceType = SourceType.YouthCenter,
                                        Status = SubsidyStatus.Active,
                                        RegionId = defaultRegion.Id,
                                        CategoryId = category.Id
                                    };

                                    await _subsidyRepository.AddAsync(subsidy);
                                    collected++;
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarning(ex, "온통청년 항목 처리 중 오류");
                                    skipped++;
                                }
                            }
                        }
                    }
                    catch (JsonException)
                    {
                        // If not JSON, it might be HTML - parse with HtmlAgilityPack
                        _logger.LogInformation("온통청년 응답이 HTML 형식 - HTML 파싱 시도");
                        var doc = new HtmlAgilityPack.HtmlDocument();
                        doc.LoadHtml(content);

                        var policyNodes = doc.DocumentNode.SelectNodes("//ul[contains(@class,'policy-list')]//li")
                                         ?? doc.DocumentNode.SelectNodes("//div[contains(@class,'result')]//li");

                        if (policyNodes != null)
                        {
                            foreach (var node in policyNodes)
                            {
                                if (cancellationToken.IsCancellationRequested) break;

                                var titleNode = node.SelectSingleNode(".//a") ?? node.SelectSingleNode(".//strong");
                                var title = titleNode?.InnerText?.Trim();
                                if (string.IsNullOrWhiteSpace(title)) continue;

                                var externalId = $"youth_{title.GetHashCode():X8}";
                                if (await _subsidyRepository.ExistsAsync(externalId))
                                {
                                    skipped++;
                                    continue;
                                }

                                var defaultRegion = (await _regionRepository.GetByCodeAsync("ALL"))!;
                                var category = (await _categoryRepository.GetByCodeAsync("EMPLOYMENT"))!;

                                var subsidy = new Subsidy
                                {
                                    Title = System.Net.WebUtility.HtmlDecode(title),
                                    Description = "",
                                    Organization = "청년정책",
                                    ExternalId = externalId,
                                    SourceType = SourceType.YouthCenter,
                                    Status = SubsidyStatus.Active,
                                    RegionId = defaultRegion.Id,
                                    CategoryId = category.Id
                                };

                                await _subsidyRepository.AddAsync(subsidy);
                                collected++;
                            }
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "온통청년 접속 실패");
            }

            log.ItemsCollected = collected;
            log.ItemsSkipped = skipped;
            log.Status = collected > 0 ? CollectionStatus.Completed : CollectionStatus.PartiallyCompleted;
            log.CompletedAt = DateTime.UtcNow;
            await _logRepository.UpdateAsync(log);

            _logger.LogInformation("온통청년 수집 완료: {Collected}건 수집, {Skipped}건 건너뜀", collected, skipped);
            return collected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "온통청년 수집 중 오류 발생");
            log.Status = CollectionStatus.Failed;
            log.ErrorMessage = ex.Message;
            log.CompletedAt = DateTime.UtcNow;
            await _logRepository.UpdateAsync(log);
            return 0;
        }
    }

    private static string? GetStringProp(JsonElement element, string name)
    {
        if (element.TryGetProperty(name, out var prop) && prop.ValueKind == JsonValueKind.String)
            return prop.GetString();
        return null;
    }
}
