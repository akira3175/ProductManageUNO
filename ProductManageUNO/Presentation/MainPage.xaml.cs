using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ProductManageUNO.Presentation;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();

        // --- KẾT NỐI VIEWMODEL ---
        // Lấy MainModel đã được đăng ký trong App.xaml.cs
        if (Application.Current is App app && app.Host != null)
        {
            DataContext = app.Host.Services.GetService(typeof(MainModel));
        }
    }
}
