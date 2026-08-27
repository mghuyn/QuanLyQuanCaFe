using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using QuanLyQuanCaFe.Models;

namespace QuanLyQuanCaFe.Services
{
    public class MenuManagementService
    {
        public List<string> GetDanhMucNames()
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                var data = db.DanhMucSanPhams
                    .Where(x => x.ConHoatDong)
                    .OrderBy(x => x.ThuTuHienThi)
                    .Select(x => x.TenDanhMuc)
                    .ToList();

                data.Insert(0, "Tất cả");
                return data;
            }
        }

        public List<MenuProductModel> GetSanPhams(string tuKhoa = "", string tenDanhMuc = "Tất cả")
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                var query = db.SanPhams.Select(x => new MenuProductModel
                {
                    MaSanPham = x.MaSanPham,
                    MaDanhMuc = x.MaDanhMuc,
                    TenSanPham = x.TenSanPham,
                    TenDanhMuc = x.DanhMucSanPham.TenDanhMuc,
                    MoTa = x.MoTa,
                    HinhAnh = x.DuongDanHinhAnh,
                    ConHoatDong = x.ConHoatDong && x.DangBan,

                    GiaBan = x.BienTheSanPhams
                        .Where(v => v.MacDinh)
                        .Select(v => v.GiaBan)
                        .FirstOrDefault()
                });

                if (!string.IsNullOrWhiteSpace(tuKhoa))
                {
                    query = query.Where(x => x.TenSanPham.Contains(tuKhoa));
                }

                if (!string.IsNullOrWhiteSpace(tenDanhMuc) && tenDanhMuc != "Tất cả")
                {
                    query = query.Where(x => x.TenDanhMuc == tenDanhMuc);
                }

                return query
                    .OrderBy(x => x.TenDanhMuc)
                    .ThenBy(x => x.TenSanPham)
                    .ToList();
            }
        }

        public void ThemSanPham(string tenSanPham, string tenDanhMuc, string moTa)
        {
            ThemSanPham(tenSanPham, tenDanhMuc, moTa, "", 1);
        }

        public void ThemSanPham(string tenSanPham, string tenDanhMuc, string moTa, string hinhAnh, decimal giaBan)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                if (string.IsNullOrWhiteSpace(tenSanPham))
                    throw new Exception("Tên sản phẩm không được để trống.");

                if (string.IsNullOrWhiteSpace(tenDanhMuc) || tenDanhMuc == "Tất cả")
                    throw new Exception("Vui lòng chọn danh mục.");

                if (giaBan <= 0)
                    throw new Exception("Giá bán phải lớn hơn 0.");

                var danhMuc = db.DanhMucSanPhams
                    .FirstOrDefault(x => x.TenDanhMuc == tenDanhMuc);

                if (danhMuc == null)
                    throw new Exception("Không tìm thấy danh mục.");

                var sanPham = db.SanPhams.Create();

                sanPham.MaCodeSanPham = TaoMaCodeSanPham();
                sanPham.MaDanhMuc = danhMuc.MaDanhMuc;
                sanPham.TenSanPham = tenSanPham.Trim();
                sanPham.MoTa = string.IsNullOrWhiteSpace(moTa) ? "" : moTa.Trim();
                sanPham.DuongDanHinhAnh = string.IsNullOrWhiteSpace(hinhAnh) ? "" : hinhAnh.Trim();
                sanPham.ConHoatDong = true;
                sanPham.DangBan = true;
                sanPham.NgayTao = DateTime.Now;
                sanPham.NgayCapNhat = DateTime.Now;

                db.SanPhams.Add(sanPham);
                SaveChangesWithValidationMessage(db);

                var bienThe = db.BienTheSanPhams.Create();

                bienThe.MaCodeBienThe = TaoMaCodeBienThe();
                bienThe.MaSanPham = sanPham.MaSanPham;

                // Nếu database của bạn có MaKichCo = 1 thì ổn.
                // Nếu lỗi khóa ngoại MaKichCo, gửi ảnh bảng KichCoSanPhams cho mình.
                bienThe.MaKichCo = 1;

                bienThe.MaSKU = sanPham.MaCodeSanPham + "-MD";
                bienThe.GiaBan = giaBan;
                bienThe.GiaVon = 0;
                bienThe.MaVach = null;
                bienThe.TenHienThi = "Mặc định";
                bienThe.MacDinh = true;
                bienThe.DangBan = true;
                bienThe.ConHoatDong = true;
                bienThe.NgayTao = DateTime.Now;
                bienThe.NgayCapNhat = DateTime.Now;

                db.BienTheSanPhams.Add(bienThe);
                SaveChangesWithValidationMessage(db);
            }
        }

        public void CapNhatSanPham(int maSanPham, string tenSanPham, string tenDanhMuc, string moTa)
        {
            CapNhatSanPham(maSanPham, tenSanPham, tenDanhMuc, moTa, "", 0);
        }

        public void CapNhatSanPham(int maSanPham, string tenSanPham, string tenDanhMuc, string moTa, string hinhAnh, decimal giaBan)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                if (string.IsNullOrWhiteSpace(tenSanPham))
                    throw new Exception("Tên sản phẩm không được để trống.");

                if (string.IsNullOrWhiteSpace(tenDanhMuc) || tenDanhMuc == "Tất cả")
                    throw new Exception("Vui lòng chọn danh mục.");

                if (giaBan <= 0)
                    throw new Exception("Giá bán phải lớn hơn 0.");

                var sanPham = db.SanPhams
                    .FirstOrDefault(x => x.MaSanPham == maSanPham);

                if (sanPham == null)
                    throw new Exception("Không tìm thấy sản phẩm.");

                var danhMuc = db.DanhMucSanPhams
                    .FirstOrDefault(x => x.TenDanhMuc == tenDanhMuc);

                if (danhMuc == null)
                    throw new Exception("Không tìm thấy danh mục.");

                sanPham.MaDanhMuc = danhMuc.MaDanhMuc;
                sanPham.TenSanPham = tenSanPham.Trim();
                sanPham.MoTa = string.IsNullOrWhiteSpace(moTa) ? "" : moTa.Trim();

                if (!string.IsNullOrWhiteSpace(hinhAnh))
                    sanPham.DuongDanHinhAnh = hinhAnh.Trim();

                sanPham.NgayCapNhat = DateTime.Now;

                var bienThe = db.BienTheSanPhams
                    .FirstOrDefault(x => x.MaSanPham == maSanPham && x.MacDinh);

                if (bienThe == null)
                {
                    bienThe = db.BienTheSanPhams.Create();

                    bienThe.MaCodeBienThe = TaoMaCodeBienThe();
                    bienThe.MaSanPham = sanPham.MaSanPham;
                    bienThe.MaKichCo = 1;
                    bienThe.MaSKU = sanPham.MaCodeSanPham + "-MD";
                    bienThe.GiaVon = 0;
                    bienThe.MaVach = null;
                    bienThe.TenHienThi = "Mặc định";
                    bienThe.MacDinh = true;
                    bienThe.DangBan = true;
                    bienThe.ConHoatDong = true;
                    bienThe.NgayTao = DateTime.Now;

                    db.BienTheSanPhams.Add(bienThe);
                }

                bienThe.GiaBan = giaBan;
                bienThe.DangBan = sanPham.ConHoatDong;
                bienThe.ConHoatDong = true;
                bienThe.NgayCapNhat = DateTime.Now;

                SaveChangesWithValidationMessage(db);
            }
        }

        public void DoiTrangThaiSanPham(int maSanPham)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                var sanPham = db.SanPhams
                    .FirstOrDefault(x => x.MaSanPham == maSanPham);

                if (sanPham == null)
                    throw new Exception("Không tìm thấy sản phẩm.");

                bool trangThaiMoi = !(sanPham.ConHoatDong && sanPham.DangBan);

                sanPham.ConHoatDong = trangThaiMoi;
                sanPham.DangBan = trangThaiMoi;
                sanPham.NgayCapNhat = DateTime.Now;

                var bienThes = db.BienTheSanPhams
                    .Where(x => x.MaSanPham == maSanPham)
                    .ToList();

                foreach (var bienThe in bienThes)
                {
                    bienThe.DangBan = trangThaiMoi;
                    bienThe.ConHoatDong = trangThaiMoi;
                    bienThe.NgayCapNhat = DateTime.Now;
                }

                SaveChangesWithValidationMessage(db);
            }
        }

        private string TaoMaCodeSanPham()
        {
            return "SP" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }

        private string TaoMaCodeBienThe()
        {
            return "BT" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }

        private void SaveChangesWithValidationMessage(QuanLyQuanCaPheDbEntities1 db)
        {
            try
            {
                db.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                var sb = new StringBuilder();

                foreach (var eve in ex.EntityValidationErrors)
                {
                    sb.AppendLine("Entity: " + eve.Entry.Entity.GetType().Name);

                    foreach (var ve in eve.ValidationErrors)
                    {
                        sb.AppendLine("- " + ve.PropertyName + ": " + ve.ErrorMessage);
                    }
                }

                throw new Exception(sb.ToString());
            }
        }
    }
}