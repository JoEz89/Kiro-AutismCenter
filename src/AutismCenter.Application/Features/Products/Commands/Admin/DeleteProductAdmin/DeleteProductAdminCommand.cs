using MediatR;

namespace AutismCenter.Application.Features.Products.Commands.Admin.DeleteProductAdmin;

public record DeleteProductAdminCommand(
    Guid ProductId
) : IRequest<DeleteProductAdminResponse>;