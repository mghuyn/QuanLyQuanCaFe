using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using QuanLyQuanCaFe.Commands;
using QuanLyQuanCaFe.Core;
using QuanLyQuanCaFe.Models;
using QuanLyQuanCaFe.Services;

namespace QuanLyQuanCaFe.ViewModels
{
    public class SanPhamTestViewModel : BaseViewModel
    {
        private readonly MenuManagementService _service;

        private string _tuKhoa;
        private string _danhMucDangChon;
        private string _trangThaiDangChon;
        private string _cheDoForm;

        private MenuProductModel _sanPhamDangChon;

        private string _tenSanPhamNhap;
        private string _moTaNhap;
        private string _danhMucNhap;
        private string _duongDanAnhDangChon;
        private decimal _giaBanNhap;
        private string _thongBao;

        public ObservableCollection<string> DanhMucs { get; set; }
        public ObservableCollection<string> TrangThaiFilters { get; set; }
        public ObservableCollection<MenuProductModel> SanPhams { get; set; }

        public string TuKhoa
        {
            get => _tuKhoa;
            set
            {
                SetProperty(ref _tuKhoa, value);
                LoadSanPham();
            }
        }

        public string DanhMucDangChon
        {
            get => _danhMucDangChon;
            set
            {
                SetProperty(ref _danhMucDangChon, value);
                LoadSanPham();
            }
        }

        public string TrangThaiDangChon
        {
            get => _trangThaiDangChon;
            set
            {
                SetProperty(ref _trangThaiDangChon, value);
                LoadSanPham();
            }
        }

        public string CheDoForm
        {
            get => _cheDoForm;
            set
            {
                SetProperty(ref _cheDoForm, value);
                OnPropertyChanged(nameof(TieuDePanel));
                OnPropertyChanged(nameof(TenNutLuu));
            }
        }

        public string TieuDePanel
        {
            get
            {
                if (CheDoForm == "ADD")
                    return "Thêm sản phẩm";

                if (CheDoForm == "EDIT")
                    return "Cập nhật sản phẩm";

                return "Chi tiết sản phẩm";
            }
        }

        public string TenNutLuu
        {
            get
            {
                if (CheDoForm == "ADD")
                    return "Thêm sản phẩm";

                return "Lưu thay đổi";
            }
        }

        public MenuProductModel SanPhamDangChon
        {
            get => _sanPhamDangChon;
            set
            {
                SetProperty(ref _sanPhamDangChon, value);
                FillForm(value);
            }
        }

        public string TenSanPhamNhap
        {
            get => _tenSanPhamNhap;
            set => SetProperty(ref _tenSanPhamNhap, value);
        }

        public string MoTaNhap
        {
            get => _moTaNhap;
            set => SetProperty(ref _moTaNhap, value);
        }

        public string DanhMucNhap
        {
            get => _danhMucNhap;
            set => SetProperty(ref _danhMucNhap, value);
        }

        public string DuongDanAnhDangChon
        {
            get => _duongDanAnhDangChon;
            set => SetProperty(ref _duongDanAnhDangChon, value);
        }

        public decimal GiaBanNhap
        {
            get => _giaBanNhap;
            set => SetProperty(ref _giaBanNhap, value);
        }

        public string ThongBao
        {
            get => _thongBao;
            set => SetProperty(ref _thongBao, value);
        }

        public ICommand ChonSanPhamCommand { get; set; }
        public ICommand BatDauThemCommand { get; set; }
        public ICommand BatDauSuaCommand { get; set; }
        public ICommand ChonAnhCommand { get; set; }
        public ICommand LuuCommand { get; set; }
        public ICommand DoiTrangThaiCommand { get; set; }
        public ICommand ResetCommand { get; set; }

        public SanPhamTestViewModel()
        {
            _service = new MenuManagementService();

            DanhMucs = new ObservableCollection<string>();
            TrangThaiFilters = new ObservableCollection<string>
            {
                "Tất cả",
                "Đang bán",
                "Ngưng bán"
            };

            SanPhams = new ObservableCollection<MenuProductModel>();

            ChonSanPhamCommand = new RelayCommand(p => ChonSanPham(p as MenuProductModel));
            BatDauThemCommand = new RelayCommand(p => BatDauThem());
            BatDauSuaCommand = new RelayCommand(p => BatDauSua());
            ChonAnhCommand = new RelayCommand(p => ChonAnh());
            LuuCommand = new RelayCommand(p => Luu());
            DoiTrangThaiCommand = new RelayCommand(p => DoiTrangThai());
            ResetCommand = new RelayCommand(p => Reset());

            CheDoForm = "DETAIL";
            TrangThaiDangChon = "Đang bán";

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                DanhMucs.Clear();

                foreach (var item in _service.GetDanhMucNames())
                {
                    DanhMucs.Add(item);
                }

                DanhMucDangChon = "Tất cả";
                DanhMucNhap = DanhMucs.FirstOrDefault(x => x != "Tất cả");

                LoadSanPham();
            }
            catch (Exception ex)
            {
                ThongBao = "Lỗi tải dữ liệu: " + ex.Message;
            }
        }

