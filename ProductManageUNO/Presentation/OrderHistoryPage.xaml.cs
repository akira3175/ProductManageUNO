using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ProductManageUNO.Services;
using System;

namespace ProductManageUNO.Presentation;

public sealed partial class OrderHistoryPage : Page
{
    private IOrderHistoryService? _orderHistoryService;

    public OrderHistoryPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (Application.Current is App app && app.Host != null)
        {
            _orderHistoryService = app.Host.Services.GetService(typeof(IOrderHistoryService)) as IOrderHistoryService;
            await LoadOrdersAsync();
        }
    }

    private async Task LoadOrdersAsync()
    {
        try
        {
            Console.WriteLine("🔵 OrderHistoryPage: Loading orders...");

            if (_orderHistoryService == null) return;

            var orders = await _orderHistoryService.GetAllOrdersAsync();

            if (orders.Count == 0)
            {
                EmptyState.Visibility = Visibility.Visible;
                OrderListScrollViewer.Visibility = Visibility.Collapsed;
            }
            else
            {
                EmptyState.Visibility = Visibility.Collapsed;
                OrderListScrollViewer.Visibility = Visibility.Visible;
                OrdersList.ItemsSource = orders;
            }

            Console.WriteLine($"✅ OrderHistoryPage: Loaded {orders.Count} orders");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ OrderHistoryPage LoadOrdersAsync Error: {ex.Message}");
        }
    }

    private void OrderItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int orderId)
        {
            Console.WriteLine($"🔵 Navigating to order detail: {orderId}");
            Frame.Navigate(typeof(OrderDetailPage), orderId);
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }
}
