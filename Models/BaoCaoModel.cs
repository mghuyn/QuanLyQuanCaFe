using QuanLyQuanCaFe.Core;
using System;

namespace QuanLyQuanCaFe.Models
{
    public class BaoCaoTongQuanModel : BaseViewModel
    {
        public decimal DoanhThu { get; set; }
        public int SoHoaDon { get; set; }
        public int SoSanPhamBanRa { get; set; }
        public int SoKhachHang { get; set; }

        public string DoanhThuText
        {
            get { return DoanhThu.ToString("N0") + "đ"; }
            set { }
        }

        public string SoHoaDonText
        {
            get { return SoHoaDon.ToString("N0"); }
            set { }
        }

        public string SoSanPhamBanRaText
        {
            get { return SoSanPhamBanRa.ToString("N0"); }
            set { }
        }

        public string SoKhachHangText
        {
            get { return SoKhachHang.ToString("N0"); }
            set { }
        }
    }

    public class BaoCaoDoanhThuNgayModel : BaseViewModel
    {
        public DateTime Ngay { get; set; }
        public decimal DoanhThu { get; set; }
        public int SoHoaDon { get; set; }
        public double DoRongCot { get; set; }

        public string NgayText
        {
            get { return Ngay.ToString("dd/MM"); }
            set { }
        }

        public string DoanhThuText
        {
            get { return DoanhThu.ToString("N0") + "đ"; }
            set { }
        }
    }

    public class BaoCaoSanPhamBanChayModel : BaseViewModel
    {
        public string TenSanPham { get; set; }
        public string TenDanhMuc { get; set; }
        public int SoLuongBan { get; set; }
        public decimal DoanhThu { get; set; }
        public double DoRongCot { get; set; }

        public string SoLuongBanText
        {
            get { return SoLuongBan.ToString("N0") + " ly"; }
            set { }
        }

        public string DoanhThuText
        {
            get { return DoanhThu.ToString("N0") + "đ"; }
            set { }
        }
    }
}