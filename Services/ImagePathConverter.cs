using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace QuanLyQuanCaFe.Services
{
    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string path = value?.ToString();

            if (string.IsNullOrWhiteSpace(path))
                return null;

            try
            {
                path = path.Trim().Replace("\\", "/");

                string fullPath = TimDuongDanAnh(path);

                if (string.IsNullOrWhiteSpace(fullPath))
                    return null;

                if (!File.Exists(fullPath))
                    return null;

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze();

                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        private string TimDuongDanAnh(string path)
        {
            // 1. Nếu là đường dẫn tuyệt đối: D:/... hoặc C:/...
            if (Path.IsPathRooted(path))
            {
                if (File.Exists(path))
                    return path;

                return null;
            }

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // 2. Tìm trong thư mục chạy app: bin/Debug/...
            string pathTrongBin = Path.Combine(baseDir, path);
            if (File.Exists(pathTrongBin))
                return pathTrongBin;

            // 3. Tìm lùi lên các thư mục cha của project
            DirectoryInfo dir = new DirectoryInfo(baseDir);

            for (int i = 0; i < 8 && dir != null; i++)
            {
                string candidate = Path.Combine(dir.FullName, path);

                if (File.Exists(candidate))
                    return candidate;

                dir = dir.Parent;
            }

            // 4. Fix thêm trường hợp SQL lưu "picture/Menu/..." nhưng thư mục thật là "Pictures/Menu/..."
            string pathSuaPictures = path
                .Replace("picture/", "Pictures/")
                .Replace("Picture/", "Pictures/")
                .Replace("images/", "Images/")
                .Replace("image/", "Images/");

            if (pathSuaPictures != path)
            {
                string pathTrongBin2 = Path.Combine(baseDir, pathSuaPictures);
                if (File.Exists(pathTrongBin2))
                    return pathTrongBin2;

                dir = new DirectoryInfo(baseDir);

                for (int i = 0; i < 8 && dir != null; i++)
                {
                    string candidate = Path.Combine(dir.FullName, pathSuaPictures);

                    if (File.Exists(candidate))
                        return candidate;

                    dir = dir.Parent;
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}