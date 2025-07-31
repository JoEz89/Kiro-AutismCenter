using MediatR;
using AutismCenter.Domain.Interfaces;
using System.Text;

namespace AutismCenter.Application.Features.Products.Queries.Admin.ExportProducts;

public class ExportProductsHandler : IRequestHandler<ExportProductsQuery, ExportProductsResponse>
{
    private readonly IProductRepository _productRepository;

    public ExportProductsHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<ExportProductsResponse> Handle(ExportProductsQuery request, CancellationToken cancellationToken)
    {
        // Get products based on filters
        var products = await _productRepository.GetProductsForExportAsync(
            request.CategoryId,
            request.IsActive,
            request.LowStockOnly,
            cancellationToken);

        // Generate CSV content
        var csvContent = GenerateCsvContent(products);
        var fileContent = Encoding.UTF8.GetBytes(csvContent);

        var fileName = $"products_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        var contentType = "text/csv";

        return new ExportProductsResponse(fileContent, contentType, fileName);
    }

    private string GenerateCsvContent(IEnumerable<Domain.Entities.Product> products)
    {
        var csv = new StringBuilder();
        
        // Add header
        csv.AppendLine("Product ID,Product SKU,Name (EN),Name (AR),Description (EN),Description (AR),Price,Currency,Stock Quantity,Category,Image URLs,Is Active,Created At,Updated At");

        // Add data rows
        foreach (var product in products)
        {
            var imageUrls = product.ImageUrls != null ? string.Join(";", product.ImageUrls) : string.Empty;
            
            csv.AppendLine($"{product.Id}," +
                          $"\"{product.ProductSku}\"," +
                          $"\"{EscapeCsvValue(product.NameEn)}\"," +
                          $"\"{EscapeCsvValue(product.NameAr)}\"," +
                          $"\"{EscapeCsvValue(product.DescriptionEn ?? string.Empty)}\"," +
                          $"\"{EscapeCsvValue(product.DescriptionAr ?? string.Empty)}\"," +
                          $"{product.Price.Amount}," +
                          $"{product.Price.Currency}," +
                          $"{product.StockQuantity}," +
                          $"\"{EscapeCsvValue(product.Category?.NameEn ?? string.Empty)}\"," +
                          $"\"{EscapeCsvValue(imageUrls)}\"," +
                          $"{product.IsActive}," +
                          $"{product.CreatedAt:yyyy-MM-dd HH:mm:ss}," +
                          $"{product.UpdatedAt:yyyy-MM-dd HH:mm:ss}");
        }

        return csv.ToString();
    }

    private string EscapeCsvValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // Escape double quotes by doubling them
        return value.Replace("\"", "\"\"");
    }
}