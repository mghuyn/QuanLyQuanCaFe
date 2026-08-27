using QuanLyQuanCaFe.Commands;
using QuanLyQuanCaFe.Core;
using QuanLyQuanCaFe.Models;
using QuanLyQuanCaFe.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace QuanLyQuanCaFe.ViewModels
{
    public class NhanVien_VM : BaseViewModel
    {
        private readonly NhanVienService _service;

        private string _tuKhoa;
        private string _chucVuDangChon;
        private string _trangThaiDangChon;
        private string _thongBao;

        private NhanVienItemModel _nhanVienDangChon;

        public ObservableCollection<NhanVienItemModel> DanhSachNhanVien { get; set; }
        public ObservableCollection<string> ChucVuFilters { get; set; }
        public ObservableCollection<string> TrangThaiFilters { get; set; }
        public ObservableCollection<string> GioiTinhFilters { get; set; }
        public ObservableCollection<string> ChucVuGoiY { get; set; }

        public string TuKhoa
        {
            get => _tuKhoa;
            set
            {
                SetProperty(ref _tuKhoa, value);
                LoadNhanVien();
            }
        }

        public string ChucVuDangChon
        {
            get => _chucVuDangChon;
            set
            {
                SetProperty(ref _chucVuDangChon, value);
                LoadNhanVien();
            }
        }

        public string TrangThaiDangChon
        {
            get => _trangThaiDangChon;
            set
            {
                SetProperty(ref _trangThaiDangChon, value);
                LoadNhanVien();
            }
        }

        public string ThongBao
        {
            get => _thongBao;
            set => SetProperty(ref _thongBao, value);
        }

        public NhanVienItemModel NhanVienDangChon
        {
            get => _nhanVienDangChon;
            set
            {
                SetProperty(ref _nhanVienDangChon, value);

                if (value != null)
                    ThongBao = "Đã chọn nhân viên: " + value.HoTen;
            }
        }

        public int TongNhanVien
        {
            get => DanhSachNhanVien.Count;
            set { }
        }

        public int DangLam
        {
            get => DanhSachNhanVien.Count(x => x.ConHoatDong);
            set { }
        }

        public int DaNghi
        {
            get => DanhSachNhanVien.Count(x => !x.ConHoatDong);
            set { }
        }

        public decimal TongLuong
        {
            get => DanhSachNhanVien.Where(x => x.ConHoatDong).Sum(x => x.LuongCoBan);
            set { }
        }

        public string TongLuongText
        {
            get => TongLuong.ToString("N0") + "đ";
            set { }
        }

        public ICommand ChonNhanVienCommand { get; set; }
        public ICommand ThemMoiCommand { get; set; }
        public ICommand LuuCommand { get; set; }
        public ICommand AnHienCommand { get; set; }
        public ICommand ResetCommand { get; set; }
        public ICommand LamMoiCommand { get; set; }
        public ICommand DiemDanhCommand { get; set; }

        public NhanVien_VM()
        {
            _service = new NhanVienService();

            DanhSachNhanVien = new ObservableCollection<NhanVienItemModel>();
            ChucVuFilters = new ObservableCollection<string>();

            TrangThaiFilters = new ObservableCollection<string>
            {
                "Tất cả",
                "Đang làm",
                "Đã nghỉ"
            };

            GioiTinhFilters = new ObservableCollection<string>
            {
                "Nam",
                "Nữ",
                "Khác"
            };

            ChucVuGoiY = new ObservableCollection<string>
            {
                "Quản lý",
                "Thu ngân",
                "Pha chế",
                "Phục vụ",
                "Kho",
                "Bảo vệ"
            };

            ChonNhanVienCommand = new RelayCommand(p => ChonNhanVien(p as NhanVienItemModel));
            ThemMoiCommand = new RelayCommand(p => TaoFormThemMoi());
            LuuCommand = new RelayCommand(p => LuuNhanVien());
            AnHienCommand = new RelayCommand(p => AnHienNhanVien());
            ResetCommand = new RelayCommand(p => ResetForm());
            LamMoiCommand = new RelayCommand(p => LoadData());
            DiemDanhCommand = new RelayCommand(p => DiemDanhNhanVien());

            ChucVuDangChon = "Tất cả";
            TrangThaiDangChon = "Tất cả";

            LoadData();
            TaoFormThemMoi();
        }

        private void LoadData()
        {
            try
            {
                LoadChucVu();
                LoadNhanVien();
            }
            catch (Exception ex)
            {
                ThongBao = "Lỗi tải dữ liệu nhân viên: " + LayLoiChiTiet(ex);
            }
        }

        private void LoadChucVu()
        {
            ChucVuFilters.Clear();

            foreach (var item in _service.GetChucVuFilters())
            {
                ChucVuFilters.Add(item);
            }

            if (ChucVuFilters.Count == 0)
            {
                ChucVuFilters.Add("Tất cả");
            }

            if (string.IsNullOrWhiteSpace(ChucVuDangChon))
                ChucVuDangChon = "Tất cả";
        }

        private void LoadNhanVien()
        {
            try
            {
                if (DanhSachNhanVien == null)
                    return;

                DanhSachNhanVien.Clear();

                var data = _service.GetNhanViens(TuKhoa, ChucVuDangChon, TrangThaiDangChon);

                foreach (var item in data)
                {
                    DanhSachNhanVien.Add(item);
                }

                CapNhatThongKe();
            }
            catch (Exception ex)
            {
                ThongBao = "Lỗi tải danh sách nhân viên: " + LayLoiChiTiet(ex);
            }
        }

        private void CapNhatThongKe()
        {
            OnPropertyChanged(nameof(TongNhanVien));
            OnPropertyChanged(nameof(DangLam));
            OnPropertyChanged(nameof(DaNghi));
            OnPropertyChanged(nameof(TongLuong));
            OnPropertyChanged(nameof(TongLuongText));
        }

        private void ChonNhanVien(NhanVienItemModel item)
        {
            if (item == null)
                return;

            NhanVienDangChon = new NhanVienItemModel
            {
                MaNV = item.MaNV,
                MaNhanVien = item.MaNhanVien,

                HoTen = item.HoTen,
                ChucVu = item.ChucVu,
                SoDienThoai = item.SoDienThoai,
                Email = item.Email,

                NgaySinh = item.NgaySinh,
                GioiTinh = item.GioiTinh,
                DiaChi = item.DiaChi,

                NgayVaoLam = item.NgayVaoLam,
                LuongCoBan = item.LuongCoBan,

                GhiChu = item.GhiChu,
                ConHoatDong = item.ConHoatDong
            };
        }

        private void TaoFormThemMoi()
        {
            NhanVienDangChon = new NhanVienItemModel
            {
                MaNV = 0,
                MaNhanVien = _service.TaoMaNhanVien(),
                HoTen = "",
                ChucVu = "Phục vụ",
                SoDienThoai = "",
                Email = "",
                NgaySinh = null,
                GioiTinh = "Khác",
                DiaChi = "",
                NgayVaoLam = DateTime.Now.Date,
                LuongCoBan = 0,
                GhiChu = "",
                ConHoatDong = true
            };

            ThongBao = "Đang thêm nhân viên mới.";
        }

        private void LuuNhanVien()
        {
            try
            {
                if (NhanVienDangChon == null)
                {
                    ThongBao = "Chưa có dữ liệu nhân viên.";
                    return;
                }

                int maDangSua = NhanVienDangChon.MaNV;

                if (NhanVienDangChon.MaNV == 0)
                {
                    _service.ThemNhanVien(NhanVienDangChon);
                    ThongBao = "Đã thêm nhân viên mới.";
                    LoadData();
                    TaoFormThemMoi();
                }
                else
                {
                    _service.CapNhatNhanVien(NhanVienDangChon);
                    ThongBao = "Đã cập nhật nhân viên.";
                    LoadData();

                    var item = DanhSachNhanVien.FirstOrDefault(x => x.MaNV == maDangSua);
                    if (item != null)
                        ChonNhanVien(item);
                }
            }
            catch (Exception ex)
            {
                ThongBao = "Lỗi lưu nhân viên: " + LayLoiChiTiet(ex);
            }
        }

        private void AnHienNhanVien()
        {
            try
            {
                if (NhanVienDangChon == null || NhanVienDangChon.MaNV <= 0)
                {
                    ThongBao = "Vui lòng chọn nhân viên cần ẩn / hiện.";
                    return;
                }

                _service.DoiTrangThaiNhanVien(NhanVienDangChon.MaNV);

                ThongBao = "Đã đổi trạng thái nhân viên.";

                LoadData();
                TaoFormThemMoi();
            }
            catch (Exception ex)
            {
                ThongBao = "Lỗi ẩn / hiện nhân viên: " + LayLoiChiTiet(ex);
            }
        }


        private void DiemDanhNhanVien()
        {
            try
            {
                if (NhanVienDangChon == null || NhanVienDangChon.MaNV <= 0)
                {
                    ThongBao = "Vui lòng chọn nhân viên cần điểm danh.";
                    return;
                }

                ThongBao = _service.DiemDanhNhanVien(NhanVienDangChon.MaNV);
                LoadData();
            }
            catch (Exception ex)
            {
                ThongBao = "Lỗi điểm danh: " + LayLoiChiTiet(ex);
            }
        }

        private void ResetForm()
        {
            TaoFormThemMoi();
        }

        private string LayLoiChiTiet(Exception ex)
        {
            if (ex == null)
                return "";

            string message = ex.Message;

            if (ex.InnerException != null)
                message += " | Inner: " + ex.InnerException.Message;

            if (ex.InnerException != null && ex.InnerException.InnerException != null)
                message += " | Inner 2: " + ex.InnerException.InnerException.Message;

            return message;
        }
    }
}