using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using ProductManageUNO.Models;
using System;
using Microsoft.Extensions.DependencyInjection; // ✅ THÊM DÒNG NÀY

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

        Console.WriteLine("🔵 MainPage Constructor");
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        Console.WriteLine("🔵 OnNavigatedTo called");

        // ✅ SỬA LẠI CÁCH LẤY SERVICE
        try
        {
            if (Application.Current is App app && app.Host != null)
            {
                Console.WriteLine("🔵 App.Host found");
                _viewModel = app.Host.Services.GetService<MainModel>();

                if (_viewModel != null)
                {
                    DataContext = _viewModel;
                    Console.WriteLine("✅ ViewModel set successfully");
                }
                else
                {
                    Console.WriteLine("❌ Failed to get MainModel from services");
                }
            }
            else
            {
                Console.WriteLine("❌ App.Host is null");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ OnNavigatedTo Error: {ex.Message}");
        }
    }

    private async void AddToCartButton_Click(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("🔵 AddToCartButton_Click - FIRED!");
        Console.WriteLine($"🔵 ViewModel status: {_viewModel != null}");

        if (sender is Button button)
        {
            Console.WriteLine($"🔵 Button found, checking CommandParameter...");

            var product = button.CommandParameter as Product;

            if (product != null)
            {
                Console.WriteLine($"🔵 Product found: {product.ProductName}");

                if (_viewModel != null)
                {
                    Console.WriteLine("🔵 ViewModel is available, calling AddToCartCommand");
                    try
                    {
                        await _viewModel.AddToCartCommand.ExecuteAsync(product);
                        Console.WriteLine("✅ AddToCartCommand executed");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ AddToCartCommand error: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("❌ ViewModel is NULL!");

                    // ✅ FALLBACK: Thử lấy lại ViewModel
                    if (Application.Current is App app && app.Host != null)
                    {
                        _viewModel = app.Host.Services.GetService<MainModel>();
                        if (_viewModel != null)
                        {
                            DataContext = _viewModel;
                            await _viewModel.AddToCartCommand.ExecuteAsync(product);
                            Console.WriteLine("✅ ViewModel recovered and command executed");
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("❌ CommandParameter is not a Product!");
            }
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

    private void OrdersButton_Click(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("🔵 Navigating to Orders");

        try
        {
            if (Frame != null)
            {
                bool success = Frame.Navigate(typeof(OrderHistoryPage));
                Console.WriteLine($"🔵 Orders navigation result: {success}");
            }
            else
            {
                Console.WriteLine("❌ Frame is null!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Orders navigation error: {ex.Message}");
        }
    }
    private ScrollViewer? _scrollViewer;

    private void ProductListView_Loaded(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("🔵 ProductListView_Loaded fired");
        
        // Delay to ensure visual tree is ready
        DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
        {
            if (sender is ListView listView)
            {
                _scrollViewer = FindChildOfType<ScrollViewer>(listView);
                if (_scrollViewer != null)
                {
                    _scrollViewer.ViewChanged += ScrollViewer_ViewChanged;
                    Console.WriteLine("✅ ScrollViewer hooked for infinite scroll");
                }
                else
                {
                    Console.WriteLine("❌ ScrollViewer NOT found in ListView");
                }
            }
        });
    }

    private void ScrollViewer_ViewChanged(object? sender, ScrollViewerViewChangedEventArgs e)
    {
        if (_scrollViewer != null && _viewModel != null)
        {
            var verticalOffset = _scrollViewer.VerticalOffset;
            var maxOffset = _scrollViewer.ScrollableHeight;
            var extentHeight = _scrollViewer.ExtentHeight;
            var viewportHeight = _scrollViewer.ViewportHeight;

            Console.WriteLine($"📜 Scroll: offset={verticalOffset:F0}, max={maxOffset:F0}, extent={extentHeight:F0}, viewport={viewportHeight:F0}");

            // Check if near bottom (within 200 pixels)
            if (maxOffset > 0 && verticalOffset >= maxOffset - 200)
            {
                Console.WriteLine("📄 Near bottom, loading more...");
                _viewModel.LoadMoreCommand.Execute(null);
            }
        }
    }

    private static T? FindChildOfType<T>(DependencyObject parent) where T : DependencyObject
    {
        if (parent == null) return default;

        int count = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(parent, i);
            if (child is T result) return result;

            var found = FindChildOfType<T>(child);
            if (found != null) return found;
        }
        return default;
    }
}
