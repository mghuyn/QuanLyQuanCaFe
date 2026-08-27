using QuanLyQuanCaFe.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity.Validation;
using System.Text;

namespace QuanLyQuanCaFe.Services
{
    public class KhachHangService
    {
        private string ChuanHoaGioiTinh(string gioiTinh)
        {
            if (string.IsNullOrWhiteSpace(gioiTinh)) return "O";
            string gt = gioiTinh.Trim().ToLower();
            if (gt == "nam" || gt == "m" || gt == "male") return "M";
            if (gt == "nữ" || gt == "nu" || gt == "f" || gt == "female") return "F";
            return "O";
        }

        private void LuuThayDoi(QuanLyQuanCaPheDbEntities1 db)
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

        public List<HangKhachHang> GetHangKhachHangs()
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                return db.HangKhachHangs
                    .Where(x => x.ConHoatDong)
                    .OrderBy(x => x.DiemToiThieu)
                    .ToList();
            }
        }

        public List<string> GetTenHangFilters()
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                var data = db.HangKhachHangs
                    .Where(x => x.ConHoatDong)
                    .OrderBy(x => x.DiemToiThieu)
                    .Select(x => x.TenHang)
                    .ToList();

                data.Insert(0, "Tất cả");
                return data;
            }
        }

        public List<KhachHangItemModel> GetKhachHangs(string tuKhoa, string tenHang, string trangThai)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                var query = db.KhachHangs
                    .Select(x => new KhachHangItemModel
                    {
                        MaKH = x.MaKH,
                        MaKhachHang = x.MaKhachHang,
                        MaHangKH = x.MaHangKH,
                        TenHang = x.HangKhachHang.TenHang,

                        HoTen = x.HoTen,
                        SoDienThoai = x.SoDienThoai,
                        Email = x.Email,
                        NgaySinh = x.NgaySinh,
                        GioiTinh = x.GioiTinh,
                        DiaChi = x.DiaChi,

                        NgayThamGia = x.NgayThamGia,
                        DiemTichLuy = x.DiemTichLuy,
                        TongChiTieu = x.TongChiTieu,
                        LanGheCuoi = x.LanGheCuoi,

                        GhiChu = x.GhiChu,
                        ConHoatDong = x.ConHoatDong
                    });

                if (!string.IsNullOrWhiteSpace(tuKhoa))
                {
                    query = query.Where(x =>
                        x.HoTen.Contains(tuKhoa) ||
                        x.SoDienThoai.Contains(tuKhoa) ||
                        x.Email.Contains(tuKhoa) ||
                        x.MaKhachHang.Contains(tuKhoa));
                }

                if (!string.IsNullOrWhiteSpace(tenHang) && tenHang != "Tất cả")
                {
                    query = query.Where(x => x.TenHang == tenHang);
                }

                if (trangThai == "Đang hoạt động")
                {
                    query = query.Where(x => x.ConHoatDong);
                }
                else if (trangThai == "Đã ẩn")
                {
                    query = query.Where(x => !x.ConHoatDong);
                }

                return query
                    .OrderByDescending(x => x.ConHoatDong)
                    .ThenByDescending(x => x.TongChiTieu)
                    .ThenBy(x => x.HoTen)
                    .ToList();
            }
        }

        public string TaoMaKhachHang()
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                int soLuong = db.KhachHangs.Count() + 1;
                string ma;

                do
                {
                    ma = "KH" + soLuong.ToString("0000");
                    soLuong++;
                }
                while (db.KhachHangs.Any(x => x.MaKhachHang == ma));

                return ma;
            }
        }

        public void ThemKhachHang(KhachHangItemModel model)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                if (model == null)
                    throw new Exception("Dữ liệu khách hàng không hợp lệ.");

                if (string.IsNullOrWhiteSpace(model.HoTen))
                    throw new Exception("Vui lòng nhập họ tên khách hàng.");

                if (string.IsNullOrWhiteSpace(model.SoDienThoai))
                    throw new Exception("Vui lòng nhập số điện thoại.");

                bool trungSdt = db.KhachHangs.Any(x => x.SoDienThoai == model.SoDienThoai);

                if (trungSdt)
                    throw new Exception("Số điện thoại này đã tồn tại.");

                int maHang = model.MaHangKH;

                if (maHang <= 0)
                {
                    var hangMacDinh = db.HangKhachHangs
                        .Where(x => x.ConHoatDong)
                        .OrderBy(x => x.DiemToiThieu)
                        .FirstOrDefault();

                    if (hangMacDinh == null)
                        throw new Exception("Chưa có hạng khách hàng.");

                    maHang = hangMacDinh.MaHangKH;
                }

                var kh = new KhachHang();
                kh.MaKhachHang = string.IsNullOrWhiteSpace(model.MaKhachHang)
                    ? TaoMaKhachHang()
                    : model.MaKhachHang;

                kh.MaHangKH = maHang;
                kh.HoTen = model.HoTen.Trim();
                kh.SoDienThoai = model.SoDienThoai.Trim();
                kh.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
                kh.NgaySinh = model.NgaySinh;
                kh.GioiTinh = ChuanHoaGioiTinh(model.GioiTinh);
                kh.DiaChi = model.DiaChi;
                kh.NgayThamGia = DateTime.Now;
                kh.DiemTichLuy = 0;
                kh.TongChiTieu = 0;
                kh.LanGheCuoi = null;
                kh.GhiChu = model.GhiChu;
                kh.ConHoatDong = true;
                kh.NgayTao = DateTime.Now;
                kh.NgayCapNhat = DateTime.Now;

                db.KhachHangs.Add(kh);
                LuuThayDoi(db);
            }
        }

        public void CapNhatKhachHang(KhachHangItemModel model)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                if (model == null)
                    throw new Exception("Dữ liệu khách hàng không hợp lệ.");

                if (model.MaKH <= 0)
                    throw new Exception("Vui lòng chọn khách hàng cần cập nhật.");

                if (string.IsNullOrWhiteSpace(model.HoTen))
                    throw new Exception("Vui lòng nhập họ tên khách hàng.");

                if (string.IsNullOrWhiteSpace(model.SoDienThoai))
                    throw new Exception("Vui lòng nhập số điện thoại.");

                bool trungSdt = db.KhachHangs.Any(x =>
                    x.SoDienThoai == model.SoDienThoai &&
                    x.MaKH != model.MaKH);

                if (trungSdt)
                    throw new Exception("Số điện thoại này đã thuộc khách hàng khác.");

                var kh = db.KhachHangs.FirstOrDefault(x => x.MaKH == model.MaKH);

                if (kh == null)
                    throw new Exception("Không tìm thấy khách hàng.");

                int maHangCapNhat = model.MaHangKH;
                if (maHangCapNhat <= 0)
                {
                    var hangMacDinh = db.HangKhachHangs
                        .Where(x => x.ConHoatDong)
                        .OrderBy(x => x.DiemToiThieu)
                        .FirstOrDefault();

                    if (hangMacDinh == null)
                        throw new Exception("Chưa có hạng khách hàng.");

                    maHangCapNhat = hangMacDinh.MaHangKH;
                }

                kh.MaHangKH = maHangCapNhat;
                kh.HoTen = model.HoTen.Trim();
                kh.SoDienThoai = model.SoDienThoai.Trim();
                kh.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
                kh.NgaySinh = model.NgaySinh;
                kh.GioiTinh = ChuanHoaGioiTinh(model.GioiTinh);
                kh.DiaChi = model.DiaChi;
                kh.GhiChu = model.GhiChu;
                kh.NgayCapNhat = DateTime.Now;

                LuuThayDoi(db);
            }
        }

        public void DoiTrangThaiKhachHang(int maKH)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                var kh = db.KhachHangs.FirstOrDefault(x => x.MaKH == maKH);

                if (kh == null)
                    throw new Exception("Không tìm thấy khách hàng.");

                kh.ConHoatDong = !kh.ConHoatDong;
                kh.NgayCapNhat = DateTime.Now;

                LuuThayDoi(db);
            }
        }
    }
}