using System.ComponentModel.DataAnnotations;

namespace ProductManageUNO.Models;

/// <summary>
/// Local order saved to SQLite for order history
/// </summary>
public class LocalOrder
{
    [System.ComponentModel.DataAnnotations.Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Order ID from API
    /// </summary>
    public int ApiOrderId { get; set; }
    
    /// <summary>
    /// Customer info
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerAddress { get; set; } = string.Empty;
    
    /// <summary>
    /// Order details
    /// </summary>
    public decimal TotalAmount { get; set; }
    public int TotalItems { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime OrderDate { get; set; } = DateTime.Now;
    
    /// <summary>
    /// Formatted total amount for display
    /// </summary>
    public string TotalAmountFormatted => TotalAmount.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN")) + "đ";
    
    /// <summary>
    /// Formatted order date for display
    /// </summary>
    public string OrderDateFormatted => OrderDate.ToString("dd/MM/yyyy HH:mm");
    
    /// <summary>
    /// Status display text in Vietnamese
    /// </summary>
    public string StatusDisplay => Status?.ToLower() switch
    {
        "pending" => "Chờ xử lý",
        "confirmed" => "Đã xác nhận",
        "completed" => "Hoàn thành",
        "cancelled" => "Đã hủy",
        _ => Status ?? "N/A"
    };
}
