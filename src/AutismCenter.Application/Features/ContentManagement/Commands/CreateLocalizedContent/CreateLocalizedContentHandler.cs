using AutismCenter.Application.Common.Exceptions;
using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.ContentManagement.Commands.CreateLocalizedContent;

public class CreateLocalizedContentHandler : IRequestHandler<CreateLocalizedContentCommand, CreateLocalizedContentResponse>
{
    private readonly ILocalizedContentRepository _localizedContentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateLocalizedContentHandler(
        ILocalizedContentRepository localizedContentRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _localizedContentRepository = localizedContentRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<CreateLocalizedContentResponse> Handle(CreateLocalizedContentCommand request, CancellationToken cancellationToken)
    {
        // Check if content already exists for this key and language
        var existingContent = await _localizedContentRepository.GetByKeyAndLanguageAsync(request.Key, request.Language);
        if (existingContent != null)
        {
            throw new ConflictException($"Content with key '{request.Key}' already exists for language '{request.Language}'");
        }

        var currentUserId = _currentUserService.UserId ?? throw new UnauthorizedException("User not authenticated");
        var currentUserEmail = _currentUserService.Email ?? "system";

        var localizedContent = new LocalizedContent(
            request.Key,
            request.Language,
            request.Content,
            request.Category,
            request.Description,
            currentUserEmail);

        await _localizedContentRepository.AddAsync(localizedContent);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateLocalizedContentResponse(
            localizedContent.Id,
            localizedContent.Key,
            localizedContent.Language,
            localizedContent.Content,
            localizedContent.Category,
            localizedContent.Description,
            localizedContent.IsActive,
            localizedContent.CreatedAt);
    }
}