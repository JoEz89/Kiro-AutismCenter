using MediatR;

namespace AutismCenter.Application.Features.Products.Queries.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<GetProductByIdResponse>;