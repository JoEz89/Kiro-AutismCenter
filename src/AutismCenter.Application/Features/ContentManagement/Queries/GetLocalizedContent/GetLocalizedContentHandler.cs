using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.ContentManagement.Queries.GetLocalizedContent;

public class GetLocalizedContentHandler : IRequestHandler<GetLocalizedContentQuery, GetLocalizedContentResponse?>
{
    private readonly ILocalizedContentRepository _localizedContentRepository;

    public GetLocalizedContentHandler(ILocalizedContentRepository localizedContentRepository)
    {
        _localizedContentRepository = localizedContentRepository;
    }

    public async Task<GetLocalizedContentResponse?> Handle(GetLocalizedContentQuery request, CancellationToken cancellationToken)
    {
        var localizedContent = await _localizedContentRepository.GetByIdAsync(request.Id);
        
        if (localizedContent == null)
        {
            return null;
        }

        return new GetLocalizedContentResponse(
            localizedContent.Id,
            localizedContent.Key,
            localizedContent.Language,
            localizedContent.Content,
            localizedContent.Category,
            localizedContent.Description,
            localizedContent.IsActive,
            localizedContent.CreatedBy,
            localizedContent.UpdatedBy,
            localizedContent.CreatedAt,
            localizedContent.UpdatedAt);
    }
}