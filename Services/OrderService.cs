using System;
using System.Collections.Generic;
using System.Linq;
using QuanLyQuanCaFe.Core;
using QuanLyQuanCaFe.Models;
using System.Data.Entity.Validation;
using System.Text;
using System.Data.Entity.Infrastructure;

namespace QuanLyQuanCaFe.Services
{
    public class OrderService
    {
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
            catch (DbUpdateException ex)
            {
                string message = ex.Message;

                if (ex.InnerException != null)
                    message += "\nInner 1: " + ex.InnerException.Message;

                if (ex.InnerException != null && ex.InnerException.InnerException != null)
                    message += "\nInner 2: " + ex.InnerException.InnerException.Message;

                throw new Exception(message);
            }
        }

        private void CapNhatTrangThaiBan(QuanLyQuanCaPheDbEntities1 db, int? maBan, string trangThai)
        {
            if (maBan == null)
                return;

            var ban = db.BanCafes.FirstOrDefault(x => x.MaBan == maBan.Value);

            if (ban == null)
                return;

            ban.TrangThai = trangThai;
            ban.NgayCapNhat = DateTime.Now;
        }

        private void TatTriggerTuDongTruKhoNeuCo(QuanLyQuanCaPheDbEntities1 db)
        {
            try
            {
                db.Database.ExecuteSqlCommand("IF OBJECT_ID('dbo.trg_HoaDonBans_AutoConsumeInventory', 'TR') IS NOT NULL DISABLE TRIGGER dbo.trg_HoaDonBans_AutoConsumeInventory ON dbo.HoaDonBans");
            }
            catch
            {
                // Nếu tài khoản SQL không có quyền DISABLE TRIGGER thì bỏ qua.
                // File Data/04_POS_Status_Constraint_Patch.sql sẽ DROP trigger này khi dựng DB chuẩn.
            }
        }

        private void TatTriggerTuDongCapNhatTrangThaiBanNeuCo(QuanLyQuanCaPheDbEntities1 db)
        {
            try
            {
                db.Database.ExecuteSqlCommand("IF OBJECT_ID('dbo.trg_HoaDonBans_UpdateTableStatus', 'TR') IS NOT NULL DISABLE TRIGGER dbo.trg_HoaDonBans_UpdateTableStatus ON dbo.HoaDonBans");
            }
            catch
            {
                // Trigger cũ tự đổi bàn về AVAILABLE khi hóa đơn bị ngắt MaBan.
                // Nếu không đủ quyền DISABLE TRIGGER thì chạy file Data/09_Fix_Table_Status_Workflow.sql trong SQL Server.
            }
        }

        private bool HoaDonDaTruKho(QuanLyQuanCaPheDbEntities1 db, int maHoaDonBan)
        {
            return db.GiaoDichKhos.Any(x =>
                x.MaThamChieu == maHoaDonBan &&
                (x.LoaiThamChieu == "HOA_DON_BAN" || x.LoaiThamChieu == "SALES_ORDER") &&
                (x.LoaiGiaoDich == "SALE_CONSUMPTION" || x.LoaiGiaoDich == "SALE" || x.LoaiGiaoDich == "SALE_OUT"));
        }

        private void TruNguyenLieuTheoChiTietHoaDon(QuanLyQuanCaPheDbEntities1 db, HoaDonBan hoaDon)
        {
            if (hoaDon == null)
                throw new Exception("Không tìm thấy hóa đơn để trừ kho.");

            var gioHang = hoaDon.ChiTietHoaDonBans
                .Where(x => x.TrangThaiMon != "CANCELLED")
                .GroupBy(x => x.MaBienThe)
                .Select(g => new CartItemModel
                {
                    MaBienThe = g.Key,
                    SoLuong = g.Sum(x => x.SoLuong),
                    DonGia = g.FirstOrDefault() != null ? g.FirstOrDefault().DonGia : 0,
                    TenSanPham = g.FirstOrDefault() != null && g.FirstOrDefault().BienTheSanPham != null && g.FirstOrDefault().BienTheSanPham.SanPham != null ? g.FirstOrDefault().BienTheSanPham.SanPham.TenSanPham : "Món"
                })
                .ToList();

            if (gioHang.Count > 0)
                TruNguyenLieuTheoCongThuc(db, hoaDon.MaHoaDonBan, hoaDon.MaHoaDon, gioHang);
        }

        public string ThanhToan(List<CartItemModel> gioHang, decimal tienKhachTra = 0)
        {
            return TaoHoaDon(gioHang, null, null, "TAKE_AWAY", "COMPLETED", "PAID", "CASH", tienKhachTra, null, true);
        }

        public string LuuTam(List<CartItemModel> gioHang, int? maBan, int? maKH, string loaiHoaDon, string ghiChu)
        {
            return TaoHoaDon(gioHang, maBan, maKH, loaiHoaDon, "DRAFT", "UNPAID", "", 0, ghiChu, false);
        }

