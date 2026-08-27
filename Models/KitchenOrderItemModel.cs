using System;

namespace QuanLyQuanCaFe.Models
{
    public class KitchenOrderItemModel
    {
        public int MaChiTietHoaDonBan { get; set; }
        public string MaChiTietHoaDonBanList { get; set; }
        public int MaHoaDonBan { get; set; }
        public int MaBienThe { get; set; }
        public string MaHoaDon { get; set; }
        public string MaHoaDonHienThi
        {
            get
            {
                if (string.IsNullOrWhiteSpace(MaHoaDon)) return "HD mới";
                string so = "";
                foreach (char c in MaHoaDon) if (char.IsDigit(c)) so += c;
                if (so.Length >= 6) return "HD" + so.Substring(so.Length - 6);
                return MaHoaDon;
            }
        }
        public string TenMon { get; set; }
        public int SoLuong { get; set; }
        public string TrangThaiMon { get; set; }
        public string GhiChu { get; set; }
        public string GhiChuHoaDon { get; set; }
        public string TenBan { get; set; }
        public string LoaiHoaDon { get; set; }
        public DateTime NgayLapHoaDon { get; set; }
        public DateTime? BatDauLuc { get; set; }
        public DateTime? HoanThanhLuc { get; set; }

        public string SoLuongText => "x" + SoLuong;
        public string ThoiGianText => NgayLapHoaDon.ToString("HH:mm");
        public string ViTriText
        {
            get
            {
                if (LoaiHoaDon == "DINE_IN" && !string.IsNullOrWhiteSpace(TenBan)) return TenBan;
                return "Mang về";
            }
        }

        public string GhiChuMonText
        {
            get { return string.IsNullOrWhiteSpace(GhiChu) ? "Không có ghi chú món" : GhiChu; }
        }

        public string GhiChuHoaDonText
        {
            get { return string.IsNullOrWhiteSpace(GhiChuHoaDon) ? "Không có ghi chú hóa đơn" : GhiChuHoaDon; }
        }

        public string CongThucText { get; set; }

        public string CongThucHienThi
        {
            get { return string.IsNullOrWhiteSpace(CongThucText) ? "Chưa khai báo công thức" : CongThucText; }
        }

        public string TrangThaiText
        {
            get
            {
                switch (TrangThaiMon)
                {
                    case "NEW": return "Chờ pha chế";
                    case "DOING": return "Đang làm";
                    case "PREPARING": return "Đang làm";
                    case "DONE": return "Hoàn tất";
                    case "CANCELLED": return "Đã hủy";
                    default: return string.IsNullOrWhiteSpace(TrangThaiMon) ? "Không rõ" : TrangThaiMon;
                }
            }
        }

        public string TrangThaiBackground
        {
            get
            {
                switch (TrangThaiMon)
                {
                    case "NEW": return "#FEF3C7";
                    case "DOING": return "#DBEAFE";
                    case "PREPARING": return "#DBEAFE";
                    case "DONE": return "#DCFCE7";
                    case "CANCELLED": return "#FEE2E2";
                    default: return "#F1F5F9";
                }
            }
        }

        public string TrangThaiColor
        {
            get
            {
                switch (TrangThaiMon)
                {
                    case "NEW": return "#D97706";
                    case "DOING": return "#1D4ED8";
                    case "PREPARING": return "#1D4ED8";
                    case "DONE": return "#16A34A";
                    case "CANCELLED": return "#DC2626";
                    default: return "#334155";
                }
            }
        }
    }
}
