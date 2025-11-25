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
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal Price { get; set; }
    public string Unit { get; set; }
    public int Quantity { get; set; }
}

public class ApiResPagination<T>
{
    public bool Success { get; set; }
    public T Result { get; set; }
    public string Message { get; set; }
}
