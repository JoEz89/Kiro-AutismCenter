using MediatR;

namespace AutismCenter.Application.Features.Products.Commands.UpdateStock;

public record UpdateStockCommand(
    Guid ProductId,
    int NewQuantity
) : IRequest<UpdateStockResponse>;