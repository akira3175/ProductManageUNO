using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace ProductManageUNO.Presentation;

public sealed partial class CartPage : Page
{
    private CartModel? _viewModel;

    public CartPage()
    {
        this.InitializeComponent();

        // Đăng ký converters
        Resources["StringFormatConverter"] = new StringFormatConverter();
        Resources["BoolToVisibilityConverter"] = new BoolToVisibilityConverter();
        Resources["InverseBoolToVisibilityConverter"] = new InverseBoolToVisibilityConverter();
        Resources["InverseBoolConverter"] = new InverseBoolConverter();
        Resources["PriceFormatConverter"] = new PriceFormatConverter();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (Application.Current is App app && app.Host != null)
        {
            _viewModel = app.Host.Services.GetService(typeof(CartModel)) as CartModel;
            DataContext = _viewModel;

            if (_viewModel != null)
            {
                await _viewModel.LoadCartCommand.ExecuteAsync(null);

                // ✅ DEBUG: In ra trạng thái sau khi load
                Console.WriteLine($"📊 UI Debug: CartItems.Count = {_viewModel.CartItems.Count}");
                Console.WriteLine($"📊 UI Debug: IsEmpty = {_viewModel.IsEmpty}");
                Console.WriteLine($"📊 UI Debug: TotalItems = {_viewModel.TotalItems}");
            }
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
