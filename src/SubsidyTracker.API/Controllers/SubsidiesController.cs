using Microsoft.AspNetCore.Mvc;
using SubsidyTracker.Core.DTOs;
using SubsidyTracker.Core.Interfaces;
using SubsidyTracker.Core.Models;

namespace SubsidyTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubsidiesController : ControllerBase
{
    private readonly ISubsidyRepository _subsidyRepository;

    public SubsidiesController(ISubsidyRepository subsidyRepository)
    {
        _subsidyRepository = subsidyRepository;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<SubsidyListDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? keyword = null,
        [FromQuery] int? regionId = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] int? targetGroupId = null,
        [FromQuery] SubsidyStatus? status = null,
        [FromQuery] string? sortBy = "CreatedAt",
        [FromQuery] bool sortDescending = true)
    {
        var filter = new SubsidyFilter
        {
            Page = page,
            PageSize = Math.Min(pageSize, 100),
            Keyword = keyword,
            RegionId = regionId,
            CategoryId = categoryId,
            TargetGroupId = targetGroupId,
            Status = status ?? SubsidyStatus.Active,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        var subsidies = await _subsidyRepository.GetAllAsync(filter);
        var totalCount = await _subsidyRepository.GetCountAsync(filter);

        var result = new PagedResult<SubsidyListDto>
        {
            Items = subsidies.Select(MapToListDto),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SubsidyDetailDto>> GetById(int id)
    {
        var subsidy = await _subsidyRepository.GetByIdAsync(id);
        if (subsidy == null)
            return NotFound();

        return Ok(MapToDetailDto(subsidy));
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<SubsidyListDto>>> Search(
        [FromQuery] string keyword,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return BadRequest("검색어를 입력해주세요.");

        var filter = new SubsidyFilter
        {
            Keyword = keyword,
            Page = page,
            PageSize = Math.Min(pageSize, 100)
        };

        var subsidies = await _subsidyRepository.GetAllAsync(filter);
        var totalCount = await _subsidyRepository.GetCountAsync(filter);

        return Ok(new PagedResult<SubsidyListDto>
        {
            Items = subsidies.Select(MapToListDto),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    private static SubsidyListDto MapToListDto(Subsidy s) => new()
    {
        Id = s.Id,
        Title = s.Title,
        Organization = s.Organization,
        Amount = s.Amount,
        RegionName = s.Region?.Name ?? "",
        CategoryName = s.Category?.Name ?? "",
        TargetGroups = s.SubsidyTargetGroups?.Select(stg => stg.TargetGroup?.Name ?? "").Where(n => !string.IsNullOrEmpty(n)).ToList() ?? new(),
        Status = s.Status.ToString(),
        ApplicationEndDate = s.ApplicationEndDate,
        CreatedAt = s.CreatedAt
    };

    private static SubsidyDetailDto MapToDetailDto(Subsidy s) => new()
    {
        Id = s.Id,
        Title = s.Title,
        Description = s.Description,
        Organization = s.Organization,
        Amount = s.Amount,
        EligibilityCriteria = s.EligibilityCriteria,
        ApplicationMethod = s.ApplicationMethod,
        ApplicationUrl = s.ApplicationUrl,
        ContactInfo = s.ContactInfo,
        SourceUrl = s.SourceUrl,
        ApplicationStartDate = s.ApplicationStartDate,
        ApplicationEndDate = s.ApplicationEndDate,
        Status = s.Status.ToString(),
        RegionName = s.Region?.Name ?? "",
        CategoryName = s.Category?.Name ?? "",
        TargetGroups = s.SubsidyTargetGroups?.Select(stg => stg.TargetGroup?.Name ?? "").Where(n => !string.IsNullOrEmpty(n)).ToList() ?? new(),
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt
    };
}
