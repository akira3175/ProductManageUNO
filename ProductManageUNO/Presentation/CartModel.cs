using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using ProductManageUNO.Models;
using ProductManageUNO.Services;

namespace ProductManageUNO.Presentation;

[Bindable]
public partial class CartModel : ObservableObject
{
    private readonly ICartService _cartService;

    [ObservableProperty]
    private string _title = "Giỏ Hàng";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _totalItems = 0;

    // Pre-formatted total items for UI binding (bypasses converter issues)
    public string TotalItemsFormatted => $"{TotalItems} sản phẩm";

    partial void OnTotalItemsChanged(int value)
    {
        OnPropertyChanged(nameof(TotalItemsFormatted));
    }

    [ObservableProperty]
    private decimal _totalAmount = 0;

    // Pre-formatted total amount for UI binding (bypasses converter issues)
    public string TotalAmountFormatted => TotalAmount.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN")) + "đ";

    partial void OnTotalAmountChanged(decimal value)
    {
        OnPropertyChanged(nameof(TotalAmountFormatted));
    }

    [ObservableProperty]
    private bool _isEmpty = true;

    public Visibility CartEmpty => IsEmpty ? Visibility.Visible : Visibility.Collapsed;

    partial void OnIsEmptyChanged(bool value)
    {
        OnPropertyChanged(nameof(CartEmpty));
    }

    public ObservableCollection<CartItem> CartItems { get; } = new();

    public CartModel(ICartService cartService)
    {
        _cartService = cartService;
        _ = LoadCartAsync();
    }

    [RelayCommand]
    private async Task LoadCart()
    {
        await LoadCartAsync();
    }

    private async Task LoadCartAsync()
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            Console.WriteLine("🔵 Loading cart...");

            var items = await _cartService.GetAllAsync();

            // Ensure UI updates happen on the UI thread
            var dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            
            // Define the update action
            void UpdateUi()
            {
                // STANDARD UPDATE: Clear and Add to keep the same collection instance
                // This is the most reliable way for XAML bindings
                CartItems.Clear();
                foreach (var item in items)
                {
                    CartItems.Add(item);
                }
                
                // Update TotalItems immediately from loaded items
                TotalItems = CartItems.Count;
                IsEmpty = CartItems.Count == 0;
                
                // Update totals (these are simple properties, but safe to do on UI thread)
                 _ = RefreshTotalsAsync();
                 
                 Console.WriteLine($"✅ Loaded {CartItems.Count} items on UI Thread, TotalItems={TotalItems}");
            }

            if (dispatcherQueue != null && !dispatcherQueue.HasThreadAccess)
            {
                dispatcherQueue.TryEnqueue(UpdateUi);
            }
            else
            {
                UpdateUi();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ LoadCartAsync Error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task IncreaseQuantity(CartItem item)
    {
        var success = await _cartService.UpdateQuantityAsync(item.Id, item.Quantity + 1);
        if (success)
        {
            await LoadCartAsync();
        }
    }

    [RelayCommand]
    private async Task DecreaseQuantity(CartItem item)
    {
        if (item.Quantity > 1)
        {
            var success = await _cartService.UpdateQuantityAsync(item.Id, item.Quantity - 1);
            if (success)
            {
                await LoadCartAsync();
            }
        }
        else
        {
            await RemoveItem(item);
        }
    }

    [RelayCommand]
    private async Task RemoveItem(CartItem item)
    {
        var success = await _cartService.RemoveAsync(item.Id);
        if (success)
        {
            await LoadCartAsync();
        }
    }

    [RelayCommand]
    private async Task ClearCart()
    {
        Console.WriteLine("🗑️ ClearCart command triggered!");
        var success = await _cartService.ClearCartAsync();
        Console.WriteLine($"🗑️ Clear cart result: {success}");
        if (success)
        {
            await LoadCartAsync();
        }
    }

    [RelayCommand]
    private async Task Checkout()
    {
        // TODO: Navigate to checkout page
        Console.WriteLine("ðŸ”µ Navigating to checkout...");
        // Sáº½ implement sau khi cÃ³ API Order
    }

    public async Task RefreshTotalsAsync()
    {
        TotalItems = await _cartService.GetTotalItemsAsync();
        TotalAmount = await _cartService.GetTotalAmountAsync();
        Title = "Giỏ hàng";
        IsEmpty = CartItems.Count == 0;

        // Force explicit notification for all dependent properties
        OnPropertyChanged(nameof(TotalAmountFormatted));
        OnPropertyChanged(nameof(CartEmpty));

        Console.WriteLine($"📊 Updated totals: Items={TotalItems}, Amount={TotalAmount}, Formatted={TotalAmountFormatted}, IsEmpty={IsEmpty}");
    }
}
