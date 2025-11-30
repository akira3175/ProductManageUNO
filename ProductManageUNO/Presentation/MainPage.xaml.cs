using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Navigation;
using ProductManageUNO.Models;
using System;

namespace ProductManageUNO.Presentation;

public sealed partial class MainPage : Page
{
    private MainModel? _viewModel;

    public MainPage()
    {
        this.InitializeComponent();

        // Đăng ký converter
        Resources["EmptyToVisibilityConverter"] = new EmptyToVisibilityConverter();
        Resources["StringFormatConverter"] = new StringFormatConverter();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // Kết nối ViewModel
        if (Application.Current is App app && app.Host != null)
        {
            _viewModel = app.Host.Services.GetService(typeof(MainModel)) as MainModel;
            DataContext = _viewModel;
        }
    }

    private void ProductListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        Console.WriteLine("🔵 ProductListView_ItemClick fired!");

        if (e.ClickedItem is Product product)
        {
            Console.WriteLine($"🔵 Clicked product ID: {product.Id}, Name: {product.ProductName}");

            try
            {
                // Thử navigate với Frame truyền thống
                if (Frame != null)
                {
                    bool success = Frame.Navigate(typeof(ProductDetailPage), product.Id);
                    Console.WriteLine($"🔵 Frame navigation result: {success}");
                }
                else
                {
                    Console.WriteLine("❌ Frame is null!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Navigation error: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            }
        }
        else
        {
            Console.WriteLine($"❌ ClickedItem type: {e.ClickedItem?.GetType().Name ?? "null"}");
        }
    }

    // Thêm method này cho Button version
    private void ProductItem_Click(object sender, RoutedEventArgs e)
    {
        Console.WriteLine("🔵 ProductItem_Click fired!");

        if (sender is Button button && button.Tag is int productId)
        {
            Console.WriteLine($"🔵 Navigating to product ID: {productId}");

            try
            {
                if (Frame != null)
                {
                    bool success = Frame.Navigate(typeof(ProductDetailPage), productId);
                    Console.WriteLine($"🔵 Navigation result: {success}");
                }
                else
                {
                    Console.WriteLine("❌ Frame is null!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Navigation error: {ex.Message}");
            }
        }
    }
}

// Converter để hiển thị empty state
public class EmptyToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int count)
        {
            return count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

// Converter để format chuỗi
public class StringFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null) return null;
        if (parameter is string formatString)
        {
            return string.Format(System.Globalization.CultureInfo.GetCultureInfo("vi-VN"), formatString, value);
        }
        return value.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
}
