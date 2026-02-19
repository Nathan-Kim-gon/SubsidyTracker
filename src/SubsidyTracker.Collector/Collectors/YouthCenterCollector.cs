using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SubsidyTracker.Core.Interfaces;
using SubsidyTracker.Core.Models;

namespace SubsidyTracker.Collector.Collectors;

/// <summary>
/// 온통청년 - 청년정책 API
/// 엔드포인트: https://www.youthcenter.go.kr/go/ythip/getPlcy
/// 파라미터: apiKeyNm, pageNum, pageSize, rtnType=json
///
/// 응답 구조:
/// {
///   "resultCode": 200,
///   "resultMessage": "성공적으로 데이터를 가지고 왔습니다.",
///   "result": {
///     "pagging": { "totCount": 1619, "pageNum": 1, "pageSize": 100 },
///     "youthPolicyList": [
///       {
///         "plcyNo": "...",           // 정책번호
///         "plcyNm": "...",           // 정책명
///         "plcyExplnCn": "...",      // 정책설명
///         "plcyKywdNm": "...",       // 키워드
///         "lclsfNm": "일자리",       // 대분류
///         "mclsfNm": "창업",         // 중분류
///         "plcySprtCn": "...",       // 지원내용
///         "sprvsnInstCdNm": "...",   // 주관기관명
///         "plcyAplyMthdCn": "...",   // 신청방법
///         "aplyUrlAddr": "...",      // 신청URL
///         "addAplyQlfcCndCn": "...", // 자격요건
///         "bizPrdBgngYmd": "...",    // 사업시작일
///         "bizPrdEndYmd": "...",     // 사업종료일
///         "inqCnt": "38",           // 조회수
///         "refUrlAddr1": "...",      // 참고URL
///         "rgtrInstCdNm": "...",     // 등록기관명
///         "frstRegDt": "...",        // 등록일
///         "lastMdfcnDt": "..."       // 수정일
///       }
///     ]
///   }
/// }
/// </summary>
public class YouthCenterCollector : IDataCollector
{
    public string SourceName => "YouthCenter";

    private readonly HttpClient _httpClient;
    private readonly ISubsidyRepository _subsidyRepository;
    private readonly IRegionRepository _regionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICollectionLogRepository _logRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<YouthCenterCollector> _logger;

    // 대분류(lclsfNm) → 카테고리 매핑
    private static readonly Dictionary<string, string> CategoryMapping = new(StringComparer.OrdinalIgnoreCase)
    {
        { "일자리", "EMPLOYMENT" },
        { "주거", "HOUSING" },
        { "교육", "EDUCATION" },
        { "복지·문화", "CULTURE" },
        { "복지문화", "CULTURE" },
        { "참여·권리", "ADMIN" },
        { "참여권리", "ADMIN" },
    };

