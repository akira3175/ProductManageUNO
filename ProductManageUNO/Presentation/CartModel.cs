using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProductManageUNO.Models;
using ProductManageUNO.Services;

namespace ProductManageUNO.Presentation;

public partial class CartModel : ObservableObject
{
    private readonly ICartService _cartService;

    [ObservableProperty]
    private string _title = "Giá» HÃ ng";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _totalItems = 0;

    [ObservableProperty]
    private decimal _totalAmount = 0;

    [ObservableProperty]
    private bool _isEmpty = true;

    public Visibility CartEmpty
    {
        get => IsEmpty ? Visibility.Visible : Visibility.Collapsed;
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
            Console.WriteLine("ðŸ”µ Loading cart...");

            var items = await _cartService.GetAllAsync();

            CartItems.Clear();
            foreach (var item in items)
            {
                CartItems.Add(item);
            }

            await UpdateTotalsAsync();

            Console.WriteLine($"âœ… Loaded {CartItems.Count} items");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"âŒ LoadCartAsync Error: {ex.Message}");
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
        var success = await _cartService.ClearCartAsync();
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

    private async Task UpdateTotalsAsync()
    {
        TotalItems = await _cartService.GetTotalItemsAsync();
        TotalAmount = await _cartService.GetTotalAmountAsync();
        IsEmpty = CartItems.Count == 0;

        Console.WriteLine($"📊 Updated totals: Items={TotalItems}, Amount={TotalAmount}, IsEmpty={IsEmpty}, CartCount={CartItems.Count}");
    }
}
