using System;
using System.Collections.Generic;
using System.Linq;
using QuanLyQuanCaFe.Models;

namespace QuanLyQuanCaFe.Services
{
    public class DashboardService
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

        public DashboardSummaryModel GetSummary()
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                DateTime today = DateTime.Today;
                DateTime tomorrow = today.AddDays(1);

                var hoaDonHomNay = db.HoaDonBans
                    .Where(x => x.NgayLapHoaDon >= today && x.NgayLapHoaDon < tomorrow)
                    .ToList()
                    .Where(x => LaHoaDonHopLe(x.TrangThaiHoaDon))
                    .ToList();

                var summary = new DashboardSummaryModel();

                summary.SoSanPham = db.SanPhams.Count();
                summary.SoNhanVien = db.NhanViens.Count(x => x.ConHoatDong);
                summary.SoBan = db.BanCafes.Count();
                summary.SoHoaDonHomNay = hoaDonHomNay.Count;

                return summary;
            }
        }

        public List<DashboardDoanhThuGioModel> GetDoanhThuTheoGioHomNay()
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                DateTime today = DateTime.Today;
                DateTime tomorrow = today.AddDays(1);

                int[] khungGio = new[] { 8, 10, 12, 14, 16, 18, 20, 22 };

                var result = khungGio
                    .Select(gio => new DashboardDoanhThuGioModel
                    {
                        Gio = gio,
                        GioText = gio.ToString("00") + "h",
                        DoanhThu = 0,
                        SoHoaDon = 0,
                        DoCaoCot = 0
                    })
                    .ToList();

                var hoaDons = db.HoaDonBans
                    .Where(x => x.NgayLapHoaDon >= today && x.NgayLapHoaDon < tomorrow)
                    .ToList()
                    .Where(x => LaHoaDonHopLe(x.TrangThaiHoaDon))
                    .ToList();

                foreach (var hoaDon in hoaDons)
                {
                    int hour = hoaDon.NgayLapHoaDon.Hour;
                    int bucket;

                    if (hour < 8)
                    {
                        bucket = 8;
                    }
                    else if (hour >= 22)
                    {
                        bucket = 22;
                    }
                    else
                    {
                        bucket = (hour / 2) * 2;

                        if (bucket < 8)
                            bucket = 8;
                    }

                    var item = result.FirstOrDefault(x => x.Gio == bucket);

                    if (item != null)
                    {
                        item.DoanhThu += hoaDon.TongTien;
                        item.SoHoaDon += 1;
                    }
                }

                decimal max = result.Count > 0 ? result.Max(x => x.DoanhThu) : 0;

                foreach (var item in result)
                {
                    item.DoCaoCot = max <= 0 ? 8 : (double)(item.DoanhThu / max * 170);

                    if (item.DoCaoCot > 0 && item.DoCaoCot < 8)
                        item.DoCaoCot = 8;
                }

                return result;
            }
        }
    }
}
