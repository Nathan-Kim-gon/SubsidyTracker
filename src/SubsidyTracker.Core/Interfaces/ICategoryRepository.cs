namespace SubsidyTracker.Core.Interfaces;

using SubsidyTracker.Core.Models;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync();
    Task<Category?> GetByCodeAsync(string code);
    Task<Category> AddAsync(Category category);
}
