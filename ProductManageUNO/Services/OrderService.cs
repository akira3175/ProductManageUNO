using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ProductManageUNO.Models;

namespace ProductManageUNO.Services;

public interface IOrderService
{
    Task<CustomerData?> CreateCustomerAsync(CreateCustomerRequest request);
    Task<OrderData?> CreateOrderAsync(CreateOrderRequest request);
}

public class OrderService : IOrderService
{
    private readonly HttpClient _httpClient;

    public OrderService(HttpClient httpClient)
    {
        _httpClient = httpClient;

        var baseUrl = "http://localhost:5052";

#if ANDROID
        baseUrl = "http://10.0.2.2:5052";
#endif
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// Create customer via API and return the customer with ID
    /// </summary>
    public async Task<CustomerData?> CreateCustomerAsync(CreateCustomerRequest request)
    {
        try
        {
            Console.WriteLine($"🔵 Creating customer: {request.Name}");
            
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync("/api/v1/customer", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"📥 Customer API Response: {responseContent}");
            
            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<CustomerApiResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (result?.Success == true && result.Data != null)
                {
                    Console.WriteLine($"✅ Customer created with ID: {result.Data.Id}");
                    return result.Data;
                }
            }
            
            Console.WriteLine($"❌ Customer API failed: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ CreateCustomerAsync Error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Create order via API
    /// </summary>
    public async Task<OrderData?> CreateOrderAsync(CreateOrderRequest request)
    {
        try
        {
            Console.WriteLine($"🔵 Creating order for customer ID: {request.CustomerId}");
            
            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            Console.WriteLine($"📤 Order Request: {json}");
            
            var response = await _httpClient.PostAsync("/api/v1/order", content);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"📥 Order API Response: {responseContent}");
            
            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<OrderApiResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (result?.Success == true && result.Data != null)
                {
                    Console.WriteLine($"✅ Order created with ID: {result.Data.Id}");
                    return result.Data;
                }
            }
            
            Console.WriteLine($"❌ Order API failed: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ CreateOrderAsync Error: {ex.Message}");
            return null;
        }
    }
}
