using System;
using System.IO;
using ProductManageUNO.Models;
using Microsoft.EntityFrameworkCore;

namespace ProductManageUNO.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<LocalOrder> Orders { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            // Constructor rỗng để DI container hoạt động tốt hơn
        }

        // Constructor mặc định cần thiết cho Design-time hoặc nếu DI không inject Options
        public AppDbContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
                return;

            string dbPath = "";

            if (OperatingSystem.IsWindows())
            {
                var folder = Environment.SpecialFolder.LocalApplicationData;
                var path = Environment.GetFolderPath(folder);
                dbPath = Path.Join(path, "store.db");
            }
            else if (OperatingSystem.IsAndroid())
            {
                // Sử dụng Personal folder cho Android (/data/user/0/com.package/files)
                var path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);

                // ✅ FIX: Đảm bảo thư mục tồn tại trước khi trỏ file vào
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                dbPath = Path.Combine(path, "store.db");
            }
            else if (OperatingSystem.IsIOS())
            {
                // iOS cần để trong Library folder, không phải Documents để tránh iCloud backup db rác
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library");
                dbPath = Path.Combine(path, "store.db");
            }

            // ⚠️ DEBUG LOG: In ra đường dẫn để kiểm tra trên Logcat
            Console.WriteLine($"📂 DATABASE PATH: {dbPath}");

            // Nếu dbPath rỗng, SQLite sẽ báo lỗi 14
            if (string.IsNullOrEmpty(dbPath))
            {
                throw new Exception("❌ Database path is empty! Check OperatingSystem logic.");
            }

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }
}
