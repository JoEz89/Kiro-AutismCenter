namespace AutismCenter.Application.Features.Orders.Services;

public interface IOrderNumberService
{
    Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken = default);
}