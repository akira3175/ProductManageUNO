using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ProductManageUNO.Models;
using ProductManageUNO.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProductManageUNO.Presentation;

public sealed partial class CheckoutPage : Page
{
    private CheckoutModel? _viewModel;
    private ICartService? _cartService;
    private ICustomerService? _customerService;
    private IOrderService? _orderService;
    private IOrderHistoryService? _orderHistoryService;
    private List<CartItem> _cartItems = new();
    private decimal _totalAmount;

    public CheckoutPage()
    {
        this.InitializeComponent();
        Resources["BoolToVisibilityConverter"] = new BoolToVisibilityConverter();
        Resources["PriceFormatConverter"] = new PriceFormatConverter();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (Application.Current is App app && app.Host != null)
        {
            // Get CheckoutModel and set as DataContext
            _viewModel = app.Host.Services.GetService(typeof(CheckoutModel)) as CheckoutModel;
            DataContext = _viewModel;

            // Get services directly
            _cartService = app.Host.Services.GetService(typeof(ICartService)) as ICartService;
            _customerService = app.Host.Services.GetService(typeof(ICustomerService)) as ICustomerService;
            _orderService = app.Host.Services.GetService(typeof(IOrderService)) as IOrderService;
            _orderHistoryService = app.Host.Services.GetService(typeof(IOrderHistoryService)) as IOrderHistoryService;

            // Load data via ViewModel for bindings to work
            if (_viewModel != null)
            {
                await _viewModel.LoadDataCommand.ExecuteAsync(null);
                
                // Also update local variables for PlaceOrderButton_Click
                _cartItems = _viewModel.CartItems.ToList();
                _totalAmount = _viewModel.TotalAmount;
                
                // Set OrderItemsList explicitly (in case binding issues)
                OrderItemsList.ItemsSource = _viewModel.CartItems;
                
                // ✅ DIRECT UI UPDATE - bypass binding issues on UNO Platform
                TotalAmountText.Text = _viewModel.TotalAmountFormatted;
                
                Console.WriteLine($"✅ CheckoutPage: DataContext set, TotalAmount={_viewModel.TotalAmount}, Formatted={_viewModel.TotalAmountFormatted}");
            }
            else
            {
                // Fallback to direct loading if ViewModel not available
                await LoadDataAsync();
            }
        }
    }

    private async Task LoadDataAsync()
    {
        try
        {
            Console.WriteLine("🔵 CheckoutPage: Loading data...");

            if (_cartService == null) return;

            // Load cart items
            _cartItems = await _cartService.GetAllAsync();
            _totalAmount = await _cartService.GetTotalAmountAsync();

            // Update UI directly
            OrderItemsList.ItemsSource = _cartItems;
            TotalAmountText.Text = _totalAmount.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN")) + "đ";

            Console.WriteLine($"✅ CheckoutPage: Loaded {_cartItems.Count} items, Total: {_totalAmount}");

            // Load last customer for auto-fill
            if (_customerService != null)
            {
                var lastCustomer = await _customerService.GetLastCustomerAsync();
                if (lastCustomer != null)
                {
                    NameTextBox.Text = lastCustomer.Name;
                    PhoneTextBox.Text = lastCustomer.Phone;
                    EmailTextBox.Text = lastCustomer.Email;
                    AddressTextBox.Text = lastCustomer.Address;
                    Console.WriteLine($"✅ Auto-filled customer: {lastCustomer.Name}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ CheckoutPage LoadDataAsync Error: {ex.Message}");
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }

    private async void PlaceOrderButton_Click(object sender, RoutedEventArgs e)
    {
        // Validate
        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            await ShowErrorAsync("Vui lòng nhập tên khách hàng");
            return;
        }

        if (string.IsNullOrWhiteSpace(PhoneTextBox.Text))
        {
            await ShowErrorAsync("Vui lòng nhập số điện thoại");
            return;
        }

        if (_cartItems.Count == 0)
        {
            await ShowErrorAsync("Giỏ hàng trống");
            return;
        }

        if (_orderService == null || _cartService == null)
        {
            await ShowErrorAsync("Lỗi hệ thống");
            return;
        }

        // Show loading
        LoadingOverlay.Visibility = Visibility.Visible;
        PlaceOrderButton.IsEnabled = false;

        try
        {
            Console.WriteLine("🔵 Placing order...");

            // 1. Save customer locally
            if (_customerService != null)
            {
                var localCustomer = new Customer
                {
                    Name = NameTextBox.Text,
                    Phone = PhoneTextBox.Text,
                    Email = EmailTextBox.Text,
                    Address = AddressTextBox.Text
                };
                await _customerService.SaveCustomerAsync(localCustomer);
            }

            // 2. Create customer via API
            var customerRequest = new CreateCustomerRequest
            {
                Name = NameTextBox.Text,
                Phone = PhoneTextBox.Text,
                Email = EmailTextBox.Text,
                Address = AddressTextBox.Text
            };

            var customerResult = await _orderService.CreateCustomerAsync(customerRequest);
            if (customerResult == null)
            {
                await ShowErrorAsync("Không thể tạo khách hàng. Vui lòng thử lại.");
                return;
            }

            Console.WriteLine($"✅ Customer created: ID={customerResult.Id}");

            // 3. Create order via API
            var orderRequest = new CreateOrderRequest
            {
                CustomerId = customerResult.Id,
                UserId = 1,
                PromotionId = 1,
                OrderDate = DateTime.Now,
                Status = "pending",
                TotalAmount = _totalAmount,
                DiscountAmount = 0,
                Items = _cartItems.Select(item => new OrderItemDto
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
                await ShowErrorAsync("Không thể tạo đơn hàng. Vui lòng thử lại.");
                return;
            }

            Console.WriteLine($"✅ Order created: ID={orderResult.Id}");

            // 4. Save order to local SQLite
            if (_orderHistoryService != null)
            {
                var localOrder = new LocalOrder
                {
                    ApiOrderId = orderResult.Id,
                    CustomerName = NameTextBox.Text,
                    CustomerPhone = PhoneTextBox.Text,
                    CustomerEmail = EmailTextBox.Text,
                    CustomerAddress = AddressTextBox.Text,
                    TotalAmount = _totalAmount,
                    TotalItems = _cartItems.Sum(x => x.Quantity),
                    Status = "pending",
                    OrderDate = DateTime.Now
                };
                await _orderHistoryService.SaveOrderAsync(localOrder);
            }

            // 5. Clear cart
            await _cartService.ClearCartAsync();

            // 6. Navigate to confirmation
            Frame.Navigate(typeof(OrderConfirmationPage), new OrderConfirmationData
            {
                OrderId = orderResult.Id,
                CustomerName = NameTextBox.Text,
                TotalAmount = _totalAmount
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ PlaceOrder Error: {ex.Message}");
            await ShowErrorAsync($"Lỗi: {ex.Message}");
        }
        finally
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
            PlaceOrderButton.IsEnabled = true;
        }
    }

    private async Task ShowErrorAsync(string message)
    {
        var dialog = new ContentDialog
        {
            Title = "Lỗi",
            Content = message,
            CloseButtonText = "Đóng",
            XamlRoot = this.XamlRoot
        };
        await dialog.ShowAsync();
    }
}

public class OrderConfirmationData
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}

