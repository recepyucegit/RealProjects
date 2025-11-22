using TeknoRoma.Entities;

namespace TeknoRoma.Business.DTOs;

/// <summary>
/// Order entity için Data Transfer Object
/// Sipariş detayları (OrderDetails) da dahil edilir
/// </summary>
public class OrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string ShippingCity { get; set; } = string.Empty;
    public string? ShippingDistrict { get; set; }
    public string? ShippingPostalCode { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }

    // İlişkili veriler
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }

    /// <summary>
    /// Sipariş detayları (satırlar)
    /// </summary>
    public List<OrderDetailDto> OrderDetails { get; set; } = new();

    /// <summary>
    /// Sipariş durumu Türkçe
    /// </summary>
    public string StatusText
    {
        get
        {
            return Status switch
            {
                OrderStatus.Pending => "Beklemede",
                OrderStatus.Processing => "İşleniyor",
                OrderStatus.Shipped => "Kargoya Verildi",
                OrderStatus.Delivered => "Teslim Edildi",
                OrderStatus.Cancelled => "İptal Edildi",
                _ => "Bilinmiyor"
            };
        }
    }
}

/// <summary>
/// Sipariş detayı DTO
/// </summary>
public class OrderDetailDto
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountRate { get; set; }
    public decimal LineTotal { get; set; }

    // İlişkili veriler
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
}

/// <summary>
/// Yeni sipariş oluşturma için DTO
/// </summary>
public class CreateOrderDto
{
    public PaymentMethod PaymentMethod { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public string ShippingCity { get; set; } = string.Empty;
    public string? ShippingDistrict { get; set; }
    public string? ShippingPostalCode { get; set; }
    public decimal ShippingCost { get; set; } = 0;
    public string? Notes { get; set; }
    public int CustomerId { get; set; }

    /// <summary>
    /// Sipariş satırları
    /// </summary>
    public List<CreateOrderDetailDto> OrderDetails { get; set; } = new();
}

/// <summary>
/// Yeni sipariş detayı oluşturma için DTO
/// </summary>
public class CreateOrderDetailDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal DiscountRate { get; set; } = 0;
}
