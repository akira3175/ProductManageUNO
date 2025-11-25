using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using System;

namespace ProductManageUNO.Presentation;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();

        // Kết nối ViewModel
        if (Application.Current is App app && app.Host != null)
        {
            DataContext = app.Host.Services.GetService(typeof(MainModel));
        }

        // Đăng ký converter
        Resources["EmptyToVisibilityConverter"] = new EmptyToVisibilityConverter();
        Resources["StringFormatConverter"] = new StringFormatConverter();
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

// --- Converter MỚI để sửa lỗi StringFormat ---
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
