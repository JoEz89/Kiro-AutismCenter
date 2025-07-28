using AutismCenter.Application.Common.Exceptions;
using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.ContentManagement.Commands.DeactivateLocalizedContent;

public class DeactivateLocalizedContentHandler : IRequestHandler<DeactivateLocalizedContentCommand, DeactivateLocalizedContentResponse>
{
    private readonly ILocalizedContentRepository _localizedContentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeactivateLocalizedContentHandler(
        ILocalizedContentRepository localizedContentRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _localizedContentRepository = localizedContentRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<DeactivateLocalizedContentResponse> Handle(DeactivateLocalizedContentCommand request, CancellationToken cancellationToken)
    {
        var localizedContent = await _localizedContentRepository.GetByIdAsync(request.Id);
        if (localizedContent == null)
        {
            throw new NotFoundException($"Localized content with ID '{request.Id}' not found");
        }

        var currentUserEmail = _currentUserService.Email ?? "system";
        localizedContent.Deactivate(currentUserEmail);

        await _localizedContentRepository.UpdateAsync(localizedContent);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DeactivateLocalizedContentResponse(
            localizedContent.Id,
            localizedContent.Key,
            localizedContent.IsActive,
            $"Content '{localizedContent.Key}' has been deactivated successfully");
    }
}