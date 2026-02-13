using Microsoft.Extensions.Logging;
using SubsidyTracker.Core.Interfaces;

namespace SubsidyTracker.Collector.Services;

public class CollectionService
{
    private readonly IEnumerable<IDataCollector> _collectors;
    private readonly ILogger<CollectionService> _logger;

    public CollectionService(
        IEnumerable<IDataCollector> collectors,
        ILogger<CollectionService> logger)
    {
        _collectors = collectors;
        _logger = logger;
    }

    public async Task RunAllCollectorsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("전체 데이터 수집 시작 - {Count}개 수집기", _collectors.Count());

        var totalCollected = 0;

        foreach (var collector in _collectors)
        {
            if (cancellationToken.IsCancellationRequested) break;

            try
            {
                _logger.LogInformation("수집기 실행: {SourceName}", collector.SourceName);
                var count = await collector.CollectAsync(cancellationToken);
                totalCollected += count;
                _logger.LogInformation("수집기 완료: {SourceName} - {Count}건", collector.SourceName, count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "수집기 실행 중 오류: {SourceName}", collector.SourceName);
            }

            // 수집기 간 딜레이
            await Task.Delay(2000, cancellationToken);
        }

        _logger.LogInformation("전체 데이터 수집 완료 - 총 {TotalCollected}건", totalCollected);
    }

    public async Task RunCollectorAsync(string sourceName, CancellationToken cancellationToken = default)
    {
        var collector = _collectors.FirstOrDefault(c =>
            c.SourceName.Equals(sourceName, StringComparison.OrdinalIgnoreCase));

        if (collector == null)
        {
            _logger.LogWarning("수집기를 찾을 수 없습니다: {SourceName}", sourceName);
            return;
        }

        await collector.CollectAsync(cancellationToken);
    }
}
