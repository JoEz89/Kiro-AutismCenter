using AutismCenter.Domain.Common;

namespace AutismCenter.Domain.Entities;

public class Category : BaseEntity
{
    public string NameEn { get; private set; }
    public string NameAr { get; private set; }
    public string? DescriptionEn { get; private set; }
    public string? DescriptionAr { get; private set; }
    public bool IsActive { get; private set; }

    // Navigation properties
    private readonly List<Product> _products = new();
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    private Category() { } // For EF Core

    private Category(string nameEn, string nameAr, string? descriptionEn = null, string? descriptionAr = null)
    {
        NameEn = nameEn;
        NameAr = nameAr;
        DescriptionEn = descriptionEn;
        DescriptionAr = descriptionAr;
        IsActive = true;
    }

    public static Category Create(string nameEn, string nameAr, string? descriptionEn = null, string? descriptionAr = null)
    {
        if (string.IsNullOrWhiteSpace(nameEn))
            throw new ArgumentException("English name cannot be empty", nameof(nameEn));

        if (string.IsNullOrWhiteSpace(nameAr))
            throw new ArgumentException("Arabic name cannot be empty", nameof(nameAr));

        return new Category(nameEn.Trim(), nameAr.Trim(), descriptionEn?.Trim(), descriptionAr?.Trim());
    }

    public void UpdateDetails(string nameEn, string nameAr, string? descriptionEn = null, string? descriptionAr = null)
    {
        if (string.IsNullOrWhiteSpace(nameEn))
            throw new ArgumentException("English name cannot be empty", nameof(nameEn));

        if (string.IsNullOrWhiteSpace(nameAr))
            throw new ArgumentException("Arabic name cannot be empty", nameof(nameAr));

        NameEn = nameEn.Trim();
        NameAr = nameAr.Trim();
        DescriptionEn = descriptionEn?.Trim();
        DescriptionAr = descriptionAr?.Trim();
        UpdateTimestamp();
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdateTimestamp();
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdateTimestamp();
    }

    public string GetName(bool isArabic) => isArabic ? NameAr : NameEn;

    public string GetDescription(bool isArabic) => isArabic ? DescriptionAr : DescriptionEn;
}