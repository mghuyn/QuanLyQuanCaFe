using QuanLyQuanCaFe.Core;
using System;

namespace QuanLyQuanCaFe.Models
{
    public class NhanVienItemModel : BaseViewModel
    {
        public int MaNV { get; set; }
        public string MaNhanVien { get; set; }

        public string HoTen { get; set; }
        public string ChucVu { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }

        public DateTime? NgaySinh { get; set; }
        public string GioiTinh { get; set; }
        public string DiaChi { get; set; }

        public DateTime NgayVaoLam { get; set; }
        public decimal LuongCoBan { get; set; }

        public string GhiChu { get; set; }
        public bool ConHoatDong { get; set; }

        public string LuongText
        {
            get { return LuongCoBan.ToString("N0") + "đ"; }
            set { }
        }
        public string GioiTinhText
        {
            get
            {
                if (GioiTinh == "F") return "Nữ";
                if (GioiTinh == "M") return "Nam";
                if (GioiTinh == "O") return "Khác";
                return string.IsNullOrWhiteSpace(GioiTinh) ? "Chưa rõ" : GioiTinh;
            }
        }
        public string NgayVaoLamText
        {
            get { return NgayVaoLam.ToString("dd/MM/yyyy"); }
            set { }
        }

        public string NgaySinhText
        {
            get
            {
                if (NgaySinh == null)
                    return "Chưa có";

                return NgaySinh.Value.ToString("dd/MM/yyyy");
            }
            set { }
        }

        public string TrangThaiText
        {
            get { return ConHoatDong ? "Đang làm" : "Đã nghỉ"; }
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

        public string ChucVuBackground
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ChucVu))
                    return "#F1F5F9";

                string cv = ChucVu.ToLower();

                if (cv.Contains("quản") || cv.Contains("manager"))
                    return "#FEF3C7";

                if (cv.Contains("thu ngân") || cv.Contains("cashier"))
                    return "#DBEAFE";

                if (cv.Contains("pha chế") || cv.Contains("barista"))
                    return "#DCFCE7";

                if (cv.Contains("kho"))
                    return "#EDE9FE";

                return "#F1F5F9";
            }
            set { }
        }

        public string ChucVuColor
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ChucVu))
                    return "#334155";

                string cv = ChucVu.ToLower();

                if (cv.Contains("quản") || cv.Contains("manager"))
                    return "#B45309";

                if (cv.Contains("thu ngân") || cv.Contains("cashier"))
                    return "#1D4ED8";

                if (cv.Contains("pha chế") || cv.Contains("barista"))
                    return "#16A34A";

                if (cv.Contains("kho"))
                    return "#7C3AED";

                return "#334155";
            }
            set { }
        }
    }
}