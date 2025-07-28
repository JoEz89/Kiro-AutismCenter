using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.ContentManagement.Queries.GetContentCategories;

public class GetContentCategoriesHandler : IRequestHandler<GetContentCategoriesQuery, GetContentCategoriesResponse>
{
    private readonly ILocalizedContentRepository _localizedContentRepository;

    public GetContentCategoriesHandler(ILocalizedContentRepository localizedContentRepository)
    {
        _localizedContentRepository = localizedContentRepository;
    }

    public async Task<GetContentCategoriesResponse> Handle(GetContentCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _localizedContentRepository.GetCategoriesAsync();
        
        return new GetContentCategoriesResponse(categories.OrderBy(c => c));
    }
}