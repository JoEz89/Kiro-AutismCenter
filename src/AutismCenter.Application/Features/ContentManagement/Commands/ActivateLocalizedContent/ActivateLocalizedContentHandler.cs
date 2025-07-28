using AutismCenter.Application.Common.Exceptions;
using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.ContentManagement.Commands.ActivateLocalizedContent;

public class ActivateLocalizedContentHandler : IRequestHandler<ActivateLocalizedContentCommand, ActivateLocalizedContentResponse>
{
    private readonly ILocalizedContentRepository _localizedContentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public ActivateLocalizedContentHandler(
        ILocalizedContentRepository localizedContentRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _localizedContentRepository = localizedContentRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ActivateLocalizedContentResponse> Handle(ActivateLocalizedContentCommand request, CancellationToken cancellationToken)
    {
        var localizedContent = await _localizedContentRepository.GetByIdAsync(request.Id);
        if (localizedContent == null)
        {
            throw new NotFoundException($"Localized content with ID '{request.Id}' not found");
        }

        var currentUserEmail = _currentUserService.Email ?? "system";
        localizedContent.Activate(currentUserEmail);

        await _localizedContentRepository.UpdateAsync(localizedContent);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ActivateLocalizedContentResponse(
            localizedContent.Id,
            localizedContent.Key,
            localizedContent.IsActive,
            $"Content '{localizedContent.Key}' has been activated successfully");
    }
}