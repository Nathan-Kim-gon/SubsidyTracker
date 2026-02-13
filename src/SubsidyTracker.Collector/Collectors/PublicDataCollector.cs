using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SubsidyTracker.Core.Interfaces;
using SubsidyTracker.Core.Models;

namespace SubsidyTracker.Collector.Collectors;

/// <summary>
/// 공공데이터포털 - 행정안전부_대한민국 공공서비스(혜택) 정보 API
/// 엔드포인트: https://api.odcloud.kr/api/gov24/v3/serviceList
/// Swagger: https://infuser.odcloud.kr/api/stages/44436/api-docs
///
/// 응답 구조:
/// {
///   "page": 1, "perPage": 100, "totalCount": 9000, "currentCount": 100, "matchCount": 9000,
///   "data": [
///     {
///       "서비스ID": "...",
///       "서비스명": "...",
///       "서비스목적요약": "...",
///       "지원유형": "...",
///       "지원대상": "...",
///       "선정기준": "...",
///       "지원내용": "...",
///       "신청방법": "...",
///       "신청기한": "...",
///       "소관기관코드": "...",
///       "소관기관명": "...",
///       "소관기관유형": "...",
///       "부서명": "...",
///       "사용자구분": "...",
///       "서비스분야": "...",
///       "접수기관": "...",
///       "전화문의": "...",
///       "상세조회URL": "...",
///       "조회수": 0,
///       "등록일시": "...",
///       "수정일시": "..."
///     }
///   ]
/// }
/// </summary>
public class PublicDataCollector : IDataCollector
{
    public string SourceName => "PublicDataPortal";

    private readonly HttpClient _httpClient;
    private readonly ISubsidyRepository _subsidyRepository;
    private readonly IRegionRepository _regionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICollectionLogRepository _logRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PublicDataCollector> _logger;

    // 서비스분야 → 카테고리 매핑
    private static readonly Dictionary<string, string> CategoryMapping = new(StringComparer.OrdinalIgnoreCase)
    {
        { "생활안정", "LIVING" },
        { "주거·자립", "HOUSING" },  { "주거자립", "HOUSING" },  { "주거", "HOUSING" },
        { "보육·교육", "EDUCATION" }, { "보육교육", "EDUCATION" }, { "교육", "EDUCATION" },
        { "고용·창업", "EMPLOYMENT" }, { "고용창업", "EMPLOYMENT" }, { "고용", "EMPLOYMENT" }, { "창업", "EMPLOYMENT" },
        { "보건·의료", "HEALTH" },   { "보건의료", "HEALTH" },   { "의료", "HEALTH" }, { "보건", "HEALTH" },
        { "행정·안전", "ADMIN" },    { "행정안전", "ADMIN" },
        { "문화·환경", "CULTURE" },  { "문화환경", "CULTURE" },  { "문화", "CULTURE" }, { "환경", "CULTURE" },
        { "농림·축산", "AGRICULTURE" }, { "농림축산", "AGRICULTURE" }, { "농림", "AGRICULTURE" },
    };

    // 사용자구분 → 대상그룹 매핑
    private static readonly Dictionary<string, string> TargetGroupMapping = new(StringComparer.OrdinalIgnoreCase)
    {
        { "청년", "YOUTH" },
        { "중장년", "MIDDLE" },
        { "노인", "SENIOR" }, { "어르신", "SENIOR" },
        { "장애인", "DISABLED" },
        { "저소득", "LOWINCOME" }, { "저소득층", "LOWINCOME" },
        { "다문화", "MULTICULTURAL" }, { "다문화가정", "MULTICULTURAL" },
        { "한부모", "SINGLEPARENT" }, { "한부모가정", "SINGLEPARENT" },
        { "임산부", "PREGNANT" }, { "임신", "PREGNANT" },
        { "영유아", "INFANT" }, { "영아", "INFANT" },
    };

