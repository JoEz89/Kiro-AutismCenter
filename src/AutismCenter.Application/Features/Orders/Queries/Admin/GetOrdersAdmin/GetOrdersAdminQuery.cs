using MediatR;
using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Features.Orders.Queries.Admin.GetOrdersAdmin;

public record GetOrdersAdminQuery(
    int PageNumber = 1,
    int PageSize = 20,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    OrderStatus? Status = null,
    PaymentStatus? PaymentStatus = null,
    string? SearchTerm = null, // Search by order number or customer name/email
    string? SortBy = "CreatedAt",
    bool SortDescending = true
) : IRequest<GetOrdersAdminResponse>;