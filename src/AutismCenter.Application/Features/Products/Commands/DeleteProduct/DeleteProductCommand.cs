using MediatR;

namespace AutismCenter.Application.Features.Products.Commands.DeleteProduct;

public record DeleteProductCommand(Guid Id) : IRequest<DeleteProductResponse>;