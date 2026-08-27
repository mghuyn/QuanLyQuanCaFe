using System;
using System.Collections.Generic;
using System.Linq;
using QuanLyQuanCaFe.Models;

namespace QuanLyQuanCaFe.Services
{
    public class TableService
    {
        public List<string> GetKhuVuc()
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                var data = db.KhuVucQuans
                    .Where(x => x.ConHoatDong)
                    .OrderBy(x => x.ThuTuHienThi)
                    .Select(x => x.TenKhuVuc)
                    .ToList();

                data.Insert(0, "Tất cả");
                return data;
            }
        }

        private string LayPrefixKhuVuc(string tenKhuVuc)
        {
            if (string.IsNullOrWhiteSpace(tenKhuVuc))
                return "B";

            string t = tenKhuVuc.ToUpper();

            if (t.Contains("TRỆT A") || t.Contains("TRET A") || t.Contains("GROUND FLOOR A"))
                return "A";

            if (t.Contains("TRỆT B") || t.Contains("TRET B") || t.Contains("GROUND FLOOR B"))
                return "B";

            if (t.Contains("LẦU 1C") || t.Contains("LAU 1C") || t.Contains("1C"))
                return "1C";

            if (t.Contains("LẦU 1") || t.Contains("LAU 1") || t.Contains("FIRST"))
                return "1C";

            if (t.Contains("VƯỜN") || t.Contains("VUON") || t.Contains("GARDEN"))
                return "G";

            char c = t.FirstOrDefault(ch => char.IsLetter(ch));
            return c == '\0' ? "B" : c.ToString();
        }

        private int LaySoThuTuBan(string maCode, string tenBan)
        {
            // Ưu tiên lấy số từ MaCodeBan.
            // Tránh lỗi A1 + "Bàn A1" bị thành 11.
            string s = !string.IsNullOrWhiteSpace(maCode) ? maCode : (tenBan ?? "");
            string digits = new string(s.Where(char.IsDigit).ToArray());

            int n;
            return int.TryParse(digits, out n) ? n : 9999;
        }

        private bool LaHoaDonDangMo(string trangThaiHoaDon, string trangThaiThanhToan)
        {
            string hd = (trangThaiHoaDon ?? "").Trim().ToUpper();
            string tt = (trangThaiThanhToan ?? "").Trim().ToUpper();

            if (hd == "COMPLETED" ||
                hd == "CANCELLED" ||
                hd == "DA_THANH_TOAN" ||
                hd == "DA_HUY" ||
                hd == "ĐÃ THANH TOÁN" ||
                hd == "ĐÃ HỦY" ||
                hd == "HOÀN TẤT")
                return false;

            if (tt == "PAID" ||
                tt == "CANCELLED" ||
                tt == "DA_THANH_TOAN" ||
                tt == "DA_HUY" ||
                tt == "ĐÃ THANH TOÁN" ||
                tt == "ĐÃ HỦY")
                return false;

            return true;
        }

        public List<TableCardModel> GetBan(string tenKhuVuc = "Tất cả")
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                var query = db.BanCafes
                    .Where(x => x.ConHoatDong)
                    .Select(x => new
                    {
                        x.MaBan,
                        x.MaCodeBan,
                        x.TenBan,
                        x.SucChua,
                        x.TrangThai,
                        x.GhiChu,
                        TenKhuVuc = x.KhuVucQuan.TenKhuVuc,
                        TenTang = x.KhuVucQuan.TenTang,
                        ThuTuKhuVuc = x.KhuVucQuan.ThuTuHienThi
                    });

                if (!string.IsNullOrWhiteSpace(tenKhuVuc) && tenKhuVuc != "Tất cả")
                    query = query.Where(x => x.TenKhuVuc == tenKhuVuc);

                var banData = query.ToList();
                var maBans = banData.Select(x => x.MaBan).ToList();

                var hoaDons = db.HoaDonBans
                    .Where(x => x.MaBan != null && maBans.Contains(x.MaBan.Value))
                    .OrderByDescending(x => x.NgayLapHoaDon)
                    .ToList()
                    .Where(x => LaHoaDonDangMo(x.TrangThaiHoaDon, x.TrangThaiThanhToan))
                    .GroupBy(x => x.MaBan.Value)
                    .ToDictionary(g => g.Key, g => g.First());

                var result = new List<TableCardModel>();

                foreach (var x in banData)
                {
                    HoaDonBan hd = null;
                    hoaDons.TryGetValue(x.MaBan, out hd);

                    string trangThaiThucTe = string.IsNullOrWhiteSpace(x.TrangThai)
                        ? "AVAILABLE"
                        : x.TrangThai.Trim().ToUpper();

                    string trangThaiHienThi = trangThaiThucTe;

                    /*
                     * Quan trọng:
                     * Nếu bàn đang CLEANING thì phải hiển thị Cần dọn.
                     * Không để hóa đơn cũ ép bàn thành OCCUPIED nữa.
                     */
                    if (trangThaiThucTe != "CLEANING" && trangThaiThucTe != "INACTIVE")
                    {
                        if (hd != null)
                        {
                            trangThaiHienThi = hd.TrangThaiHoaDon == "DRAFT"
                                ? "RESERVED"
                                : "OCCUPIED";
                        }
                    }

                    result.Add(new TableCardModel
                    {
                        MaBan = x.MaBan,
                        MaBanText = x.MaBan.ToString(),
                        MaCodeBan = x.MaCodeBan,
                        TenBan = x.TenBan,
                        TenKhuVuc = x.TenKhuVuc,
                        TenTang = x.TenTang,
                        ThuTuKhuVuc = x.ThuTuKhuVuc,
                        SoThuTuBan = LaySoThuTuBan(x.MaCodeBan, x.TenBan),
                        SoGhe = x.SucChua,
                        TrangThaiBan = trangThaiHienThi,
                        GhiChu = x.GhiChu,

                        MaHoaDonBanDangPhucVu = hd != null ? (int?)hd.MaHoaDonBan : null,
                        MaHoaDonDangPhucVu = hd != null ? hd.MaHoaDon : null,
                        TenKhachDangPhucVu = hd != null && hd.KhachHang != null ? hd.KhachHang.HoTen : null,
                        TongTienDangPhucVu = hd != null ? hd.TongTien : 0,
                        SoMonDangPhucVu = hd != null
                            ? hd.ChiTietHoaDonBans.Count(c => c.TrangThaiMon != "CANCELLED")
                            : 0,
                        GhiChuHoaDonDangPhucVu = hd != null ? (hd.GhiChuHoaDon ?? hd.GhiChu) : null
                    });
                }

                return result
                    .OrderBy(x => x.ThuTuKhuVuc)
                    .ThenBy(x => x.SoThuTuBan)
                    .ThenBy(x => x.TenBan)
                    .ToList();
            }
        }

        public List<TableCardModel> GetBanTrongDeChuyen(int? maBanHienTai)
        {
            return GetBan("Tất cả")
                .Where(x => x.TrangThaiBan == "AVAILABLE" &&
                            (!maBanHienTai.HasValue || x.MaBan != maBanHienTai.Value))
                .ToList();
        }

        public void ChuyenBan(int maHoaDonBan, int maBanMoi)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                var hoaDon = db.HoaDonBans.FirstOrDefault(x => x.MaHoaDonBan == maHoaDonBan);

                if (hoaDon == null)
                    throw new Exception("Không tìm thấy hóa đơn đang phục vụ.");

                if (hoaDon.TrangThaiHoaDon == "COMPLETED" || hoaDon.TrangThaiHoaDon == "CANCELLED")
                    throw new Exception("Hóa đơn đã đóng/hủy, không thể chuyển bàn.");

                var banMoi = db.BanCafes.FirstOrDefault(x => x.MaBan == maBanMoi && x.ConHoatDong);

                if (banMoi == null)
                    throw new Exception("Không tìm thấy bàn muốn chuyển đến.");

                if (banMoi.TrangThai != "AVAILABLE")
                    throw new Exception("Bàn muốn chuyển đến hiện không trống.");

                int? maBanCu = hoaDon.MaBan;

                hoaDon.MaBan = maBanMoi;
                hoaDon.NgayCapNhat = DateTime.Now;

                if (maBanCu != null)
                {
                    var banCu = db.BanCafes.FirstOrDefault(x => x.MaBan == maBanCu.Value);

                    if (banCu != null)
                    {
                        banCu.TrangThai = "AVAILABLE";
                        banCu.NgayCapNhat = DateTime.Now;
                    }
                }

                banMoi.TrangThai = "OCCUPIED";
                banMoi.NgayCapNhat = DateTime.Now;

                db.SaveChanges();
            }
        }

        private bool LaHoaDonDaDong(string trangThaiHoaDon, string trangThaiThanhToan)
        {
            string hd = (trangThaiHoaDon ?? "").Trim().ToUpper();
            string tt = (trangThaiThanhToan ?? "").Trim().ToUpper();

            return hd == "COMPLETED" ||
                   hd == "CANCELLED" ||
                   hd == "DA_HUY" ||
                   hd == "ĐÃ HỦY" ||
                   hd == "HOÀN TẤT" ||
                   tt == "PAID" ||
                   tt == "CANCELLED" ||
                   tt == "DA_THANH_TOAN" ||
                   tt == "DA_HUY" ||
                   tt == "ĐÃ THANH TOÁN" ||
                   tt == "ĐÃ HỦY";
        }

        private void DonLienKetHoaDonDaDong(QuanLyQuanCaPheDbEntities1 db, int maBan)
        {
            var hoaDonDaDong = db.HoaDonBans
                .Where(x => x.MaBan == maBan)
                .ToList()
                .Where(x => LaHoaDonDaDong(x.TrangThaiHoaDon, x.TrangThaiThanhToan))
                .ToList();

            foreach (var hd in hoaDonDaDong)
            {
                string tt = (hd.TrangThaiThanhToan ?? "").Trim().ToUpper();
                string hdt = (hd.TrangThaiHoaDon ?? "").Trim().ToUpper();

                // Nếu hóa đơn đã trả tiền nhưng trạng thái hóa đơn còn treo WAITING_KITCHEN/OPEN/PREPARING
                // thì chuẩn hóa lại để GetBan() không xem đây là hóa đơn đang phục vụ.
                if (tt == "PAID" || tt == "DA_THANH_TOAN" || tt == "ĐÃ THANH TOÁN")
                {
                    hd.TrangThaiHoaDon = "COMPLETED";
                    hd.TrangThaiThanhToan = "PAID";
                    if (hd.ThoiGianDong == null)
                        hd.ThoiGianDong = DateTime.Now;
                }
                else if (hdt == "CANCELLED" || hdt == "DA_HUY" || hdt == "ĐÃ HỦY")
                {
                    hd.TrangThaiHoaDon = "CANCELLED";
                    hd.TrangThaiThanhToan = "CANCELLED";
                }

                // Hóa đơn đã đóng vẫn giữ được lịch sử theo MaHoaDonBan/MaHoaDon.
                // Không để nó tiếp tục bám MaBan, vì TableService.GetBan() sẽ hiểu bàn còn bill.
                hd.MaBan = null;
                hd.NgayCapNhat = DateTime.Now;
            }
        }

        public void DoiTrangThaiBan(int maBan, string trangThai)
        {
            DoiTrangThaiBan(maBan, trangThai, false);
        }


        private string ChuanHoaPhuongThucThanhToan(string phuongThucHienTai)
        {
            string pt = (phuongThucHienTai ?? "").Trim().ToUpper();

            // DB đang có CHECK constraint chỉ nhận 4 giá trị này:
            // CASH, BANK_TRANSFER, CARD, EWALLET.
            // Không được dùng CONFIRMED_PAID vì sẽ lỗi SaveChanges.
            if (pt == "CASH" || pt == "BANK_TRANSFER" || pt == "CARD" || pt == "EWALLET")
                return pt;

            return "CASH";
        }

        private void DanhDauHoaDonDangMoLaDaThanhToan(QuanLyQuanCaPheDbEntities1 db, int maBan)
        {
            var hoaDonDangMo = db.HoaDonBans
                .Where(x => x.MaBan == maBan)
                .OrderByDescending(x => x.NgayLapHoaDon)
                .ToList()
                .Where(x => LaHoaDonDangMo(x.TrangThaiHoaDon, x.TrangThaiThanhToan))
                .ToList();

            foreach (var hd in hoaDonDangMo)
            {
                hd.TrangThaiHoaDon = "COMPLETED";
                hd.TrangThaiThanhToan = "PAID";
                hd.PhuongThucThanhToan = ChuanHoaPhuongThucThanhToan(hd.PhuongThucThanhToan);

                if (hd.TienKhachTra <= 0)
                    hd.TienKhachTra = hd.TongTien;

                hd.TienThua = hd.TienKhachTra - hd.TongTien;
                hd.ThoiGianDong = hd.ThoiGianDong ?? DateTime.Now;
                hd.MaBan = null;
                hd.NgayCapNhat = DateTime.Now;
            }
        }

        public void DoiTrangThaiBan(int maBan, string trangThai, bool xacNhanHoaDonDaThanhToan)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                var ban = db.BanCafes.FirstOrDefault(x => x.MaBan == maBan);

                if (ban == null)
                    throw new Exception("Không tìm thấy bàn.");

                string trangThaiMoi = string.IsNullOrWhiteSpace(trangThai)
                    ? "AVAILABLE"
                    : trangThai.Trim().ToUpper();

                // Dọn dữ liệu cũ: hóa đơn đã thanh toán/hủy nhưng vẫn còn MaBan.
                // Đây chính là nguyên nhân chọn Trống xong refresh lại nhảy về Đang phục vụ.
                DonLienKetHoaDonDaDong(db, maBan);

                var hoaDonMo = db.HoaDonBans
                    .Where(x => x.MaBan == maBan)
                    .OrderByDescending(x => x.NgayLapHoaDon)
                    .ToList()
                    .FirstOrDefault(x => LaHoaDonDangMo(x.TrangThaiHoaDon, x.TrangThaiThanhToan));

                if (hoaDonMo != null && xacNhanHoaDonDaThanhToan &&
                    (trangThaiMoi == "AVAILABLE" || trangThaiMoi == "CLEANING"))
                {
                    // Cứu dữ liệu kẹt: người dùng xác nhận bill thực tế đã thu tiền.
                    // Đánh dấu hóa đơn mở là PAID/COMPLETED, cắt MaBan, sau đó set trạng thái bàn theo lựa chọn.
                    DanhDauHoaDonDangMoLaDaThanhToan(db, maBan);
                    db.SaveChanges();

                    ban = db.BanCafes.FirstOrDefault(x => x.MaBan == maBan);
                    if (ban == null)
                        throw new Exception("Không tìm thấy bàn sau khi xác nhận hóa đơn.");
                }
                else
                {
                    if (hoaDonMo != null && trangThaiMoi == "INACTIVE")
                        throw new Exception("Bàn đang có hóa đơn chưa đóng, không thể ngưng dùng.");

                    if (hoaDonMo != null && trangThaiMoi == "AVAILABLE")
                        throw new Exception("Bàn vẫn còn hóa đơn chưa thanh toán, không thể chuyển sang Trống.");
                }

                ban.TrangThai = trangThaiMoi;
                ban.NgayCapNhat = DateTime.Now;

                db.SaveChanges();
            }
        }

        public void ThemBan(string tenKhuVuc, string tenBan, int soGhe, string ghiChu)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                if (soGhe <= 0)
                    throw new Exception("Số ghế phải lớn hơn 0.");

                var khuVuc = db.KhuVucQuans
                    .Where(x => x.ConHoatDong)
                    .OrderBy(x => x.ThuTuHienThi)
                    .FirstOrDefault(x => tenKhuVuc != "Tất cả" && x.TenKhuVuc == tenKhuVuc)
                    ?? db.KhuVucQuans
                        .Where(x => x.ConHoatDong)
                        .OrderBy(x => x.ThuTuHienThi)
                        .FirstOrDefault();

                if (khuVuc == null)
                    throw new Exception("Chưa có khu vực/tầng để tạo bàn.");

                int soThuTu = db.BanCafes.Count(x => x.MaKhuVuc == khuVuc.MaKhuVuc) + 1;
                string prefix = LayPrefixKhuVuc(khuVuc.TenKhuVuc);
                string maCode = prefix + soThuTu.ToString();

                while (db.BanCafes.Any(x => x.MaCodeBan == maCode))
                {
                    soThuTu++;
                    maCode = prefix + soThuTu.ToString();
                }

                if (string.IsNullOrWhiteSpace(tenBan))
                    tenBan = "Bàn " + maCode;

                bool trungTen = db.BanCafes.Any(x =>
                    x.ConHoatDong &&
                    x.MaKhuVuc == khuVuc.MaKhuVuc &&
                    x.TenBan == tenBan.Trim());

                if (trungTen)
                    throw new Exception("Tên bàn đã tồn tại trong khu vực này.");

                var ban = db.BanCafes.Create();

                ban.MaCodeBan = maCode;
                ban.MaKhuVuc = khuVuc.MaKhuVuc;
                ban.TenBan = tenBan.Trim();
                ban.SucChua = soGhe;
                ban.TrangThai = "AVAILABLE";
                ban.GiaTriQRCode = null;
                ban.GhiChu = ghiChu;
                ban.ConHoatDong = true;
                ban.NgayTao = DateTime.Now;
                ban.NgayCapNhat = DateTime.Now;
                ban.ViTriX = 0;
                ban.ViTriY = 0;
                ban.KieuHinh = "RECT";
                ban.MaMau = "#22C55E";

                db.BanCafes.Add(ban);
                db.SaveChanges();
            }
        }

        public void XoaBan(int maBan)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                var ban = db.BanCafes.FirstOrDefault(x => x.MaBan == maBan);

                if (ban == null)
                    throw new Exception("Không tìm thấy bàn.");

                bool coHoaDonMo = db.HoaDonBans
                    .Where(x => x.MaBan == maBan)
                    .ToList()
                    .Any(x => LaHoaDonDangMo(x.TrangThaiHoaDon, x.TrangThaiThanhToan));

                if (coHoaDonMo)
                    throw new Exception("Bàn đang có hóa đơn mở, không thể xóa/ngừng dùng.");

                ban.ConHoatDong = false;
                ban.TrangThai = "INACTIVE";
                ban.NgayCapNhat = DateTime.Now;

                db.SaveChanges();
            }
        }
    }
}