using AutismCenter.Application.Common.Exceptions;
using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.ContentManagement.Commands.UpdateLocalizedContent;

public class UpdateLocalizedContentHandler : IRequestHandler<UpdateLocalizedContentCommand, UpdateLocalizedContentResponse>
{
    private readonly ILocalizedContentRepository _localizedContentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateLocalizedContentHandler(
        ILocalizedContentRepository localizedContentRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _localizedContentRepository = localizedContentRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<UpdateLocalizedContentResponse> Handle(UpdateLocalizedContentCommand request, CancellationToken cancellationToken)
    {
        var localizedContent = await _localizedContentRepository.GetByIdAsync(request.Id);
        if (localizedContent == null)
        {
            throw new NotFoundException($"Localized content with ID '{request.Id}' not found");
        }

        var currentUserEmail = _currentUserService.Email ?? "system";

        localizedContent.UpdateContent(request.Content, currentUserEmail);
        
        if (request.Description != null)
        {
            localizedContent.UpdateDescription(request.Description, currentUserEmail);
        }

        await _localizedContentRepository.UpdateAsync(localizedContent);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new UpdateLocalizedContentResponse(
            localizedContent.Id,
            localizedContent.Key,
            localizedContent.Language,
            localizedContent.Content,
            localizedContent.Category,
            localizedContent.Description,
            localizedContent.IsActive,
            localizedContent.UpdatedAt);
    }
}