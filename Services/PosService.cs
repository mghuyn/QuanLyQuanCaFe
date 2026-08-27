using System;
using System.Collections.Generic;
using System.Linq;
using QuanLyQuanCaFe.Models;

namespace QuanLyQuanCaFe.Services
{
    public class PosService
    {
        public List<string> GetDanhMuc()
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

        public List<PosProductModel> GetSanPham(string tenDanhMuc = "Tất cả", string tuKhoa = "")
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                // Bán hàng lấy theo sản phẩm, không lấy trực tiếp theo biến thể.
                // Mục tiêu: danh sách bên Bán hàng khớp danh sách bên Sản phẩm, mỗi món chỉ hiện 1 card.
                // Giá bán lấy từ biến thể mặc định đang hoạt động.
                var query = db.SanPhams
                    .Where(x => x.ConHoatDong && x.DangBan)
                    .Select(x => new PosProductModel
                    {
                        MaSanPham = x.MaSanPham,
                        MaBienThe = x.BienTheSanPhams
                            .Where(v => v.MacDinh && v.ConHoatDong && v.DangBan)
                            .OrderBy(v => v.MaBienThe)
                            .Select(v => v.MaBienThe)
                            .FirstOrDefault(),
                        TenSanPham = x.TenSanPham,
                        TenDanhMuc = x.DanhMucSanPham.TenDanhMuc,
                        TenSize = "",
                        GiaBan = x.BienTheSanPhams
                            .Where(v => v.MacDinh && v.ConHoatDong && v.DangBan)
                            .OrderBy(v => v.MaBienThe)
                            .Select(v => v.GiaBan)
                            .FirstOrDefault(),
                        HinhAnh = x.DuongDanHinhAnh,
                        DangBan = true
                    })
                    .Where(x => x.MaBienThe > 0);

                if (!string.IsNullOrWhiteSpace(tenDanhMuc) && tenDanhMuc != "Tất cả")
                {
                    query = query.Where(x => x.TenDanhMuc == tenDanhMuc);
                }

                if (!string.IsNullOrWhiteSpace(tuKhoa))
                {
                    query = query.Where(x => x.TenSanPham.Contains(tuKhoa));
                }

