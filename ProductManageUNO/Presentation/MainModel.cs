using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProductManageUNO.Models;
using ProductManageUNO.Services;
using System.Collections.ObjectModel;

namespace ProductManageUNO.Presentation;

public partial class MainModel : ObservableObject
{
    private readonly IApiService _apiService;

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

    public ObservableCollection<Product> Products { get; } = new();
    private List<Product> _allProducts = new();

    public MainModel(IApiService apiService)
    {
        _apiService = apiService;
        Console.WriteLine("🔵 MainModel Constructor");
        _ = LoadDataAsync();
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

    partial void OnSearchTextChanged(string value)
    {
        SearchCommand.Execute(null);
    }
}
