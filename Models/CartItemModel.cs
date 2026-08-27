using QuanLyQuanCaFe.Core;

namespace QuanLyQuanCaFe.Models
{
    public class CartItemModel : BaseViewModel
    {
        private int _soLuong;

        public int MaSanPham { get; set; }
        public int MaBienThe { get; set; }
        public string TenSanPham { get; set; }
        public string TenSize { get; set; }
        public decimal DonGia { get; set; }
        private string _ghiChu;

        public string GhiChu
        {
            get => _ghiChu;
            set => SetProperty(ref _ghiChu, value);
        }

        public int SoLuong
        {
            get => _soLuong;
            set
            {
                SetProperty(ref _soLuong, value);
                OnPropertyChanged(nameof(ThanhTien));
                OnPropertyChanged(nameof(ThanhTienText));
                OnPropertyChanged(nameof(TenHienThi));
            }
        }

        public decimal ThanhTien => DonGia * SoLuong;

        public string TenHienThi
        {
            get
            {
                if (string.IsNullOrWhiteSpace(TenSize))
                    return TenSanPham;

                return TenSanPham + " - " + TenSize;
            }
        }

        public string DonGiaText => DonGia.ToString("N0") + "đ";

        public string ThanhTienText => ThanhTien.ToString("N0") + "đ";

        public string TrangThaiMonText
        {
            get { return "Đang chọn"; }
            set { }
        }

        public string TrangThaiMonBackground
        {
            get { return "#EFF6FF"; }
            set { }
        }

        public string TrangThaiMonColor
        {
            get { return "#0B63F6"; }
            set { }
        }
    }
}