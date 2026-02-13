using Microsoft.AspNetCore.Mvc;
using SubsidyTracker.Core.DTOs;
using SubsidyTracker.Core.Interfaces;

namespace SubsidyTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CollectionController : ControllerBase
{
    private readonly ICollectionLogRepository _logRepository;
    private readonly IEnumerable<IDataCollector> _collectors;

    public CollectionController(
        ICollectionLogRepository logRepository,
        IEnumerable<IDataCollector> collectors)
    {
        _logRepository = logRepository;
        _collectors = collectors;
    }

    [HttpGet("logs")]
    public async Task<ActionResult<IEnumerable<CollectionLogDto>>> GetLogs([FromQuery] int count = 10)
    {
        var logs = await _logRepository.GetRecentAsync(count);
        return Ok(logs.Select(l => new CollectionLogDto
        {
            Id = l.Id,
            Source = l.Source,
            SourceType = l.SourceType.ToString(),
            StartedAt = l.StartedAt,
            CompletedAt = l.CompletedAt,
            ItemsCollected = l.ItemsCollected,
            ItemsUpdated = l.ItemsUpdated,
            ItemsSkipped = l.ItemsSkipped,
            Status = l.Status.ToString(),
            ErrorMessage = l.ErrorMessage
        }));
    }

    [HttpPost("trigger/{sourceName}")]
    public async Task<ActionResult> TriggerCollection(string sourceName)
    {
        var collector = _collectors.FirstOrDefault(c =>
            c.SourceName.Equals(sourceName, StringComparison.OrdinalIgnoreCase));

        if (collector == null)
            return NotFound($"수집기 '{sourceName}'을(를) 찾을 수 없습니다.");

        var count = await collector.CollectAsync();
        return Ok(new { Message = $"{count}건 수집 완료", Source = sourceName });
    }

    [HttpGet("sources")]
    public ActionResult<IEnumerable<string>> GetSources()
    {
        return Ok(_collectors.Select(c => c.SourceName));
    }
}
