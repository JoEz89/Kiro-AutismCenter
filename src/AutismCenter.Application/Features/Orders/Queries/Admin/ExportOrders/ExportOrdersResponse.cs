namespace AutismCenter.Application.Features.Orders.Queries.Admin.ExportOrders;

public record ExportOrdersResponse(
    byte[] FileContent,
    string FileName,
    string ContentType,
    int RecordCount
);