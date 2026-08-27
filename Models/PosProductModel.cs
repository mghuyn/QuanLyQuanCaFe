using QuanLyQuanCaFe.Core;
using System;
using System.Linq;

namespace QuanLyQuanCaFe.Models
{
    public class PosProductModel
    {
        public int MaSanPham { get; set; }
        public int MaBienThe { get; set; }
        public string TenSanPham { get; set; }
        public string TenDanhMuc { get; set; }
        public string TenSize { get; set; }
        public decimal GiaBan { get; set; }
        public string HinhAnh { get; set; }
        public bool DangBan { get; set; }

        public string TenHienThi
        {
            get
            {
                if (string.IsNullOrWhiteSpace(TenSize))
                    return TenSanPham;

                return TenSanPham + " - " + TenSize;
            }
        }

        public string GiaBanText => GiaBan.ToString("N0") + "đ";

        public string TrangThaiText => DangBan ? "Còn bán" : "Ngưng bán";
        public string TrangThaiBackground => DangBan ? "#DCFCE7" : "#FEE2E2";
        public string TrangThaiColor => DangBan ? "#16A34A" : "#DC2626";
    }

    public class PosTableModel
    {
        public int MaBan { get; set; }
        public string TenBan { get; set; }
        public string MaCodeBan { get; set; }
        public string TenKhuVuc { get; set; }
        public int SoGhe { get; set; }
        public string TrangThai { get; set; }
        public int SoThuTuBan
        {
            get
            {
                string raw = (MaCodeBan ?? "") + " " + (TenBan ?? "");
                string digits = new string(raw.Where(char.IsDigit).ToArray());
                int n;
                return int.TryParse(digits, out n) ? n : 9999;
            }
        }
        public string TenHienThi => string.IsNullOrWhiteSpace(MaCodeBan) ? TenBan : TenBan + " • " + MaCodeBan;
        public bool CoTheChon => TrangThai == "AVAILABLE" || string.IsNullOrWhiteSpace(TrangThai);

        public string TrangThaiText
        {
            get
            {
                switch (TrangThai)
                {
                    case "AVAILABLE": return "Trống";
                    case "OCCUPIED": return "Đang phục vụ";
                    case "RESERVED": return "Đã đặt";
                    case "CLEANING": return "Cần dọn";
                    case "INACTIVE": return "Ngưng dùng";
                    default: return string.IsNullOrWhiteSpace(TrangThai) ? "Trống" : TrangThai;
                }
            }
        }

        public string TrangThaiBackground
        {
            get
            {
                switch (TrangThai)
                {
                    case "AVAILABLE": return "#DCFCE7";
                    case "OCCUPIED": return "#DBEAFE";
                    case "RESERVED": return "#FEF3C7";
                    case "CLEANING": return "#FFEDD5";
                    case "INACTIVE": return "#E5E7EB";
                    default: return "#F8FAFC";
                }
            }
        }

        public string TrangThaiColor
        {
            get
            {
                switch (TrangThai)
                {
                    case "AVAILABLE": return "#16A34A";
                    case "OCCUPIED": return "#1D4ED8";
                    case "RESERVED": return "#D97706";
                    case "CLEANING": return "#F97316";
                    case "INACTIVE": return "#475569";
                    default: return "#334155";
                }
            }
        }
    }



    public class PosTableGroupModel
    {
        public string TenKhuVuc { get; set; }
        public System.Collections.ObjectModel.ObservableCollection<PosTableModel> BanTrongKhuVuc { get; set; }

        public PosTableGroupModel()
        {
            BanTrongKhuVuc = new System.Collections.ObjectModel.ObservableCollection<PosTableModel>();
        }
    }

    public class PosCustomerModel
    {
        public int MaKH { get; set; }
        public string MaKhachHang { get; set; }
        public string HoTen { get; set; }
        public string SoDienThoai { get; set; }
        public int DiemTichLuy { get; set; }

