using QuanLyQuanCaFe.Core;
using System;

namespace QuanLyQuanCaFe.Models
{
    public class LoaiSanPhamQuanLyModel : BaseViewModel
    {
        public int MaDanhMuc { get; set; }
        public string MaCodeDanhMuc { get; set; }
        public string TenDanhMuc { get; set; }
        public string MoTa { get; set; }
        public int ThuTuHienThi { get; set; }
        public bool ConHoatDong { get; set; }

        public string TrangThaiText { get { return ConHoatDong ? "Đang dùng" : "Đã ẩn"; } set { } }
        public string TrangThaiBackground { get { return ConHoatDong ? "#DCFCE7" : "#FEE2E2"; } set { } }
        public string TrangThaiColor { get { return ConHoatDong ? "#16A34A" : "#DC2626"; } set { } }
    }

    public class NhaCungCapQuanLyModel : BaseViewModel
    {
        public int MaNCC { get; set; }
        public string MaNhaCungCap { get; set; }
        public string TenNhaCungCap { get; set; }
        public string NguoiLienHe { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public string MaSoThue { get; set; }
        public string DiaChi { get; set; }
        public string TaiKhoanNganHang { get; set; }
        public string GhiChu { get; set; }
        public bool ConHoatDong { get; set; }

        public string TrangThaiText { get { return ConHoatDong ? "Đang hợp tác" : "Ngừng hợp tác"; } set { } }
        public string TrangThaiBackground { get { return ConHoatDong ? "#DCFCE7" : "#FEE2E2"; } set { } }
        public string TrangThaiColor { get { return ConHoatDong ? "#16A34A" : "#DC2626"; } set { } }
    }

    public class DonViTinhQuanLyModel : BaseViewModel
    {
        public int MaDonVi { get; set; }
        public string MaCodeDonVi { get; set; }
        public string TenDonVi { get; set; }
        public string MoTa { get; set; }
        public DateTime NgayTao { get; set; }
        public string NgayTaoText { get { return NgayTao == DateTime.MinValue ? "" : NgayTao.ToString("dd/MM/yyyy"); } set { } }
    }
}
