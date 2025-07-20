namespace AutismCenter.Application.Features.Products.Commands.UpdateStock;

public record UpdateStockResponse(
    Guid ProductId,
    int OldQuantity,
    int NewQuantity,
    string Message
);