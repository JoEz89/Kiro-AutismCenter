using MediatR;

namespace AutismCenter.Application.Features.ContentManagement.Queries.GetContentCategories;

public record GetContentCategoriesQuery() : IRequest<GetContentCategoriesResponse>;