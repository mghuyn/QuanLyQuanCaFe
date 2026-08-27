using QuanLyQuanCaFe.Core;

namespace QuanLyQuanCaFe.Models
{
    public class TableCardModel : BaseViewModel
    {
        private bool _isSelected;

        public int MaBan { get; set; }
        public string MaBanText { get; set; }
        public string MaCodeBan { get; set; }
        public string TenBan { get; set; }
        public string TenKhuVuc { get; set; }
        public string TenTang { get; set; }
        public int ThuTuKhuVuc { get; set; }
        public int SoThuTuBan { get; set; }
        public int SoGhe { get; set; }
        public string TrangThaiBan { get; set; }
        public string GhiChu { get; set; }

        public int? MaHoaDonBanDangPhucVu { get; set; }
        public string MaHoaDonDangPhucVu { get; set; }
        public string TenKhachDangPhucVu { get; set; }
        public decimal TongTienDangPhucVu { get; set; }
        public int SoMonDangPhucVu { get; set; }
        public string GhiChuHoaDonDangPhucVu { get; set; }

        public bool CoHoaDonDangPhucVu
        {
            get { return MaHoaDonBanDangPhucVu.HasValue && MaHoaDonBanDangPhucVu.Value > 0; }
            set { }
        }

        public string MaHoaDonHienThi
        {
            get
            {
                if (string.IsNullOrWhiteSpace(MaHoaDonDangPhucVu)) return "Chưa có hóa đơn";
                string so = "";
                foreach (char c in MaHoaDonDangPhucVu) if (char.IsDigit(c)) so += c;
                if (so.Length >= 6) return "HD" + so.Substring(so.Length - 6);
                return MaHoaDonDangPhucVu;
            }
            set { }
        }

        public string KhachDangPhucVuText
        {
            get { return string.IsNullOrWhiteSpace(TenKhachDangPhucVu) ? "Khách lẻ" : TenKhachDangPhucVu; }
            set { }
        }

        public string TongTienDangPhucVuText
        {
            get { return TongTienDangPhucVu.ToString("N0") + "đ"; }
            set { }
        }

        public string ThongTinBillText
        {
            get
            {
                if (!CoHoaDonDangPhucVu) return "Chưa có bill đang phục vụ";
                return MaHoaDonHienThi + " • " + KhachDangPhucVuText + " • " + TongTienDangPhucVuText;
            }
            set { }
        }


        private string RutGonTenKhuVuc(string ten)
        {
            if (string.IsNullOrWhiteSpace(ten))
                return "Khu vực";

            string u = ten.Trim().ToUpper();

            if (u.Contains("GROUND FLOOR A") || u.Contains("TẦNG TRỆT A"))
                return "Tầng trệt A";

            if (u.Contains("GROUND FLOOR B") || u.Contains("TẦNG TRỆT B"))
                return "Tầng trệt B";

            if (u.Contains("FIRST FLOOR") || u.Contains("LẦU 1C") || u.Contains("LẦU 1"))
                return "Lầu 1C";

            if (u.Contains("GARDEN") || u.Contains("SÂN VƯỜN") || u.Contains("NGOÀI TRỜI"))
                return "Sân vườn";

            return ten.Trim();
        }

        private string LayPrefixTheoKhuVuc()
        {
            string kv = RutGonTenKhuVuc(TenKhuVuc).ToUpper();

            if (kv.Contains("TẦNG TRỆT A"))
                return "A";

            if (kv.Contains("TẦNG TRỆT B"))
                return "B";

            if (kv.Contains("LẦU 1C") || kv.Contains("LẦU 1"))
                return "1C";

            if (kv.Contains("SÂN VƯỜN"))
                return "G";

            return "";
        }

        public string TenKhuVucHienThi
        {
            get { return RutGonTenKhuVuc(TenKhuVuc); }
            set { }
        }

        public string TenBanRutGon
        {
            get
            {
                string prefix = LayPrefixTheoKhuVuc();

                if (!string.IsNullOrWhiteSpace(prefix) && SoThuTuBan > 0 && SoThuTuBan < 9999)
                    return "Bàn " + prefix + SoThuTuBan.ToString();

                if (!string.IsNullOrWhiteSpace(MaCodeBan))
                {
                    string code = MaCodeBan.Trim();
                    return code.StartsWith("Bàn") ? code : "Bàn " + code;
                }

                return string.IsNullOrWhiteSpace(TenBan) ? "Bàn" : TenBan.Trim();
            }
            set { }
        }

        public string TrangThaiText
        {
            get
            {
                switch (TrangThaiBan)
                {
                    case "AVAILABLE": return "Trống";
                    case "OCCUPIED": return "Đang phục vụ";
                    case "RESERVED": return "Đã đặt";
                    case "CLEANING": return "Cần dọn";
                    case "INACTIVE": return "Ngưng dùng";
                    default: return string.IsNullOrWhiteSpace(TrangThaiBan) ? "Trống" : TrangThaiBan;
                }
            }
        }

        public string BackgroundColor
        {
            get
            {
                switch (TrangThaiBan)
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

        public string BorderColor
        {
            get
            {
                switch (TrangThaiBan)
                {
                    case "AVAILABLE": return "#22C55E";
                    case "OCCUPIED": return "#2563EB";
                    case "RESERVED": return "#F59E0B";
                    case "CLEANING": return "#F97316";
                    case "INACTIVE": return "#94A3B8";
                    default: return "#CBD5E1";
                }
            }
        }

        public string ForegroundColor
        {
            get
            {
                switch (TrangThaiBan)
                {
                    case "AVAILABLE": return "#166534";
                    case "OCCUPIED": return "#1D4ED8";
                    case "RESERVED": return "#92400E";
                    case "CLEANING": return "#C2410C";
                    case "INACTIVE": return "#475569";
                    default: return "#334155";
                }
            }
        }

        public string MoTaXuLy
        {
            get
            {
                switch (TrangThaiBan)
                {
                    case "AVAILABLE": return "Có thể mở bill mới";
                    case "RESERVED": return "Đã giữ bàn, có thể mở bill";
                    case "OCCUPIED": return "Đang có khách / bill đang phục vụ";
                    case "CLEANING": return "Cần dọn trước khi nhận khách mới";
                    case "INACTIVE": return "Tạm ngưng sử dụng";
                    default: return "Theo dõi trạng thái bàn";
                }
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}
