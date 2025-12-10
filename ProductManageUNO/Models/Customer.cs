using System.ComponentModel.DataAnnotations;

namespace ProductManageUNO.Models;

/// <summary>
/// Customer entity for local SQLite storage
/// </summary>
public class Customer
{
    [System.ComponentModel.DataAnnotations.Key]
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// Request model for creating customer via API
/// </summary>
public class CreateCustomerRequest
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

/// <summary>
/// Response model from Customer API
/// </summary>
public class CustomerApiResponse
{
    public bool Success { get; set; }
    public int Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public CustomerData? Data { get; set; }
}

public class CustomerData
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}

/// <summary>
/// Response model from Order API
/// </summary>
public class OrderApiResponse
{
    public bool Success { get; set; }
    public int Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public OrderData? Data { get; set; }
}

public class OrderData
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int UserId { get; set; }
    public int PromotionId { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
}
