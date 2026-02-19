using Hangfire;
using Hangfire.InMemory;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using SubsidyTracker.Collector.Collectors;
using SubsidyTracker.Collector.Services;
using SubsidyTracker.Core.Interfaces;
using SubsidyTracker.Data;
using SubsidyTracker.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Load local settings (gitignored)
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.local.json", optional: true, reloadOnChange: true);

// Database - Development: SQLite, Production: PostgreSQL
string? connectionString = null;
if (builder.Environment.IsDevelopment())
{
    var dbPath = Path.Combine(builder.Environment.ContentRootPath, "..", "..", "subsidytracker.db");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite($"Data Source={dbPath}"));
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection 연결 문자열이 설정되지 않았습니다.");
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
}

// Repositories
builder.Services.AddScoped<ISubsidyRepository, SubsidyRepository>();
builder.Services.AddScoped<IRegionRepository, RegionRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICollectionLogRepository, CollectionLogRepository>();

// Data Collectors + Collection Service
builder.Services.AddHttpClient<PublicDataCollector>();
builder.Services.AddHttpClient<YouthCenterCollector>();
builder.Services.AddScoped<IDataCollector, PublicDataCollector>();
builder.Services.AddScoped<IDataCollector, YouthCenterCollector>();
builder.Services.AddScoped<CollectionService>();

// Hangfire - Development: InMemory, Production: PostgreSQL
builder.Services.AddHangfire(config =>
{
    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
          .UseSimpleAssemblyNameTypeSerializer()
          .UseRecommendedSerializerSettings();

    if (builder.Environment.IsDevelopment())
        config.UseInMemoryStorage();
    else
        config.UsePostgreSqlStorage(options =>
            options.UseNpgsqlConnection(connectionString!));
});
builder.Services.AddHangfireServer();

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            policy.WithOrigins(
                      "https://bojogeum.co.kr",
                      "https://www.bojogeum.co.kr")
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
    });
});

var app = builder.Build();

// Auto-create/migrate database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (app.Environment.IsDevelopment())
    {
        await dbContext.Database.EnsureCreatedAsync();
    }
    else
    {
        await dbContext.Database.MigrateAsync();
    }
}

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SubsidyTracker API v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Hangfire Dashboard
app.UseHangfireDashboard("/hangfire");

// 데이터 수집 반복 작업 (24시간마다)
RecurringJob.AddOrUpdate<CollectionService>(
    "collect-all",
    service => service.RunAllCollectorsAsync(CancellationToken.None),
    Cron.Daily);

app.Run();