                return query
                    .OrderBy(x => x.TenDanhMuc)
                    .ThenBy(x => x.TenSanPham)
                    .ToList();
            }
        }

        public List<PosTableModel> GetBanCafes(string tuKhoa = "")
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                var query = db.BanCafes
                    .Where(x => x.ConHoatDong)
                    .Select(x => new PosTableModel
                    {
                        MaBan = x.MaBan,
                        TenBan = x.TenBan,
                        MaCodeBan = x.MaCodeBan,
                        TenKhuVuc = x.KhuVucQuan.TenKhuVuc,
                        SoGhe = x.SucChua,
                        TrangThai = x.TrangThai
                    });

                if (!string.IsNullOrWhiteSpace(tuKhoa))
                {
                    query = query.Where(x => x.TenBan.Contains(tuKhoa) || x.MaCodeBan.Contains(tuKhoa));
                }

                return query
                    .ToList()
                    .OrderBy(x => x.TenKhuVuc)
                    .ThenBy(x => x.SoThuTuBan)
                    .ThenBy(x => x.TenBan)
                    .ToList();
            }
        }

        public List<PosCustomerModel> GetKhachHangs(string tuKhoa = "")
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                var query = db.KhachHangs
                    .Where(x => x.ConHoatDong)
                    .Select(x => new PosCustomerModel
                    {
                        MaKH = x.MaKH,
                        MaKhachHang = x.MaKhachHang,
                        HoTen = x.HoTen,
                        SoDienThoai = x.SoDienThoai,
                        DiemTichLuy = x.DiemTichLuy
                    });

                if (!string.IsNullOrWhiteSpace(tuKhoa))
                {
                    query = query.Where(x =>
                        x.HoTen.Contains(tuKhoa) ||
                        x.SoDienThoai.Contains(tuKhoa) ||
                        x.MaKhachHang.Contains(tuKhoa));
                }

                return query
                    .OrderBy(x => x.HoTen)
                    .Take(80)
                    .ToList();
            }
        }



        public bool KiemTraTonKhoMon(int maBienThe, int soLuongCanBan, out string thongBao)
        {
            thongBao = "";

            if (soLuongCanBan <= 0)
            {
                thongBao = "Số lượng món không hợp lệ.";
                return false;
            }

            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                var bienThe = db.BienTheSanPhams.FirstOrDefault(x => x.MaBienThe == maBienThe);

                if (bienThe == null)
                {
                    thongBao = "Không tìm thấy món trong hệ thống.";
                    return false;
                }

                string tenMon = bienThe.SanPham != null ? bienThe.SanPham.TenSanPham : "món này";

                if (bienThe.SanPham != null && !bienThe.SanPham.DangBan)
                {
                    thongBao = tenMon + " hiện đang ngưng bán.";
                    return false;
                }

                var congThucs = db.ChiTietCongThucs
                    .Where(x => x.MaBienThe == maBienThe)
                    .ToList();

                // Nếu món chưa khai báo công thức thì không chặn bán, vì có thể là món không quản lý tồn nguyên liệu.
                if (congThucs.Count == 0)
                    return true;

                foreach (var ct in congThucs)
                {
                    var nguyenLieu = db.NguyenLieus.FirstOrDefault(x => x.MaNguyenLieu == ct.MaNguyenLieu);

                    if (nguyenLieu == null)
                    {
                        thongBao = "Công thức của " + tenMon + " đang thiếu nguyên liệu.";
                        return false;
                    }

                    decimal soLuongCan = ct.SoLuongCan * soLuongCanBan;

                    if (ct.PhanTramHaoHut > 0)
                    {
                        soLuongCan += soLuongCan * ct.PhanTramHaoHut / 100;
                    }

                    if (nguyenLieu.SoLuongHienTai < soLuongCan)
                    {
                        thongBao = "Không đủ nguyên liệu cho " + tenMon + ". Thiếu: " + nguyenLieu.TenNguyenLieu + ". Tồn hiện tại: " + nguyenLieu.SoLuongHienTai.ToString("N2") + ", cần: " + soLuongCan.ToString("N2") + ".";
                        return false;
                    }
                }

                return true;
            }
        }

        public bool KiemTraTonKhoGioHang(List<CartItemModel> gioHang, out string thongBao)
        {
            thongBao = "";

            if (gioHang == null || gioHang.Count == 0)
            {
                thongBao = "Giỏ hàng đang trống.";
                return false;
            }

            var nhomMon = gioHang
                .GroupBy(x => x.MaBienThe)
                .Select(g => new
                {
                    MaBienThe = g.Key,
                    SoLuong = g.Sum(x => x.SoLuong)
                })
                .ToList();

            foreach (var item in nhomMon)
            {
                if (!KiemTraTonKhoMon(item.MaBienThe, item.SoLuong, out thongBao))
                    return false;
            }

            return true;
        }

        public List<PosHoaDonHistoryModel> GetLichSuHoaDon(DateTime tuNgay, DateTime denNgay, string tuKhoa = "", string trangThai = "Tất cả")
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                DateTime start = tuNgay.Date;
                DateTime end = denNgay.Date.AddDays(1);

                var query = db.HoaDonBans
                    .Where(x => x.NgayLapHoaDon >= start && x.NgayLapHoaDon < end)
                    .Select(x => new PosHoaDonHistoryModel
                    {
                        MaHoaDonBan = x.MaHoaDonBan,
                        MaHoaDon = x.MaHoaDon,
                        NgayLapHoaDon = x.NgayLapHoaDon,
                        TenBan = x.BanCafe != null ? x.BanCafe.TenBan : "",
                        TenKhachHang = x.KhachHang != null ? x.KhachHang.HoTen : x.TenKhachTam,
                        LoaiHoaDon = x.LoaiHoaDon,
                        TrangThaiHoaDon = x.TrangThaiHoaDon,
                        TrangThaiThanhToan = x.TrangThaiThanhToan,
                        TongTien = x.TongTien,
                        GhiChu = x.GhiChuHoaDon ?? x.GhiChu,
                        TongSoMon = x.ChiTietHoaDonBans.Select(ct => (int?)ct.SoLuong).DefaultIfEmpty(0).Sum() ?? 0
                    });

                if (!string.IsNullOrWhiteSpace(tuKhoa))
                {
                    query = query.Where(x =>
                        x.MaHoaDon.Contains(tuKhoa) ||
                        x.TenBan.Contains(tuKhoa) ||
                        x.TenKhachHang.Contains(tuKhoa));
                }

                if (!string.IsNullOrWhiteSpace(trangThai) && trangThai != "Tất cả")
                {
                    query = query.Where(x => x.TrangThaiHoaDon == trangThai);
                }

                return query
                    .OrderByDescending(x => x.NgayLapHoaDon)
                    .ToList();
            }
        }

        public List<PosHoaDonDetailItemModel> GetChiTietHoaDon(int maHoaDonBan)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                return db.ChiTietHoaDonBans
                    .Where(x => x.MaHoaDonBan == maHoaDonBan)
                    .OrderBy(x => x.MaChiTietHoaDonBan)
                    .Select(x => new PosHoaDonDetailItemModel
                    {
                        MaChiTietHoaDonBan = x.MaChiTietHoaDonBan,
                        TenSanPham = x.BienTheSanPham.SanPham.TenSanPham,
                        TenSize = "",
                        SoLuong = x.SoLuong,
                        DonGia = x.DonGia,
                        ThanhTien = x.ThanhTien ?? (x.DonGia * x.SoLuong),
                        TrangThaiMon = x.TrangThaiMon,
                        GhiChu = x.YeuCauDacBiet
                    })
                    .ToList();
            }
        }
    }
}
