using System.Collections.ObjectModel;
using QuanLyQuanCaFe.Core;

namespace QuanLyQuanCaFe.Models
{
    public class KhuVucBanGroupModel : BaseViewModel
    {
        public string TenKhuVuc { get; set; }
        public string TenTang { get; set; }
        public int ThuTuKhuVuc { get; set; }
        public ObservableCollection<TableCardModel> BanTrongKhuVuc { get; set; }

        public KhuVucBanGroupModel()
        {
            BanTrongKhuVuc = new ObservableCollection<TableCardModel>();
        }

        public int TongBan
        {
            get { return BanTrongKhuVuc == null ? 0 : BanTrongKhuVuc.Count; }
            set { }
        }

        private string RutGonTenKhuVuc(string ten)
        {
            if (string.IsNullOrWhiteSpace(ten)) return "Khu vực";
            string u = ten.Trim().ToUpper();

            if (u.Contains("GROUND FLOOR A") || u.Contains("TẦNG TRỆT A")) return "Tầng trệt A";
            if (u.Contains("GROUND FLOOR B") || u.Contains("TẦNG TRỆT B")) return "Tầng trệt B";
            if (u.Contains("FIRST FLOOR") || u.Contains("LẦU 1C") || u.Contains("LẦU 1")) return "Lầu 1C";
            if (u.Contains("GARDEN") || u.Contains("SÂN VƯỜN") || u.Contains("NGOÀI TRỜI")) return "Sân vườn";

            return ten.Trim();
        }

        public string TieuDeKhuVuc
        {
            get { return RutGonTenKhuVuc(TenKhuVuc); }
            set { }
        }
    }
}
