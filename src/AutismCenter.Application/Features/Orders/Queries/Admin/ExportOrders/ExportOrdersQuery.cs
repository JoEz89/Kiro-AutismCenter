using MediatR;
using AutismCenter.Domain.Enums;

namespace AutismCenter.Application.Features.Orders.Queries.Admin.ExportOrders;

public record ExportOrdersQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    OrderStatus? Status = null,
    string Format = "CSV" // CSV, Excel
) : IRequest<ExportOrdersResponse>;