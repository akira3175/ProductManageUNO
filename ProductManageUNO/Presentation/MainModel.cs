using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProductManageUNO.Models;
using ProductManageUNO.Services;
using System.Collections.ObjectModel;

namespace ProductManageUNO.Presentation;

public partial class MainModel : ObservableObject
{
    private readonly IApiService _apiService;
    private readonly ICartService _cartService;

    [ObservableProperty]
    private string _title = "Danh Sách Sản Phẩm";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalItems = 0;

    [ObservableProperty]
    private int _cartItemCount = 0;

    public ObservableCollection<Product> Products { get; } = new();
    private List<Product> _allProducts = new();

    public MainModel(IApiService apiService, ICartService cartService)
    {
        _apiService = apiService;
        _cartService = cartService;
        Console.WriteLine("🔵 MainModel Constructor");
        _ = LoadDataAsync();
        _ = UpdateCartCountAsync();
    }

    [RelayCommand]
    private async Task LoadData()
    {
        Console.WriteLine("🔵 LoadDataCommand triggered");
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        if (IsLoading)
        {
            Console.WriteLine("⚠️ Already loading, skipping...");
            return;
        }

        try
        {
            IsLoading = true;
            Console.WriteLine("🌐 Loading data...");

            var data = await _apiService.GetProductsAsync(CurrentPage, 50);
            _allProducts = data ?? new List<Product>();

            Products.Clear();
            foreach (var item in _allProducts)
            {
                Products.Add(item);
            }

            TotalItems = _allProducts.Count;
            Console.WriteLine($"✅ Loaded {TotalItems} products");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            TotalItems = 0;
            Products.Clear();
        }
        finally
        {
            IsLoading = false;
            Console.WriteLine("✅ Loading complete");
        }
    }

    [RelayCommand]
    private void Search()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            Products.Clear();
            foreach (var item in _allProducts)
            {
                Products.Add(item);
            }
        }
        else
        {
            var filtered = _allProducts
                .Where(p => p.ProductName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                           p.Barcode.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                           p.Category?.CategoryName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true)
                .ToList();

            Products.Clear();
            foreach (var item in filtered)
            {
                Products.Add(item);
            }
        }
    }

    [RelayCommand]
    private async Task AddToCart(Product product)
    {
        try
        {
            Console.WriteLine($"🔵 Adding to cart: {product.ProductName}");

            var cartItem = new CartItem
            {
                ProductId = product.Id,
                ProductName = product.ProductName,
                Barcode = product.Barcode,
                Unit = product.Unit,
                Price = product.Price,
                Quantity = 1,
                AddedAt = DateTime.Now
            };

            var success = await _cartService.AddToCartAsync(cartItem);

            if (success)
            {
                await UpdateCartCountAsync();
                Console.WriteLine("✅ Added to cart successfully");
            }
            else
            {
                Console.WriteLine("❌ Failed to add to cart");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ AddToCart Error: {ex.Message}");
        }
    }

    private async Task UpdateCartCountAsync()
    {
        try
        {
            CartItemCount = await _cartService.GetTotalItemsAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ UpdateCartCount Error: {ex.Message}");
            CartItemCount = 0;
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        SearchCommand.Execute(null);
    }
}