        private void LoadSanPham()
        {
            try
            {
                if (SanPhams == null)
                    return;

                SanPhams.Clear();

                var data = _service.GetSanPhams(TuKhoa, DanhMucDangChon);

                if (TrangThaiDangChon == "Đang bán")
                {
                    data = data.Where(x => x.ConHoatDong).ToList();
                }
                else if (TrangThaiDangChon == "Ngưng bán")
                {
                    data = data.Where(x => !x.ConHoatDong).ToList();
                }

                foreach (var item in data)
                {
                    SanPhams.Add(item);
                }

                ThongBao = "Đã tải " + SanPhams.Count + " sản phẩm.";
            }
            catch (Exception ex)
            {
                ThongBao = "Lỗi tải sản phẩm: " + ex.Message;
            }
        }

        private void ChonSanPham(MenuProductModel item)
        {
            if (item == null)
                return;

            CheDoForm = "EDIT";
            SanPhamDangChon = item;
            ThongBao = "Bạn có thể chỉnh sửa rồi bấm Lưu thay đổi.";
        }

        private void FillForm(MenuProductModel item)
        {
            if (item == null)
                return;

            TenSanPhamNhap = item.TenSanPham;
            MoTaNhap = item.MoTa;
            DanhMucNhap = item.TenDanhMuc;
            DuongDanAnhDangChon = item.HinhAnh;
            GiaBanNhap = item.GiaBan;
        }

        private void BatDauThem()
        {
            CheDoForm = "ADD";
            SanPhamDangChon = null;
            TenSanPhamNhap = "";
            MoTaNhap = "";
            DuongDanAnhDangChon = "";
            GiaBanNhap = 0;
            DanhMucNhap = DanhMucs.FirstOrDefault(x => x != "Tất cả");
            ThongBao = "Nhập thông tin sản phẩm mới.";
        }

        private void BatDauSua()
        {
            if (SanPhamDangChon == null)
            {
                ThongBao = "Vui lòng chọn sản phẩm cần cập nhật.";
                return;
            }

            CheDoForm = "EDIT";
            FillForm(SanPhamDangChon);
            ThongBao = "Đang cập nhật: " + SanPhamDangChon.TenSanPham;
        }

