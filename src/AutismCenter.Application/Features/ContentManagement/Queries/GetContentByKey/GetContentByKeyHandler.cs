using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.ContentManagement.Queries.GetContentByKey;

public class GetContentByKeyHandler : IRequestHandler<GetContentByKeyQuery, GetContentByKeyResponse?>
{
    private readonly ILocalizedContentRepository _localizedContentRepository;

    public GetContentByKeyHandler(ILocalizedContentRepository localizedContentRepository)
    {
        _localizedContentRepository = localizedContentRepository;
    }

    public async Task<GetContentByKeyResponse?> Handle(GetContentByKeyQuery request, CancellationToken cancellationToken)
    {
        var allContent = await _localizedContentRepository.GetAllAsync();
        var contentForKey = allContent.Where(c => c.Key == request.Key).ToList();
        
        if (!contentForKey.Any())
        {
            return null;
        }

        var category = contentForKey.First().Category;
        var translations = contentForKey.Select(c => new ContentTranslation(
            c.Id,
            c.Language,
            c.Content,
            c.Description,
            c.IsActive,
            c.UpdatedBy,
            c.UpdatedAt));

        return new GetContentByKeyResponse(
            request.Key,
            category,
            translations);
    }
}