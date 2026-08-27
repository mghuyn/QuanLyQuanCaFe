namespace QuanLyQuanCaFe.Models
{
    public class DashboardSummaryModel
    {
        public int SoSanPham { get; set; }
        public int SoNhanVien { get; set; }
        public int SoBan { get; set; }
        public int SoHoaDonHomNay { get; set; }
    }

    public class DashboardDoanhThuGioModel
    {
        public int Gio { get; set; }
        public string GioText { get; set; }
        public decimal DoanhThu { get; set; }
        public int SoHoaDon { get; set; }
        public double DoCaoCot { get; set; }

        public string DoanhThuText
        {
            get { return DoanhThu.ToString("N0") + "đ"; }
            set { }
        }

        public string SoHoaDonText
        {
            get { return SoHoaDon.ToString("N0") + " HĐ"; }
            set { }
        }
    }
}
