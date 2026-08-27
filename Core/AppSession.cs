using System.Collections.Generic;
using System.Linq;
using QuanLyQuanCaFe.Models;

namespace QuanLyQuanCaFe.Core
{
    public static class AppSession
    {
        public static TaiKhoan CurrentUser { get; private set; }

        public static List<string> PermissionCodes { get; private set; } = new List<string>();

        public static string TenDangNhap => CurrentUser?.TenDangNhap;

        public static string HoTenNhanVien => CurrentUser?.NhanVien?.HoTen;

        public static string MaCodeVaiTro => CurrentUser?.NhanVien?.VaiTro?.MaCodeVaiTro;

        public static string TenVaiTro => CurrentUser?.NhanVien?.VaiTro?.TenVaiTro;

        public static void SetCurrentUser(TaiKhoan user, List<string> permissionCodes)
        {
            CurrentUser = user;
            PermissionCodes = permissionCodes ?? new List<string>();
        }

        public static bool HasPermission(string permissionCode)
        {
            if (string.IsNullOrWhiteSpace(permissionCode))
                return true;

            return PermissionCodes.Any(x => x == permissionCode);
        }

        public static void Clear()
        {
            CurrentUser = null;
            PermissionCodes.Clear();
        }
    }
}