        public string GuiPhaChe(List<CartItemModel> gioHang, int? maBan, int? maKH, string loaiHoaDon, string ghiChu)
        {
            return TaoHoaDon(gioHang, maBan, maKH, loaiHoaDon, "WAITING_KITCHEN", "UNPAID", "", 0, ghiChu, false);
        }

        public string ThanhToanMoi(List<CartItemModel> gioHang, int? maBan, int? maKH, string loaiHoaDon, string ghiChu, string phuongThuc, decimal tienKhachTra)
        {
            return TaoHoaDon(gioHang, maBan, maKH, loaiHoaDon, "COMPLETED", "PAID", phuongThuc, tienKhachTra, ghiChu, true);
        }

        public void ThanhToanHoaDonDaCo(int maHoaDonBan, string phuongThuc, decimal tienKhachTra)
        {
            if (AppSession.CurrentUser == null || AppSession.CurrentUser.NhanVien == null)
                throw new Exception("Không tìm thấy thông tin nhân viên đăng nhập.");

            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                TatTriggerTuDongCapNhatTrangThaiBanNeuCo(db);

                var hoaDon = db.HoaDonBans.FirstOrDefault(x => x.MaHoaDonBan == maHoaDonBan);

                if (hoaDon == null)
                    throw new Exception("Không tìm thấy hóa đơn.");

                if (hoaDon.TrangThaiHoaDon == "CANCELLED")
                    throw new Exception("Hóa đơn đã hủy, không thể thanh toán.");

                if (hoaDon.TrangThaiThanhToan == "PAID" || hoaDon.TrangThaiHoaDon == "COMPLETED")
                    throw new Exception("Hóa đơn này đã thanh toán.");

                if (tienKhachTra <= 0)
                    tienKhachTra = hoaDon.TongTien;

                // DB cũ có trigger tự trừ kho khi hóa đơn chuyển COMPLETED.
                // POS mới đã trừ kho ở bước Gửi pha chế/Thanh toán ngay nên tắt trigger này để tránh lỗi constraint và trừ kho 2 lần.
                TatTriggerTuDongTruKhoNeuCo(db);

                // Nếu đây là hóa đơn lưu tạm chưa từng trừ kho thì thanh toán mới trừ kho một lần.
                if (!HoaDonDaTruKho(db, hoaDon.MaHoaDonBan))
                    TruNguyenLieuTheoChiTietHoaDon(db, hoaDon);

                hoaDon.TrangThaiHoaDon = "COMPLETED";
                hoaDon.TrangThaiThanhToan = "PAID";
                hoaDon.PhuongThucThanhToan = string.IsNullOrWhiteSpace(phuongThuc) ? "CASH" : phuongThuc;
                hoaDon.TienKhachTra = tienKhachTra;
                hoaDon.TienThua = tienKhachTra - hoaDon.TongTien;
                hoaDon.ThoiGianDong = DateTime.Now;
                hoaDon.NgayCapNhat = DateTime.Now;

                foreach (var item in hoaDon.ChiTietHoaDonBans)
                {
                    if (item.TrangThaiMon != "CANCELLED")
                        item.TrangThaiMon = "DONE";
                }

                // Thanh toán tại bàn: bàn không được trả về Trống ngay.
                // Đúng nghiệp vụ: thanh toán xong -> Cần dọn, sau khi nhân viên dọn mới bấm Trống.
                int? maBanDaThanhToan = hoaDon.MaBan;
                CapNhatTrangThaiBan(db, maBanDaThanhToan, "CLEANING");

                // Cắt liên kết bill khỏi bàn để bàn không còn hóa đơn đang phục vụ.
                hoaDon.MaBan = null;

                SaveChangesWithValidationMessage(db);
            }
        }

        public void HuyHoaDon(int maHoaDonBan, string lyDo)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                TatTriggerTuDongCapNhatTrangThaiBanNeuCo(db);

                var hoaDon = db.HoaDonBans.FirstOrDefault(x => x.MaHoaDonBan == maHoaDonBan);

                if (hoaDon == null)
                    throw new Exception("Không tìm thấy hóa đơn.");

                if (hoaDon.TrangThaiThanhToan == "PAID" || hoaDon.TrangThaiHoaDon == "COMPLETED")
                    throw new Exception("Hóa đơn đã thanh toán, không thể hủy.");

                if (hoaDon.TrangThaiHoaDon != "DRAFT")
                    throw new Exception("Chỉ được hủy bill khi hóa đơn còn ở trạng thái Lưu tạm. Hóa đơn đã gửi pha chế/đang làm/sẵn sàng thì không được hủy để tránh sai kho và sai quy trình pha chế.");

                bool daCoMonDaPhaChe = hoaDon.ChiTietHoaDonBans.Any(x =>
                    x.TrangThaiMon == "DOING" ||
                    x.TrangThaiMon == "DONE" ||
                    x.TrangThaiMon == "PREPARING" ||
                    x.TrangThaiMon == "READY");

                if (daCoMonDaPhaChe)
                    throw new Exception("Không thể hủy bill vì đã có món được pha chế/xử lý.");

                hoaDon.TrangThaiHoaDon = "CANCELLED";
                hoaDon.TrangThaiThanhToan = "CANCELLED";
                hoaDon.LyDoHuy = string.IsNullOrWhiteSpace(lyDo) ? "Hủy từ màn bán hàng" : lyDo;
                hoaDon.NgayCapNhat = DateTime.Now;

                if (AppSession.CurrentUser != null && AppSession.CurrentUser.NhanVien != null)
                    hoaDon.MaNhanVienHuy = AppSession.CurrentUser.NhanVien.MaNV;

                foreach (var item in hoaDon.ChiTietHoaDonBans)
                {
                    item.TrangThaiMon = "CANCELLED";
                    item.LyDoHuy = hoaDon.LyDoHuy;
                }

                CapNhatTrangThaiBan(db, hoaDon.MaBan, "AVAILABLE");

                SaveChangesWithValidationMessage(db);
            }
        }

        private string TaoHoaDon(List<CartItemModel> gioHang, int? maBan, int? maKH, string loaiHoaDon, string trangThaiHoaDon, string trangThaiThanhToan, string phuongThuc, decimal tienKhachTra, string ghiChu, bool dongHoaDon)
        {
            if (gioHang == null || gioHang.Count == 0)
                throw new Exception("Giỏ hàng đang trống.");

            if (AppSession.CurrentUser == null || AppSession.CurrentUser.NhanVien == null)
                throw new Exception("Không tìm thấy thông tin nhân viên đăng nhập.");

            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                TatTriggerTuDongCapNhatTrangThaiBanNeuCo(db);

                decimal tienHang = gioHang.Sum(x => x.ThanhTien);
                decimal tongTien = tienHang;

                if (tienKhachTra <= 0 && trangThaiThanhToan == "PAID")
                    tienKhachTra = tongTien;

                string maHoaDon = TaoMaHoaDon();

                var hoaDon = db.HoaDonBans.Create();

                hoaDon.MaHoaDon = maHoaDon;
                hoaDon.MaBan = loaiHoaDon == "DINE_IN" ? maBan : null;
                hoaDon.MaKH = maKH;
                hoaDon.MaThuNgan = AppSession.CurrentUser.NhanVien.MaNV;
                hoaDon.MaDatBan = null;
                hoaDon.MaKhuyenMaiApDung = null;

                hoaDon.LoaiHoaDon = string.IsNullOrWhiteSpace(loaiHoaDon) ? "TAKE_AWAY" : loaiHoaDon;
                hoaDon.TrangThaiHoaDon = trangThaiHoaDon;
                hoaDon.TrangThaiThanhToan = trangThaiThanhToan;
                hoaDon.PhuongThucThanhToan = string.IsNullOrWhiteSpace(phuongThuc) ? null : phuongThuc;
                hoaDon.KenhNguon = "POS";

                hoaDon.NgayLapHoaDon = DateTime.Now;
                hoaDon.ThoiGianDong = dongHoaDon ? (DateTime?)DateTime.Now : null;

                hoaDon.TienHang = tienHang;
                hoaDon.TienTopping = 0;
                hoaDon.TienGiam = 0;
                hoaDon.TienThue = 0;
                hoaDon.PhiDichVu = 0;
                hoaDon.TongTien = tongTien;
                hoaDon.TienKhachTra = trangThaiThanhToan == "PAID" ? tienKhachTra : 0;
                hoaDon.TienThua = trangThaiThanhToan == "PAID" ? tienKhachTra - tongTien : 0;

                hoaDon.SoKhach = 1;
                hoaDon.GhiChu = ghiChu;
                hoaDon.GhiChuHoaDon = ghiChu;
                hoaDon.NgayTao = DateTime.Now;
                hoaDon.NgayCapNhat = DateTime.Now;

                db.HoaDonBans.Add(hoaDon);
                SaveChangesWithValidationMessage(db);

                foreach (var item in gioHang)
                {
                    int soLuongChiTiet = item.SoLuong <= 0 ? 1 : item.SoLuong;

                    // Tách mỗi ly/món thành một dòng chi tiết riêng để pha chế xử lý độc lập.
                    // Ví dụ 2 ly cà phê: có thể hoàn tất/hủy từng ly, ghi chú từng dòng rõ hơn.
                    for (int i = 0; i < soLuongChiTiet; i++)
                    {
                        var chiTiet = db.ChiTietHoaDonBans.Create();

                        chiTiet.MaHoaDonBan = hoaDon.MaHoaDonBan;
                        chiTiet.MaBienThe = item.MaBienThe;
                        chiTiet.SoLuong = 1;
                        chiTiet.DonGia = item.DonGia;
                        chiTiet.TienGiam = 0;
                        chiTiet.ThanhTien = item.DonGia;
                        chiTiet.TrangThaiMon = trangThaiHoaDon == "DRAFT" ? "DRAFT" : (trangThaiHoaDon == "COMPLETED" ? "DONE" : "NEW");
                        chiTiet.YeuCauDacBiet = item.GhiChu;
                        chiTiet.MaNhanVienPhaChe = null;

                        db.ChiTietHoaDonBans.Add(chiTiet);
                    }
                }

                if (trangThaiHoaDon != "DRAFT" && trangThaiHoaDon != "CANCELLED")
                {
                    TruNguyenLieuTheoCongThuc(db, hoaDon.MaHoaDonBan, maHoaDon, gioHang);
                }

                if (trangThaiHoaDon == "COMPLETED")
                    TatTriggerTuDongTruKhoNeuCo(db);

                if (hoaDon.LoaiHoaDon == "DINE_IN")
                {
                    if (trangThaiHoaDon == "COMPLETED")
                    {
                        // Thanh toán ngay tại bàn: giữ bàn ở Cần dọn, không trả về Trống.
                        int? maBanDaThanhToan = hoaDon.MaBan;
                        CapNhatTrangThaiBan(db, maBanDaThanhToan, "CLEANING");

                        // Cắt liên kết bill khỏi bàn để nhân viên có thể dọn xong rồi bấm Trống.
                        hoaDon.MaBan = null;
                    }
                    else if (trangThaiHoaDon == "CANCELLED")
                        CapNhatTrangThaiBan(db, hoaDon.MaBan, "AVAILABLE");
                    else if (trangThaiHoaDon == "DRAFT")
                        CapNhatTrangThaiBan(db, hoaDon.MaBan, "RESERVED");
                    else
                        CapNhatTrangThaiBan(db, hoaDon.MaBan, "OCCUPIED");
                }

                SaveChangesWithValidationMessage(db);

                return maHoaDon;
            }
        }

        private void TruNguyenLieuTheoCongThuc(QuanLyQuanCaPheDbEntities1 db, int maHoaDonBan, string maHoaDon, List<CartItemModel> gioHang)
        {
            foreach (var item in gioHang)
            {
                var congThucs = db.ChiTietCongThucs
                    .Where(x => x.MaBienThe == item.MaBienThe)
                    .ToList();

                foreach (var ct in congThucs)
                {
                    var nguyenLieu = db.NguyenLieus.FirstOrDefault(x => x.MaNguyenLieu == ct.MaNguyenLieu);

                    if (nguyenLieu == null)
                        throw new Exception("Không tìm thấy nguyên liệu trong công thức.");

                    decimal soLuongCan = ct.SoLuongCan * item.SoLuong;

                    if (ct.PhanTramHaoHut > 0)
                    {
                        soLuongCan += soLuongCan * ct.PhanTramHaoHut / 100;
                    }

                    if (nguyenLieu.SoLuongHienTai < soLuongCan)
                    {
                        throw new Exception("Không đủ tồn kho nguyên liệu: " + nguyenLieu.TenNguyenLieu);
                    }

                    nguyenLieu.SoLuongHienTai -= soLuongCan;
                    nguyenLieu.NgayCapNhat = DateTime.Now;

                    var giaoDich = db.GiaoDichKhos.Create();
                    giaoDich.MaNguyenLieu = nguyenLieu.MaNguyenLieu;
                    giaoDich.LoaiGiaoDich = "SALE_CONSUMPTION";
                    giaoDich.LoaiThamChieu = "HOA_DON_BAN";
                    giaoDich.MaThamChieu = maHoaDonBan;
                    giaoDich.SoLuongThayDoi = -soLuongCan;
                    giaoDich.DonGiaVon = nguyenLieu.GiaNhapCuoi;
                    giaoDich.NgayGiaoDich = DateTime.Now;
                    giaoDich.MaNhanVienTao = AppSession.CurrentUser != null && AppSession.CurrentUser.NhanVien != null
                        ? (int?)AppSession.CurrentUser.NhanVien.MaNV
                        : null;
                    giaoDich.GhiChu = "Xuất kho tự động từ hóa đơn " + maHoaDon;

                    db.GiaoDichKhos.Add(giaoDich);
                }
            }
        }

        private string TaoMaHoaDon()
        {
            return "HD" + DateTime.Now.ToString("yyyyMMddHHmmss");
        }
    }
}