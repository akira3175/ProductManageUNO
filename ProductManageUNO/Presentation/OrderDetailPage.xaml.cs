using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ProductManageUNO.Models;
using ProductManageUNO.Services;
using System;

namespace ProductManageUNO.Presentation;

public sealed partial class OrderDetailPage : Page
{
    private IOrderHistoryService? _orderHistoryService;
    private int _currentOrderId;
    private OrderDetailData? _orderData;

    public OrderDetailPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        Console.WriteLine("🔵 OrderDetailPage OnNavigatedTo");

        if (Application.Current is App app && app.Host != null)
        {
            _orderHistoryService = app.Host.Services.GetService(typeof(IOrderHistoryService)) as IOrderHistoryService;

            if (_orderHistoryService == null)
            {
                Console.WriteLine("❌ OrderHistoryService is null!");
                ShowError();
                return;
            }

            if (e.Parameter is int orderId)
            {
                _currentOrderId = orderId;
                Console.WriteLine($"🔵 Loading order ID: {orderId}");
                await LoadOrderAsync(orderId);
            }
            else
            {
                Console.WriteLine($"❌ Invalid parameter type: {e.Parameter?.GetType().Name ?? "null"}");
                ShowError();
            }
        }
        else
        {
            Console.WriteLine("❌ App.Host is null!");
            ShowError();
        }
    }

    private async Task LoadOrderAsync(int orderId)
    {
        try
        {
            ShowLoading(true);

            _orderData = await _orderHistoryService!.GetOrderByIdAsync(orderId);

            if (_orderData != null)
            {
                DisplayOrderData(_orderData);
                ShowContent();
            }
            else
            {
                ShowError();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ LoadOrderAsync Error: {ex.Message}");
            ShowError();
        }
        finally
        {
            ShowLoading(false);
        }
    }

    private void DisplayOrderData(OrderDetailData order)
    {
        // Order summary
        OrderIdText.Text = $"Đơn #{order.Id}";
        StatusText.Text = order.StatusDisplay;
        OrderDateText.Text = order.OrderDateFormatted;
        UserText.Text = $"Nhân viên: {order.User?.FullName ?? "N/A"}";

        // Customer info
        CustomerNameText.Text = order.Customer?.Name ?? "N/A";
        CustomerPhoneText.Text = order.Customer?.Phone ?? "N/A";
        CustomerEmailText.Text = order.Customer?.Email ?? "N/A";
        CustomerAddressText.Text = order.Customer?.Address ?? "N/A";

        // Order items
        OrderItemsList.ItemsSource = order.Items;

        // Total and discount
        TotalText.Text = order.TotalAmountFormatted;
        
        if (order.DiscountAmount > 0)
        {
            DiscountRow.Visibility = Visibility.Visible;
            DiscountDivider.Visibility = Visibility.Visible;
            DiscountText.Text = $"-{order.DiscountAmountFormatted}";
        }
        else
        {
            DiscountRow.Visibility = Visibility.Collapsed;
            DiscountDivider.Visibility = Visibility.Collapsed;
        }

        Console.WriteLine($"✅ Displayed order #{order.Id} with {order.Items?.Count ?? 0} items");
    }

    private void ShowLoading(bool isLoading)
    {
        LoadingState.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
        HeaderProgressRing.IsActive = isLoading;
        
        if (isLoading)
        {
            ContentScrollViewer.Visibility = Visibility.Collapsed;
            ErrorState.Visibility = Visibility.Collapsed;
        }
    }

    private void ShowContent()
    {
        LoadingState.Visibility = Visibility.Collapsed;
        ErrorState.Visibility = Visibility.Collapsed;
        ContentScrollViewer.Visibility = Visibility.Visible;
    }

    private void ShowError()
    {
        LoadingState.Visibility = Visibility.Collapsed;
        ContentScrollViewer.Visibility = Visibility.Collapsed;
        ErrorState.Visibility = Visibility.Visible;
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }

    private async void RetryButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentOrderId > 0)
        {
            await LoadOrderAsync(_currentOrderId);
        }
    }
}
