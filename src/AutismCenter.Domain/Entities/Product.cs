using AutismCenter.Domain.Common;
using AutismCenter.Domain.ValueObjects;
using AutismCenter.Domain.Events;

namespace AutismCenter.Domain.Entities;

public class Product : BaseEntity
{
    public string NameEn { get; private set; }
    public string NameAr { get; private set; }
    public string DescriptionEn { get; private set; }
    public string DescriptionAr { get; private set; }
    public Money Price { get; private set; }
    public int StockQuantity { get; private set; }
    public Guid CategoryId { get; private set; }
    public bool IsActive { get; private set; }
    public string ProductSku { get; private set; }

    private readonly List<string> _imageUrls = new();
    public IReadOnlyCollection<string> ImageUrls => _imageUrls.AsReadOnly();

    // Navigation properties
    public Category Category { get; private set; } = null!;

    private readonly List<OrderItem> _orderItems = new();
    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

    private Product() { } // For EF Core

    private Product(string nameEn, string nameAr, string descriptionEn, string descriptionAr, 
                   Money price, int stockQuantity, Guid categoryId, string productSku)
    {
        NameEn = nameEn;
        NameAr = nameAr;
        DescriptionEn = descriptionEn;
        DescriptionAr = descriptionAr;
        Price = price;
        StockQuantity = stockQuantity;
        CategoryId = categoryId;
        ProductSku = productSku;
        IsActive = true;
    }

    public static Product Create(string nameEn, string nameAr, string descriptionEn, string descriptionAr,
                                Money price, int stockQuantity, Guid categoryId, string productSku)
    {
        if (string.IsNullOrWhiteSpace(nameEn))
            throw new ArgumentException("English name cannot be empty", nameof(nameEn));

        if (string.IsNullOrWhiteSpace(nameAr))
            throw new ArgumentException("Arabic name cannot be empty", nameof(nameAr));

        if (string.IsNullOrWhiteSpace(descriptionEn))
            throw new ArgumentException("English description cannot be empty", nameof(descriptionEn));

        if (string.IsNullOrWhiteSpace(descriptionAr))
            throw new ArgumentException("Arabic description cannot be empty", nameof(descriptionAr));

        if (stockQuantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative", nameof(stockQuantity));

        if (string.IsNullOrWhiteSpace(productSku))
            throw new ArgumentException("Product SKU cannot be empty", nameof(productSku));

        var product = new Product(nameEn.Trim(), nameAr.Trim(), descriptionEn.Trim(), descriptionAr.Trim(),
                                 price, stockQuantity, categoryId, productSku.Trim());

        product.AddDomainEvent(new ProductCreatedEvent(product.Id, product.NameEn));

        return product;
    }

    public void UpdateDetails(string nameEn, string nameAr, string descriptionEn, string descriptionAr, Money price)
    {
        if (string.IsNullOrWhiteSpace(nameEn))
            throw new ArgumentException("English name cannot be empty", nameof(nameEn));

        if (string.IsNullOrWhiteSpace(nameAr))
            throw new ArgumentException("Arabic name cannot be empty", nameof(nameAr));

        if (string.IsNullOrWhiteSpace(descriptionEn))
            throw new ArgumentException("English description cannot be empty", nameof(descriptionEn));

        if (string.IsNullOrWhiteSpace(descriptionAr))
            throw new ArgumentException("Arabic description cannot be empty", nameof(descriptionAr));

        NameEn = nameEn.Trim();
        NameAr = nameAr.Trim();
        DescriptionEn = descriptionEn.Trim();
        DescriptionAr = descriptionAr.Trim();
        Price = price;
        UpdateTimestamp();

        AddDomainEvent(new ProductUpdatedEvent(Id));
    }

    public void UpdateStock(int newQuantity)
    {
        if (newQuantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative", nameof(newQuantity));

        var oldQuantity = StockQuantity;
        StockQuantity = newQuantity;
        UpdateTimestamp();

        AddDomainEvent(new ProductStockUpdatedEvent(Id, oldQuantity, newQuantity));
    }

    public void ReduceStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        if (StockQuantity < quantity)
            throw new InvalidOperationException($"Insufficient stock. Available: {StockQuantity}, Requested: {quantity}");

        StockQuantity -= quantity;
        UpdateTimestamp();

        AddDomainEvent(new ProductStockReducedEvent(Id, quantity, StockQuantity));
    }

    public void RestoreStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        StockQuantity += quantity;
        UpdateTimestamp();

        AddDomainEvent(new ProductStockRestoredEvent(Id, quantity, StockQuantity));
    }

    public void AddImage(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("Image URL cannot be empty", nameof(imageUrl));

        if (!_imageUrls.Contains(imageUrl))
        {
            _imageUrls.Add(imageUrl);
            UpdateTimestamp();
        }
    }

    public void RemoveImage(string imageUrl)
    {
        if (_imageUrls.Remove(imageUrl))
        {
            UpdateTimestamp();
        }
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdateTimestamp();

        AddDomainEvent(new ProductActivatedEvent(Id));
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdateTimestamp();

        AddDomainEvent(new ProductDeactivatedEvent(Id));
    }

    public bool IsInStock() => StockQuantity > 0;

    public bool HasSufficientStock(int requestedQuantity) => StockQuantity >= requestedQuantity;

    public string GetName(bool isArabic) => isArabic ? NameAr : NameEn;

    public string GetDescription(bool isArabic) => isArabic ? DescriptionAr : DescriptionEn;
}