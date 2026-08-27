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
    public class KhachHang_VM : BaseViewModel
    {
        private readonly KhachHangService _service;

        private string _tuKhoa;
        private string _hangDangChon;
        private string _trangThaiDangChon;
        private string _thongBao;

        private KhachHangItemModel _khachHangDangChon;

        public ObservableCollection<KhachHangItemModel> DanhSachKhachHang { get; set; }
        public ObservableCollection<HangKhachHang> DanhSachHang { get; set; }
        public ObservableCollection<string> HangFilters { get; set; }
        public ObservableCollection<string> TrangThaiFilters { get; set; }
        public ObservableCollection<string> GioiTinhFilters { get; set; }

        public string TuKhoa
        {
            get => _tuKhoa;
            set
            {
                SetProperty(ref _tuKhoa, value);
                LoadKhachHang();
            }
        }

        public string HangDangChon
        {
            get => _hangDangChon;
            set
            {
                SetProperty(ref _hangDangChon, value);
                LoadKhachHang();
            }
        }

        public string TrangThaiDangChon
        {
            get => _trangThaiDangChon;
            set
            {
                SetProperty(ref _trangThaiDangChon, value);
                LoadKhachHang();
            }
        }

        public string ThongBao
        {
            get => _thongBao;
            set => SetProperty(ref _thongBao, value);
        }

        public KhachHangItemModel KhachHangDangChon
        {
            get => _khachHangDangChon;
            set
            {
                SetProperty(ref _khachHangDangChon, value);

                if (value != null)
                    ThongBao = "Đã chọn khách hàng: " + value.HoTen;
            }
        }

        public int TongKhachHang
        {
            get => DanhSachKhachHang.Count;
            set { }
        }

        public int DangHoatDong
        {
            get => DanhSachKhachHang.Count(x => x.ConHoatDong);
            set { }
        }

        public int DaAn
        {
            get => DanhSachKhachHang.Count(x => !x.ConHoatDong);
            set { }
        }

        public decimal TongChiTieu
        {
            get => DanhSachKhachHang.Sum(x => x.TongChiTieu);
            set { }
        }

        public string TongChiTieuText
        {
            get => TongChiTieu.ToString("N0") + "đ";
            set { }
        }

        public ICommand ChonKhachHangCommand { get; set; }
        public ICommand ThemMoiCommand { get; set; }
        public ICommand LuuCommand { get; set; }
        public ICommand AnHienCommand { get; set; }
        public ICommand ResetCommand { get; set; }
        public ICommand LamMoiCommand { get; set; }

        public KhachHang_VM()
        {
            _service = new KhachHangService();

            DanhSachKhachHang = new ObservableCollection<KhachHangItemModel>();
            DanhSachHang = new ObservableCollection<HangKhachHang>();
            HangFilters = new ObservableCollection<string>();

            TrangThaiFilters = new ObservableCollection<string>
            {
                "Tất cả",
                "Đang hoạt động",
                "Đã ẩn"
            };

            GioiTinhFilters = new ObservableCollection<string>
            {
                "Nam",
                "Nữ",
                "Khác"
            };

            ChonKhachHangCommand = new RelayCommand(p => ChonKhachHang(p as KhachHangItemModel));
            ThemMoiCommand = new RelayCommand(p => TaoFormThemMoi());
            LuuCommand = new RelayCommand(p => LuuKhachHang());
            AnHienCommand = new RelayCommand(p => AnHienKhachHang());
            ResetCommand = new RelayCommand(p => ResetForm());
            LamMoiCommand = new RelayCommand(p => LoadData());

            HangDangChon = "Tất cả";
            TrangThaiDangChon = "Tất cả";

            LoadData();
            TaoFormThemMoi();
        }

        private void LoadData()
        {
            try
            {
                LoadHangKhachHang();
                LoadKhachHang();
            }
            catch (Exception ex)
            {
                ThongBao = "Lỗi tải dữ liệu khách hàng: " + LayLoiChiTiet(ex);
            }
        }

        private void LoadHangKhachHang()
        {
            DanhSachHang.Clear();
            HangFilters.Clear();

            foreach (var item in _service.GetHangKhachHangs())
            {
                DanhSachHang.Add(item);
            }

            foreach (var item in _service.GetTenHangFilters())
            {
                HangFilters.Add(item);
            }

            if (string.IsNullOrWhiteSpace(HangDangChon))
                HangDangChon = "Tất cả";
        }

        private void LoadKhachHang()
        {
            try
            {
                if (DanhSachKhachHang == null)
                    return;

                DanhSachKhachHang.Clear();

                var data = _service.GetKhachHangs(TuKhoa, HangDangChon, TrangThaiDangChon);

                foreach (var item in data)
                {
                    DanhSachKhachHang.Add(item);
                }

                CapNhatThongKe();
            }
            catch (Exception ex)
            {
                ThongBao = "Lỗi tải danh sách khách hàng: " + LayLoiChiTiet(ex);
            }
        }

        private void CapNhatThongKe()
        {
            OnPropertyChanged(nameof(TongKhachHang));
            OnPropertyChanged(nameof(DangHoatDong));
            OnPropertyChanged(nameof(DaAn));
            OnPropertyChanged(nameof(TongChiTieu));
            OnPropertyChanged(nameof(TongChiTieuText));
        }

        private void ChonKhachHang(KhachHangItemModel item)
        {
            if (item == null)
                return;

            KhachHangDangChon = new KhachHangItemModel
            {
                MaKH = item.MaKH,
                MaKhachHang = item.MaKhachHang,
                MaHangKH = item.MaHangKH,
                TenHang = item.TenHang,

                HoTen = item.HoTen,
                SoDienThoai = item.SoDienThoai,
                Email = item.Email,
                NgaySinh = item.NgaySinh,
                GioiTinh = item.GioiTinh,
                DiaChi = item.DiaChi,

                NgayThamGia = item.NgayThamGia,
                DiemTichLuy = item.DiemTichLuy,
                TongChiTieu = item.TongChiTieu,
                LanGheCuoi = item.LanGheCuoi,

                GhiChu = item.GhiChu,
                ConHoatDong = item.ConHoatDong
            };
        }

        private void TaoFormThemMoi()
        {
            int maHangMacDinh = 0;

            if (DanhSachHang.Count > 0)
                maHangMacDinh = DanhSachHang[0].MaHangKH;

            KhachHangDangChon = new KhachHangItemModel
            {
                MaKH = 0,
                MaKhachHang = _service.TaoMaKhachHang(),
                MaHangKH = maHangMacDinh,
                HoTen = "",
                SoDienThoai = "",
                Email = "",
                NgaySinh = null,
                GioiTinh = "Khác",
                DiaChi = "",
                NgayThamGia = DateTime.Now,
                DiemTichLuy = 0,
                TongChiTieu = 0,
                LanGheCuoi = null,
                GhiChu = "",
                ConHoatDong = true
            };

            ThongBao = "Đang thêm khách hàng mới.";
        }

        private void LuuKhachHang()
        {
            try
            {
                if (KhachHangDangChon == null)
                {
                    ThongBao = "Chưa có dữ liệu khách hàng.";
                    return;
                }

                int maDangSua = KhachHangDangChon.MaKH;

                if (KhachHangDangChon.MaKH == 0)
                {
                    _service.ThemKhachHang(KhachHangDangChon);
                    ThongBao = "Đã thêm khách hàng mới.";
                    LoadKhachHang();
                    TaoFormThemMoi();
                }
                else
                {
                    _service.CapNhatKhachHang(KhachHangDangChon);
                    ThongBao = "Đã cập nhật khách hàng.";
                    LoadKhachHang();

                    var item = DanhSachKhachHang.FirstOrDefault(x => x.MaKH == maDangSua);
                    if (item != null)
                        ChonKhachHang(item);
                }
            }
            catch (Exception ex)
            {
                ThongBao = "Lỗi lưu khách hàng: " + LayLoiChiTiet(ex);
            }
        }

        private void AnHienKhachHang()
        {
            try
            {
                if (KhachHangDangChon == null || KhachHangDangChon.MaKH <= 0)
                {
                    ThongBao = "Vui lòng chọn khách hàng cần ẩn / hiện.";
                    return;
                }

                _service.DoiTrangThaiKhachHang(KhachHangDangChon.MaKH);

                ThongBao = "Đã đổi trạng thái khách hàng.";

                LoadKhachHang();
                TaoFormThemMoi();
            }
            catch (Exception ex)
            {
                ThongBao = "Lỗi ẩn / hiện khách hàng: " + LayLoiChiTiet(ex);
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