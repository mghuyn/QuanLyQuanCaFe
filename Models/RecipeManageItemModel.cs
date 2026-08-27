namespace QuanLyQuanCaFe.Models
{
    public class RecipeManageItemModel
    {
        public int MaBienThe { get; set; }
        public int MaSanPham { get; set; }
        public int MaNguyenLieu { get; set; }
        public string TenSanPham { get; set; }
        public string TenDanhMuc { get; set; }
        public string TenNguyenLieu { get; set; }
        public string TenDonVi { get; set; }
        public decimal SoLuongCan { get; set; }
        public decimal PhanTramHaoHut { get; set; }
        public string GhiChu { get; set; }

        public string DinhLuongText => SoLuongCan.ToString("N3") + " " + TenDonVi;
        public string HaoHutText => PhanTramHaoHut.ToString("N1") + "%";
        public string MonText => TenSanPham + " - " + TenDanhMuc;
    }
}
