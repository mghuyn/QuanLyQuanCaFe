using System;
using System.Collections.Generic;
using System.Linq;
using QuanLyQuanCaFe.Models;

namespace QuanLyQuanCaFe.Services
{
    public class KitchenService
    {

        public List<RecipeProductOptionModel> GetSanPhamCongThucOptions()
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                return db.BienTheSanPhams
                    .Where(x => x.ConHoatDong && x.DangBan && x.MacDinh && x.SanPham.ConHoatDong && x.SanPham.DangBan)
                    .Select(x => new RecipeProductOptionModel
                    {
                        MaBienThe = x.MaBienThe,
                        MaSanPham = x.MaSanPham,
                        TenSanPham = x.SanPham.TenSanPham,
                        TenDanhMuc = x.SanPham.DanhMucSanPham.TenDanhMuc,
                        TenHienThi = x.SanPham.TenSanPham,
                        GiaBan = x.GiaBan
                    })
                    .OrderBy(x => x.TenDanhMuc)
                    .ThenBy(x => x.TenSanPham)
                    .ToList();
            }
        }

        public List<KhoItemModel> GetNguyenLieuCongThucOptions()
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                return db.NguyenLieus
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
                    })
                    .OrderBy(x => x.TenDanhMuc)
                    .ThenBy(x => x.TenNguyenLieu)
                    .ToList();
            }
        }

        public List<RecipeManageItemModel> GetCongThucs(int? maBienThe)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                var query = db.ChiTietCongThucs.AsQueryable();

                if (maBienThe != null && maBienThe.Value > 0)
                    query = query.Where(x => x.MaBienThe == maBienThe.Value);

                return query
                    .Select(x => new RecipeManageItemModel
                    {
                        MaBienThe = x.MaBienThe,
                        MaSanPham = x.BienTheSanPham.MaSanPham,
                        MaNguyenLieu = x.MaNguyenLieu,
                        TenSanPham = x.BienTheSanPham.SanPham.TenSanPham,
                        TenDanhMuc = x.BienTheSanPham.SanPham.DanhMucSanPham.TenDanhMuc,
                        TenNguyenLieu = x.NguyenLieu.TenNguyenLieu,
                        TenDonVi = x.NguyenLieu.DonViTinh.TenDonVi,
                        SoLuongCan = x.SoLuongCan,
                        PhanTramHaoHut = x.PhanTramHaoHut,
                        GhiChu = x.GhiChu
                    })
                    .OrderBy(x => x.TenDanhMuc)
                    .ThenBy(x => x.TenSanPham)
                    .ThenBy(x => x.TenNguyenLieu)
                    .ToList();
            }
        }

        public void LuuCongThuc(int maBienThe, int maNguyenLieu, decimal soLuongCan, decimal phanTramHaoHut, string ghiChu)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                if (maBienThe <= 0) throw new Exception("Vui lòng chọn món cần khai báo công thức.");
                if (maNguyenLieu <= 0) throw new Exception("Vui lòng chọn nguyên liệu.");
                if (soLuongCan <= 0) throw new Exception("Số lượng nguyên liệu phải lớn hơn 0.");
                if (phanTramHaoHut < 0 || phanTramHaoHut > 100) throw new Exception("Phần trăm hao hụt phải từ 0 đến 100.");

                var bienThe = db.BienTheSanPhams.FirstOrDefault(x => x.MaBienThe == maBienThe && x.ConHoatDong && x.DangBan);
                if (bienThe == null) throw new Exception("Món đã chọn không còn tồn tại hoặc đang ngừng bán.");

                var nguyenLieu = db.NguyenLieus.FirstOrDefault(x => x.MaNguyenLieu == maNguyenLieu && x.ConHoatDong);
                if (nguyenLieu == null) throw new Exception("Nguyên liệu đã chọn không còn tồn tại.");

                var ct = db.ChiTietCongThucs.FirstOrDefault(x => x.MaBienThe == maBienThe && x.MaNguyenLieu == maNguyenLieu);
                if (ct == null)
                {
                    ct = db.ChiTietCongThucs.Create();
                    ct.MaBienThe = maBienThe;
                    ct.MaNguyenLieu = maNguyenLieu;
                    db.ChiTietCongThucs.Add(ct);
                }

                ct.SoLuongCan = soLuongCan;
                ct.PhanTramHaoHut = phanTramHaoHut;
                ct.GhiChu = ghiChu ?? "";

                db.SaveChanges();
            }
        }

        public void XoaCongThuc(int maBienThe, int maNguyenLieu)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                var ct = db.ChiTietCongThucs.FirstOrDefault(x => x.MaBienThe == maBienThe && x.MaNguyenLieu == maNguyenLieu);
                if (ct == null) throw new Exception("Không tìm thấy dòng công thức cần xóa.");
                db.ChiTietCongThucs.Remove(ct);
                db.SaveChanges();
            }
        }

        public List<KitchenOrderItemModel> GetMonCanPhaChe()
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                DateTime today = DateTime.Today;
                DateTime tomorrow = today.AddDays(1);

                var data = db.ChiTietHoaDonBans
                    .Where(x =>
                        x.TrangThaiMon == "NEW" ||
                        x.TrangThaiMon == "DOING" ||
                        x.TrangThaiMon == "PREPARING" ||
                        (x.TrangThaiMon == "DONE" && x.HoanThanhLuc >= today && x.HoanThanhLuc < tomorrow))
                    .Select(x => new KitchenOrderItemModel
                    {
                        MaChiTietHoaDonBan = x.MaChiTietHoaDonBan,
                        MaChiTietHoaDonBanList = x.MaChiTietHoaDonBan.ToString(),
                        MaHoaDonBan = x.MaHoaDonBan,
                        MaBienThe = x.MaBienThe,
                        MaHoaDon = x.HoaDonBan.MaHoaDon,
                        TenMon = x.BienTheSanPham.SanPham.TenSanPham,
                        SoLuong = x.SoLuong,
                        TrangThaiMon = x.TrangThaiMon == "PREPARING" ? "DOING" : x.TrangThaiMon,
                        GhiChu = x.YeuCauDacBiet,
                        GhiChuHoaDon = x.HoaDonBan.GhiChuHoaDon ?? x.HoaDonBan.GhiChu,
                        TenBan = x.HoaDonBan.BanCafe != null ? x.HoaDonBan.BanCafe.TenBan : "",
                        LoaiHoaDon = x.HoaDonBan.LoaiHoaDon,
                        NgayLapHoaDon = x.HoaDonBan.NgayLapHoaDon,
                        BatDauLuc = x.BatDauLuc,
                        HoanThanhLuc = x.HoanThanhLuc
                    })
                    .OrderBy(x => x.TrangThaiMon == "NEW" ? 0 : x.TrangThaiMon == "DOING" ? 1 : 2)
                    .ThenBy(x => x.NgayLapHoaDon)
                    .ThenBy(x => x.MaChiTietHoaDonBan)
                    .Take(120)
                    .ToList();

                var maBienThes = data.Select(x => x.MaBienThe).Distinct().ToList();
                var congThucData = db.ChiTietCongThucs
                    .Where(x => maBienThes.Contains(x.MaBienThe))
                    .Select(x => new
                    {
                        x.MaBienThe,
                        TenNguyenLieu = x.NguyenLieu.TenNguyenLieu,
                        x.SoLuongCan,
                        DonVi = x.NguyenLieu.DonViTinh.TenDonVi
                    })
                    .ToList();

                foreach (var item in data)
                {
                    var ct = congThucData
                        .Where(x => x.MaBienThe == item.MaBienThe)
                        .Select(x => x.TenNguyenLieu + " " + x.SoLuongCan.ToString("N2") + " " + x.DonVi)
                        .ToList();

                    item.CongThucText = ct.Count == 0 ? "Chưa khai báo công thức" : string.Join("; ", ct);
                }

                return data;
            }
        }

        public void CapNhatTrangThaiMon(string maChiTietHoaDonBanList, string trangThaiMoi)
        {
            if (string.IsNullOrWhiteSpace(maChiTietHoaDonBanList))
                throw new Exception("Không có món cần cập nhật.");

            var ids = maChiTietHoaDonBanList
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => int.Parse(x))
                .ToList();

            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                var items = db.ChiTietHoaDonBans.Where(x => ids.Contains(x.MaChiTietHoaDonBan)).ToList();
                if (items.Count == 0) throw new Exception("Không tìm thấy món cần cập nhật.");

                string trangThaiLuu = trangThaiMoi == "PREPARING" ? "DOING" : trangThaiMoi;

                foreach (var item in items)
                {
                    if (item.TrangThaiMon == "CANCELLED")
                        continue;

                    if (item.TrangThaiMon == "DONE" && trangThaiLuu != "DONE")
                        throw new Exception("Món đã hoàn tất, không thể đổi ngược trạng thái.");

                    item.TrangThaiMon = trangThaiLuu;
                    if (trangThaiLuu == "DOING") item.BatDauLuc = DateTime.Now;
                    if (trangThaiLuu == "DONE") item.HoanThanhLuc = DateTime.Now;
                }

                CapNhatHoaDonSauKhiDoiMon(db, items.Select(x => x.MaHoaDonBan).Distinct().ToList());
                db.SaveChanges();
            }
        }

        public void HuyMon(string maChiTietHoaDonBanList, string lyDo)
        {
            if (string.IsNullOrWhiteSpace(maChiTietHoaDonBanList))
                throw new Exception("Không có món cần hủy.");

            var ids = maChiTietHoaDonBanList
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => int.Parse(x))
                .ToList();

            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                var items = db.ChiTietHoaDonBans.Where(x => ids.Contains(x.MaChiTietHoaDonBan)).ToList();
                if (items.Count == 0) throw new Exception("Không tìm thấy món cần hủy.");

                foreach (var item in items)
                {
                    if (item.TrangThaiMon == "DONE")
                        throw new Exception("Món đã hoàn tất, không nên hủy từ pha chế.");

                    if (item.TrangThaiMon == "CANCELLED")
                        continue;

                    HoanNguyenLieuChoMon(db, item.MaBienThe, item.SoLuong, item.HoaDonBan.MaHoaDon);

                    item.TrangThaiMon = "CANCELLED";
                    item.LyDoHuy = string.IsNullOrWhiteSpace(lyDo) ? "Hủy món từ màn pha chế" : lyDo;
                }

                CapNhatHoaDonSauKhiDoiMon(db, items.Select(x => x.MaHoaDonBan).Distinct().ToList());
                db.SaveChanges();
            }
        }

        private void HoanNguyenLieuChoMon(QuanLyQuanCaPheDbEntities1 db, int maBienThe, int soLuongMon, string maHoaDon)
        {
            var congThucs = db.ChiTietCongThucs.Where(x => x.MaBienThe == maBienThe).ToList();

            foreach (var ct in congThucs)
            {
                var nguyenLieu = db.NguyenLieus.FirstOrDefault(x => x.MaNguyenLieu == ct.MaNguyenLieu);
                if (nguyenLieu == null) continue;

                decimal soLuongHoan = ct.SoLuongCan * soLuongMon;
                if (ct.PhanTramHaoHut > 0) soLuongHoan += soLuongHoan * ct.PhanTramHaoHut / 100;

                nguyenLieu.SoLuongHienTai += soLuongHoan;
                nguyenLieu.NgayCapNhat = DateTime.Now;

                var gd = db.GiaoDichKhos.Create();
                gd.MaNguyenLieu = nguyenLieu.MaNguyenLieu;
                gd.LoaiGiaoDich = "RETURN";
                gd.LoaiThamChieu = "HUY_MON_PHA_CHE";
                gd.MaThamChieu = null;
                gd.SoLuongThayDoi = soLuongHoan;
                gd.DonGiaVon = nguyenLieu.GiaNhapCuoi;
                gd.NgayGiaoDich = DateTime.Now;
                gd.GhiChu = "Hoàn kho do hủy món của hóa đơn " + maHoaDon;
                db.GiaoDichKhos.Add(gd);
            }
        }


        private void DatBanDangPhucVuNeuCo(QuanLyQuanCaPheDbEntities1 db, HoaDonBan hoaDon)
        {
            if (hoaDon == null || hoaDon.MaBan == null) return;
            var ban = db.BanCafes.FirstOrDefault(x => x.MaBan == hoaDon.MaBan.Value);
            if (ban != null && ban.TrangThai != "OCCUPIED")
            {
                ban.TrangThai = "OCCUPIED";
                ban.NgayCapNhat = DateTime.Now;
            }
        }

        private void CapNhatHoaDonSauKhiDoiMon(QuanLyQuanCaPheDbEntities1 db, List<int> maHoaDons)
        {
            foreach (var maHoaDon in maHoaDons)
            {
                var hoaDon = db.HoaDonBans.FirstOrDefault(x => x.MaHoaDonBan == maHoaDon);
                if (hoaDon == null || hoaDon.TrangThaiHoaDon == "COMPLETED" || hoaDon.TrangThaiHoaDon == "CANCELLED")
                    continue;

                var cacMon = hoaDon.ChiTietHoaDonBans.Where(x => x.TrangThaiMon != "CANCELLED").ToList();

                if (cacMon.Count == 0)
                {
                    hoaDon.TrangThaiHoaDon = "CANCELLED";
                    hoaDon.TrangThaiThanhToan = "CANCELLED";
                    hoaDon.LyDoHuy = "Tất cả món trong bill đã bị hủy từ pha chế.";
                    if (hoaDon.MaBan != null)
                    {
                        var ban = db.BanCafes.FirstOrDefault(x => x.MaBan == hoaDon.MaBan.Value);
                        if (ban != null) ban.TrangThai = "AVAILABLE";
                    }
                }
                else if (cacMon.All(x => x.TrangThaiMon == "DONE"))
                {
                    hoaDon.TrangThaiHoaDon = "READY";
                    DatBanDangPhucVuNeuCo(db, hoaDon);
                }
                else if (cacMon.Any(x => x.TrangThaiMon == "DOING" || x.TrangThaiMon == "PREPARING" || x.TrangThaiMon == "DONE"))
                {
                    hoaDon.TrangThaiHoaDon = "PREPARING";
                    DatBanDangPhucVuNeuCo(db, hoaDon);
                }
                else
                {
                    hoaDon.TrangThaiHoaDon = "WAITING_KITCHEN";
                    DatBanDangPhucVuNeuCo(db, hoaDon);
                }

                hoaDon.NgayCapNhat = DateTime.Now;
            }
        }
    }
}
