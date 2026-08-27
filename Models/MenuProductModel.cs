using QuanLyQuanCaFe.Core;

namespace QuanLyQuanCaFe.Models
{
    public class MenuProductModel : BaseViewModel
    {
        private bool _isSelected;
        private bool _conHoatDong;

        public int MaSanPham { get; set; }
        public int MaDanhMuc { get; set; }

        public string TenSanPham { get; set; }
        public string TenDanhMuc { get; set; }
        public string MoTa { get; set; }
        public string HinhAnh { get; set; }

        public decimal GiaBan { get; set; }

        public string GiaBanText
        {
            get
            {
                return GiaBan.ToString("N0") + "đ";
            }
        }

        public bool ConHoatDong
        {
            get => _conHoatDong;
            set
            {
                SetProperty(ref _conHoatDong, value);
                OnPropertyChanged(nameof(TrangThaiText));
                OnPropertyChanged(nameof(TrangThaiColor));
                OnPropertyChanged(nameof(TrangThaiBackground));
            }
        }

        public string TrangThaiText
        {
            get
            {
                return ConHoatDong ? "Đang bán" : "Ngưng bán";
            }
        }

        public string TrangThaiColor
        {
            get
            {
                return ConHoatDong ? "#0B63F6" : "#DC2626";
            }
        }

        public string TrangThaiBackground
        {
            get
            {
                return ConHoatDong ? "#EFF6FF" : "#FEE2E2";
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}