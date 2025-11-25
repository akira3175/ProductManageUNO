using System.Net.Http.Json;
using ProductManageUNO.Models;

namespace ProductManageUNO.Services;

public interface IApiService
{
    Task<List<Product>> GetProductsAsync();
}

public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;

    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;

        // --- CẤU HÌNH LẠI PORT (Quan trọng) ---
        // Hãy kiểm tra cửa sổ Console của Backend để chắc chắn port là 5126
        var baseUrl = "http://localhost:5052";

        // Android Emulator cần dùng IP 10.0.2.2 để trỏ về máy tính
#if ANDROID
        baseUrl = "http://10.0.2.2:5052";
#endif
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(10); // Thêm timeout để không bị treo quá lâu
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        try
        {
            // Endpoint API
            var response = await _httpClient.GetFromJsonAsync<ApiResPagination<List<Product>>>("/api/Inventory?page=1&pageSize=50");

            if (response is not null && response.Success)
            {
                return response.Result;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"API Error: {ex.Message}");
        }
        // Trả về danh sách rỗng nếu lỗi, giúp tắt loading indicator
        return new List<Product>();
    }
}
