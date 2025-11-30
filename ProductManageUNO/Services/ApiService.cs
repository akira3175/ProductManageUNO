using System.Net.Http.Json;
using ProductManageUNO.Models;

namespace ProductManageUNO.Services;

public interface IApiService
{
    Task<List<Product>> GetProductsAsync(int page = 1, int pageSize = 10);
    Task<Product?> GetProductByIdAsync(int id);
}

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;

        var baseUrl = "http://localhost:5052";

#if ANDROID
        baseUrl = "http://10.0.2.2:5052";
#endif
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public async Task<List<Product>> GetProductsAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResPagination<List<Product>>>(
                $"/api/Product?page={page}&pageSize={pageSize}");

            if (response is not null && response.Success)
            {
                return response.Result ?? new List<Product>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API Error: {ex.Message}");
        }

        return new List<Product>();
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResDetail<Product>>(
                $"/api/Product/{id}");

            if (response is not null && response.Success)
            {
                return response.Data;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API Error GetProductById: {ex.Message}");
        }

        return null;
    }
}
