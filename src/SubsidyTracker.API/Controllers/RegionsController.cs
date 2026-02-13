using Microsoft.AspNetCore.Mvc;
using SubsidyTracker.Core.DTOs;
using SubsidyTracker.Core.Interfaces;
using SubsidyTracker.Core.Models;

namespace SubsidyTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegionsController : ControllerBase
{
    private readonly IRegionRepository _regionRepository;

    public RegionsController(IRegionRepository regionRepository)
    {
        _regionRepository = regionRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RegionDto>>> GetAll()
    {
        var regions = await _regionRepository.GetTopLevelAsync();
        return Ok(regions.Select(MapToDto));
    }

    [HttpGet("{id}/children")]
    public async Task<ActionResult<IEnumerable<RegionDto>>> GetChildren(int id)
    {
        var children = await _regionRepository.GetChildrenAsync(id);
        return Ok(children.Select(r => new RegionDto
        {
            Id = r.Id,
            Name = r.Name,
            Code = r.Code
        }));
    }

    private static RegionDto MapToDto(Region r) => new()
    {
        Id = r.Id,
        Name = r.Name,
        Code = r.Code,
        Children = r.Children?.Select(c => new RegionDto
        {
            Id = c.Id,
            Name = c.Name,
            Code = c.Code
        }).ToList() ?? new()
    };
}
