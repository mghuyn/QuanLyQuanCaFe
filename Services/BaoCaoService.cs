using QuanLyQuanCaFe.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuanLyQuanCaFe.Services
{
    public class BaoCaoService
    {
        private bool LaHoaDonHopLe(string trangThai)
        {
            if (string.IsNullOrWhiteSpace(trangThai))
                return true;

            string tt = trangThai.Trim().ToUpper();

            return tt != "CANCELLED"
                && tt != "CANCELED"
                && tt != "ĐÃ HỦY"
                && tt != "DA HUY"
                && tt != "HUY";
        }

        public BaoCaoTongQuanModel GetTongQuan(DateTime tuNgay, DateTime denNgay)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                DateTime start = tuNgay.Date;
                DateTime end = denNgay.Date.AddDays(1);

                var hoaDons = db.HoaDonBans
                    .Where(x =>
                        x.NgayLapHoaDon >= start &&
                        x.NgayLapHoaDon < end)
                    .ToList()
                    .Where(x => LaHoaDonHopLe(x.TrangThaiHoaDon))
                    .ToList();

                decimal doanhThu = hoaDons
                    .Select(x => (decimal?)x.TongTien)
                    .DefaultIfEmpty(0)
                    .Sum() ?? 0;

                int soHoaDon = hoaDons.Count;

                int soKhachHang = hoaDons
                    .Where(x => x.MaKH != null)
                    .Select(x => x.MaKH)
                    .Distinct()
                    .Count();

                var chiTietHoaDons = db.ChiTietHoaDonBans
                    .Where(x =>
                        x.HoaDonBan.NgayLapHoaDon >= start &&
                        x.HoaDonBan.NgayLapHoaDon < end)
                    .ToList()
                    .Where(x => x.HoaDonBan != null && LaHoaDonHopLe(x.HoaDonBan.TrangThaiHoaDon))
                    .ToList();

                int soSanPhamBanRa = chiTietHoaDons
                    .Select(x => (int?)x.SoLuong)
                    .DefaultIfEmpty(0)
                    .Sum() ?? 0;

                return new BaoCaoTongQuanModel
                {
                    DoanhThu = doanhThu,
                    SoHoaDon = soHoaDon,
                    SoKhachHang = soKhachHang,
                    SoSanPhamBanRa = soSanPhamBanRa
                };
            }
        }

        public List<BaoCaoDoanhThuNgayModel> GetDoanhThuTheoNgay(DateTime tuNgay, DateTime denNgay)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                DateTime start = tuNgay.Date;
                DateTime end = denNgay.Date.AddDays(1);

                var hoaDons = db.HoaDonBans
                    .Where(x =>
                        x.NgayLapHoaDon >= start &&
                        x.NgayLapHoaDon < end)
                    .ToList()
                    .Where(x => LaHoaDonHopLe(x.TrangThaiHoaDon))
                    .ToList();

                var raw = hoaDons
                    .GroupBy(x => x.NgayLapHoaDon.Date)
                    .Select(g => new
                    {
                        Ngay = g.Key,
                        DoanhThu = g.Select(x => (decimal?)x.TongTien).DefaultIfEmpty(0).Sum() ?? 0,
                        SoHoaDon = g.Count()
                    })
                    .OrderBy(x => x.Ngay)
                    .ToList();

                var result = new List<BaoCaoDoanhThuNgayModel>();

                DateTime cursor = start;

                while (cursor <= denNgay.Date)
                {
                    var item = raw.FirstOrDefault(x => x.Ngay == cursor.Date);

                    result.Add(new BaoCaoDoanhThuNgayModel
                    {
                        Ngay = cursor,
                        DoanhThu = item != null ? item.DoanhThu : 0,
                        SoHoaDon = item != null ? item.SoHoaDon : 0,
                        DoRongCot = 0
                    });

                    cursor = cursor.AddDays(1);
                }

                decimal max = result.Count > 0? result.Max(x => x.DoanhThu): 0;

                foreach (var item in result)
                {
                    item.DoRongCot = max <= 0? 0: (double)(item.DoanhThu / max * 360);
                }

                return result;
            }
        }

        public List<BaoCaoSanPhamBanChayModel> GetSanPhamBanChay(DateTime tuNgay, DateTime denNgay)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                DateTime start = tuNgay.Date;
                DateTime end = denNgay.Date.AddDays(1);

                var chiTietHoaDons = db.ChiTietHoaDonBans
                    .Where(x =>
                        x.HoaDonBan.NgayLapHoaDon >= start &&
                        x.HoaDonBan.NgayLapHoaDon < end)
                    .ToList().Where(x => x.HoaDonBan != null && LaHoaDonHopLe(x.HoaDonBan.TrangThaiHoaDon)).ToList();

                var data = chiTietHoaDons
                    .GroupBy(x => new
                    {
                        TenSanPham = x.BienTheSanPham.SanPham.TenSanPham,
                        TenDanhMuc = x.BienTheSanPham.SanPham.DanhMucSanPham.TenDanhMuc
                    })
                    .Select(g => new BaoCaoSanPhamBanChayModel
                    {
                        TenSanPham = g.Key.TenSanPham,
                        TenDanhMuc = g.Key.TenDanhMuc,
                        SoLuongBan = g.Select(x => (int?)x.SoLuong).DefaultIfEmpty(0).Sum() ?? 0,
                        DoanhThu = g.Select(x => (decimal?)x.ThanhTien).DefaultIfEmpty(0).Sum() ?? 0
                    })
                    .OrderByDescending(x => x.SoLuongBan)
                    .ThenByDescending(x => x.DoanhThu)
                    .Take(10)
                    .ToList();

                int max = data.Count > 0
                    ? data.Max(x => x.SoLuongBan)
                    : 0;

                foreach (var item in data)
                {
                    item.DoRongCot = max <= 0
                        ? 0
                        : (double)item.SoLuongBan / max * 280;
                }

                return data;
            }
        }
    }
}