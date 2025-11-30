using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Navigation;
using System;

namespace ProductManageUNO.Presentation;

public sealed partial class ProductDetailPage : Page
{
    public ProductDetailModel ViewModel { get; private set; }
    private int _currentProductId;

    public ProductDetailPage()
    {
        this.InitializeComponent();

        // Đăng ký converters
        Resources["NullToVisibilityConverter"] = new NullToVisibilityConverter();
        Resources["StringNotEmptyToVisibilityConverter"] = new StringNotEmptyToVisibilityConverter();
        Resources["BoolToVisibilityConverter"] = new BoolToVisibilityConverter();
        Resources["PriceFormatConverter"] = new PriceFormatConverter();
        Resources["DateFormatConverter"] = new DateFormatConverter();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        Console.WriteLine("🔵 ProductDetailPage OnNavigatedTo");

        if (Application.Current is App app && app.Host != null)
        {
            ViewModel = app.Host.Services.GetService(typeof(ProductDetailModel)) as ProductDetailModel;
            
            if (ViewModel == null)
            {
                Console.WriteLine("❌ ViewModel is null!");
                return;
            }

            Console.WriteLine("✅ ViewModel initialized");

            if (e.Parameter is int productId)
            {
                _currentProductId = productId;
                Console.WriteLine($"🔵 Loading product ID: {productId}");
                await ViewModel.LoadProductAsync(productId);
            }
            else
            {
                Console.WriteLine($"❌ Invalid parameter type: {e.Parameter?.GetType().Name ?? "null"}");
            }
        }
        else
        {
            Console.WriteLine("❌ App.Host is null!");
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (Frame.CanGoBack)
        {
            Frame.GoBack();
        }
    }

    private async void RetryButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel != null && _currentProductId > 0)
        {
            await ViewModel.LoadProductAsync(_currentProductId);
        }
    }
}

// Converter để kiểm tra null
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value != null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

// Converter để kiểm tra string not empty
public class StringNotEmptyToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return !string.IsNullOrEmpty(value as string) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

// Converter bool to visibility
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is bool b && b ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

// Converter format giá
public class PriceFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is decimal price)
        {
            return price.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN"));
        }
        if (value is double priceDouble)
        {
            return priceDouble.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN"));
        }
        if (value is int priceInt)
        {
            return priceInt.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("vi-VN"));
        }
        return "0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

// Converter format date
public class DateFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTime date)
        {
            return $"Ngày tạo: {date:dd/MM/yyyy HH:mm}";
        }
        return "Ngày tạo: N/A";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
