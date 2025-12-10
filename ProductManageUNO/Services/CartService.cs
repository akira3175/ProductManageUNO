using Microsoft.EntityFrameworkCore;
using ProductManageUNO.Data;
using ProductManageUNO.Models;

namespace ProductManageUNO.Services;

public interface ICartService
{
    Task<List<CartItem>> GetAllAsync();
    Task<CartItem?> GetByProductIdAsync(int productId);
    Task<bool> AddToCartAsync(CartItem cartItem);
    Task<bool> UpdateQuantityAsync(int cartItemId, int quantity);
    Task<bool> RemoveAsync(int cartItemId);
    Task<bool> ClearCartAsync();
    Task<int> GetTotalItemsAsync();
    Task<decimal> GetTotalAmountAsync();
}

public class CartService : ICartService
{
    private readonly AppDbContext _context;

    public CartService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CartItem>> GetAllAsync()
    {
        try
        {
            return await _context.CartItems
                .AsNoTracking()
                .OrderByDescending(x => x.AddedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ GetAllAsync Error: {ex.Message}");
            return new List<CartItem>();
        }
    }

    public async Task<CartItem?> GetByProductIdAsync(int productId)
    {
        try
        {
            return await _context.CartItems
                .FirstOrDefaultAsync(x => x.ProductId == productId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ GetByProductIdAsync Error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> AddToCartAsync(CartItem cartItem)
    {
        try
        {
            var existingItem = await GetByProductIdAsync(cartItem.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += cartItem.Quantity;
                existingItem.AddedAt = DateTime.Now;
                _context.CartItems.Update(existingItem);
            }
            else
            {
                await _context.CartItems.AddAsync(cartItem);
            }

            await _context.SaveChangesAsync();
            Console.WriteLine($"✅ Added {cartItem.ProductName} to cart");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ AddToCartAsync Error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateQuantityAsync(int cartItemId, int quantity)
    {
        try
        {
            if (quantity <= 0)
            {
                return await RemoveAsync(cartItemId);
            }

            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item == null) return false;

            item.Quantity = quantity;
            _context.CartItems.Update(item);
            await _context.SaveChangesAsync();

            Console.WriteLine($"✅ Updated quantity for {item.ProductName}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ UpdateQuantityAsync Error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> RemoveAsync(int cartItemId)
    {
        try
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item == null) return false;

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();

            Console.WriteLine($"✅ Removed {item.ProductName} from cart");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ RemoveAsync Error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ClearCartAsync()
    {
        try
        {
            var items = await _context.CartItems.ToListAsync();
            _context.CartItems.RemoveRange(items);
            await _context.SaveChangesAsync();

            Console.WriteLine("✅ Cart cleared");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ClearCartAsync Error: {ex.Message}");
            return false;
        }
    }

    public async Task<int> GetTotalItemsAsync()
    {
        try
        {
            return await _context.CartItems.SumAsync(x => x.Quantity);
        }
        catch
        {
            return 0;
        }
    }

    public async Task<decimal> GetTotalAmountAsync()
    {
        try
        {
            var items = await _context.CartItems.AsNoTracking().ToListAsync();
            return items.Sum(x => x.Subtotal);
        }
        catch
        {
            return 0;
        }
    }
}
