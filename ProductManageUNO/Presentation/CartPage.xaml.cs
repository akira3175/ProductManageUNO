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

    private async void RemoveItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is ProductManageUNO.Models.CartItem cartItem && _viewModel != null)
        {
            await _viewModel.RemoveItemCommand.ExecuteAsync(cartItem);
            ForceRefreshItemsSource();
        }
    }

    private async void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is ProductManageUNO.Models.CartItem cartItem && _viewModel != null)
        {
            await _viewModel.DecreaseQuantityCommand.ExecuteAsync(cartItem);
            ForceRefreshItemsSource();
        }
    }

    private async void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is ProductManageUNO.Models.CartItem cartItem && _viewModel != null)
        {
            await _viewModel.IncreaseQuantityCommand.ExecuteAsync(cartItem);
            ForceRefreshItemsSource();
        }
    }

    private async void DecreaseQuantity_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        if (sender is FrameworkElement element && element.Tag is ProductManageUNO.Models.CartItem cartItem && _viewModel != null)
        {
            await _viewModel.DecreaseQuantityCommand.ExecuteAsync(cartItem);
            ForceRefreshItemsSource();
        }
    }

    private async void IncreaseQuantity_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        if (sender is FrameworkElement element && element.Tag is ProductManageUNO.Models.CartItem cartItem && _viewModel != null)
        {
            await _viewModel.IncreaseQuantityCommand.ExecuteAsync(cartItem);
            ForceRefreshItemsSource();
        }
    }

    /// <summary>
    /// Force the ItemsControl to rebind by setting ItemsSource to null, then back to the collection.
    /// Also directly updates TotalAmount display. This is a workaround for UNO Platform binding issues.
    /// </summary>
    private async void ForceRefreshItemsSource()
    {
        if (_viewModel != null)
        {
            // Refresh items list
            CartItemsList.ItemsSource = null;
            CartItemsList.ItemsSource = _viewModel.CartItems;
            
            // Refresh totals from database
            await _viewModel.RefreshTotalsAsync();
            
            // DIRECTLY UPDATE UI - bypass all binding
            TotalAmountText.Text = _viewModel.TotalAmountFormatted;
            
            Console.WriteLine($"🔄 Force refreshed. Count: {_viewModel.CartItems.Count}, Total: {_viewModel.TotalAmountFormatted}");
        }
    }

    private void CheckoutButton_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel != null && _viewModel.CartItems.Count > 0)
        {
            Frame.Navigate(typeof(CheckoutPage));
        }
    }
}
