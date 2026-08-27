using System;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using QuanLyQuanCaFe.Core;
using QuanLyQuanCaFe.Models;



namespace QuanLyQuanCaFe.Services
{
    public class AuthService
    {
        public bool Login(string username, string password, out string errorMessage)
        {
            errorMessage = "";

            if (string.IsNullOrWhiteSpace(username))
            {
                errorMessage = "Vui lòng nhập tên đăng nhập.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                errorMessage = "Vui lòng nhập mật khẩu.";
                return false;
            }

            using (var db = new QuanLyQuanCaPheDbEntities1())
            {
                var user = db.TaiKhoans
                    .Include(x => x.NhanVien)
                    .Include(x => x.NhanVien.VaiTro)
                    .FirstOrDefault(x => x.TenDangNhap == username);

                if (user == null)
                {
                    errorMessage = "Tên đăng nhập không tồn tại.";
                    return false;
                }

                if (user.BiKhoa)
                {
                    errorMessage = "Tài khoản đã bị khóa.";
                    return false;
                }

                if (user.NhanVien == null || !user.NhanVien.ConHoatDong)
                {
                    errorMessage = "Nhân viên không còn hoạt động.";
                    return false;
                }

                if (!VerifyPassword(password, user.MatKhauHash))
                {
                    user.SoLanDangNhapSai += 1;

                    if (user.SoLanDangNhapSai >= 5)
                    {
                        user.BiKhoa = true;
                    }

                    user.NgayCapNhat = DateTime.Now;
                    db.SaveChanges();

                    errorMessage = "Mật khẩu không đúng.";
                    return false;
                }

                user.SoLanDangNhapSai = 0;
                user.LanDangNhapCuoi = DateTime.Now;
                user.NgayCapNhat = DateTime.Now;
                db.SaveChanges();

                int maVaiTro = user.NhanVien.MaVaiTro;

                var permissionCodes = db.PhanQuyenVaiTros
                    .Where(x => x.MaVaiTro == maVaiTro && x.DuocCapQuyen)
                    .Select(x => x.Quyen.MaCodeQuyen)
                    .ToList();

                AppSession.SetCurrentUser(user, permissionCodes);
                return true;
            }
        }

        private bool VerifyPassword(string password, byte[] storedHash)
        {
            if (storedHash == null || storedHash.Length == 0)
                return false;

            // Trường hợp 1: SQL HASHBYTES với chuỗi varchar
            if (CompareBytes(storedHash, Sha256(Encoding.Default.GetBytes(password))))
                return true;

            // Trường hợp 2: C# hash UTF8
            if (CompareBytes(storedHash, Sha256(Encoding.UTF8.GetBytes(password))))
                return true;

            // Trường hợp 3: SQL HASHBYTES với chuỗi nvarchar, ví dụ N'manager123'
            if (CompareBytes(storedHash, Sha256(Encoding.Unicode.GetBytes(password))))
                return true;

            // Trường hợp 4: ASCII
            if (CompareBytes(storedHash, Sha256(Encoding.ASCII.GetBytes(password))))
                return true;

            return false;
        }

        private byte[] Sha256(byte[] input)
        {
            using (var sha = SHA256.Create())
            {
                return sha.ComputeHash(input);
            }
        }

        private bool CompareBytes(byte[] a, byte[] b)
        {
            if (a == null || b == null)
                return false;

            if (a.Length != b.Length)
                return false;

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    return false;
            }

            return true;
        }
    }
}