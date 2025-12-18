using Microsoft.EntityFrameworkCore;
using ProductManageUNO.Data;
using ProductManageUNO.Models;
using System.Text.Json;

namespace ProductManageUNO.Services;

public interface IOrderHistoryService
{
    Task<List<LocalOrder>> GetAllOrdersAsync();
    Task<bool> SaveOrderAsync(LocalOrder order);
    Task<OrderDetailData?> GetOrderByIdAsync(int orderId);
}

public class OrderHistoryService : IOrderHistoryService
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;

    public OrderHistoryService(AppDbContext context, HttpClient httpClient)
    {
        _context = context;
        _context.Database.EnsureCreated();
        
        _httpClient = httpClient;
        var baseUrl = "http://localhost:5052";
#if ANDROID
        baseUrl = "http://10.0.2.2:5052";
#endif
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// Get all orders from local SQLite, sorted by date descending
    /// </summary>
    public async Task<List<LocalOrder>> GetAllOrdersAsync()
    {
        try
        {
            return await _context.Orders
                .AsNoTracking()
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ GetAllOrdersAsync Error: {ex.Message}");
            return new List<LocalOrder>();
        }
    }

    /// <summary>
    /// Get order detail from API by ID
    /// </summary>
    public async Task<OrderDetailData?> GetOrderByIdAsync(int orderId)
    {
        try
        {
            Console.WriteLine($"🔵 Getting order detail for ID: {orderId}");
            
            var response = await _httpClient.GetAsync($"/api/v1/order/{orderId}");
            var responseContent = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"📥 Order Detail API Response: {responseContent}");
            
            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<OrderDetailResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (result?.Success == true && result.Data != null)
                {
                    Console.WriteLine($"✅ Order detail loaded: {result.Data.Items?.Count ?? 0} items");
                    return result.Data;
                }
            }
            
            Console.WriteLine($"❌ Order Detail API failed: {response.StatusCode}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ GetOrderByIdAsync Error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Save order to local SQLite
    /// </summary>
    public async Task<bool> SaveOrderAsync(LocalOrder order)
    {
        try
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            Console.WriteLine($"✅ Saved order #{order.ApiOrderId} to local storage");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ SaveOrderAsync Error: {ex.Message}");
            return false;
        }
    }
}

