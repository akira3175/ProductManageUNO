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

    public ObservableCollection<Product> Products { get; } = new();

    // Inject IApiService vào Constructor
    public MainModel(IApiService apiService)
    {
        _apiService = apiService;
        LoadDataCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadData()
    {
        if (IsLoading) return;
        IsLoading = true;

        var data = await _apiService.GetProductsAsync();

        Products.Clear();
        foreach (var item in data)
        {
            Products.Add(item);
        }

        IsLoading = false;
    }
}
