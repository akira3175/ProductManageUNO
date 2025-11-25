namespace ProductManageUNO.Models;

public class CartItem
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
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

public class ApiResPagination<T>
{
    public bool Success { get; set; }
    public int Status { get; set; }
    public string Message { get; set; }
    public T Result { get; set; }
    public MetaData Meta { get; set; }
}

public class MetaData
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPage { get; set; }
    public int TotalItems { get; set; }
}
