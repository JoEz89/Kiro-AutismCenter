namespace AutismCenter.WebApi.Models.Admin;

/// <summary>
/// Request model for getting product analytics
/// </summary>
public class GetProductAnalyticsRequest
{
    /// <summary>
    /// Start date for analytics period
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date for analytics period
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Filter by category ID
    /// </summary>
    public Guid? CategoryId { get; set; }

    /// <summary>
    /// Group analytics by (day, week, month, category)
    /// </summary>
    public string GroupBy { get; set; } = "day";
}

/// <summary>
/// Request model for getting inventory report
/// </summary>
public class GetInventoryReportRequest
{
    /// <summary>
    /// Filter by category ID
    /// </summary>
    public Guid? CategoryId { get; set; }

    /// <summary>
    /// Low stock threshold for filtering
    /// </summary>
    public int? LowStockThreshold { get; set; } = 10;

    /// <summary>
    /// Include out of stock items
    /// </summary>
    public bool IncludeOutOfStock { get; set; } = true;

    /// <summary>
    /// Sort field (name, stock, category)
    /// </summary>
    public string SortBy { get; set; } = "name";
}

/// <summary>
/// Request model for getting categories with admin details
/// </summary>
public class GetCategoriesAdminRequest
{
    /// <summary>
    /// Filter by active status
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Include product count for each category
    /// </summary>
    public bool IncludeProductCount { get; set; } = true;
}

/// <summary>
/// Request model for creating a new product
/// </summary>
public class CreateProductAdminRequest
{
    /// <summary>
    /// Product name in English
    /// </summary>
    public string NameEn { get; set; } = string.Empty;

    /// <summary>
    /// Product name in Arabic
    /// </summary>
    public string NameAr { get; set; } = string.Empty;

    /// <summary>
    /// Product description in English
    /// </summary>
    public string? DescriptionEn { get; set; }

    /// <summary>
    /// Product description in Arabic
    /// </summary>
    public string? DescriptionAr { get; set; }

    /// <summary>
    /// Product price
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Initial stock quantity
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Category ID
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Product image URLs
    /// </summary>
    public List<string> ImageUrls { get; set; } = new();

    /// <summary>
    /// Whether the product is active
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Request model for updating an existing product
/// </summary>
public class UpdateProductAdminRequest
{
    /// <summary>
    /// Product name in English
    /// </summary>
    public string NameEn { get; set; } = string.Empty;

    /// <summary>
    /// Product name in Arabic
    /// </summary>
    public string NameAr { get; set; } = string.Empty;

    /// <summary>
    /// Product description in English
    /// </summary>
    public string? DescriptionEn { get; set; }

    /// <summary>
    /// Product description in Arabic
    /// </summary>
    public string? DescriptionAr { get; set; }

    /// <summary>
    /// Product price
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Stock quantity
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Category ID
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Product image URLs
    /// </summary>
    public List<string> ImageUrls { get; set; } = new();

    /// <summary>
    /// Whether the product is active
    /// </summary>
    public bool IsActive { get; set; }
}

/// <summary>
/// Request model for bulk updating stock quantities
/// </summary>
public class BulkUpdateStockRequest
{
    /// <summary>
    /// List of stock updates
    /// </summary>
    public List<StockUpdateItem> StockUpdates { get; set; } = new();
}

/// <summary>
/// Individual stock update item
/// </summary>
public class StockUpdateItem
{
    /// <summary>
    /// Product ID
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// New stock quantity
    /// </summary>
    public int StockQuantity { get; set; }
}

/// <summary>
/// Request model for creating a new category
/// </summary>
public class CreateCategoryRequest
{
    /// <summary>
    /// Category name in English
    /// </summary>
    public string NameEn { get; set; } = string.Empty;

    /// <summary>
    /// Category name in Arabic
    /// </summary>
    public string NameAr { get; set; } = string.Empty;

    /// <summary>
    /// Category description in English
    /// </summary>
    public string? DescriptionEn { get; set; }

    /// <summary>
    /// Category description in Arabic
    /// </summary>
    public string? DescriptionAr { get; set; }

    /// <summary>
    /// Whether the category is active
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Request model for updating an existing category
/// </summary>
public class UpdateCategoryRequest
{
    /// <summary>
    /// Category name in English
    /// </summary>
    public string NameEn { get; set; } = string.Empty;

    /// <summary>
    /// Category name in Arabic
    /// </summary>
    public string NameAr { get; set; } = string.Empty;

    /// <summary>
    /// Category description in English
    /// </summary>
    public string? DescriptionEn { get; set; }

    /// <summary>
    /// Category description in Arabic
    /// </summary>
    public string? DescriptionAr { get; set; }

    /// <summary>
    /// Whether the category is active
    /// </summary>
    public bool IsActive { get; set; }
}