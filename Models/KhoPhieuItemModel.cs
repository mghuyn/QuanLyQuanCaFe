using QuanLyQuanCaFe.Core;

namespace QuanLyQuanCaFe.Models
{
    public class KhoPhieuItemModel : BaseViewModel
    {
        private decimal _soLuong;
        private decimal _donGia;

        public int MaNguyenLieu { get; set; }
        public string TenNguyenLieu { get; set; }
        public string DonViTinh { get; set; }

        public decimal SoLuong
        {
            get => _soLuong;
            set
            {
                SetProperty(ref _soLuong, value);
                OnPropertyChanged(nameof(ThanhTien));
                OnPropertyChanged(nameof(SoLuongText));
                OnPropertyChanged(nameof(ThanhTienText));
            }
        }

        public decimal DonGia
        {
            get => _donGia;
            set
            {
                SetProperty(ref _donGia, value);
                OnPropertyChanged(nameof(ThanhTien));
                OnPropertyChanged(nameof(DonGiaText));
                OnPropertyChanged(nameof(ThanhTienText));
            }
        }

        public decimal ThanhTien
        {
            get { return SoLuong * DonGia; }
        }

        public string SoLuongText
        {
            get { return SoLuong.ToString("N2") + " " + DonViTinh; }
        }

        public string DonGiaText
        {
            get { return DonGia.ToString("N0") + "đ"; }
        }

        public string ThanhTienText
        {
            get { return ThanhTien.ToString("N0") + "đ"; }
        }
    }
}