    public PublicDataCollector(
        HttpClient httpClient,
        ISubsidyRepository subsidyRepository,
        IRegionRepository regionRepository,
        ICategoryRepository categoryRepository,
        ICollectionLogRepository logRepository,
        IConfiguration configuration,
        ILogger<PublicDataCollector> logger)
    {
        _httpClient = httpClient;
        _subsidyRepository = subsidyRepository;
        _regionRepository = regionRepository;
        _categoryRepository = categoryRepository;
        _logRepository = logRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<int> CollectAsync(CancellationToken cancellationToken = default)
    {
        var log = new CollectionLog
        {
            Source = SourceName,
            SourceType = SourceType.PublicDataPortal,
            StartedAt = DateTime.UtcNow,
            Status = CollectionStatus.Running
        };
        await _logRepository.AddAsync(log);

        try
        {
            var apiKey = _configuration["PublicDataPortal:ApiKey"];
            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_API_KEY_HERE")
            {
                _logger.LogWarning("공공데이터포털 API 키가 설정되지 않았습니다. appsettings.json의 PublicDataPortal:ApiKey를 설정해주세요.");
                log.Status = CollectionStatus.Failed;
                log.ErrorMessage = "API 키가 설정되지 않았습니다.";
                log.CompletedAt = DateTime.UtcNow;
                await _logRepository.UpdateAsync(log);
                return 0;
            }

            var collected = 0;
            var updated = 0;
            var skipped = 0;
            var page = 1;
            var pageSize = 100;
            var hasMore = true;

            while (hasMore && !cancellationToken.IsCancellationRequested)
            {
                // 실제 API: GET https://api.odcloud.kr/api/gov24/v3/serviceList
                var url = $"https://api.odcloud.kr/api/gov24/v3/serviceList?page={page}&perPage={pageSize}&returnType=JSON";

                _logger.LogInformation("공공데이터포털 API 호출: page={Page}", page);

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.TryAddWithoutValidation("Authorization", $"Infuser {apiKey}");
                var response = await _httpClient.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogWarning("API 호출 실패: {StatusCode} - {Body}", response.StatusCode, errorBody);
                    break;
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var json = JsonDocument.Parse(content);
                var root = json.RootElement;

                // 응답 구조: { page, perPage, totalCount, currentCount, matchCount, data: [...] }
                if (!root.TryGetProperty("data", out var dataArray) || dataArray.GetArrayLength() == 0)
                {
                    hasMore = false;
                    break;
                }

                var totalCount = root.TryGetProperty("totalCount", out var tc) ? tc.GetInt32() : 0;
                if (page == 1)
                    _logger.LogInformation("총 {TotalCount}건의 서비스 발견", totalCount);

                foreach (var item in dataArray.EnumerateArray())
                {
                    try
                    {
                        var externalId = GetStr(item, "서비스ID");
                        if (string.IsNullOrEmpty(externalId))
                        {
                            skipped++;
                            continue;
                        }

                        // 중복 체크
                        var existing = await _subsidyRepository.GetByExternalIdAsync(externalId);
                        if (existing != null)
                        {
                            // 수정일시가 변경된 경우 업데이트
                            var modifiedDate = GetStr(item, "수정일시");
                            if (!string.IsNullOrEmpty(modifiedDate) && existing.UpdatedAt < DateTime.UtcNow.AddDays(-1))
                            {
                                UpdateSubsidyFromApi(existing, item);
                                await _subsidyRepository.UpdateAsync(existing);
                                updated++;
                            }
                            else
                            {
                                skipped++;
                            }
                            continue;
                        }

                        // 카테고리 매핑
                        var categoryCode = ResolveCategoryCode(GetStr(item, "서비스분야"));
                        var category = await _categoryRepository.GetByCodeAsync(categoryCode)
                                       ?? (await _categoryRepository.GetByCodeAsync("ETC"))!;

                        // 지역은 소관기관명에서 추론
                        var region = await ResolveRegionFromOrgAsync(GetStr(item, "소관기관명"));

                        var subsidy = new Subsidy
                        {
                            Title = GetStr(item, "서비스명") ?? "제목 없음",
                            Description = GetStr(item, "서비스목적요약") ?? "",
                            Organization = GetStr(item, "소관기관명") ?? "",
                            Amount = GetStr(item, "지원내용"),
                            EligibilityCriteria = BuildEligibility(item),
                            ApplicationMethod = GetStr(item, "신청방법"),
                            ApplicationUrl = GetStr(item, "상세조회URL"),
                            ContactInfo = GetStr(item, "전화문의"),
                            SourceUrl = GetStr(item, "상세조회URL")
                                        ?? $"https://www.gov.kr/portal/rcvfvrSvc/dtlEx/{externalId}",
                            ExternalId = externalId,
                            SourceType = SourceType.PublicDataPortal,
                            Status = SubsidyStatus.Active,
                            RegionId = region.Id,
                            CategoryId = category.Id
                        };

                        // 신청기한 파싱
                        var deadline = GetStr(item, "신청기한");
                        if (!string.IsNullOrEmpty(deadline) && deadline != "상시" && deadline != "별도공지")
                        {
                            if (DateTime.TryParse(deadline, out var endDate))
                                subsidy.ApplicationEndDate = endDate;
                        }

                        await _subsidyRepository.AddAsync(subsidy);
                        collected++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "항목 처리 중 오류 발생");
                        skipped++;
                    }
                }

                var currentCount = root.TryGetProperty("currentCount", out var cc) ? cc.GetInt32() : dataArray.GetArrayLength();
                if (currentCount < pageSize)
                    hasMore = false;
                else
                    page++;

                // Rate limiting - 공공데이터포털 일일 10,000건 제한 고려
                await Task.Delay(300, cancellationToken);
            }

            log.ItemsCollected = collected;
            log.ItemsUpdated = updated;
            log.ItemsSkipped = skipped;
            log.Status = CollectionStatus.Completed;
            log.CompletedAt = DateTime.UtcNow;
            await _logRepository.UpdateAsync(log);

            _logger.LogInformation("공공데이터포털 수집 완료: {Collected}건 수집, {Updated}건 업데이트, {Skipped}건 건너뜀",
                collected, updated, skipped);

            return collected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "공공데이터포털 수집 중 오류 발생");
            log.Status = CollectionStatus.Failed;
            log.ErrorMessage = ex.Message;
            log.CompletedAt = DateTime.UtcNow;
            await _logRepository.UpdateAsync(log);
            return 0;
        }
    }

    private void UpdateSubsidyFromApi(Subsidy subsidy, JsonElement item)
    {
        subsidy.Title = GetStr(item, "서비스명") ?? subsidy.Title;
        subsidy.Description = GetStr(item, "서비스목적요약") ?? subsidy.Description;
        subsidy.Organization = GetStr(item, "소관기관명") ?? subsidy.Organization;
        subsidy.Amount = GetStr(item, "지원내용") ?? subsidy.Amount;
        subsidy.EligibilityCriteria = BuildEligibility(item) ?? subsidy.EligibilityCriteria;
        subsidy.ApplicationMethod = GetStr(item, "신청방법") ?? subsidy.ApplicationMethod;
        subsidy.ContactInfo = GetStr(item, "전화문의") ?? subsidy.ContactInfo;
    }

    private static string? BuildEligibility(JsonElement item)
    {
        var target = GetStr(item, "지원대상");
        var criteria = GetStr(item, "선정기준");

        if (string.IsNullOrEmpty(target) && string.IsNullOrEmpty(criteria))
            return null;

        var parts = new List<string>();
        if (!string.IsNullOrEmpty(target)) parts.Add($"[지원대상] {target}");
        if (!string.IsNullOrEmpty(criteria)) parts.Add($"[선정기준] {criteria}");
        return string.Join("\n", parts);
    }

    private static string ResolveCategoryCode(string? serviceDomain)
    {
        if (string.IsNullOrEmpty(serviceDomain)) return "ETC";

        foreach (var (keyword, code) in CategoryMapping)
        {
            if (serviceDomain.Contains(keyword))
                return code;
        }
        return "ETC";
    }

    private async Task<Region> ResolveRegionFromOrgAsync(string? orgName)
    {
        if (string.IsNullOrEmpty(orgName))
            return (await _regionRepository.GetByCodeAsync("ALL"))!;

        // 소관기관명에서 지역명 추출 시도
        var regions = await _regionRepository.GetTopLevelAsync();
        foreach (var region in regions)
        {
            if (region.Code == "ALL") continue;
            // "서울특별시" → "서울", "경기도" → "경기" 등으로 매칭
            var shortName = region.Name.Replace("특별시", "").Replace("광역시", "")
                                       .Replace("특별자치시", "").Replace("특별자치도", "").Replace("도", "");
            if (orgName.Contains(shortName))
                return region;
        }

        return (await _regionRepository.GetByCodeAsync("ALL"))!;
    }

    private static string? GetStr(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop))
        {
            return prop.ValueKind switch
            {
                JsonValueKind.String => prop.GetString(),
                JsonValueKind.Number => prop.GetRawText(),
                _ => null
            };
        }
        return null;
    }
}
