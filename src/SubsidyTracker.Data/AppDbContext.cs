using Microsoft.EntityFrameworkCore;
using SubsidyTracker.Core.Models;

namespace SubsidyTracker.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Subsidy> Subsidies => Set<Subsidy>();
    public DbSet<Region> Regions => Set<Region>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<TargetGroup> TargetGroups => Set<TargetGroup>();
    public DbSet<SubsidyTargetGroup> SubsidyTargetGroups => Set<SubsidyTargetGroup>();
    public DbSet<CollectionLog> CollectionLogs => Set<CollectionLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Subsidy
        modelBuilder.Entity<Subsidy>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.Organization).HasMaxLength(200);
            entity.Property(e => e.Amount).HasMaxLength(200);
            entity.Property(e => e.ExternalId).HasMaxLength(100);
            entity.Property(e => e.ApplicationUrl).HasMaxLength(500);
            entity.Property(e => e.SourceUrl).HasMaxLength(500);
            entity.Property(e => e.EligibilityCriteria).HasColumnType("text");
            entity.Property(e => e.ApplicationMethod).HasColumnType("text");
            entity.Property(e => e.ContactInfo).HasMaxLength(300);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.SourceType).HasConversion<string>().HasMaxLength(30);
            entity.HasIndex(e => e.ExternalId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.RegionId);
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.ViewCount);
            entity.HasOne(e => e.Region).WithMany(r => r.Subsidies).HasForeignKey(e => e.RegionId);
            entity.HasOne(e => e.Category).WithMany(c => c.Subsidies).HasForeignKey(e => e.CategoryId);
        });

        // Region
        modelBuilder.Entity<Region>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.HasOne(e => e.Parent).WithMany(e => e.Children).HasForeignKey(e => e.ParentId).OnDelete(DeleteBehavior.Restrict);
        });

        // Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        // TargetGroup
        modelBuilder.Entity<TargetGroup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        // SubsidyTargetGroup (many-to-many join)
        modelBuilder.Entity<SubsidyTargetGroup>(entity =>
        {
            entity.HasKey(e => new { e.SubsidyId, e.TargetGroupId });
            entity.HasOne(e => e.Subsidy).WithMany(s => s.SubsidyTargetGroups).HasForeignKey(e => e.SubsidyId);
            entity.HasOne(e => e.TargetGroup).WithMany(t => t.SubsidyTargetGroups).HasForeignKey(e => e.TargetGroupId);
        });

        // CollectionLog
        modelBuilder.Entity<CollectionLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Source).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SourceType).HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.ErrorMessage).HasColumnType("text");
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Regions (17 시도)
        modelBuilder.Entity<Region>().HasData(
            new Region { Id = 1, Name = "전국", Code = "ALL" },
            new Region { Id = 2, Name = "서울특별시", Code = "SEOUL", ParentId = null },
            new Region { Id = 3, Name = "부산광역시", Code = "BUSAN", ParentId = null },
            new Region { Id = 4, Name = "대구광역시", Code = "DAEGU", ParentId = null },
            new Region { Id = 5, Name = "인천광역시", Code = "INCHEON", ParentId = null },
            new Region { Id = 6, Name = "광주광역시", Code = "GWANGJU", ParentId = null },
            new Region { Id = 7, Name = "대전광역시", Code = "DAEJEON", ParentId = null },
            new Region { Id = 8, Name = "울산광역시", Code = "ULSAN", ParentId = null },
            new Region { Id = 9, Name = "세종특별자치시", Code = "SEJONG", ParentId = null },
            new Region { Id = 10, Name = "경기도", Code = "GYEONGGI", ParentId = null },
            new Region { Id = 11, Name = "강원특별자치도", Code = "GANGWON", ParentId = null },
            new Region { Id = 12, Name = "충청북도", Code = "CHUNGBUK", ParentId = null },
            new Region { Id = 13, Name = "충청남도", Code = "CHUNGNAM", ParentId = null },
            new Region { Id = 14, Name = "전북특별자치도", Code = "JEONBUK", ParentId = null },
            new Region { Id = 15, Name = "전라남도", Code = "JEONNAM", ParentId = null },
            new Region { Id = 16, Name = "경상북도", Code = "GYEONGBUK", ParentId = null },
            new Region { Id = 17, Name = "경상남도", Code = "GYEONGNAM", ParentId = null },
            new Region { Id = 18, Name = "제주특별자치도", Code = "JEJU", ParentId = null }
        );

        // Seed Categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "생활안정", Code = "LIVING", Description = "생활안정 지원금" },
            new Category { Id = 2, Name = "주거·자립", Code = "HOUSING", Description = "주거 및 자립 지원" },
            new Category { Id = 3, Name = "보육·교육", Code = "EDUCATION", Description = "보육 및 교육 지원" },
            new Category { Id = 4, Name = "고용·창업", Code = "EMPLOYMENT", Description = "고용 및 창업 지원" },
            new Category { Id = 5, Name = "보건·의료", Code = "HEALTH", Description = "보건 및 의료 지원" },
            new Category { Id = 6, Name = "행정·안전", Code = "ADMIN", Description = "행정 및 안전 지원" },
            new Category { Id = 7, Name = "문화·환경", Code = "CULTURE", Description = "문화 및 환경 지원" },
            new Category { Id = 8, Name = "농림·축산", Code = "AGRICULTURE", Description = "농림 및 축산 지원" },
            new Category { Id = 9, Name = "기타", Code = "ETC", Description = "기타 지원" },
            new Category { Id = 10, Name = "청년", Code = "YOUTH", Description = "청년 정책 및 지원" }
        );

        // Seed TargetGroups
        modelBuilder.Entity<TargetGroup>().HasData(
            new TargetGroup { Id = 1, Name = "청년", Code = "YOUTH" },
            new TargetGroup { Id = 2, Name = "중장년", Code = "MIDDLE" },
            new TargetGroup { Id = 3, Name = "노인", Code = "SENIOR" },
            new TargetGroup { Id = 4, Name = "장애인", Code = "DISABLED" },
            new TargetGroup { Id = 5, Name = "저소득층", Code = "LOWINCOME" },
            new TargetGroup { Id = 6, Name = "다문화가정", Code = "MULTICULTURAL" },
            new TargetGroup { Id = 7, Name = "한부모가정", Code = "SINGLEPARENT" },
            new TargetGroup { Id = 8, Name = "임산부", Code = "PREGNANT" },
            new TargetGroup { Id = 9, Name = "영유아", Code = "INFANT" },
            new TargetGroup { Id = 10, Name = "전체", Code = "ALL" }
        );
    }
}
