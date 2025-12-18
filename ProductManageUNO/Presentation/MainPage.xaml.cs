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
    private string? _pendingSearchText;
    private CancellationTokenSource? _searchDebounceToken;

    public MainPage()
    {
        this.InitializeComponent();

        // Đăng ký converter
        Resources["EmptyToVisibilityConverter"] = new EmptyToVisibilityConverter();
        Resources["StringFormatConverter"] = new StringFormatConverter();
        Resources["CountToVisibilityConverter"] = new CountToVisibilityConverter();

        Console.WriteLine("🔵 MainPage Constructor");
        
        // Initialize ViewModel in Loaded event
        this.Loaded += MainPage_Loaded;
    }
    
    private async void MainPage_Loaded(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("🔵 MainPage_Loaded fired");
        
        // Only initialize once
        if (_viewModel != null) return;
        
        try
        {
            if (Application.Current is App app && app.Host != null)
            {
                Console.WriteLine("🔵 App.Host found in Loaded");
                _viewModel = app.Host.Services.GetService<MainModel>();

                if (_viewModel != null)
                {
                    DataContext = _viewModel;
                    Console.WriteLine($"✅ ViewModel set in Loaded, CartItemCount: {_viewModel.CartItemCount}");
                    
                    // Subscribe for cart badge updates
                    _viewModel.PropertyChanged += ViewModel_PropertyChanged;
                    
                    // Refresh cart count
                    await _viewModel.RefreshCartCountAsync();
                    UpdateCartBadge();
                    
                    // Apply pending search if any
                    if (!string.IsNullOrEmpty(_pendingSearchText))
                    {
                        Console.WriteLine($"🔍 Applying pending search in Loaded: '{_pendingSearchText}'");
                        _viewModel.SearchText = _pendingSearchText;
                        _viewModel.SearchCommand.Execute(null);
                        _pendingSearchText = null;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ MainPage_Loaded Error: {ex.Message}");
        }
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        Console.WriteLine("🔵 OnNavigatedTo called");

        // ✅ RESET EmptyStateGrid về Collapsed ngay khi navigate đến
        if (EmptyStateGrid != null)
        {
            EmptyStateGrid.Visibility = Visibility.Collapsed;
            Console.WriteLine("🔄 EmptyStateGrid reset to Collapsed");
        }

        try
        {
            if (Application.Current is App app && app.Host != null)
            {
                Console.WriteLine("🔵 App.Host found");
                
                // Unsubscribe old if exists
                if (_viewModel != null)
                {
                    _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
                    _viewModel.Products.CollectionChanged -= Products_CollectionChanged;
                }
                
                _viewModel = app.Host.Services.GetService<MainModel>();

                if (_viewModel != null)
                {
                    DataContext = _viewModel;
                    Console.WriteLine($"✅ ViewModel set successfully, CartItemCount: {_viewModel.CartItemCount}");
                    
                    // Subscribe để update badge khi cart count thay đổi
                    _viewModel.PropertyChanged += ViewModel_PropertyChanged;
                    
                    // ✅ Subscribe để update empty state khi Products thay đổi
                    _viewModel.Products.CollectionChanged += Products_CollectionChanged;
                    
                    // Force load cart count từ service
                    await _viewModel.RefreshCartCountAsync();
                    Console.WriteLine($"✅ After refresh, CartItemCount: {_viewModel.CartItemCount}");
                    
                    // Update badge ngay
                    UpdateCartBadge();
                    
                    // ✅ Update empty state sau khi ViewModel setup xong
                    UpdateEmptyState();
                    
                    // Apply pending search nếu có
                    if (!string.IsNullOrEmpty(_pendingSearchText))
                    {
                        Console.WriteLine($"🔍 Applying pending search: '{_pendingSearchText}'");
                        _viewModel.SearchText = _pendingSearchText;
                        _viewModel.SearchCommand.Execute(null);
                        _pendingSearchText = null;
                    }
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
    
    private void Products_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        // ✅ Update empty state on UI thread when Products collection changes
        DispatcherQueue.TryEnqueue(() => UpdateEmptyState());
    }
    
    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainModel.CartItemCount))
        {
            DispatcherQueue.TryEnqueue(() => UpdateCartBadge());
        }
    }
    
    private void UpdateCartBadge()
    {
        if (_viewModel == null || CartBadgeText == null || CartBadge == null)
        {
            Console.WriteLine("⚠️ Cannot update badge - elements not ready");
            return;
        }
        
        var count = _viewModel.CartItemCount;
        Console.WriteLine($"🛒 Updating badge: {count}");
        
        CartBadgeText.Text = count.ToString();
        CartBadge.Visibility = count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }
    
    private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        Console.WriteLine($"🔍 SearchBox_TextChanged fired! Text: '{SearchBox.Text}'");
        
        // Cancel previous search nếu có
        _searchDebounceToken?.Cancel();
        _searchDebounceToken = new CancellationTokenSource();
        var token = _searchDebounceToken.Token;
        
        try
        {
            // Debounce - đợi 300ms sau khi user ngừng gõ
            await Task.Delay(300, token);
            
            // Lazy init ViewModel nếu chưa có
            if (_viewModel == null)
            {
                Console.WriteLine("⚠️ ViewModel null, initializing now...");
                await InitializeViewModelIfNeeded();
            }
            
            // Sau 300ms, execute search
            if (_viewModel != null)
            {
                _viewModel.SearchText = SearchBox.Text;
                _viewModel.SearchCommand.Execute(null);
                Console.WriteLine($"✅ Search executed: '{SearchBox.Text}'");
                
                // Update empty state after search
                UpdateEmptyState();
            }
            else
            {
                Console.WriteLine("❌ ViewModel still null after init attempt");
            }
        }
        catch (TaskCanceledException)
        {
            // User đã gõ ký tự mới, search này bị cancel
            Console.WriteLine("🔍 Search cancelled - user still typing");
        }
    }
    
    private async Task InitializeViewModelIfNeeded()
    {
        if (_viewModel != null) return;
        
        try
        {
            if (Application.Current is App app && app.Host != null)
            {
                Console.WriteLine("🔵 Initializing ViewModel...");
                _viewModel = app.Host.Services.GetService<MainModel>();

                if (_viewModel != null)
                {
                    DataContext = _viewModel;
                    Console.WriteLine($"✅ ViewModel initialized, CartItemCount: {_viewModel.CartItemCount}");
                    
                    // Subscribe for cart badge updates
                    _viewModel.PropertyChanged += ViewModel_PropertyChanged;
                    
                    // Refresh cart count
                    await _viewModel.RefreshCartCountAsync();
                    UpdateCartBadge();
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
            Console.WriteLine($"❌ InitializeViewModelIfNeeded Error: {ex.Message}");
        }
    }
    
    private void UpdateEmptyState()
    {
        if (_viewModel != null && EmptyStateGrid != null && ProductListView != null)
        {
            bool isEmpty = _viewModel.Products.Count == 0;
            bool hasSearchText = !string.IsNullOrWhiteSpace(_viewModel.SearchText);
            
            // ✅ Chỉ hiển thị EmptyState khi có search text VÀ không có kết quả
            // Nếu không có search text, không hiển thị EmptyState (đang loading hoặc chưa search)
            bool showEmptyState = isEmpty && hasSearchText;
            
            EmptyStateGrid.Visibility = showEmptyState ? Visibility.Visible : Visibility.Collapsed;
            ProductListView.Visibility = showEmptyState ? Visibility.Collapsed : Visibility.Visible;
            
            Console.WriteLine($"📊 Empty state: {EmptyStateGrid.Visibility}, Products: {_viewModel.Products.Count}, SearchText: '{_viewModel.SearchText}'");
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