        public string TenHienThi
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SoDienThoai))
                    return HoTen;

                return HoTen + " • " + SoDienThoai;
            }
        }
    }

    public class PosHoaDonHistoryModel : BaseViewModel
    {
        public int MaHoaDonBan { get; set; }
        public string MaHoaDon { get; set; }
        public DateTime NgayLapHoaDon { get; set; }
        public string TenBan { get; set; }
        public string TenKhachHang { get; set; }
        public string LoaiHoaDon { get; set; }
        public string TrangThaiHoaDon { get; set; }
        public string TrangThaiThanhToan { get; set; }
        public decimal TongTien { get; set; }
        public string GhiChu { get; set; }
        public int TongSoMon { get; set; }

        public string NgayLapText => NgayLapHoaDon.ToString("dd/MM/yyyy HH:mm");
        public string GioText => NgayLapHoaDon.ToString("HH:mm");

        public string MaHoaDonHienThi
        {
            get
            {
                if (string.IsNullOrWhiteSpace(MaHoaDon))
                    return "HD mới";

                string so = new string(MaHoaDon.Where(char.IsDigit).ToArray());

                if (so.Length >= 6)
                    return "HD" + so.Substring(so.Length - 6);

                return MaHoaDon;
            }
        }

        public string TongTienText => TongTien.ToString("N0") + "đ";
        public string TongSoMonText => TongSoMon.ToString("N0") + " món";

        public string LoaiHoaDonText
        {
            get
            {
                if (LoaiHoaDon == "DINE_IN") return string.IsNullOrWhiteSpace(TenBan) ? "Ngồi lại" : TenBan;
                if (LoaiHoaDon == "TAKE_AWAY") return "Mang về";
                return LoaiHoaDon;
            }
        }

        public string TrangThaiText
        {
            get
            {
                switch (TrangThaiHoaDon)
                {
                    case "DRAFT": return "Lưu tạm";
                    case "WAITING_KITCHEN": return "Chờ pha chế";
                    case "PREPARING": return "Đang pha chế";
                    case "READY": return "Sẵn sàng";
                    case "COMPLETED": return "Đã thanh toán";
                    case "CANCELLED": return "Đã hủy";
                    default: return string.IsNullOrWhiteSpace(TrangThaiHoaDon) ? "Không rõ" : TrangThaiHoaDon;
                }
            }
        }

        public string TrangThaiBackground
        {
            get
            {
                switch (TrangThaiHoaDon)
                {
                    case "COMPLETED": return "#DCFCE7";
                    case "CANCELLED": return "#FEE2E2";
                    case "WAITING_KITCHEN": return "#FEF3C7";
                    case "DOING": return "#DBEAFE";
                    case "PREPARING": return "#DBEAFE";
                    case "READY": return "#E0F2FE";
                    default: return "#F1F5F9";
                }
            }
        }

        public string TrangThaiColor
        {
            get
            {
                switch (TrangThaiHoaDon)
                {
                    case "COMPLETED": return "#16A34A";
                    case "CANCELLED": return "#DC2626";
                    case "WAITING_KITCHEN": return "#D97706";
                    case "DOING": return "#1D4ED8";
                    case "PREPARING": return "#1D4ED8";
                    case "READY": return "#0284C7";
                    default: return "#334155";
                }
            }
        }

        public bool CoTheHuy
        {
            get { return TrangThaiHoaDon == "DRAFT"; }
            set { }
        }

        public bool CoTheThanhToan
        {
            get
            {
                return TrangThaiHoaDon != "COMPLETED"
                    && TrangThaiHoaDon != "CANCELLED"
                    && TrangThaiThanhToan != "PAID";
            }
            set { }
        }
    }

    public class PosHoaDonDetailItemModel
    {
        public int MaChiTietHoaDonBan { get; set; }
        public string TenSanPham { get; set; }
        public string TenSize { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }
        public string TrangThaiMon { get; set; }
        public string GhiChu { get; set; }

        public string TenHienThi
        {
            get
            {
                if (string.IsNullOrWhiteSpace(TenSize))
                    return TenSanPham;

                return TenSanPham + " - " + TenSize;
            }
        }

        public string SoLuongText => "x" + SoLuong;
        public string DonGiaText => DonGia.ToString("N0") + "đ";
        public string ThanhTienText => ThanhTien.ToString("N0") + "đ";

        public string TrangThaiMonText
        {
            get
            {
                switch (TrangThaiMon)
                {
                    case "DRAFT": return "Lưu tạm";
                    case "NEW": return "Chờ pha chế";
                    case "DOING": return "Đang làm";
                    case "PREPARING": return "Đang làm";
                    case "DONE": return "Hoàn tất";
                    case "CANCELLED": return "Đã hủy";
                    default: return string.IsNullOrWhiteSpace(TrangThaiMon) ? "Không rõ" : TrangThaiMon;
                }
            }
        }

        public string TrangThaiMonBackground
        {
            get
            {
                switch (TrangThaiMon)
                {
                    case "DONE": return "#DCFCE7";
                    case "DOING": return "#DBEAFE";
                    case "PREPARING": return "#DBEAFE";
                    case "NEW": return "#FEF3C7";
                    case "CANCELLED": return "#FEE2E2";
                    case "DRAFT": return "#F1F5F9";
                    default: return "#F1F5F9";
                }
            }
            set { }
        }

        public string TrangThaiMonColor
        {
            get
            {
                switch (TrangThaiMon)
                {
                    case "DONE": return "#16A34A";
                    case "DOING": return "#1D4ED8";
                    case "PREPARING": return "#1D4ED8";
                    case "NEW": return "#D97706";
                    case "CANCELLED": return "#DC2626";
                    case "DRAFT": return "#334155";
                    default: return "#334155";
                }
            }
            set { }
        }
    }
}
