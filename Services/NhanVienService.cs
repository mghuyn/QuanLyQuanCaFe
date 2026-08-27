using QuanLyQuanCaFe.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity.Validation;
using System.Text;

namespace QuanLyQuanCaFe.Services
{
    public class NhanVienService
    {
        private string ChuanHoaGioiTinh(string gioiTinh)
        {
            if (string.IsNullOrWhiteSpace(gioiTinh)) return "O";
            string gt = gioiTinh.Trim().ToLower();
            if (gt == "nam" || gt == "m" || gt == "male") return "M";
            if (gt == "nữ" || gt == "nu" || gt == "f" || gt == "female") return "F";
            return "O";
        }

        private int LayVaiTroMacDinh(QuanLyQuanCaPheDbEntities1 db)
        {
            var vaiTro = db.VaiTros
                .Where(x => x.ConHoatDong)
                .OrderBy(x => x.MaVaiTro)
                .FirstOrDefault();

            if (vaiTro == null)
                throw new Exception("Chưa có vai trò nhân viên trong database.");

            return vaiTro.MaVaiTro;
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

        public List<string> GetChucVuFilters()
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                var data = db.NhanViens
                    .Where(x => x.ChucVu != null && x.ChucVu != "")
                    .Select(x => x.ChucVu)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                data.Insert(0, "Tất cả");

                return data;
            }
        }

        public List<NhanVienItemModel> GetNhanViens(string tuKhoa, string chucVu, string trangThai)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                var query = db.NhanViens
                    .Select(x => new NhanVienItemModel
                    {
                        MaNV = x.MaNV,
                        MaNhanVien = x.MaNhanVien,

                        HoTen = x.HoTen,
                        ChucVu = x.ChucVu,
                        SoDienThoai = x.SoDienThoai,
                        Email = x.Email,

                        NgaySinh = x.NgaySinh,
                        GioiTinh = x.GioiTinh,
                        DiaChi = x.DiaChi,

                        NgayVaoLam = x.NgayVaoLam,
                        LuongCoBan = x.LuongCoBan,

                        GhiChu = x.GhiChu,
                        ConHoatDong = x.ConHoatDong
                    });

                if (!string.IsNullOrWhiteSpace(tuKhoa))
                {
                    query = query.Where(x =>
                        x.MaNhanVien.Contains(tuKhoa) ||
                        x.HoTen.Contains(tuKhoa) ||
                        x.SoDienThoai.Contains(tuKhoa) ||
                        x.Email.Contains(tuKhoa));
                }

                if (!string.IsNullOrWhiteSpace(chucVu) && chucVu != "Tất cả")
                {
                    query = query.Where(x => x.ChucVu == chucVu);
                }

                if (trangThai == "Đang làm")
                {
                    query = query.Where(x => x.ConHoatDong);
                }
                else if (trangThai == "Đã nghỉ")
                {
                    query = query.Where(x => !x.ConHoatDong);
                }

