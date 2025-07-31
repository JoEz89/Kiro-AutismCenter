namespace AutismCenter.Application.Features.Products.Queries.Admin.ExportProducts;

public record ExportProductsResponse(
    byte[] FileContent,
    string ContentType,
    string FileName
);