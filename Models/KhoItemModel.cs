using QuanLyQuanCaFe.Core;

namespace QuanLyQuanCaFe.Models
{
    public class KhoItemModel : BaseViewModel
    {
        public int MaNguyenLieu { get; set; }
        public string MaCodeNguyenLieu { get; set; }
        public string TenNguyenLieu { get; set; }
        public string TenDanhMuc { get; set; }
        public int MaDonVi { get; set; }
        public string TenDonVi { get; set; }
        public decimal SoLuongHienTai { get; set; }
        public decimal SoLuongToiThieu { get; set; }
        public decimal? SoLuongToiDa { get; set; }
        public decimal GiaNhapCuoi { get; set; }
        public string ViTriLuuKho { get; set; }
        public string HinhAnh { get; set; }
        public string GhiChu { get; set; }
        public bool ConHoatDong { get; set; }

        public string SoLuongText
        {
            get { return SoLuongHienTai.ToString("N0") + " " + TenDonVi; }
        }

        public string MucToiThieuText
        {
            get { return SoLuongToiThieu.ToString("N0") + " " + TenDonVi; }
        }

        public string GiaNhapText
        {
            get { return GiaNhapCuoi.ToString("N0") + "đ"; }
        }

        public string TrangThaiText
        {
            get
            {
                if (SoLuongHienTai <= 0)
                    return "Hết hàng";

                if (SoLuongHienTai <= SoLuongToiThieu)
                    return "Tồn thấp";

                return "Ổn định";
            }
        }

        public string TrangThaiBackground
        {
            get
            {
                if (SoLuongHienTai <= 0)
                    return "#FEE2E2";

                if (SoLuongHienTai <= SoLuongToiThieu)
                    return "#FEF3C7";

                return "#DCFCE7";
            }
        }

        public string TrangThaiColor
        {
            get
            {
                if (SoLuongHienTai <= 0)
                    return "#DC2626";

                if (SoLuongHienTai <= SoLuongToiThieu)
                    return "#D97706";

                return "#16A34A";
            }
        }

        public double PhanTramTon
        {
            get
            {
                if (SoLuongToiDa == null || SoLuongToiDa <= 0)
                    return SoLuongHienTai > 0 ? 55 : 0;

                var value = (double)(SoLuongHienTai / SoLuongToiDa.Value * 100);

                if (value > 100)
                    return 100;

                if (value < 0)
                    return 0;

                return value;
            }
        }

        public double DoRongThanhTon
        {
            get
            {
                return PhanTramTon * 1.8;
            }
        }
    }
}
