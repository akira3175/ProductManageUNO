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

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (Application.Current is App app && app.Host != null)
        {
            _viewModel = app.Host.Services.GetService(typeof(CartModel)) as CartModel;
            DataContext = _viewModel;
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