        private void ChonAnh()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Chọn ảnh sản phẩm",
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                // Giữ đường dẫn gốc để preview/copy.
                // Khi bấm Lưu, hàm LuuAnhVaoThuMucProject sẽ đổi thành đường dẫn tương đối.
                DuongDanAnhDangChon = dialog.FileName;
                ThongBao = "Đã chọn ảnh.";
            }
        }

        private void Luu()
        {
            if (CheDoForm == "ADD")
            {
                ThemSanPham();
                return;
            }

            if (SanPhamDangChon != null)
            {
                CapNhatSanPham();
                return;
            }

            ThongBao = "Vui lòng chọn sản phẩm hoặc bấm + Thêm sản phẩm.";
        }

        private void ThemSanPham()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TenSanPhamNhap))
                {
                    ThongBao = "Vui lòng nhập tên sản phẩm.";
                    return;
                }

                if (string.IsNullOrWhiteSpace(DanhMucNhap) || DanhMucNhap == "Tất cả")
                {
                    ThongBao = "Vui lòng chọn danh mục.";
                    return;
                }

                if (GiaBanNhap <= 0)
                {
                    ThongBao = "Vui lòng nhập giá bán lớn hơn 0.";
                    return;
                }

                string hinhAnhLuu = LuuAnhVaoThuMucProject(DuongDanAnhDangChon);

                _service.ThemSanPham(
      TenSanPhamNhap,
      DanhMucNhap,
      MoTaNhap,
      hinhAnhLuu,
      GiaBanNhap);

                ThongBao = "Thêm sản phẩm thành công.";

                // Reset bộ lọc để món mới chắc chắn hiện ra
                TuKhoa = "";
                DanhMucDangChon = "Tất cả";
                TrangThaiDangChon = "Đang bán";

                LoadSanPham();

                SanPhamDangChon = SanPhams
                    .OrderByDescending(x => x.MaSanPham)
                    .FirstOrDefault();

                CheDoForm = "DETAIL";
            }
            catch (Exception ex)
            {
                ThongBao = "Lỗi thêm sản phẩm: " + ex.Message;
            }
        }

        private void CapNhatSanPham()
        {
            try
            {
                if (SanPhamDangChon == null)
                {
                    ThongBao = "Vui lòng chọn sản phẩm cần cập nhật.";
                    return;
                }

                if (string.IsNullOrWhiteSpace(TenSanPhamNhap))
                {
                    ThongBao = "Vui lòng nhập tên sản phẩm.";
                    return;
                }

                if (string.IsNullOrWhiteSpace(DanhMucNhap) || DanhMucNhap == "Tất cả")
                {
                    ThongBao = "Vui lòng chọn danh mục.";
                    return;
                }

                if (GiaBanNhap <= 0)
                {
                    ThongBao = "Vui lòng nhập giá bán lớn hơn 0.";
                    return;
                }

                string hinhAnhLuu = LuuAnhVaoThuMucProject(DuongDanAnhDangChon);

                int maSanPham = SanPhamDangChon.MaSanPham;

                _service.CapNhatSanPham(
                    maSanPham,
                    TenSanPhamNhap,
                    DanhMucNhap,
                    MoTaNhap,
                    hinhAnhLuu,
                    GiaBanNhap);

                LoadSanPham();

                SanPhamDangChon = SanPhams.FirstOrDefault(x => x.MaSanPham == maSanPham);

                CheDoForm = "EDIT";
                ThongBao = "Đã lưu thay đổi sản phẩm.";
            }
            catch (Exception ex)
            {
                ThongBao = "Lỗi cập nhật: " + ex.Message;
            }
        }

        private void DoiTrangThai()
        {
            try
            {
                if (SanPhamDangChon == null)
                {
                    ThongBao = "Vui lòng chọn sản phẩm trước.";
                    return;
                }

                int maSanPham = SanPhamDangChon.MaSanPham;

                _service.DoiTrangThaiSanPham(maSanPham);

                LoadSanPham();

                SanPhamDangChon = SanPhams.FirstOrDefault(x => x.MaSanPham == maSanPham);

                if (SanPhamDangChon != null)
                {
                    ThongBao = SanPhamDangChon.ConHoatDong
                        ? "Đã hiện sản phẩm: " + SanPhamDangChon.TenSanPham
                        : "Đã ẩn sản phẩm: " + SanPhamDangChon.TenSanPham;
                }
                else
                {
                    ThongBao = "Đã ngừng bán và ẩn sản phẩm khỏi danh sách đang bán.";
                }

                CheDoForm = "DETAIL";
            }
            catch (Exception ex)
            {
                ThongBao = "Lỗi Ẩn / Hiện: " + ex.Message;
            }
        }

        private void Reset()
        {
            CheDoForm = "DETAIL";
            SanPhamDangChon = null;
            TenSanPhamNhap = "";
            MoTaNhap = "";
            DuongDanAnhDangChon = "";
            GiaBanNhap = 0;
            DanhMucNhap = DanhMucs.FirstOrDefault(x => x != "Tất cả");
            ThongBao = "Đã reset.";
            LoadSanPham();
        }

        private string LuuAnhVaoThuMucProject(string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
                return "";

            // Nếu đã là đường dẫn tương đối rồi thì giữ nguyên.
            // Ví dụ: Pictures/Menu/anh1.jpg
            if (!System.IO.Path.IsPathRooted(sourcePath))
                return sourcePath.Replace("\\", "/");

            // Nếu là đường dẫn tuyệt đối nhưng file không tồn tại thì vẫn trả lại dạng chuẩn hóa.
            if (!System.IO.File.Exists(sourcePath))
                return sourcePath.Replace("\\", "/");

            string fileName = System.IO.Path.GetFileName(sourcePath);

            string folderCap1 = System.IO.Path.GetDirectoryName(sourcePath);
            if (string.IsNullOrWhiteSpace(folderCap1))
                return fileName;

            string tenThuMucGanNhat = System.IO.Path.GetFileName(folderCap1);

            string folderCap2 = System.IO.Path.GetDirectoryName(folderCap1);
            if (string.IsNullOrWhiteSpace(folderCap2))
                return (tenThuMucGanNhat + "/" + fileName).Replace("\\", "/");

            string tenThuMucThuHai = System.IO.Path.GetFileName(folderCap2);

            // Lấy 2 thư mục cuối + tên file.
            // VD: D:\DoAn\QuanLyQuanCaFe\Pictures\Menu\anh1.jpg
            // => Pictures/Menu/anh1.jpg
            string relativePath = System.IO.Path.Combine(
                tenThuMucThuHai,
                tenThuMucGanNhat,
                fileName
            );

            // Copy ảnh vào thư mục chạy của app theo đúng relativePath.
            // VD: bin/Debug/Pictures/Menu/anh1.jpg
            string destPath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                relativePath
            );

            string destFolder = System.IO.Path.GetDirectoryName(destPath);

            if (!System.IO.Directory.Exists(destFolder))
                System.IO.Directory.CreateDirectory(destFolder);

            if (!string.Equals(sourcePath, destPath, StringComparison.OrdinalIgnoreCase))
            {
                System.IO.File.Copy(sourcePath, destPath, true);
            }

            // Chỉ lưu tương đối vào database.
            return relativePath.Replace("\\", "/");
        }
    }
}