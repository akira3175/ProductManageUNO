using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace ProductManageUNO.Presentation;

public sealed partial class OrderConfirmationPage : Page
{
    public OrderConfirmationPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is OrderConfirmationData data)
        {
            OrderIdText.Text = $"#{data.OrderId}";
            CustomerNameText.Text = data.CustomerName;
            TotalAmountText.Text = data.TotalAmount.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN")) + "đ";
            
            Console.WriteLine($"✅ Order Confirmation: Order #{data.OrderId}");
        }
    }

    private void BackToHomeButton_Click(object sender, RoutedEventArgs e)
    {
        // Navigate back to main page and clear navigation stack
        Frame.Navigate(typeof(MainPage));
        
        // Clear back stack to prevent going back to checkout
        Frame.BackStack.Clear();
    }

    private void ViewOrdersButton_Click(object sender, RoutedEventArgs e)
    {
        // Navigate to order history page
        Frame.Navigate(typeof(OrderHistoryPage));
    }
}
