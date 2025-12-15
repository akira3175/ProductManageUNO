using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using ProductManageUNO.Models;
using ProductManageUNO.Services;

namespace ProductManageUNO.Presentation;

[Bindable]
public partial class CheckoutModel : ObservableObject
{
    private readonly ICartService _cartService;
    private readonly ICustomerService _customerService;
    private readonly IOrderService _orderService;

    [ObservableProperty]
    private string _title = "Thanh toán";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _customerName = string.Empty;

    [ObservableProperty]
    private string _customerPhone = string.Empty;

    [ObservableProperty]
    private string _customerEmail = string.Empty;

    [ObservableProperty]
    private string _customerAddress = string.Empty;

    [ObservableProperty]
    private decimal _totalAmount = 0;

    [ObservableProperty]
    private int _totalItems = 0;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    public string TotalAmountFormatted => TotalAmount.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN")) + "đ";

    partial void OnTotalAmountChanged(decimal value)
    {
        OnPropertyChanged(nameof(TotalAmountFormatted));
    }

    public ObservableCollection<CartItem> CartItems { get; } = new();

    // Store the created order ID for navigation
    public int? CreatedOrderId { get; private set; }
    public int? CreatedCustomerId { get; private set; }

    public CheckoutModel(ICartService cartService, ICustomerService customerService, IOrderService orderService)
    {
        _cartService = cartService;
        _customerService = customerService;
        _orderService = orderService;
    }

    [RelayCommand]
    public async Task LoadData()
    {
        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            // Load cart items
            var items = await _cartService.GetAllAsync();
            CartItems.Clear();
            foreach (var item in items)
            {
                CartItems.Add(item);
            }

            // Calculate totals
            TotalItems = await _cartService.GetTotalItemsAsync();
            TotalAmount = await _cartService.GetTotalAmountAsync();

            // Load last customer info for auto-fill
            var lastCustomer = await _customerService.GetLastCustomerAsync();
            if (lastCustomer != null)
            {
                CustomerName = lastCustomer.Name;
                CustomerPhone = lastCustomer.Phone;
                CustomerEmail = lastCustomer.Email;
                CustomerAddress = lastCustomer.Address;
            }

            Console.WriteLine($"✅ Checkout loaded: {CartItems.Count} items, Total: {TotalAmount}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ LoadData Error: {ex.Message}");
            HasError = true;
            ErrorMessage = "Không thể tải dữ liệu";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task PlaceOrder()
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(CustomerName))
        {
            HasError = true;
            ErrorMessage = "Vui lòng nhập tên khách hàng";
            return;
        }

        if (string.IsNullOrWhiteSpace(CustomerPhone))
        {
            HasError = true;
            ErrorMessage = "Vui lòng nhập số điện thoại";
            return;
        }

        if (CartItems.Count == 0)
        {
            HasError = true;
            ErrorMessage = "Giỏ hàng trống";
            return;
        }

        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            // 1. Save customer locally
            var localCustomer = new Customer
            {
                Name = CustomerName,
                Phone = CustomerPhone,
                Email = CustomerEmail,
                Address = CustomerAddress
            };
            await _customerService.SaveCustomerAsync(localCustomer);

            // 2. Create customer via API
            var customerRequest = new CreateCustomerRequest
            {
                Name = CustomerName,
                Phone = CustomerPhone,
                Email = CustomerEmail,
                Address = CustomerAddress
            };

            var customerResult = await _orderService.CreateCustomerAsync(customerRequest);
            if (customerResult == null)
            {
                HasError = true;
                ErrorMessage = "Không thể tạo khách hàng. Vui lòng thử lại.";
                return;
            }

            CreatedCustomerId = customerResult.Id;

            // 3. Create order via API using customer ID from response
            var orderRequest = new CreateOrderRequest
            {
                CustomerId = customerResult.Id, // Use ID from API response
                //UserId = 1, // Default user ID
                //PromotionId = 1, // Default promotion ID
                OrderDate = DateTime.Now,
                Status = "pending",
                TotalAmount = TotalAmount,
                DiscountAmount = 0,
                Items = CartItems.Select(item => new OrderItemDto
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Subtotal = item.Subtotal
                }).ToList()
            };

            var orderResult = await _orderService.CreateOrderAsync(orderRequest);
            if (orderResult == null)
            {
                HasError = true;
                ErrorMessage = "Không thể tạo đơn hàng. Vui lòng thử lại.";
                return;
            }

            CreatedOrderId = orderResult.Id;

            // 4. Clear cart after successful order
            await _cartService.ClearCartAsync();

            Console.WriteLine($"✅ Order placed successfully! Order ID: {CreatedOrderId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ PlaceOrder Error: {ex.Message}");
            HasError = true;
            ErrorMessage = $"Lỗi: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public bool IsOrderPlaced => CreatedOrderId.HasValue;
}
