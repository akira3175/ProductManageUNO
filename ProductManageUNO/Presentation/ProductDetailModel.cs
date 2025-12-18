using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProductManageUNO.Models;
using ProductManageUNO.Services;
using Microsoft.UI.Xaml.Data;

namespace ProductManageUNO.Presentation;

[Bindable]
public partial class ProductDetailModel : ObservableObject
{
    private readonly IApiService _apiService;
    private readonly ICartService _cartService;

    [ObservableProperty]
    private Product? _product;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private int _quantity = 1;

    [ObservableProperty]
    private bool _isAddingToCart;

    [ObservableProperty]
    private string _addToCartMessage = string.Empty;

    [ObservableProperty]
    private bool _showSuccessMessage;

    public ProductDetailModel(IApiService apiService, ICartService cartService)
    {
        _apiService = apiService;
        _cartService = cartService;
    }

    public async Task LoadProductAsync(int productId)
    {
        if (IsLoading)
            return;

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            Quantity = 1; // Reset quantity khi load sản phẩm mới
            ShowSuccessMessage = false;
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
    private void IncreaseQuantity()
    {
        if (Quantity < 99)
        {
            Quantity++;
        }
    }

    [RelayCommand]
    private void DecreaseQuantity()
    {
        if (Quantity > 1)
        {
            Quantity--;
        }
    }

    [RelayCommand]
    private async Task AddToCart()
    {
        if (Product == null || IsAddingToCart)
            return;

        try
        {
            IsAddingToCart = true;
            ShowSuccessMessage = false;

            var cartItem = new CartItem
            {
                ProductId = Product.Id,
                ProductName = Product.ProductName,
                Barcode = Product.Barcode,
                Price = Product.Price,
                Quantity = Quantity,
                Unit = Product.Unit,
                AddedAt = DateTime.Now
            };

            var success = await _cartService.AddToCartAsync(cartItem);

            if (success)
            {
                AddToCartMessage = $"Đã thêm {Quantity} {Product.Unit} vào giỏ hàng!";
                ShowSuccessMessage = true;
                Console.WriteLine($"✅ Added {Quantity}x {Product.ProductName} to cart");

                // Auto hide message after 3 seconds
                _ = Task.Run(async () =>
                {
                    await Task.Delay(3000);
                    ShowSuccessMessage = false;
                });
            }
            else
            {
                AddToCartMessage = "Không thể thêm vào giỏ hàng";
                ShowSuccessMessage = true;
            }
        }
        catch (Exception ex)
        {
            AddToCartMessage = $"Lỗi: {ex.Message}";
            ShowSuccessMessage = true;
            Console.WriteLine($"❌ Error adding to cart: {ex.Message}");
        }
        finally
        {
            IsAddingToCart = false;
        }
    }

    [RelayCommand]
    private async Task Refresh(int productId)
    {
        await LoadProductAsync(productId);
    }
}
