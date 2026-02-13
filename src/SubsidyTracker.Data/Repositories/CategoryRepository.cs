using Microsoft.EntityFrameworkCore;
using SubsidyTracker.Core.Interfaces;
using SubsidyTracker.Core.Models;

namespace SubsidyTracker.Data.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context) => _context = context;

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Categories.OrderBy(c => c.Id).ToListAsync();
    }

    public async Task<Category?> GetByCodeAsync(string code)
    {
        return await _context.Categories.FirstOrDefaultAsync(c => c.Code == code);
    }

    public async Task<Category> AddAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }
}
