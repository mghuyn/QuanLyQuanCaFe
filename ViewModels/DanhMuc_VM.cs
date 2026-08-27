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
    public class DanhMuc_VM : BaseViewModel
    {
        private readonly DanhMucService _service;
        private string _tuKhoaLoai;
        private string _trangThaiLoai;
        private string _tuKhoaNCC;
        private string _trangThaiNCC;
        private string _tuKhoaDVT;
        private string _thongBao;
        private LoaiSanPhamQuanLyModel _loaiDangChon;
        private NhaCungCapQuanLyModel _nhaCungCapDangChon;
        private DonViTinhQuanLyModel _donViTinhDangChon;

        public ObservableCollection<LoaiSanPhamQuanLyModel> DanhSachLoaiSanPham { get; set; }
        public ObservableCollection<NhaCungCapQuanLyModel> DanhSachNhaCungCap { get; set; }
        public ObservableCollection<DonViTinhQuanLyModel> DanhSachDonViTinh { get; set; }
        public ObservableCollection<string> TrangThaiLoaiFilters { get; set; }
        public ObservableCollection<string> TrangThaiNCCFilters { get; set; }

        public string TuKhoaLoai { get => _tuKhoaLoai; set { SetProperty(ref _tuKhoaLoai, value); LoadLoaiSanPham(); } }
        public string TrangThaiLoai { get => _trangThaiLoai; set { SetProperty(ref _trangThaiLoai, value); LoadLoaiSanPham(); } }
        public string TuKhoaNCC { get => _tuKhoaNCC; set { SetProperty(ref _tuKhoaNCC, value); LoadNhaCungCap(); } }
        public string TrangThaiNCC { get => _trangThaiNCC; set { SetProperty(ref _trangThaiNCC, value); LoadNhaCungCap(); } }
        public string TuKhoaDVT { get => _tuKhoaDVT; set { SetProperty(ref _tuKhoaDVT, value); LoadDonViTinh(); } }
        public string ThongBao { get => _thongBao; set => SetProperty(ref _thongBao, value); }

        public LoaiSanPhamQuanLyModel LoaiDangChon { get => _loaiDangChon; set => SetProperty(ref _loaiDangChon, value); }
        public NhaCungCapQuanLyModel NhaCungCapDangChon { get => _nhaCungCapDangChon; set => SetProperty(ref _nhaCungCapDangChon, value); }
        public DonViTinhQuanLyModel DonViTinhDangChon { get => _donViTinhDangChon; set => SetProperty(ref _donViTinhDangChon, value); }

        public int TongLoai => DanhSachLoaiSanPham.Count;
        public int TongNhaCungCap => DanhSachNhaCungCap.Count;
        public int TongDonViTinh => DanhSachDonViTinh.Count;

        public ICommand ThemMoiLoaiCommand { get; set; }
        public ICommand LuuLoaiCommand { get; set; }
        public ICommand DoiTrangThaiLoaiCommand { get; set; }
        public ICommand ChonLoaiCommand { get; set; }

        public ICommand ThemMoiNCCCommand { get; set; }
        public ICommand LuuNCCCommand { get; set; }
        public ICommand DoiTrangThaiNCCCommand { get; set; }
        public ICommand ChonNCCCommand { get; set; }

        public ICommand ThemMoiDVTCommand { get; set; }
        public ICommand LuuDVTCommand { get; set; }
        public ICommand ChonDVTCommand { get; set; }
        public ICommand LamMoiCommand { get; set; }

        public DanhMuc_VM()
        {
            _service = new DanhMucService();
            DanhSachLoaiSanPham = new ObservableCollection<LoaiSanPhamQuanLyModel>();
            DanhSachNhaCungCap = new ObservableCollection<NhaCungCapQuanLyModel>();
            DanhSachDonViTinh = new ObservableCollection<DonViTinhQuanLyModel>();
            TrangThaiLoaiFilters = new ObservableCollection<string> { "Tất cả", "Đang dùng", "Đã ẩn" };
            TrangThaiNCCFilters = new ObservableCollection<string> { "Tất cả", "Đang hợp tác", "Ngừng hợp tác" };

            ThemMoiLoaiCommand = new RelayCommand(p => TaoFormLoaiMoi());
            LuuLoaiCommand = new RelayCommand(p => LuuLoai());
            DoiTrangThaiLoaiCommand = new RelayCommand(p => DoiTrangThaiLoai());
            ChonLoaiCommand = new RelayCommand(p => ChonLoai(p as LoaiSanPhamQuanLyModel));

            ThemMoiNCCCommand = new RelayCommand(p => TaoFormNCCMoi());
            LuuNCCCommand = new RelayCommand(p => LuuNCC());
            DoiTrangThaiNCCCommand = new RelayCommand(p => DoiTrangThaiNCC());
            ChonNCCCommand = new RelayCommand(p => ChonNCC(p as NhaCungCapQuanLyModel));

            ThemMoiDVTCommand = new RelayCommand(p => TaoFormDVTMoi());
            LuuDVTCommand = new RelayCommand(p => LuuDVT());
            ChonDVTCommand = new RelayCommand(p => ChonDVT(p as DonViTinhQuanLyModel));
            LamMoiCommand = new RelayCommand(p => LoadAll());

            TrangThaiLoai = "Tất cả";
            TrangThaiNCC = "Tất cả";
            LoadAll();
            TaoFormLoaiMoi();
            TaoFormNCCMoi();
            TaoFormDVTMoi();
        }

        private void LoadAll()
        {
            LoadLoaiSanPham();
            LoadNhaCungCap();
            LoadDonViTinh();
        }

        private void LoadLoaiSanPham()
        {
            try
            {
                if (DanhSachLoaiSanPham == null) return;
                DanhSachLoaiSanPham.Clear();
                foreach (var item in _service.GetLoaiSanPhams(TuKhoaLoai, TrangThaiLoai)) DanhSachLoaiSanPham.Add(item);
                OnPropertyChanged(nameof(TongLoai));
            }
            catch (Exception ex) { ThongBao = "Lỗi tải loại sản phẩm: " + LayLoiChiTiet(ex); }
        }

        private void LoadNhaCungCap()
        {
            try
            {
                if (DanhSachNhaCungCap == null) return;
                DanhSachNhaCungCap.Clear();
                foreach (var item in _service.GetNhaCungCaps(TuKhoaNCC, TrangThaiNCC)) DanhSachNhaCungCap.Add(item);
                OnPropertyChanged(nameof(TongNhaCungCap));
            }
            catch (Exception ex) { ThongBao = "Lỗi tải nhà cung cấp: " + LayLoiChiTiet(ex); }
        }

        private void LoadDonViTinh()
        {
            try
            {
                if (DanhSachDonViTinh == null) return;
                DanhSachDonViTinh.Clear();
                foreach (var item in _service.GetDonViTinhs(TuKhoaDVT)) DanhSachDonViTinh.Add(item);
                OnPropertyChanged(nameof(TongDonViTinh));
            }
            catch (Exception ex) { ThongBao = "Lỗi tải đơn vị tính: " + LayLoiChiTiet(ex); }
        }

        private void TaoFormLoaiMoi()
        {
            LoaiDangChon = new LoaiSanPhamQuanLyModel { MaDanhMuc = 0, MaCodeDanhMuc = _service.TaoMaDanhMuc(), TenDanhMuc = "", MoTa = "", ThuTuHienThi = DanhSachLoaiSanPham.Count + 1, ConHoatDong = true };
            ThongBao = "Đang thêm loại sản phẩm mới.";
        }

        private void ChonLoai(LoaiSanPhamQuanLyModel item)
        {
            if (item == null) return;
            LoaiDangChon = new LoaiSanPhamQuanLyModel { MaDanhMuc = item.MaDanhMuc, MaCodeDanhMuc = item.MaCodeDanhMuc, TenDanhMuc = item.TenDanhMuc, MoTa = item.MoTa, ThuTuHienThi = item.ThuTuHienThi, ConHoatDong = item.ConHoatDong };
        }

        private void LuuLoai()
        {
            try
            {
                if (LoaiDangChon.MaDanhMuc == 0) _service.ThemLoaiSanPham(LoaiDangChon);
                else _service.CapNhatLoaiSanPham(LoaiDangChon);
                LoadLoaiSanPham();
                TaoFormLoaiMoi();
                ThongBao = "Đã lưu loại sản phẩm.";
            }
            catch (Exception ex) { ThongBao = "Lỗi lưu loại sản phẩm: " + LayLoiChiTiet(ex); }
        }

        private void DoiTrangThaiLoai()
        {
            try
            {
                if (LoaiDangChon == null || LoaiDangChon.MaDanhMuc <= 0) { ThongBao = "Vui lòng chọn loại sản phẩm cần ẩn/hiện."; return; }
                _service.DoiTrangThaiLoaiSanPham(LoaiDangChon.MaDanhMuc);
                LoadLoaiSanPham();
                TaoFormLoaiMoi();
                ThongBao = "Đã đổi trạng thái loại sản phẩm.";
            }
            catch (Exception ex) { ThongBao = "Lỗi đổi trạng thái loại sản phẩm: " + LayLoiChiTiet(ex); }
        }

        private void TaoFormNCCMoi()
        {
            NhaCungCapDangChon = new NhaCungCapQuanLyModel { MaNCC = 0, MaNhaCungCap = _service.TaoMaNhaCungCap(), TenNhaCungCap = "", NguoiLienHe = "", SoDienThoai = "", Email = "", DiaChi = "", MaSoThue = "", TaiKhoanNganHang = "", GhiChu = "", ConHoatDong = true };
            ThongBao = "Đang thêm nhà cung cấp mới.";
        }

        private void ChonNCC(NhaCungCapQuanLyModel item)
        {
            if (item == null) return;
            NhaCungCapDangChon = new NhaCungCapQuanLyModel { MaNCC = item.MaNCC, MaNhaCungCap = item.MaNhaCungCap, TenNhaCungCap = item.TenNhaCungCap, NguoiLienHe = item.NguoiLienHe, SoDienThoai = item.SoDienThoai, Email = item.Email, MaSoThue = item.MaSoThue, DiaChi = item.DiaChi, TaiKhoanNganHang = item.TaiKhoanNganHang, GhiChu = item.GhiChu, ConHoatDong = item.ConHoatDong };
        }

        private void LuuNCC()
        {
            try
            {
                if (NhaCungCapDangChon.MaNCC == 0) _service.ThemNhaCungCap(NhaCungCapDangChon);
                else _service.CapNhatNhaCungCap(NhaCungCapDangChon);
                LoadNhaCungCap();
                TaoFormNCCMoi();
                ThongBao = "Đã lưu nhà cung cấp.";
            }
            catch (Exception ex) { ThongBao = "Lỗi lưu nhà cung cấp: " + LayLoiChiTiet(ex); }
        }

        private void DoiTrangThaiNCC()
        {
            try
            {
                if (NhaCungCapDangChon == null || NhaCungCapDangChon.MaNCC <= 0) { ThongBao = "Vui lòng chọn nhà cung cấp cần ẩn/hiện."; return; }
                _service.DoiTrangThaiNhaCungCap(NhaCungCapDangChon.MaNCC);
                LoadNhaCungCap();
                TaoFormNCCMoi();
                ThongBao = "Đã đổi trạng thái nhà cung cấp.";
            }
            catch (Exception ex) { ThongBao = "Lỗi đổi trạng thái nhà cung cấp: " + LayLoiChiTiet(ex); }
        }

        private void TaoFormDVTMoi()
        {
            DonViTinhDangChon = new DonViTinhQuanLyModel { MaDonVi = 0, MaCodeDonVi = _service.TaoMaDonVi(), TenDonVi = "", MoTa = "", NgayTao = DateTime.Now };
            ThongBao = "Đang thêm đơn vị tính mới.";
        }

        private void ChonDVT(DonViTinhQuanLyModel item)
        {
            if (item == null) return;
            DonViTinhDangChon = new DonViTinhQuanLyModel { MaDonVi = item.MaDonVi, MaCodeDonVi = item.MaCodeDonVi, TenDonVi = item.TenDonVi, MoTa = item.MoTa, NgayTao = item.NgayTao };
        }

        private void LuuDVT()
        {
            try
            {
                if (DonViTinhDangChon.MaDonVi == 0) _service.ThemDonViTinh(DonViTinhDangChon);
                else _service.CapNhatDonViTinh(DonViTinhDangChon);
                LoadDonViTinh();
                TaoFormDVTMoi();
                ThongBao = "Đã lưu đơn vị tính.";
            }
            catch (Exception ex) { ThongBao = "Lỗi lưu đơn vị tính: " + LayLoiChiTiet(ex); }
        }

        private string LayLoiChiTiet(Exception ex)
        {
            if (ex == null) return "";
            string msg = ex.Message;
            if (ex.InnerException != null) msg += " | Inner: " + ex.InnerException.Message;
            if (ex.InnerException != null && ex.InnerException.InnerException != null) msg += " | Inner 2: " + ex.InnerException.InnerException.Message;
            return msg;
        }
    }
}