                return query
                    .OrderByDescending(x => x.ConHoatDong)
                    .ThenBy(x => x.ChucVu)
                    .ThenBy(x => x.HoTen)
                    .ToList();
            }
        }

        public string TaoMaNhanVien()
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                int soLuong = db.NhanViens.Count() + 1;
                string ma;

                do
                {
                    ma = "NV" + soLuong.ToString("0000");
                    soLuong++;
                }
                while (db.NhanViens.Any(x => x.MaNhanVien == ma));

                return ma;
            }
        }

        public void ThemNhanVien(NhanVienItemModel model)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                if (model == null)
                    throw new Exception("Dữ liệu nhân viên không hợp lệ.");

                if (string.IsNullOrWhiteSpace(model.HoTen))
                    throw new Exception("Vui lòng nhập họ tên nhân viên.");

                if (string.IsNullOrWhiteSpace(model.SoDienThoai))
                    throw new Exception("Vui lòng nhập số điện thoại.");

                if (string.IsNullOrWhiteSpace(model.ChucVu))
                    throw new Exception("Vui lòng nhập chức vụ.");

                bool trungSdt = db.NhanViens.Any(x => x.SoDienThoai == model.SoDienThoai);

                if (trungSdt)
                    throw new Exception("Số điện thoại này đã tồn tại.");

                var nv = new NhanVien();

                nv.MaNhanVien = string.IsNullOrWhiteSpace(model.MaNhanVien)
                    ? TaoMaNhanVien()
                    : model.MaNhanVien;

                nv.HoTen = model.HoTen.Trim();
                nv.ChucVu = model.ChucVu.Trim();
                nv.SoDienThoai = model.SoDienThoai.Trim();
                nv.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
                if (nv.MaVaiTro <= 0)
                    nv.MaVaiTro = LayVaiTroMacDinh(db);

                nv.NgaySinh = model.NgaySinh;
                nv.GioiTinh = ChuanHoaGioiTinh(model.GioiTinh);
                nv.DiaChi = model.DiaChi;

                nv.NgayVaoLam = model.NgayVaoLam == DateTime.MinValue
                    ? DateTime.Now.Date
                    : model.NgayVaoLam.Date;

                nv.LuongCoBan = model.LuongCoBan;
                nv.GhiChu = model.GhiChu;
                nv.ConHoatDong = true;
                nv.NgayTao = DateTime.Now;
                nv.NgayCapNhat = DateTime.Now;

                db.NhanViens.Add(nv);
                LuuThayDoi(db);
            }
        }

        public void CapNhatNhanVien(NhanVienItemModel model)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                if (model == null)
                    throw new Exception("Dữ liệu nhân viên không hợp lệ.");

                if (model.MaNV <= 0)
                    throw new Exception("Vui lòng chọn nhân viên cần cập nhật.");

                if (string.IsNullOrWhiteSpace(model.HoTen))
                    throw new Exception("Vui lòng nhập họ tên nhân viên.");

                if (string.IsNullOrWhiteSpace(model.SoDienThoai))
                    throw new Exception("Vui lòng nhập số điện thoại.");

                if (string.IsNullOrWhiteSpace(model.ChucVu))
                    throw new Exception("Vui lòng nhập chức vụ.");

                bool trungSdt = db.NhanViens.Any(x =>
                    x.SoDienThoai == model.SoDienThoai &&
                    x.MaNV != model.MaNV);

                if (trungSdt)
                    throw new Exception("Số điện thoại này đã thuộc nhân viên khác.");

                var nv = db.NhanViens.FirstOrDefault(x => x.MaNV == model.MaNV);

                if (nv == null)
                    throw new Exception("Không tìm thấy nhân viên.");

                nv.HoTen = model.HoTen.Trim();
                nv.ChucVu = model.ChucVu.Trim();
                nv.SoDienThoai = model.SoDienThoai.Trim();
                nv.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
                if (nv.MaVaiTro <= 0)
                    nv.MaVaiTro = LayVaiTroMacDinh(db);

                nv.NgaySinh = model.NgaySinh;
                nv.GioiTinh = ChuanHoaGioiTinh(model.GioiTinh);
                nv.DiaChi = model.DiaChi;

                nv.NgayVaoLam = model.NgayVaoLam.Date;
                nv.LuongCoBan = model.LuongCoBan;
                nv.GhiChu = model.GhiChu;
                nv.NgayCapNhat = DateTime.Now;

                LuuThayDoi(db);
            }
        }

        public void DoiTrangThaiNhanVien(int maNV)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                var nv = db.NhanViens.FirstOrDefault(x => x.MaNV == maNV);

                if (nv == null)
                    throw new Exception("Không tìm thấy nhân viên.");

                nv.ConHoatDong = !nv.ConHoatDong;
                nv.NgayCapNhat = DateTime.Now;

                LuuThayDoi(db);
            }
        }
        public string DiemDanhNhanVien(int maNV)
        {
            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                var nv = db.NhanViens.FirstOrDefault(x => x.MaNV == maNV);
                if (nv == null) throw new Exception("Không tìm thấy nhân viên.");

                DateTime today = DateTime.Today;
                var ca = db.CaLams
                    .Where(x => x.MaNV == maNV && x.NgayLam == today && x.TrangThai != "CANCELLED")
                    .OrderBy(x => x.GioBatDau)
                    .FirstOrDefault();

                if (ca == null)
                {
                    ca = new CaLam();
                    ca.MaNV = maNV;
                    ca.MaCodeCa = "AUTO" + maNV + DateTime.Now.ToString("yyyyMMddHHmmss");
                    ca.NgayLam = today;
                    ca.GioBatDau = DateTime.Now.TimeOfDay;
                    ca.GioKetThuc = DateTime.Now.AddHours(8).TimeOfDay;
                    ca.LoaiCa = "AUTO";
                    ca.TrangThai = "CHECKED_IN";
                    ca.GhiChu = "Tự tạo ca khi điểm danh từ màn nhân viên";
                    ca.NgayTao = DateTime.Now;
                    ca.NgayCapNhat = DateTime.Now;
                    db.CaLams.Add(ca);
                    db.SaveChanges();
                    return "Đã check-in và tự tạo ca hôm nay cho " + nv.HoTen + ".";
                }

                if (ca.TrangThai == "PLANNED")
                {
                    ca.TrangThai = "CHECKED_IN";
                    ca.NgayCapNhat = DateTime.Now;
                    db.SaveChanges();
                    return "Đã check-in ca hôm nay cho " + nv.HoTen + ".";
                }

                if (ca.TrangThai == "CHECKED_IN")
                {
                    ca.TrangThai = "COMPLETED";
                    ca.NgayCapNhat = DateTime.Now;
                    db.SaveChanges();
                    return "Đã check-out/hoàn thành ca hôm nay cho " + nv.HoTen + ".";
                }

                if (ca.TrangThai == "COMPLETED")
                    return "Ca hôm nay của " + nv.HoTen + " đã hoàn thành trước đó.";

                if (ca.TrangThai == "ABSENT")
                    throw new Exception("Ca hôm nay đã bị đánh dấu vắng, không thể điểm danh nhanh.");

                return "Trạng thái ca hiện tại: " + ca.TrangThai;
            }
        }

    }
}