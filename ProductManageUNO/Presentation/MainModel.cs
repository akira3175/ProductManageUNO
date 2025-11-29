using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProductManageUNO.Models;
using ProductManageUNO.Services;
using System.Collections.ObjectModel;
using Microsoft.UI.Dispatching;

namespace ProductManageUNO.Presentation;

public partial class MainModel : ObservableObject
{
    private readonly IApiService _apiService;
    private readonly DispatcherQueue? _dispatcherQueue;

    [ObservableProperty]
    private string _title = "Danh Sách Sản Phẩm";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _showLoadingUI; // Chỉ hiện UI loading nếu load > 300ms

    // Computed properties cho XAML binding
    public bool IsNotLoading => !ShowLoadingUI;

    partial void OnShowLoadingUIChanged(bool value)
    {
        OnPropertyChanged(nameof(IsNotLoading));
        Console.WriteLine($"📊 ShowLoadingUI changed: {value}, IsNotLoading: {IsNotLoading}");
    }

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
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
        Console.WriteLine("🔵 MainModel Constructor - Starting initial load");
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
        Console.WriteLine($"🟡 LoadDataAsync START - IsLoading: {IsLoading}");

        if (IsLoading)
        {
            Console.WriteLine("⚠️ Already loading, returning...");
            return;
        }

        IsLoading = true;
        ShowLoadingUI = false; // Ban đầu ẩn

        // Task để hiện loading sau 300ms
        var showLoadingTask = Task.Delay(300).ContinueWith(_ =>
        {
            if (IsLoading) // Chỉ hiện nếu vẫn đang load
            {
                ShowLoadingUI = true;
                Console.WriteLine("🔵 ShowLoadingUI = true (sau 300ms)");
            }
        });

        try
        {
            Console.WriteLine("🌐 Calling API...");
            var data = await _apiService.GetProductsAsync(CurrentPage, 50);
            _allProducts = data ?? new List<Product>();

            Console.WriteLine($"✅ API returned {_allProducts.Count} products");

            Products.Clear();
            foreach (var item in _allProducts)
            {
                Products.Add(item);
            }
            TotalItems = _allProducts.Count;

            Console.WriteLine($"✅ UI Updated - Products: {Products.Count}, Total: {TotalItems}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Load Error: {ex.Message}");

            TotalItems = 0;
            Products.Clear();
        }
        finally
        {
            IsLoading = false;
            ShowLoadingUI = false;
            Console.WriteLine("🟢 IsLoading & ShowLoadingUI = false");

            // QUAN TRỌNG: Force notify UI
            OnPropertyChanged(nameof(ShowLoadingUI));
            OnPropertyChanged(nameof(IsNotLoading));
            Console.WriteLine("📢 Manually triggered PropertyChanged for ShowLoadingUI");
        }

        Console.WriteLine($"🟡 LoadDataAsync END");
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
