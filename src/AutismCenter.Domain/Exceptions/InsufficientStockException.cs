namespace AutismCenter.Domain.Exceptions;

public class InsufficientStockException : DomainException
{
    public Guid ProductId { get; }
    public int RequestedQuantity { get; }
    public int AvailableQuantity { get; }

    public InsufficientStockException(Guid productId, int requestedQuantity, int availableQuantity) 
        : base($"Insufficient stock for product '{productId}'. Requested: {requestedQuantity}, Available: {availableQuantity}")
    {
        ProductId = productId;
        RequestedQuantity = requestedQuantity;
        AvailableQuantity = availableQuantity;
    }
}