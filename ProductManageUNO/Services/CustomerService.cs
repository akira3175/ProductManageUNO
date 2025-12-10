using Microsoft.EntityFrameworkCore;
using ProductManageUNO.Data;
using ProductManageUNO.Models;

namespace ProductManageUNO.Services;

public interface ICustomerService
{
    Task<Customer?> GetLastCustomerAsync();
    Task<bool> SaveCustomerAsync(Customer customer);
}

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _context;

    public CustomerService(AppDbContext context)
    {
        _context = context;
        _context.Database.EnsureCreated();
    }

    /// <summary>
    /// Get the last saved customer (for auto-fill)
    /// </summary>
    public async Task<Customer?> GetLastCustomerAsync()
    {
        try
        {
            return await _context.Customers
                .OrderByDescending(c => c.CreatedAt)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ GetLastCustomerAsync Error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Save customer info locally
    /// </summary>
    public async Task<bool> SaveCustomerAsync(Customer customer)
    {
        try
        {
            // Clear old customers and save new one
            var existingCustomers = await _context.Customers.ToListAsync();
            _context.Customers.RemoveRange(existingCustomers);
            
            customer.CreatedAt = DateTime.Now;
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();
            
            Console.WriteLine($"✅ Saved customer: {customer.Name}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ SaveCustomerAsync Error: {ex.Message}");
            return false;
        }
    }
}
