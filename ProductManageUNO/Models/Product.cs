using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Data;
using System.ComponentModel.DataAnnotations;

namespace ProductManageUNO.Models;

[Bindable]
public partial class CartItem : ObservableObject
{
    [System.ComponentModel.DataAnnotations.Key]
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public decimal Price { get; set; }
    
    private int _quantity;
    public int Quantity 
    { 
        get => _quantity;
        set
        {
            if (SetProperty(ref _quantity, value))
            {
                OnPropertyChanged(nameof(Subtotal));
            }
        }
    }

    public string Unit { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; } = DateTime.Now;

    // Calculated property
    public decimal Subtotal => Price * Quantity;
}

// DTO cho việc tạo Order
public class CreateOrderRequest
{
    public int CustomerId { get; set; }
    public int UserId { get; set; }
    public int PromotionId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.Now;
    public string Status { get; set; } = "Pending";
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Subtotal { get; set; }
}

public class Product
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public int SupplierId { get; set; }
    public string ProductName { get; set; }
    public string Barcode { get; set; }
    public decimal Price { get; set; }
    public string Unit { get; set; }
    public DateTime CreatedAt { get; set; }
    public Category Category { get; set; }
    public Supplier Supplier { get; set; }
}

public class Category
{
    public int Id { get; set; }
    public string CategoryName { get; set; }
}

public class Supplier
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
}

// Response cho danh sách (pagination)
public class ApiResPagination<T>
{
    public bool Success { get; set; }
    public int Status { get; set; }
    public string Message { get; set; }
    public T Result { get; set; }
    public MetaData Meta { get; set; }
}

// Response cho single item
public class ApiResDetail<T>
{
    public bool Success { get; set; }
    public int Status { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
}

public class MetaData
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPage { get; set; }
    public int TotalItems { get; set; }
}
