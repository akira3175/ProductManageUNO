using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace ProductManageUNO.Presentation;

public sealed partial class ProductDetailPage : Page
{
    public ProductDetailModel ViewModel { get; private set; }
    private int _currentProductId;

    public ProductDetailPage()
    {
        this.InitializeComponent();

        // Đăng ký converters
        Resources["NullToVisibilityConverter"] = new NullToVisibilityConverter();
        Resources["StringNotEmptyToVisibilityConverter"] = new StringNotEmptyToVisibilityConverter();
        Resources["BoolToVisibilityConverter"] = new BoolToVisibilityConverter();
        Resources["PriceFormatConverter"] = new PriceFormatConverter();
        Resources["DateFormatConverter"] = new DateFormatConverter();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        Console.WriteLine("🔵 ProductDetailPage OnNavigatedTo");

        if (Application.Current is App app && app.Host != null)
        {
            ViewModel = app.Host.Services.GetService(typeof(ProductDetailModel)) as ProductDetailModel;

            if (ViewModel == null)
            {
                Console.WriteLine("❌ ViewModel is null!");
                return;
            }

            Console.WriteLine("✅ ViewModel initialized");

            if (e.Parameter is int productId)
            {
                _currentProductId = productId;
                Console.WriteLine($"🔵 Loading product ID: {productId}");
                await ViewModel.LoadProductAsync(productId);
            }
            else
            {
                Console.WriteLine($"❌ Invalid parameter type: {e.Parameter?.GetType().Name ?? "null"}");
            }
        }
        else
        {
            Console.WriteLine("❌ App.Host is null!");
        }
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
        if (ViewModel != null && _currentProductId > 0)
        {
            await ViewModel.LoadProductAsync(_currentProductId);
        }
    }
}
