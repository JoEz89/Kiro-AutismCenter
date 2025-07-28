using AutismCenter.Application.Common.Exceptions;
using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.ContentManagement.Commands.DeleteLocalizedContent;

public class DeleteLocalizedContentHandler : IRequestHandler<DeleteLocalizedContentCommand, DeleteLocalizedContentResponse>
{
    private readonly ILocalizedContentRepository _localizedContentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteLocalizedContentHandler(
        ILocalizedContentRepository localizedContentRepository,
        IUnitOfWork unitOfWork)
    {
        _localizedContentRepository = localizedContentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DeleteLocalizedContentResponse> Handle(DeleteLocalizedContentCommand request, CancellationToken cancellationToken)
    {
        var localizedContent = await _localizedContentRepository.GetByIdAsync(request.Id);
        if (localizedContent == null)
        {
            throw new NotFoundException($"Localized content with ID '{request.Id}' not found");
        }

        await _localizedContentRepository.DeleteAsync(request.Id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new DeleteLocalizedContentResponse(
            true,
            $"Localized content '{localizedContent.Key}' for language '{localizedContent.Language}' has been deleted successfully");
    }
}