using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Enums;
using AutismCenter.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AutismCenter.Infrastructure.Repositories;

public class EmailTemplateRepository : IEmailTemplateRepository
{
    private readonly IApplicationDbContext _context;

    public EmailTemplateRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmailTemplate?> GetByTemplateKeyAndLanguageAsync(string templateKey, Language language)
    {
        return await _context.EmailTemplates
            .FirstOrDefaultAsync(et => et.TemplateKey == templateKey && et.Language == language);
    }

    public async Task<IEnumerable<EmailTemplate>> GetAllAsync(Language? language = null)
    {
        var query = _context.EmailTemplates.AsQueryable();
        
        if (language.HasValue)
        {
            query = query.Where(et => et.Language == language.Value);
        }

        return await query.OrderBy(et => et.TemplateKey).ToListAsync();
    }

    public async Task<IEnumerable<EmailTemplate>> GetActiveAsync(Language? language = null)
    {
        var query = _context.EmailTemplates.Where(et => et.IsActive);
        
        if (language.HasValue)
        {
            query = query.Where(et => et.Language == language.Value);
        }

        return await query.OrderBy(et => et.TemplateKey).ToListAsync();
    }

    public async Task<EmailTemplate?> GetByIdAsync(Guid id)
    {
        return await _context.EmailTemplates.FindAsync(id);
    }

    public async Task AddAsync(EmailTemplate emailTemplate)
    {
        await _context.EmailTemplates.AddAsync(emailTemplate);
    }

    public async Task UpdateAsync(EmailTemplate emailTemplate)
    {
        _context.EmailTemplates.Update(emailTemplate);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
        {
            _context.EmailTemplates.Remove(entity);
        }
    }

    public async Task<bool> ExistsAsync(string templateKey, Language language)
    {
        return await _context.EmailTemplates
            .AnyAsync(et => et.TemplateKey == templateKey && et.Language == language);
    }

    public async Task<IEnumerable<string>> GetTemplateKeysAsync()
    {
        return await _context.EmailTemplates
            .Select(et => et.TemplateKey)
            .Distinct()
            .OrderBy(tk => tk)
            .ToListAsync();
    }
}