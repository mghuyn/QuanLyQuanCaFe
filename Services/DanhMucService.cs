using QuanLyQuanCaFe.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuanLyQuanCaFe.Services
{
    public class DanhMucService
    {
        public List<LoaiSanPhamQuanLyModel> GetLoaiSanPhams(string tuKhoa, string trangThai)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                var query = db.DanhMucSanPhams.Select(x => new LoaiSanPhamQuanLyModel
                {
                    MaDanhMuc = x.MaDanhMuc,
                    MaCodeDanhMuc = x.MaCodeDanhMuc,
                    TenDanhMuc = x.TenDanhMuc,
                    MoTa = x.MoTa,
                    ThuTuHienThi = x.ThuTuHienThi,
                    ConHoatDong = x.ConHoatDong
                });

                if (!string.IsNullOrWhiteSpace(tuKhoa))
                {
                    query = query.Where(x => x.MaCodeDanhMuc.Contains(tuKhoa) || x.TenDanhMuc.Contains(tuKhoa));
                }

                if (trangThai == "Đang dùng") query = query.Where(x => x.ConHoatDong);
                else if (trangThai == "Đã ẩn") query = query.Where(x => !x.ConHoatDong);

                return query.OrderByDescending(x => x.ConHoatDong).ThenBy(x => x.ThuTuHienThi).ThenBy(x => x.TenDanhMuc).ToList();
            }
        }

        public string TaoMaDanhMuc()
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                int stt = db.DanhMucSanPhams.Count() + 1;
                string ma;
                do { ma = "DM" + stt.ToString("000"); stt++; }
                while (db.DanhMucSanPhams.Any(x => x.MaCodeDanhMuc == ma));
                return ma;
            }
        }

        public void ThemLoaiSanPham(LoaiSanPhamQuanLyModel model)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                if (model == null) throw new Exception("Dữ liệu loại sản phẩm không hợp lệ.");
                if (string.IsNullOrWhiteSpace(model.TenDanhMuc)) throw new Exception("Vui lòng nhập tên loại sản phẩm.");
                if (db.DanhMucSanPhams.Any(x => x.TenDanhMuc == model.TenDanhMuc.Trim())) throw new Exception("Tên loại sản phẩm đã tồn tại.");

                var dm = new DanhMucSanPham();
                dm.MaCodeDanhMuc = string.IsNullOrWhiteSpace(model.MaCodeDanhMuc) ? TaoMaDanhMuc() : model.MaCodeDanhMuc.Trim();
                dm.TenDanhMuc = model.TenDanhMuc.Trim();
                dm.MoTa = model.MoTa;
                dm.ThuTuHienThi = model.ThuTuHienThi;
                dm.ConHoatDong = true;
                dm.NgayTao = DateTime.Now;
                dm.NgayCapNhat = DateTime.Now;
                db.DanhMucSanPhams.Add(dm);
                db.SaveChanges();
            }
        }

        public void CapNhatLoaiSanPham(LoaiSanPhamQuanLyModel model)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                if (model == null || model.MaDanhMuc <= 0) throw new Exception("Vui lòng chọn loại sản phẩm cần cập nhật.");
                if (string.IsNullOrWhiteSpace(model.TenDanhMuc)) throw new Exception("Vui lòng nhập tên loại sản phẩm.");
                if (db.DanhMucSanPhams.Any(x => x.TenDanhMuc == model.TenDanhMuc.Trim() && x.MaDanhMuc != model.MaDanhMuc)) throw new Exception("Tên loại sản phẩm đã tồn tại.");

                var dm = db.DanhMucSanPhams.FirstOrDefault(x => x.MaDanhMuc == model.MaDanhMuc);
                if (dm == null) throw new Exception("Không tìm thấy loại sản phẩm.");

                dm.TenDanhMuc = model.TenDanhMuc.Trim();
                dm.MoTa = model.MoTa;
                dm.ThuTuHienThi = model.ThuTuHienThi;
                dm.NgayCapNhat = DateTime.Now;
                db.SaveChanges();
            }
        }

        public void DoiTrangThaiLoaiSanPham(int maDanhMuc)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                var dm = db.DanhMucSanPhams.FirstOrDefault(x => x.MaDanhMuc == maDanhMuc);
                if (dm == null) throw new Exception("Không tìm thấy loại sản phẩm.");
                dm.ConHoatDong = !dm.ConHoatDong;
                dm.NgayCapNhat = DateTime.Now;
                db.SaveChanges();
            }
        }

        public List<NhaCungCapQuanLyModel> GetNhaCungCaps(string tuKhoa, string trangThai)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                var query = db.NhaCungCaps.Select(x => new NhaCungCapQuanLyModel
                {
                    MaNCC = x.MaNCC,
                    MaNhaCungCap = x.MaNhaCungCap,
                    TenNhaCungCap = x.TenNhaCungCap,
                    NguoiLienHe = x.NguoiLienHe,
                    SoDienThoai = x.SoDienThoai,
                    Email = x.Email,
                    MaSoThue = x.MaSoThue,
                    DiaChi = x.DiaChi,
                    TaiKhoanNganHang = x.TaiKhoanNganHang,
                    GhiChu = x.GhiChu,
                    ConHoatDong = x.ConHoatDong
                });

                if (!string.IsNullOrWhiteSpace(tuKhoa))
                {
                    query = query.Where(x => x.MaNhaCungCap.Contains(tuKhoa) || x.TenNhaCungCap.Contains(tuKhoa) || x.SoDienThoai.Contains(tuKhoa) || x.Email.Contains(tuKhoa));
                }

                if (trangThai == "Đang hợp tác") query = query.Where(x => x.ConHoatDong);
                else if (trangThai == "Ngừng hợp tác") query = query.Where(x => !x.ConHoatDong);

                return query.OrderByDescending(x => x.ConHoatDong).ThenBy(x => x.TenNhaCungCap).ToList();
            }
        }

        public string TaoMaNhaCungCap()
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                int stt = db.NhaCungCaps.Count() + 1;
                string ma;
                do { ma = "NCC" + stt.ToString("000"); stt++; }
                while (db.NhaCungCaps.Any(x => x.MaNhaCungCap == ma));
                return ma;
            }
        }

        public void ThemNhaCungCap(NhaCungCapQuanLyModel model)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                if (model == null) throw new Exception("Dữ liệu nhà cung cấp không hợp lệ.");
                if (string.IsNullOrWhiteSpace(model.TenNhaCungCap)) throw new Exception("Vui lòng nhập tên nhà cung cấp.");
                if (string.IsNullOrWhiteSpace(model.SoDienThoai)) throw new Exception("Vui lòng nhập số điện thoại nhà cung cấp.");
                if (db.NhaCungCaps.Any(x => x.SoDienThoai == model.SoDienThoai.Trim())) throw new Exception("Số điện thoại nhà cung cấp đã tồn tại.");

                var ncc = new NhaCungCap();
                ncc.MaNhaCungCap = string.IsNullOrWhiteSpace(model.MaNhaCungCap) ? TaoMaNhaCungCap() : model.MaNhaCungCap.Trim();
                ncc.TenNhaCungCap = model.TenNhaCungCap.Trim();
                ncc.NguoiLienHe = model.NguoiLienHe;
                ncc.SoDienThoai = model.SoDienThoai.Trim();
                ncc.Email = model.Email;
                ncc.MaSoThue = model.MaSoThue;
                ncc.DiaChi = model.DiaChi;
                ncc.TaiKhoanNganHang = model.TaiKhoanNganHang;
                ncc.GhiChu = model.GhiChu;
                ncc.ConHoatDong = true;
                ncc.NgayTao = DateTime.Now;
                ncc.NgayCapNhat = DateTime.Now;
                db.NhaCungCaps.Add(ncc);
                db.SaveChanges();
            }
        }

        public void CapNhatNhaCungCap(NhaCungCapQuanLyModel model)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                if (model == null || model.MaNCC <= 0) throw new Exception("Vui lòng chọn nhà cung cấp cần cập nhật.");
                if (string.IsNullOrWhiteSpace(model.TenNhaCungCap)) throw new Exception("Vui lòng nhập tên nhà cung cấp.");
                if (string.IsNullOrWhiteSpace(model.SoDienThoai)) throw new Exception("Vui lòng nhập số điện thoại nhà cung cấp.");
                if (db.NhaCungCaps.Any(x => x.SoDienThoai == model.SoDienThoai.Trim() && x.MaNCC != model.MaNCC)) throw new Exception("Số điện thoại này đã thuộc nhà cung cấp khác.");

                var ncc = db.NhaCungCaps.FirstOrDefault(x => x.MaNCC == model.MaNCC);
                if (ncc == null) throw new Exception("Không tìm thấy nhà cung cấp.");
                ncc.TenNhaCungCap = model.TenNhaCungCap.Trim();
                ncc.NguoiLienHe = model.NguoiLienHe;
                ncc.SoDienThoai = model.SoDienThoai.Trim();
                ncc.Email = model.Email;
                ncc.MaSoThue = model.MaSoThue;
                ncc.DiaChi = model.DiaChi;
                ncc.TaiKhoanNganHang = model.TaiKhoanNganHang;
                ncc.GhiChu = model.GhiChu;
                ncc.NgayCapNhat = DateTime.Now;
                db.SaveChanges();
            }
        }

        public void DoiTrangThaiNhaCungCap(int maNCC)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                var ncc = db.NhaCungCaps.FirstOrDefault(x => x.MaNCC == maNCC);
                if (ncc == null) throw new Exception("Không tìm thấy nhà cung cấp.");
                ncc.ConHoatDong = !ncc.ConHoatDong;
                ncc.NgayCapNhat = DateTime.Now;
                db.SaveChanges();
            }
        }

        public List<DonViTinhQuanLyModel> GetDonViTinhs(string tuKhoa)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                var query = db.DonViTinhs.Select(x => new DonViTinhQuanLyModel
                {
                    MaDonVi = x.MaDonVi,
                    MaCodeDonVi = x.MaCodeDonVi,
                    TenDonVi = x.TenDonVi,
                    MoTa = x.MoTa,
                    NgayTao = x.NgayTao
                });

                if (!string.IsNullOrWhiteSpace(tuKhoa))
                    query = query.Where(x => x.MaCodeDonVi.Contains(tuKhoa) || x.TenDonVi.Contains(tuKhoa));

                return query.OrderBy(x => x.TenDonVi).ToList();
            }
        }

        public string TaoMaDonVi()
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                int stt = db.DonViTinhs.Count() + 1;
                string ma;
                do { ma = "DVT" + stt.ToString("000"); stt++; }
                while (db.DonViTinhs.Any(x => x.MaCodeDonVi == ma));
                return ma;
            }
        }

        public void ThemDonViTinh(DonViTinhQuanLyModel model)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                if (model == null) throw new Exception("Dữ liệu đơn vị tính không hợp lệ.");
                if (string.IsNullOrWhiteSpace(model.TenDonVi)) throw new Exception("Vui lòng nhập tên đơn vị tính.");
                if (db.DonViTinhs.Any(x => x.TenDonVi == model.TenDonVi.Trim())) throw new Exception("Tên đơn vị tính đã tồn tại.");

                var dvt = new DonViTinh();
                dvt.MaCodeDonVi = string.IsNullOrWhiteSpace(model.MaCodeDonVi) ? TaoMaDonVi() : model.MaCodeDonVi.Trim();
                dvt.TenDonVi = model.TenDonVi.Trim();
                dvt.MoTa = model.MoTa;
                dvt.NgayTao = DateTime.Now;
                db.DonViTinhs.Add(dvt);
                db.SaveChanges();
            }
        }

        public void CapNhatDonViTinh(DonViTinhQuanLyModel model)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                if (model == null || model.MaDonVi <= 0) throw new Exception("Vui lòng chọn đơn vị tính cần cập nhật.");
                if (string.IsNullOrWhiteSpace(model.TenDonVi)) throw new Exception("Vui lòng nhập tên đơn vị tính.");
                if (db.DonViTinhs.Any(x => x.TenDonVi == model.TenDonVi.Trim() && x.MaDonVi != model.MaDonVi)) throw new Exception("Tên đơn vị tính đã tồn tại.");

                var dvt = db.DonViTinhs.FirstOrDefault(x => x.MaDonVi == model.MaDonVi);
                if (dvt == null) throw new Exception("Không tìm thấy đơn vị tính.");
                dvt.TenDonVi = model.TenDonVi.Trim();
                dvt.MoTa = model.MoTa;
                db.SaveChanges();
            }
        }
    }
}
