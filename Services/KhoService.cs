using QuanLyQuanCaFe.Core;
using QuanLyQuanCaFe.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuanLyQuanCaFe.Services
{
    public class KhoService
    {
        public List<string> GetDanhMucKho()
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                var data = db.NguyenLieus
                    .Where(x => x.ConHoatDong)
                    .Select(x => x.TenDanhMuc)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                data.Insert(0, "Tất cả");
                return data;
            }
        }

        public List<NhaCC_Model> GetNhaCungCaps()
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                return db.NhaCungCaps
                    .OrderBy(x => x.TenNhaCungCap)
                    .Select(x => new NhaCC_Model
                    {
                        MaNhaCungCap = x.MaNhaCungCap,
                        TenNhaCungCap = x.TenNhaCungCap
                    })
                    .ToList();
            }
        }
        public List<PhieuKhoModel> GetLichSuPhieuKho(string tuKhoa = "", string loaiPhieu = "Tất cả")
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                var query = db.PhieuKhos
                    .Select(x => new PhieuKhoModel
                    {
                        MaPhieuKho = x.MaPhieuKho,
                        MaCodePhieuKho = x.MaCodePhieuKho,
                        LoaiPhieu = x.LoaiPhieu,
                        NgayLap = x.NgayLap,
                        GhiChu = x.GhiChu,
                        TongTien = x.TongTien,
                        TrangThai = x.TrangThai
                    });

                if (!string.IsNullOrWhiteSpace(tuKhoa))
                {
                    query = query.Where(x =>
                        x.MaCodePhieuKho.Contains(tuKhoa) ||
                        x.GhiChu.Contains(tuKhoa));
                }

                if (!string.IsNullOrWhiteSpace(loaiPhieu) && loaiPhieu != "Tất cả")
                {
                    if (loaiPhieu == "Phiếu nhập")
                        query = query.Where(x => x.LoaiPhieu == "NHAP");

                    if (loaiPhieu == "Phiếu xuất")
                        query = query.Where(x => x.LoaiPhieu == "XUAT");
                }

                return query
                    .OrderByDescending(x => x.NgayLap)
                    .ToList();
            }
        }
        public List<KhoItemModel> GetDanhSachKho(string tuKhoa, string danhMuc)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                var query = db.NguyenLieus
                    .Where(x => x.ConHoatDong)
                    .Select(x => new KhoItemModel
                    {
                        MaNguyenLieu = x.MaNguyenLieu,
                        MaCodeNguyenLieu = x.MaCodeNguyenLieu,
                        TenNguyenLieu = x.TenNguyenLieu,
                        TenDanhMuc = x.TenDanhMuc,
                        MaDonVi = x.MaDonVi,
                        TenDonVi = x.DonViTinh.TenDonVi,
                        SoLuongHienTai = x.SoLuongHienTai,
                        SoLuongToiThieu = x.SoLuongToiThieu,
                        SoLuongToiDa = x.SoLuongToiDa,
                        GiaNhapCuoi = x.GiaNhapCuoi,
                        ViTriLuuKho = x.ViTriLuuKho,
                        HinhAnh = x.DuongDanHinhAnh,
                        GhiChu = x.GhiChu,
                        ConHoatDong = x.ConHoatDong
                    });

                if (!string.IsNullOrWhiteSpace(tuKhoa))
                {
                    query = query.Where(x =>
                        x.TenNguyenLieu.Contains(tuKhoa) ||
                        x.MaCodeNguyenLieu.Contains(tuKhoa) ||
                        x.TenDanhMuc.Contains(tuKhoa));
                }

                if (!string.IsNullOrWhiteSpace(danhMuc) && danhMuc != "Tất cả")
                {
                    query = query.Where(x => x.TenDanhMuc == danhMuc);
                }

                return query
                    .OrderBy(x => x.SoLuongHienTai <= x.SoLuongToiThieu ? 0 : 1)
                    .ThenBy(x => x.TenNguyenLieu)
                    .ToList();
            }
        }


        public List<string> GetDonViNames()
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                return db.DonViTinhs
                    .OrderBy(x => x.TenDonVi)
                    .Select(x => x.TenDonVi)
                    .ToList();
            }
        }

        public void ThemNguyenLieu(string tenNguyenLieu, string tenDanhMuc, string tenDonVi, decimal soLuongHienTai, decimal soLuongToiThieu, decimal? soLuongToiDa, decimal giaNhapCuoi, string viTri, string ghiChu)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                if (string.IsNullOrWhiteSpace(tenNguyenLieu)) throw new Exception("Vui lòng nhập tên nguyên liệu.");
                if (string.IsNullOrWhiteSpace(tenDanhMuc)) throw new Exception("Vui lòng nhập danh mục nguyên liệu.");
                if (string.IsNullOrWhiteSpace(tenDonVi)) throw new Exception("Vui lòng chọn đơn vị tính.");
                if (soLuongHienTai < 0 || soLuongToiThieu < 0 || giaNhapCuoi < 0) throw new Exception("Số lượng và giá nhập không được âm.");
                if (soLuongToiDa != null && soLuongToiDa.Value < soLuongToiThieu) throw new Exception("Tồn tối đa phải lớn hơn hoặc bằng tồn tối thiểu.");

                if (db.NguyenLieus.Any(x => x.TenNguyenLieu == tenNguyenLieu))
                    throw new Exception("Tên nguyên liệu đã tồn tại.");

                var donVi = db.DonViTinhs.FirstOrDefault(x => x.TenDonVi == tenDonVi);
                if (donVi == null) throw new Exception("Không tìm thấy đơn vị tính: " + tenDonVi);

                var nextId = (db.NguyenLieus.Select(x => (int?)x.MaNguyenLieu).Max() ?? 0) + 1;
                var nl = db.NguyenLieus.Create();
                nl.MaCodeNguyenLieu = "NL" + nextId.ToString("000");
                nl.TenNguyenLieu = tenNguyenLieu.Trim();
                nl.MaDonVi = donVi.MaDonVi;
                nl.TenDanhMuc = tenDanhMuc.Trim();
                nl.SoLuongHienTai = soLuongHienTai;
                nl.SoLuongToiThieu = soLuongToiThieu;
                nl.SoLuongToiDa = soLuongToiDa;
                nl.GiaNhapCuoi = giaNhapCuoi;
                nl.ViTriLuuKho = string.IsNullOrWhiteSpace(viTri) ? "Kho chính" : viTri.Trim();
                nl.SoNgayCanhBaoHetHan = 30;
                nl.DuongDanHinhAnh = "";
                nl.GhiChu = ghiChu ?? "";
                nl.ConHoatDong = true;
                nl.NgayTao = DateTime.Now;
                nl.NgayCapNhat = DateTime.Now;

                db.NguyenLieus.Add(nl);
                db.SaveChanges();
            }
        }

        public void CapNhatNguyenLieu(int maNguyenLieu, string tenNguyenLieu, string tenDanhMuc, string tenDonVi, decimal soLuongHienTai, decimal soLuongToiThieu, decimal? soLuongToiDa, decimal giaNhapCuoi, string viTri, string ghiChu)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                var nl = db.NguyenLieus.FirstOrDefault(x => x.MaNguyenLieu == maNguyenLieu);
                if (nl == null) throw new Exception("Không tìm thấy nguyên liệu cần cập nhật.");
                if (string.IsNullOrWhiteSpace(tenNguyenLieu)) throw new Exception("Vui lòng nhập tên nguyên liệu.");
                if (string.IsNullOrWhiteSpace(tenDanhMuc)) throw new Exception("Vui lòng nhập danh mục nguyên liệu.");
                if (soLuongHienTai < 0 || soLuongToiThieu < 0 || giaNhapCuoi < 0) throw new Exception("Số lượng và giá nhập không được âm.");
                if (soLuongToiDa != null && soLuongToiDa.Value < soLuongToiThieu) throw new Exception("Tồn tối đa phải lớn hơn hoặc bằng tồn tối thiểu.");

                if (db.NguyenLieus.Any(x => x.MaNguyenLieu != maNguyenLieu && x.TenNguyenLieu == tenNguyenLieu))
                    throw new Exception("Tên nguyên liệu đã tồn tại.");

                var donVi = db.DonViTinhs.FirstOrDefault(x => x.TenDonVi == tenDonVi);
                if (donVi == null) throw new Exception("Không tìm thấy đơn vị tính: " + tenDonVi);

                nl.TenNguyenLieu = tenNguyenLieu.Trim();
                nl.MaDonVi = donVi.MaDonVi;
                nl.TenDanhMuc = tenDanhMuc.Trim();
                nl.SoLuongHienTai = soLuongHienTai;
                nl.SoLuongToiThieu = soLuongToiThieu;
                nl.SoLuongToiDa = soLuongToiDa;
                nl.GiaNhapCuoi = giaNhapCuoi;
                nl.ViTriLuuKho = string.IsNullOrWhiteSpace(viTri) ? "Kho chính" : viTri.Trim();
                nl.GhiChu = ghiChu ?? "";
                nl.ConHoatDong = true;
                nl.NgayCapNhat = DateTime.Now;

                db.SaveChanges();
            }
        }

        public void XoaNguyenLieu(int maNguyenLieu)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                var nl = db.NguyenLieus.FirstOrDefault(x => x.MaNguyenLieu == maNguyenLieu);
                if (nl == null) throw new Exception("Không tìm thấy nguyên liệu cần xóa.");

                bool dangDuocThamChieu = db.ChiTietCongThucs.Any(x => x.MaNguyenLieu == maNguyenLieu)
                    || db.GiaoDichKhos.Any(x => x.MaNguyenLieu == maNguyenLieu)
                    || db.ChiTietPhieuKhos.Any(x => x.MaNguyenLieu == maNguyenLieu)
                    || db.ChiTietHoaDonNhaps.Any(x => x.MaNguyenLieu == maNguyenLieu);

                if (dangDuocThamChieu)
                {
                    nl.ConHoatDong = false;
                    nl.NgayCapNhat = DateTime.Now;
                }
                else
                {
                    db.NguyenLieus.Remove(nl);
                }

                db.SaveChanges();
            }
        }

        public string TaoPhieuNhapKho(List<KhoPhieuItemModel> chiTietPhieu, string ghiChu)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                if (chiTietPhieu == null || chiTietPhieu.Count == 0)
                    throw new Exception("Phiếu nhập đang trống.");

                string maCode = "PNK" + DateTime.Now.ToString("yyyyMMddHHmmss");
                decimal tongTien = chiTietPhieu.Sum(x => x.ThanhTien);

                var phieu = db.PhieuKhos.Create();
                phieu.MaCodePhieuKho = maCode;
                phieu.LoaiPhieu = "NHAP";
                phieu.NgayLap = DateTime.Now;
                phieu.MaNhanVienLap = AppSession.CurrentUser != null ? AppSession.CurrentUser.MaNV : (int?)null;
                phieu.GhiChu = string.IsNullOrWhiteSpace(ghiChu) ? "" : ghiChu;
                phieu.TongTien = tongTien;
                phieu.TrangThai = "Đã hoàn tất";

                db.PhieuKhos.Add(phieu);
                db.SaveChanges();

                foreach (var item in chiTietPhieu)
                {
                    var nl = db.NguyenLieus.FirstOrDefault(x => x.MaNguyenLieu == item.MaNguyenLieu);

                    if (nl == null)
                        throw new Exception("Không tìm thấy nguyên liệu: " + item.TenNguyenLieu);

                    if (item.SoLuong <= 0)
                        throw new Exception("Số lượng nhập phải lớn hơn 0.");

                    if (item.DonGia <= 0)
                        throw new Exception("Đơn giá nhập phải lớn hơn 0.");

                    var ct = db.ChiTietPhieuKhos.Create();
                    ct.MaPhieuKho = phieu.MaPhieuKho;
                    ct.MaNguyenLieu = item.MaNguyenLieu;
                    ct.SoLuong = item.SoLuong;
                    ct.DonGia = item.DonGia;

                    db.ChiTietPhieuKhos.Add(ct);

                    nl.SoLuongHienTai += item.SoLuong;
                    nl.GiaNhapCuoi = item.DonGia;
                    nl.NgayCapNhat = DateTime.Now;

                    var gd = db.GiaoDichKhos.Create();
                    gd.MaNguyenLieu = item.MaNguyenLieu;
                    gd.LoaiGiaoDich = "PURCHASE";
                    gd.LoaiThamChieu = "PHIEU_NHAP";
                    gd.MaThamChieu = phieu.MaPhieuKho;
                    gd.SoLuongThayDoi = item.SoLuong;
                    gd.DonGiaVon = item.DonGia;
                    gd.NgayGiaoDich = DateTime.Now;
                    gd.MaNhanVienTao = AppSession.CurrentUser != null ? AppSession.CurrentUser.MaNV : (int?)null;
                    gd.GhiChu = maCode;

                    db.GiaoDichKhos.Add(gd);
                }

                db.SaveChanges();

                return maCode;
            }
        }

        public string TaoPhieuXuatKho(List<KhoPhieuItemModel> chiTietPhieu, string ghiChu)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                if (chiTietPhieu == null || chiTietPhieu.Count == 0)
                    throw new Exception("Phiếu xuất đang trống.");

                string maCode = "PXK" + DateTime.Now.ToString("yyyyMMddHHmmss");
                decimal tongTien = chiTietPhieu.Sum(x => x.ThanhTien);

                var phieu = db.PhieuKhos.Create();
                phieu.MaCodePhieuKho = maCode;
                phieu.LoaiPhieu = "XUAT";
                phieu.NgayLap = DateTime.Now;
                phieu.MaNhanVienLap = AppSession.CurrentUser != null ? AppSession.CurrentUser.MaNV : (int?)null;
                phieu.GhiChu = string.IsNullOrWhiteSpace(ghiChu) ? "" : ghiChu;
                phieu.TongTien = tongTien;
                phieu.TrangThai = "Đã hoàn tất";

                db.PhieuKhos.Add(phieu);
                db.SaveChanges();

                foreach (var item in chiTietPhieu)
                {
                    var nl = db.NguyenLieus.FirstOrDefault(x => x.MaNguyenLieu == item.MaNguyenLieu);

                    if (nl == null)
                        throw new Exception("Không tìm thấy nguyên liệu: " + item.TenNguyenLieu);

                    if (item.SoLuong <= 0)
                        throw new Exception("Số lượng xuất phải lớn hơn 0.");

                    if (nl.SoLuongHienTai < item.SoLuong)
                        throw new Exception("Tồn kho không đủ cho nguyên liệu: " + item.TenNguyenLieu);

                    var ct = db.ChiTietPhieuKhos.Create();
                    ct.MaPhieuKho = phieu.MaPhieuKho;
                    ct.MaNguyenLieu = item.MaNguyenLieu;
                    ct.SoLuong = item.SoLuong;
                    ct.DonGia = item.DonGia;

                    db.ChiTietPhieuKhos.Add(ct);

                    nl.SoLuongHienTai -= item.SoLuong;
                    nl.NgayCapNhat = DateTime.Now;

                    var gd = db.GiaoDichKhos.Create();
                    gd.MaNguyenLieu = item.MaNguyenLieu;
                    gd.LoaiGiaoDich = "ADJUSTMENT";
                    gd.LoaiThamChieu = "PHIEU_XUAT";
                    gd.MaThamChieu = phieu.MaPhieuKho;
                    gd.SoLuongThayDoi = -item.SoLuong;
                    gd.DonGiaVon = item.DonGia;
                    gd.NgayGiaoDich = DateTime.Now;
                    gd.MaNhanVienTao = AppSession.CurrentUser != null ? AppSession.CurrentUser.MaNV : (int?)null;
                    gd.GhiChu = maCode;

                    db.GiaoDichKhos.Add(gd);
                }

                db.SaveChanges();

                return maCode;
            }
        }
    }
}