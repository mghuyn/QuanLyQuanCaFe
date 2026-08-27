namespace QuanLyQuanCaFe.Models
{
    public class NhaCC_Model
    {
        public string MaNhaCungCap { get; set; }
        public string TenNhaCungCap { get; set; }

        public override string ToString()
        {
            return TenNhaCungCap;
        }
    }
}