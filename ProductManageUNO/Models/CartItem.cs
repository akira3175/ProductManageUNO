//using CommunityToolkit.Mvvm.ComponentModel;
//using Microsoft.UI.Xaml.Data;

//namespace ProductManageUNO.Models;

//[Bindable]
//public partial class CartItem : ObservableObject
//{
//    [System.ComponentModel.DataAnnotations.Key]
//    public int Id { get; set; }

//    public int ProductId { get; set; }
//    public string ProductName { get; set; } = string.Empty;
//    public string Barcode { get; set; } = string.Empty;
//    public string Unit { get; set; } = string.Empty;
//    public DateTime AddedAt { get; set; } = DateTime.Now;

//    [ObservableProperty]
//    [NotifyPropertyChangedFor(nameof(Subtotal))]
//    private decimal _price;

//    [ObservableProperty]
//    [NotifyPropertyChangedFor(nameof(Subtotal))]
//    private int _quantity;

//    // ✅ FIX: Calculated property với explicit notification
//    public decimal Subtotal => Price * Quantity;

//    // ✅ THÊM: Public method để force notify từ bên ngoài nếu cần
//    public void RefreshSubtotal()
//    {
//        OnPropertyChanged(nameof(Subtotal));
//    }
//}
