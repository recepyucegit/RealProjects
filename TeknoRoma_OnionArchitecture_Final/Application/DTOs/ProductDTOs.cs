using TeknoRoma.Domain.Enums;

namespace TeknoRoma.Application.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Barcode { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public string? Description { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public int Stock { get; set; }
    public int MinStock { get; set; }
    public StockStatus StockStatus { get; set; }
    public string StockStatusDisplay => StockStatus.ToString();
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateProductDto
{
    public string Barcode { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public string? Description { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public int Stock { get; set; }
    public int MinStock { get; set; } = 10;
    public int CategoryId { get; set; }
    public int? SupplierId { get; set; }
}

public class UpdateProductDto
{
    public int Id { get; set; }
    public string Barcode { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public string? Description { get; set; }
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public int Stock { get; set; }
    public int MinStock { get; set; }
    public int CategoryId { get; set; }
    public int? SupplierId { get; set; }
}

public class ProductStockUpdateDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public bool IsAddition { get; set; } = true;
}