    public YouthCenterCollector(
        HttpClient httpClient,
        ISubsidyRepository subsidyRepository,
        IRegionRepository regionRepository,
        ICategoryRepository categoryRepository,
        ICollectionLogRepository logRepository,
        IConfiguration configuration,
        ILogger<YouthCenterCollector> logger)
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
            SourceType = SourceType.YouthCenter,
            StartedAt = DateTime.UtcNow,
            Status = CollectionStatus.Running
        };
        await _logRepository.AddAsync(log);

        try
        {
            var apiKey = _configuration["YouthCenter:ApiKey"];
            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_API_KEY_HERE")
            {
                _logger.LogWarning("온통청년 API 키가 설정되지 않았습니다. appsettings의 YouthCenter:ApiKey를 설정해주세요.");
                log.Status = CollectionStatus.Failed;
                log.ErrorMessage = "API 키가 설정되지 않았습니다.";
                log.CompletedAt = DateTime.UtcNow;
                await _logRepository.UpdateAsync(log);
                return 0;
            }

            var collected = 0;
            var updated = 0;
            var skipped = 0;
            var pageNum = 1;
            var pageSize = 100;
            var hasMore = true;
            var seenExternalIds = new List<string>();

            while (hasMore && !cancellationToken.IsCancellationRequested)
            {
                var url = $"https://www.youthcenter.go.kr/go/ythip/getPlcy?apiKeyNm={apiKey}&pageNum={pageNum}&pageSize={pageSize}&rtnType=json";

                _logger.LogInformation("온통청년 API 호출: page={Page}", pageNum);

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                var response = await _httpClient.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("온통청년 API 호출 실패: {StatusCode}", response.StatusCode);
                    break;
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var json = JsonDocument.Parse(content);
                var root = json.RootElement;

                var resultCode = root.TryGetProperty("resultCode", out var rc) ? rc.GetInt32() : 0;
                if (resultCode != 200)
                {
                    var msg = root.TryGetProperty("resultMessage", out var rm) ? rm.GetString() : "Unknown error";
                    _logger.LogWarning("온통청년 API 오류: {Message}", msg);
                    break;
                }

                if (!root.TryGetProperty("result", out var result))
                    break;

                if (!result.TryGetProperty("youthPolicyList", out var policyList) || policyList.GetArrayLength() == 0)
                {
                    hasMore = false;
                    break;
                }

                var totCount = 0;
                if (result.TryGetProperty("pagging", out var pagging))
                    totCount = pagging.TryGetProperty("totCount", out var tc) ? tc.GetInt32() : 0;

                if (pageNum == 1)
                    _logger.LogInformation("온통청년 총 {TotalCount}건의 청년정책 발견", totCount);

                foreach (var item in policyList.EnumerateArray())
                {
                    try
                    {
                        var plcyNo = GetStr(item, "plcyNo");
                        if (string.IsNullOrEmpty(plcyNo))
                        {
                            skipped++;
                            continue;
                        }

                        var externalId = $"youth_{plcyNo}";
                        seenExternalIds.Add(externalId);

                        // 중복 체크
                        var existing = await _subsidyRepository.GetByExternalIdAsync(externalId);
                        if (existing != null)
                        {
                            // 수정일이 변경된 경우 업데이트
                            var lastModified = GetStr(item, "lastMdfcnDt");
                            if (!string.IsNullOrEmpty(lastModified) && existing.UpdatedAt < DateTime.UtcNow.AddDays(-1))
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

                        // 온통청년 데이터는 모두 "청년" 카테고리
                        var category = await _categoryRepository.GetByCodeAsync("YOUTH")
                                       ?? (await _categoryRepository.GetByCodeAsync("ETC"))!;

                        // 지역 매핑 (주관기관명에서 추론)
                        var region = await ResolveRegionAsync(GetStr(item, "sprvsnInstCdNm") ?? GetStr(item, "rgtrInstCdNm"));

                        var subsidy = new Subsidy
                        {
                            Title = GetStr(item, "plcyNm") ?? "제목 없음",
                            Description = GetStr(item, "plcyExplnCn") ?? "",
                            Organization = GetStr(item, "sprvsnInstCdNm") ?? "온통청년",
                            Amount = GetStr(item, "plcySprtCn"),
                            EligibilityCriteria = BuildEligibility(item),
                            ApplicationMethod = GetStr(item, "plcyAplyMthdCn"),
                            ApplicationUrl = GetStr(item, "aplyUrlAddr"),
                            ContactInfo = GetStr(item, "sprvsnInstPicNm"),
                            SourceUrl = GetStr(item, "refUrlAddr1")
                                        ?? $"https://www.youthcenter.go.kr/youthPolicy/bis498/{plcyNo}",
                            ExternalId = externalId,
                            SourceType = SourceType.YouthCenter,
                            Status = SubsidyStatus.Active,
                            RegionId = region.Id,
                            CategoryId = category.Id,
                            ViewCount = GetInt(item, "inqCnt")
                        };

                        // 사업기간 파싱
                        var endDate = GetStr(item, "bizPrdEndYmd")?.Trim();
                        if (!string.IsNullOrEmpty(endDate) && endDate.Length >= 8)
                        {
                            if (DateTime.TryParse(endDate, out var parsedEnd))
                                subsidy.ApplicationEndDate = parsedEnd;
                        }
                        var startDate = GetStr(item, "bizPrdBgngYmd")?.Trim();
                        if (!string.IsNullOrEmpty(startDate) && startDate.Length >= 8)
                        {
                            if (DateTime.TryParse(startDate, out var parsedStart))
                                subsidy.ApplicationStartDate = parsedStart;
                        }

                        await _subsidyRepository.AddAsync(subsidy);
                        collected++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "온통청년 항목 처리 중 오류 발생");
                        skipped++;
                    }
                }

                if (policyList.GetArrayLength() < pageSize)
                    hasMore = false;
                else
                    pageNum++;

                // Rate limiting
                await Task.Delay(300, cancellationToken);
            }

            // API에서 사라진 항목을 Closed 처리
            var closed = 0;
            if (seenExternalIds.Count > 0)
            {
                closed = await _subsidyRepository.CloseMissingAsync(SourceType.YouthCenter, seenExternalIds);
                if (closed > 0)
                    _logger.LogInformation("만료 처리: {Closed}건의 청년정책이 Closed로 변경됨", closed);
            }

            log.ItemsCollected = collected;
            log.ItemsUpdated = updated;
            log.ItemsSkipped = skipped;
            log.Status = CollectionStatus.Completed;
            log.CompletedAt = DateTime.UtcNow;
            await _logRepository.UpdateAsync(log);

            _logger.LogInformation("온통청년 수집 완료: {Collected}건 수집, {Updated}건 업데이트, {Skipped}건 건너뜀, {Closed}건 만료",
                collected, updated, skipped, closed);

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

    private void UpdateSubsidyFromApi(Subsidy subsidy, JsonElement item)
    {
        subsidy.Title = GetStr(item, "plcyNm") ?? subsidy.Title;
        subsidy.Description = GetStr(item, "plcyExplnCn") ?? subsidy.Description;
        subsidy.Organization = GetStr(item, "sprvsnInstCdNm") ?? subsidy.Organization;
        subsidy.Amount = GetStr(item, "plcySprtCn") ?? subsidy.Amount;
        subsidy.EligibilityCriteria = BuildEligibility(item) ?? subsidy.EligibilityCriteria;
        subsidy.ApplicationMethod = GetStr(item, "plcyAplyMthdCn") ?? subsidy.ApplicationMethod;
        subsidy.ContactInfo = GetStr(item, "sprvsnInstPicNm") ?? subsidy.ContactInfo;
        subsidy.ViewCount = GetInt(item, "inqCnt");
    }

    private static string? BuildEligibility(JsonElement item)
    {
        var qualification = GetStr(item, "addAplyQlfcCndCn");
        var excluded = GetStr(item, "ptcpPrpTrgtCn");
        var minAge = GetStr(item, "sprtTrgtMinAge");
        var maxAge = GetStr(item, "sprtTrgtMaxAge");

        var parts = new List<string>();
        if (!string.IsNullOrEmpty(minAge) && !string.IsNullOrEmpty(maxAge)
            && minAge != "0" && maxAge != "0")
            parts.Add($"[연령] {minAge}세 ~ {maxAge}세");
        if (!string.IsNullOrEmpty(qualification))
            parts.Add($"[자격요건] {qualification}");
        if (!string.IsNullOrEmpty(excluded))
            parts.Add($"[참여제외] {excluded}");

        return parts.Count > 0 ? string.Join("\n", parts) : null;
    }

    private static string ResolveCategoryCode(string? lclsfNm)
    {
        if (string.IsNullOrEmpty(lclsfNm)) return "ETC";

        foreach (var (keyword, code) in CategoryMapping)
        {
            if (lclsfNm.Contains(keyword))
                return code;
        }
        return "ETC";
    }

    private async Task<Region> ResolveRegionAsync(string? instName)
    {
        if (string.IsNullOrEmpty(instName))
            return (await _regionRepository.GetByCodeAsync("ALL"))!;

        var regions = await _regionRepository.GetTopLevelAsync();
        foreach (var region in regions)
        {
            if (region.Code == "ALL") continue;
            var shortName = region.Name.Replace("특별시", "").Replace("광역시", "")
                                       .Replace("특별자치시", "").Replace("특별자치도", "").Replace("도", "");
            if (instName.Contains(shortName))
                return region;
        }

        return (await _regionRepository.GetByCodeAsync("ALL"))!;
    }

    private static int GetInt(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop))
        {
            if (prop.ValueKind == JsonValueKind.Number)
                return prop.GetInt32();
            if (prop.ValueKind == JsonValueKind.String && int.TryParse(prop.GetString(), out var val))
                return val;
        }
        return 0;
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
