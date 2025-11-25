// File: Data/AppDbContext.cs
using System;
using System.Collections.Generic;
using System.IO;
using ProductManageUNO.Models;
using Microsoft.EntityFrameworkCore;

namespace ProductManageUNO.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<CartItem> CartItems { get; set; } // Ta sẽ tạo Model này ở Giai đoạn 2

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string dbPath = "";

            // Logic xác định đường dẫn DB tùy nền tảng
            if (OperatingSystem.IsWindows())
            {
                var folder = Environment.SpecialFolder.LocalApplicationData;
                var path = Environment.GetFolderPath(folder);
                dbPath = Path.Join(path, "store.db");
            }
            else if (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS())
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                dbPath = Path.Combine(path, "store.db");
            }
            // WebAssembly không hỗ trợ SQLite trực tiếp theo cách này (thường dùng LocalStorage), 
            // ta sẽ xử lý riêng cho Web sau.

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }
}
