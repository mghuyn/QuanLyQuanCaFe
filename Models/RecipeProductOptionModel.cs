namespace QuanLyQuanCaFe.Models
{
    public class RecipeProductOptionModel
    {
        public int MaBienThe { get; set; }
        public int MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public string TenDanhMuc { get; set; }
        public string TenHienThi { get; set; }
        public decimal GiaBan { get; set; }

        public string HienThi
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(TenDanhMuc))
                    return TenSanPham + " - " + TenDanhMuc;

                return TenSanPham;
            }
        }
    }
}
