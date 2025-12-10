using Microsoft.EntityFrameworkCore;
using ProductManageUNO.Data;
using ProductManageUNO.Models;

namespace ProductManageUNO.Services;

public interface IOrderHistoryService
{
    Task<List<LocalOrder>> GetAllOrdersAsync();
    Task<bool> SaveOrderAsync(LocalOrder order);
}

public class OrderHistoryService : IOrderHistoryService
{
    private readonly AppDbContext _context;

    public OrderHistoryService(AppDbContext context)
    {
        _context = context;
        _context.Database.EnsureCreated();
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
