using Hangfire;
using Hangfire.InMemory;
using Microsoft.EntityFrameworkCore;
using SubsidyTracker.Collector.Collectors;
using SubsidyTracker.Core.Interfaces;
using SubsidyTracker.Data;
using SubsidyTracker.Data.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Load local settings (gitignored)
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.local.json", optional: true, reloadOnChange: true);

// Database - Development: SQLite, Production: PostgreSQL
if (builder.Environment.IsDevelopment())
{
    var dbPath = Path.Combine(builder.Environment.ContentRootPath, "..", "..", "subsidytracker.db");
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

// Data Collectors (API에서 수동 트리거 가능)
builder.Services.AddHttpClient<PublicDataCollector>();
builder.Services.AddHttpClient<BokjiroCrawler>();
builder.Services.AddHttpClient<YouthCenterCollector>();
builder.Services.AddScoped<IDataCollector, PublicDataCollector>();
builder.Services.AddScoped<IDataCollector, BokjiroCrawler>();
builder.Services.AddScoped<IDataCollector, YouthCenterCollector>();

// Hangfire - Development: InMemory, Production: PostgreSQL
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseInMemoryStorage());
builder.Services.AddHangfireServer();

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS (웹/앱 클라이언트 접근 허용)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
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
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "SubsidyTracker API v1");
    options.RoutePrefix = string.Empty;
});

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Hangfire Dashboard
app.UseHangfireDashboard("/hangfire");

app.Run();
