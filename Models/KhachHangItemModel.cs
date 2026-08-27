using QuanLyQuanCaFe.Core;
using System;

namespace QuanLyQuanCaFe.Models
{
    public class KhachHangItemModel : BaseViewModel
    {
        public int MaKH { get; set; }
        public string MaKhachHang { get; set; }
        public int MaHangKH { get; set; }
        public string TenHang { get; set; }

        public string HoTen { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string GioiTinh { get; set; }
        public string DiaChi { get; set; }

        public DateTime NgayThamGia { get; set; }
        public int DiemTichLuy { get; set; }
        public decimal TongChiTieu { get; set; }
        public DateTime? LanGheCuoi { get; set; }

        public string GhiChu { get; set; }
        public bool ConHoatDong { get; set; }

        public string TongChiTieuText
        {
            get { return TongChiTieu.ToString("N0") + "đ"; }
            set { }
        }

        public string DiemText
        {
            get { return DiemTichLuy.ToString("N0") + " điểm"; }
            set { }
        }

        public string NgayThamGiaText
        {
            get { return NgayThamGia.ToString("dd/MM/yyyy"); }
            set { }
        }

        public string LanGheCuoiText
        {
            get
            {
                if (LanGheCuoi == null)
                    return "Chưa có";

                return LanGheCuoi.Value.ToString("dd/MM/yyyy HH:mm");
            }
            set { }
        }

        public string TrangThaiText
        {
            get { return ConHoatDong ? "Đang hoạt động" : "Đã ẩn"; }
            set { }
        }

        public string TrangThaiBackground
        {
            get { return ConHoatDong ? "#DCFCE7" : "#FEE2E2"; }
            set { }
        }

        public string TrangThaiColor
        {
            get { return ConHoatDong ? "#16A34A" : "#DC2626"; }
            set { }
        }

        public string HangBackground
        {
            get
            {
                if (TenHang == null)
                    return "#F1F5F9";

                string ten = TenHang.ToLower();

                if (ten.Contains("vip") || ten.Contains("kim cương") || ten.Contains("diamond"))
                    return "#FEF3C7";

                if (ten.Contains("vàng") || ten.Contains("gold"))
                    return "#FEF3C7";

                if (ten.Contains("bạc") || ten.Contains("silver"))
                    return "#E0F2FE";

                return "#EEF2FF";
            }
            set { }
        }

        public string HangColor
        {
            get
            {
                if (TenHang == null)
                    return "#334155";

                string ten = TenHang.ToLower();

                if (ten.Contains("vip") || ten.Contains("kim cương") || ten.Contains("diamond"))
                    return "#B45309";

                if (ten.Contains("vàng") || ten.Contains("gold"))
                    return "#D97706";

                if (ten.Contains("bạc") || ten.Contains("silver"))
                    return "#0284C7";

                return "#4F46E5";
            }
            set { }
        }
    }
}