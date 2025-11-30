using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using ProductManageUNO.Models;
using System;

namespace ProductManageUNO.Presentation;

public sealed partial class MainPage : Page
{
    private MainModel? _viewModel;

    public MainPage()
    {
        this.InitializeComponent();

        // Đăng ký converter
        Resources["EmptyToVisibilityConverter"] = new EmptyToVisibilityConverter();
        Resources["StringFormatConverter"] = new StringFormatConverter();
        Resources["CountToVisibilityConverter"] = new CountToVisibilityConverter();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // Kết nối ViewModel
        if (Application.Current is App app && app.Host != null)
        {
            _viewModel = app.Host.Services.GetService(typeof(MainModel)) as MainModel;
            DataContext = _viewModel;
        }
    }

    private void ProductInfo_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is Grid grid && grid.Tag is int productId)
        {
            Console.WriteLine($"🔵 Navigating to product ID: {productId}");

            try
            {
                if (Frame != null)
                {
                    bool success = Frame.Navigate(typeof(ProductDetailPage), productId);
                    Console.WriteLine($"🔵 Navigation result: {success}");
                }
                else
                {
                    Console.WriteLine("❌ Frame is null!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Navigation error: {ex.Message}");
            }
        }
    }

    private void CartButton_Click(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("🔵 Navigating to Cart");

        try
        {
            if (Frame != null)
            {
                bool success = Frame.Navigate(typeof(CartPage));
                Console.WriteLine($"🔵 Cart navigation result: {success}");
            }
            else
            {
                Console.WriteLine("❌ Frame is null!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Cart navigation error: {ex.Message}");
        }
    }
}
