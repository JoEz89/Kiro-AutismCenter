namespace AutismCenter.Domain.Exceptions;

public class ProductNotFoundException : DomainException
{
    public ProductNotFoundException(Guid productId) 
        : base($"Product with ID '{productId}' was not found.")
    {
    }

    public ProductNotFoundException(string productSku) 
        : base($"Product with SKU '{productSku}' was not found.")
    {
    }
}