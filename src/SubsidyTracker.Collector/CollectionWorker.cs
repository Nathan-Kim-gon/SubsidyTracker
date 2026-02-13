using SubsidyTracker.Collector.Services;

namespace SubsidyTracker.Collector;

public class CollectionWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CollectionWorker> _logger;
    private readonly IConfiguration _configuration;

    public CollectionWorker(
        IServiceProvider serviceProvider,
        ILogger<CollectionWorker> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("데이터 수집 워커 시작");

        // 시작 시 초기 수집 실행
        await RunCollectionAsync(stoppingToken);

        // 이후 설정된 간격으로 반복 실행
        var intervalHours = _configuration.GetValue<int>("Collection:IntervalHours", 24);

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("다음 수집까지 {Hours}시간 대기", intervalHours);
            await Task.Delay(TimeSpan.FromHours(intervalHours), stoppingToken);

            await RunCollectionAsync(stoppingToken);
        }
    }

    private async Task RunCollectionAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var collectionService = scope.ServiceProvider.GetRequiredService<CollectionService>();
            await collectionService.RunAllCollectorsAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "데이터 수집 실행 중 오류 발생");
        }
    }
}
