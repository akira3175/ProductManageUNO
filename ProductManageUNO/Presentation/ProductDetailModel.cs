using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProductManageUNO.Models;
using ProductManageUNO.Services;
using Microsoft.UI.Xaml.Data; // Thêm dòng này

namespace ProductManageUNO.Presentation;

[Bindable] // Thêm attribute này
public partial class ProductDetailModel : ObservableObject
{
    private readonly IApiService _apiService;

    [ObservableProperty]
    private Product? _product;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public ProductDetailModel(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task LoadProductAsync(int productId)
    {
        if (IsLoading)
            return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            Console.WriteLine($"🔵 Loading product detail for ID: {productId}");

            Product = await _apiService.GetProductByIdAsync(productId);

            if (Product == null)
            {
                ErrorMessage = "Không tìm thấy sản phẩm";
                Console.WriteLine("❌ Product not found");
            }
            else
            {
                Console.WriteLine($"✅ Loaded product: {Product.ProductName}");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi khi tải chi tiết: {ex.Message}";
            Console.WriteLine($"❌ Error loading product detail: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task Refresh(int productId)
    {
        await LoadProductAsync(productId);
    }
}
