using AutismCenter.Domain.Interfaces;

namespace AutismCenter.Application.Features.Orders.Services;

public class OrderNumberService : IOrderNumberService
{
    private readonly IOrderRepository _orderRepository;

    public OrderNumberService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken = default)
    {
        const int maxAttempts = 10;
        var currentYear = DateTime.UtcNow.Year;
        
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Generate a random 6-digit number
            var random = new Random();
            var sequenceNumber = random.Next(100000, 999999);
            
            var orderNumber = $"ORD-{currentYear}-{sequenceNumber:D6}";
            
            // Check if this order number already exists
            var exists = await _orderRepository.OrderNumberExistsAsync(orderNumber, cancellationToken);
            
            if (!exists)
            {
                return orderNumber;
            }
        }
        
        // If we couldn't generate a unique number after max attempts, use a GUID-based approach
        var guidSuffix = Guid.NewGuid().ToString("N")[..6].ToUpper();
        return $"ORD-{currentYear}-{guidSuffix}";
    }
}