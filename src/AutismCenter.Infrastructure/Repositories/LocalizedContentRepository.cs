using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Enums;
using AutismCenter.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutismCenter.Infrastructure.Repositories;

public class LocalizedContentRepository : ILocalizedContentRepository
{
    private readonly IApplicationDbContext _context;

    public LocalizedContentRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LocalizedContent?> GetByKeyAndLanguageAsync(string key, Language language)
    {
        return await _context.LocalizedContents
            .FirstOrDefaultAsync(lc => lc.Key == key && lc.Language == language);
    }

    public async Task<IEnumerable<LocalizedContent>> GetByCategoryAsync(string category, Language? language = null)
    {
        var query = _context.LocalizedContents.Where(lc => lc.Category == category);
        
        if (language.HasValue)
        {
            query = query.Where(lc => lc.Language == language.Value);
        }

        return await query.OrderBy(lc => lc.Key).ToListAsync();
    }

    public async Task<IEnumerable<LocalizedContent>> GetAllAsync(Language? language = null)
    {
        var query = _context.LocalizedContents.AsQueryable();
        
        if (language.HasValue)
        {
            query = query.Where(lc => lc.Language == language.Value);
        }

        return await query.OrderBy(lc => lc.Category).ThenBy(lc => lc.Key).ToListAsync();
    }

    public async Task<IEnumerable<LocalizedContent>> GetActiveAsync(Language? language = null)
    {
        var query = _context.LocalizedContents.Where(lc => lc.IsActive);
        
        if (language.HasValue)
        {
            query = query.Where(lc => lc.Language == language.Value);
        }

        return await query.OrderBy(lc => lc.Category).ThenBy(lc => lc.Key).ToListAsync();
    }

    public async Task<LocalizedContent?> GetByIdAsync(Guid id)
    {
        return await _context.LocalizedContents.FindAsync(id);
    }

    public async Task AddAsync(LocalizedContent localizedContent)
    {
        await _context.LocalizedContents.AddAsync(localizedContent);
    }

    public async Task UpdateAsync(LocalizedContent localizedContent)
    {
        _context.LocalizedContents.Update(localizedContent);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.LocalizedContents.Remove(entity);
        }
    }

    public async Task<bool> ExistsAsync(string key, Language language)
    {
        return await _context.LocalizedContents
            .AnyAsync(lc => lc.Key == key && lc.Language == language);
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        return await _context.LocalizedContents
            .Select(lc => lc.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    public async Task<IEnumerable<LocalizedContent>> SearchAsync(string searchTerm, Language? language = null)
    {
        var query = _context.LocalizedContents
            .Where(lc => lc.Key.Contains(searchTerm) || 
                        lc.Content.Contains(searchTerm) || 
                        (lc.Description != null && lc.Description.Contains(searchTerm)));
        
        if (language.HasValue)
        {
            query = query.Where(lc => lc.Language == language.Value);
        }

        return await query.OrderBy(lc => lc.Category).ThenBy(lc => lc.Key).ToListAsync();
    }
}