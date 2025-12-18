namespace ProductManageUNO.Models;

/// <summary>
/// API response wrapper for order detail
/// </summary>
public class OrderDetailResponse
{
    public bool Success { get; set; }
    public int Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public OrderDetailData? Data { get; set; }
}

/// <summary>
/// Order detail data from API
/// </summary>
public class OrderDetailData
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = "pending";
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public OrderCustomer? Customer { get; set; }
    public OrderUser? User { get; set; }
    public List<OrderItem> Items { get; set; } = new();

    // Formatted properties for display
    public string TotalAmountFormatted => TotalAmount.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN")) + "đ";
    public string DiscountAmountFormatted => DiscountAmount.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN")) + "đ";
    public string OrderDateFormatted => OrderDate.ToString("dd/MM/yyyy HH:mm");
    
    public string StatusDisplay => Status?.ToLower() switch
    {
        "pending" => "Chờ xử lý",
        "confirmed" => "Đã xác nhận",
        "completed" => "Hoàn thành",
        "cancelled" => "Đã hủy",
        _ => Status ?? "N/A"
    };
}

/// <summary>
/// Customer info in order detail
/// </summary>
public class OrderCustomer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

/// <summary>
/// User info in order detail
/// </summary>
public class OrderUser
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

/// <summary>
/// Order item in order detail
/// </summary>
public class OrderItem
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Subtotal { get; set; }

    // Formatted properties for display
    public string PriceFormatted => Price.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN")) + "đ";
    public string SubtotalFormatted => Subtotal.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN")) + "đ";
}
