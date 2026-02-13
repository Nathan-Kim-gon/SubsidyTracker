using Microsoft.EntityFrameworkCore;
using SubsidyTracker.Collector;
using SubsidyTracker.Collector.Collectors;
using SubsidyTracker.Collector.Services;
using SubsidyTracker.Core.Interfaces;
using SubsidyTracker.Data;
using SubsidyTracker.Data.Repositories;

var builder = Host.CreateApplicationBuilder(args);

// Load local settings (gitignored)
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.local.json", optional: true, reloadOnChange: true);

// Database - Development: SQLite, Production: PostgreSQL
var environment = builder.Environment.EnvironmentName;
if (environment == "Development")
{
    var dbPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "subsidytracker.db");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite($"Data Source={dbPath}"));
}
else
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection 연결 문자열이 설정되지 않았습니다.");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
}

// Repositories
builder.Services.AddScoped<ISubsidyRepository, SubsidyRepository>();
builder.Services.AddScoped<IRegionRepository, RegionRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICollectionLogRepository, CollectionLogRepository>();

// HttpClient
builder.Services.AddHttpClient<PublicDataCollector>();
builder.Services.AddHttpClient<BokjiroCrawler>();
builder.Services.AddHttpClient<YouthCenterCollector>();

// Collectors
builder.Services.AddScoped<IDataCollector, PublicDataCollector>();
builder.Services.AddScoped<IDataCollector, BokjiroCrawler>();
builder.Services.AddScoped<IDataCollector, YouthCenterCollector>();

// Collection Service
builder.Services.AddScoped<CollectionService>();

// Background Worker
builder.Services.AddHostedService<CollectionWorker>();

var host = builder.Build();
host.Run();
