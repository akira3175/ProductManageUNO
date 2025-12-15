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
    private bool _isLoadingMore;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalItems = 0;

    [ObservableProperty]
    private int _cartItemCount = 0;

    [ObservableProperty]
    private bool _hasMoreItems = true;

    private const int PageSize = 10;

    public ObservableCollection<Product> Products { get; } = new();
    private List<Product> _allProducts = new();
    private int _displayedCount = 0;
    private bool _isLoadingMoreInProgress = false;

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
        _displayedCount = 0;
        HasMoreItems = true;
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

            // Load all data from API (cached locally)
            var data = await _apiService.GetProductsAsync(1, 200);
            _allProducts = data ?? new List<Product>();
            TotalItems = _allProducts.Count;

            // Clear and load first batch
            Products.Clear();
            _displayedCount = 0;
            await LoadMoreItemsAsync();

            Console.WriteLine($"✅ Loaded {Products.Count}/{TotalItems} products");
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
    private async Task LoadMore()
    {
        // Prevent multiple concurrent LoadMore calls
        if (_isLoadingMoreInProgress || !HasMoreItems || IsLoading)
        {
            Console.WriteLine($"📄 LoadMore skipped: inProgress={_isLoadingMoreInProgress}, HasMoreItems={HasMoreItems}, IsLoading={IsLoading}");
            return;
        }

        _isLoadingMoreInProgress = true;
        try
        {
            await LoadMoreItemsAsync();
        }
        finally
        {
            // Small delay to prevent rapid-fire scroll events
            await Task.Delay(100);
            _isLoadingMoreInProgress = false;
        }
    }

    private async Task LoadMoreItemsAsync()
    {
        if (_displayedCount >= _allProducts.Count)
        {
            HasMoreItems = false;
            return;
        }

        IsLoadingMore = true;

        // Small delay to allow UI to update loading state
        await Task.Delay(50);

        var itemsToAdd = _allProducts
            .Skip(_displayedCount)
            .Take(PageSize)
            .ToList();

        foreach (var item in itemsToAdd)
        {
            Products.Add(item);
        }

        _displayedCount += itemsToAdd.Count;
        HasMoreItems = _displayedCount < _allProducts.Count;

        Console.WriteLine($"📄 Loaded more: {Products.Count}/{TotalItems} (HasMore: {HasMoreItems})");

        IsLoadingMore = false;
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
            Console.WriteLine($"🔵 Product ID: {product.Id}, Price: {product.Price}");

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

            Console.WriteLine($"🔵 Calling CartService.AddToCartAsync...");
            var success = await _cartService.AddToCartAsync(cartItem);

            Console.WriteLine($"🔵 AddToCartAsync result: {success}");

            if (success)
            {
                await UpdateCartCountAsync();
                Console.WriteLine($"✅ Cart count updated: {CartItemCount}");
            }
            else
            {
                Console.WriteLine("❌ Failed to add to cart");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ AddToCart Error: {ex.Message}");
            Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
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
