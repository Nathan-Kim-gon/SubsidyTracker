namespace SubsidyTracker.Core.Interfaces;

public interface IDataCollector
{
    string SourceName { get; }
    Task<int> CollectAsync(CancellationToken cancellationToken = default);
}
