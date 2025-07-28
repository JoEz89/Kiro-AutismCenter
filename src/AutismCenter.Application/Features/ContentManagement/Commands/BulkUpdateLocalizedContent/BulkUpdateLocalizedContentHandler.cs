using AutismCenter.Application.Common.Interfaces;
using AutismCenter.Domain.Entities;
using AutismCenter.Domain.Interfaces;
using MediatR;

namespace AutismCenter.Application.Features.ContentManagement.Commands.BulkUpdateLocalizedContent;

public class BulkUpdateLocalizedContentHandler : IRequestHandler<BulkUpdateLocalizedContentCommand, BulkUpdateLocalizedContentResponse>
{
    private readonly ILocalizedContentRepository _localizedContentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public BulkUpdateLocalizedContentHandler(
        ILocalizedContentRepository localizedContentRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _localizedContentRepository = localizedContentRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<BulkUpdateLocalizedContentResponse> Handle(BulkUpdateLocalizedContentCommand request, CancellationToken cancellationToken)
    {
        var currentUserEmail = _currentUserService.Email ?? "system";
        var results = new List<BulkUpdateLocalizedContentResult>();
        var successfulUpdates = 0;
        var failedUpdates = 0;

        foreach (var item in request.Items)
        {
            try
            {
                var existingContent = await _localizedContentRepository.GetByKeyAndLanguageAsync(item.Key, item.Language);
                
                if (existingContent != null)
                {
                    // Update existing content
                    existingContent.UpdateContent(item.Content, currentUserEmail);
                    if (item.Description != null)
                    {
                        existingContent.UpdateDescription(item.Description, currentUserEmail);
                    }
                    await _localizedContentRepository.UpdateAsync(existingContent);
                }
                else
                {
                    // Create new content
                    var newContent = new LocalizedContent(
                        item.Key,
                        item.Language,
                        item.Content,
                        request.Category,
                        item.Description,
                        currentUserEmail);
                    
                    await _localizedContentRepository.AddAsync(newContent);
                }

                results.Add(new BulkUpdateLocalizedContentResult(
                    item.Key,
                    item.Language.ToString(),
                    true));
                
                successfulUpdates++;
            }
            catch (Exception ex)
            {
                results.Add(new BulkUpdateLocalizedContentResult(
                    item.Key,
                    item.Language.ToString(),
                    false,
                    ex.Message));
                
                failedUpdates++;
            }
        }

        if (successfulUpdates > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new BulkUpdateLocalizedContentResponse(
            request.Items.Count(),
            successfulUpdates,
            failedUpdates,
            results);
    }
}