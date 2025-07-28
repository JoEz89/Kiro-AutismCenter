using MediatR;

namespace AutismCenter.Application.Features.Products.Commands.Admin.DeleteCategory;

public record DeleteCategoryCommand(
    Guid CategoryId
) : IRequest<DeleteCategoryResponse>;