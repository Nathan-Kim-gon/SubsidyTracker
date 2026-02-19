using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SubsidyTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Code = table.Column<string>(maxLength: 20, nullable: false),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CollectionLogs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Source = table.Column<string>(maxLength: 100, nullable: false),
                    SourceType = table.Column<string>(maxLength: 30, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ItemsCollected = table.Column<int>(nullable: false),
                    ItemsUpdated = table.Column<int>(nullable: false),
                    ItemsSkipped = table.Column<int>(nullable: false),
                    Status = table.Column<string>(maxLength: 30, nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Code = table.Column<string>(maxLength: 20, nullable: false),
                    ParentId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Regions_Regions_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TargetGroups",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(maxLength: 50, nullable: false),
                    Code = table.Column<string>(maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TargetGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subsidies",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Organization = table.Column<string>(maxLength: 200, nullable: false),
                    Amount = table.Column<string>(maxLength: 200, nullable: true),
                    EligibilityCriteria = table.Column<string>(type: "text", nullable: true),
                    ApplicationMethod = table.Column<string>(type: "text", nullable: true),
                    ApplicationUrl = table.Column<string>(maxLength: 500, nullable: true),
                    ContactInfo = table.Column<string>(maxLength: 300, nullable: true),
                    SourceUrl = table.Column<string>(maxLength: 500, nullable: true),
                    ExternalId = table.Column<string>(maxLength: 100, nullable: true),
                    ApplicationStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApplicationEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(maxLength: 20, nullable: false),
                    SourceType = table.Column<string>(maxLength: 30, nullable: false),
                    ViewCount = table.Column<int>(nullable: false),
                    RegionId = table.Column<int>(nullable: false),
                    CategoryId = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subsidies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subsidies_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Subsidies_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubsidyTargetGroups",
                columns: table => new
                {
                    SubsidyId = table.Column<int>(nullable: false),
                    TargetGroupId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubsidyTargetGroups", x => new { x.SubsidyId, x.TargetGroupId });
                    table.ForeignKey(
                        name: "FK_SubsidyTargetGroups_Subsidies_SubsidyId",
                        column: x => x.SubsidyId,
                        principalTable: "Subsidies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubsidyTargetGroups_TargetGroups_TargetGroupId",
                        column: x => x.TargetGroupId,
                        principalTable: "TargetGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Code", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "LIVING", "생활안정 지원금", "생활안정" },
                    { 2, "HOUSING", "주거 및 자립 지원", "주거·자립" },
                    { 3, "EDUCATION", "보육 및 교육 지원", "보육·교육" },
                    { 4, "EMPLOYMENT", "고용 및 창업 지원", "고용·창업" },
                    { 5, "HEALTH", "보건 및 의료 지원", "보건·의료" },
                    { 6, "ADMIN", "행정 및 안전 지원", "행정·안전" },
                    { 7, "CULTURE", "문화 및 환경 지원", "문화·환경" },
                    { 8, "AGRICULTURE", "농림 및 축산 지원", "농림·축산" },
                    { 9, "ETC", "기타 지원", "기타" },
                    { 10, "YOUTH", "청년 정책 및 지원", "청년" }
                });

            migrationBuilder.InsertData(
                table: "Regions",
                columns: new[] { "Id", "Code", "Name", "ParentId" },
                values: new object[,]
                {
                    { 1, "ALL", "전국", null },
                    { 2, "SEOUL", "서울특별시", null },
                    { 3, "BUSAN", "부산광역시", null },
                    { 4, "DAEGU", "대구광역시", null },
                    { 5, "INCHEON", "인천광역시", null },
                    { 6, "GWANGJU", "광주광역시", null },
                    { 7, "DAEJEON", "대전광역시", null },
                    { 8, "ULSAN", "울산광역시", null },
                    { 9, "SEJONG", "세종특별자치시", null },
                    { 10, "GYEONGGI", "경기도", null },
                    { 11, "GANGWON", "강원특별자치도", null },
                    { 12, "CHUNGBUK", "충청북도", null },
                    { 13, "CHUNGNAM", "충청남도", null },
                    { 14, "JEONBUK", "전북특별자치도", null },
                    { 15, "JEONNAM", "전라남도", null },
                    { 16, "GYEONGBUK", "경상북도", null },
                    { 17, "GYEONGNAM", "경상남도", null },
                    { 18, "JEJU", "제주특별자치도", null }
                });

            migrationBuilder.InsertData(
                table: "TargetGroups",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { 1, "YOUTH", "청년" },
                    { 2, "MIDDLE", "중장년" },
                    { 3, "SENIOR", "노인" },
                    { 4, "DISABLED", "장애인" },
                    { 5, "LOWINCOME", "저소득층" },
                    { 6, "MULTICULTURAL", "다문화가정" },
                    { 7, "SINGLEPARENT", "한부모가정" },
                    { 8, "PREGNANT", "임산부" },
                    { 9, "INFANT", "영유아" },
                    { 10, "ALL", "전체" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Code",
                table: "Categories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Regions_Code",
                table: "Regions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Regions_ParentId",
                table: "Regions",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Subsidies_CategoryId",
                table: "Subsidies",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Subsidies_CreatedAt",
                table: "Subsidies",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Subsidies_ExternalId",
                table: "Subsidies",
                column: "ExternalId");

            migrationBuilder.CreateIndex(
                name: "IX_Subsidies_RegionId",
                table: "Subsidies",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Subsidies_Status",
                table: "Subsidies",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Subsidies_ViewCount",
                table: "Subsidies",
                column: "ViewCount");

            migrationBuilder.CreateIndex(
                name: "IX_SubsidyTargetGroups_TargetGroupId",
                table: "SubsidyTargetGroups",
                column: "TargetGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TargetGroups_Code",
                table: "TargetGroups",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CollectionLogs");

            migrationBuilder.DropTable(
                name: "SubsidyTargetGroups");

            migrationBuilder.DropTable(
                name: "Subsidies");

            migrationBuilder.DropTable(
                name: "TargetGroups");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Regions");
        }
    }
}
