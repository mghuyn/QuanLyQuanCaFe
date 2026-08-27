using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using QuanLyQuanCaFe.Commands;
using QuanLyQuanCaFe.Core;
using QuanLyQuanCaFe.Models;
using QuanLyQuanCaFe.Services;

namespace QuanLyQuanCaFe.ViewModels
{
    public class PosViewModel : BaseViewModel
    {
        private readonly PosService _posService;
        private readonly OrderService _orderService;

        private string _tuKhoa;
        private string _danhMucDangChon;
        private decimal _tongTien;
        private string _thongBao;
        private bool _dangLapHoaDon;
        private bool _hienPopupThongBao;
        private string _tieuDeThongBao;
        private string _noiDungThongBao;
        private string _iconThongBao;

        private string _loaiHoaDon;
        private PosTableModel _banDangChon;
        private PosCustomerModel _khachHangDangChon;
        private string _ghiChuHoaDon;

        private bool _hienPopupBan;
        private bool _hienPopupKhachHang;
        private bool _hienPopupThanhToan;
        private string _tuKhoaBan;
        private string _tuKhoaKhachHang;
        private string _phuongThucThanhToan;
        private decimal _tienKhachTra;
        private bool _thanhToanHoaDonCu;
        private bool _dangHopNhatGioHang;

        private DateTime? _tuNgay;
        private DateTime? _denNgay;
        private string _tuKhoaHoaDon;
        private string _trangThaiHoaDonDangChon;
        private PosHoaDonHistoryModel _hoaDonDangChon;

        public ObservableCollection<string> DanhMucs { get; set; }
        public ObservableCollection<PosProductModel> SanPhams { get; set; }
        public ObservableCollection<CartItemModel> GioHang { get; set; }
        public ObservableCollection<PosTableModel> DanhSachBan { get; set; }
        public ObservableCollection<PosTableGroupModel> DanhSachBanTheoKhuVuc { get; set; }
        public ObservableCollection<PosCustomerModel> DanhSachKhachHang { get; set; }
        public ObservableCollection<string> TrangThaiHoaDonFilters { get; set; }
        public ObservableCollection<PosHoaDonHistoryModel> DanhSachHoaDon { get; set; }
        public ObservableCollection<PosHoaDonDetailItemModel> ChiTietHoaDonDangChon { get; set; }
        public ObservableCollection<string> PaymentMethods { get; set; }

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

        public decimal TongTien
        {
            get => _tongTien;
            set
            {
                SetProperty(ref _tongTien, value);
                OnPropertyChanged(nameof(TongTienText));
                OnPropertyChanged(nameof(TienThua));
                OnPropertyChanged(nameof(TienThuaText));
                OnPropertyChanged(nameof(TongTienThanhToanPopup));
                OnPropertyChanged(nameof(TongTienThanhToanPopupText));
            }
        }

        public string TongTienText => TongTien.ToString("N0") + "đ";


        public string ThongBao
        {
            get => _thongBao;
            set => SetProperty(ref _thongBao, value);
        }

        public bool DangLapHoaDon
        {
            get => _dangLapHoaDon;
            set
            {
                SetProperty(ref _dangLapHoaDon, value);
                CapNhatTrangThaiNutBill();
                OnPropertyChanged(nameof(GoiYTrangThaiBill));
            }
        }

        public bool ChuaLapHoaDon
        {
            get { return !DangLapHoaDon; }
            set { }
        }

        public bool HienPopupThongBao
        {
            get => _hienPopupThongBao;
            set => SetProperty(ref _hienPopupThongBao, value);
        }

        public string TieuDeThongBao
        {
            get => _tieuDeThongBao;
            set => SetProperty(ref _tieuDeThongBao, value);
        }

        public string NoiDungThongBao
        {
            get => _noiDungThongBao;
            set => SetProperty(ref _noiDungThongBao, value);
        }

        public string IconThongBao
        {
            get => _iconThongBao;
            set => SetProperty(ref _iconThongBao, value);
        }

        public string LoaiHoaDon
        {
            get => _loaiHoaDon;
            set
            {
                if (SetProperty(ref _loaiHoaDon, value))
                {
                    if (_loaiHoaDon == "TAKE_AWAY")
                        BanDangChon = null;

                    OnPropertyChanged(nameof(LoaiHoaDonText));
                    OnPropertyChanged(nameof(IsDineIn));
                    OnPropertyChanged(nameof(IsTakeAway));
                    CapNhatTrangThaiNutBill();
                }
            }
        }

        public string LoaiHoaDonText => LoaiHoaDon == "DINE_IN" ? "Ngồi lại" : "Mang về";

        public bool IsDineIn
        {
            get => LoaiHoaDon == "DINE_IN";
            set
            {
                if (value)
                    LoaiHoaDon = "DINE_IN";
            }
        }

        public bool IsTakeAway
        {
            get => LoaiHoaDon == "TAKE_AWAY";
            set
            {
                if (value)
                    LoaiHoaDon = "TAKE_AWAY";
            }
        }

        public bool CoMonTrongBill => GioHang != null && GioHang.Count > 0;

        public bool CanThaoTacBill
        {
            get
            {
                if (!DangLapHoaDon)
                    return false;

                if (!CoMonTrongBill)
                    return false;

                if (LoaiHoaDon == "DINE_IN" && BanDangChon == null)
                    return false;

                return true;
            }
            set { }
        }

        public bool CoTheChonBan => DangLapHoaDon && LoaiHoaDon == "DINE_IN";

        public string GoiYTrangThaiBill
        {
            get
            {
                if (!DangLapHoaDon)
                    return "Bấm Tạo hóa đơn mới để bắt đầu lập bill.";

                if (!CoMonTrongBill)
                    return "Chọn món để bắt đầu lập hóa đơn.";

                if (LoaiHoaDon == "DINE_IN" && BanDangChon == null)
                    return "Hóa đơn ngồi lại cần chọn bàn trước khi lưu/gửi pha chế/thanh toán.";

                return "Bill đã đủ điều kiện để lưu tạm, gửi pha chế hoặc thanh toán.";
            }
            set { }
        }

        public PosTableModel BanDangChon
        {
            get => _banDangChon;
            set
            {
                SetProperty(ref _banDangChon, value);
                OnPropertyChanged(nameof(TenBanDangChon));
                CapNhatTrangThaiNutBill();
            }
        }

        public string TenBanDangChon => BanDangChon != null ? BanDangChon.TenBan : "Chưa chọn bàn";

        public PosCustomerModel KhachHangDangChon
        {
            get => _khachHangDangChon;
            set
            {
                SetProperty(ref _khachHangDangChon, value);
                OnPropertyChanged(nameof(TenKhachHangDangChon));
            }
        }

        public string TenKhachHangDangChon => KhachHangDangChon != null ? KhachHangDangChon.HoTen : "Khách lẻ";

        public string GhiChuHoaDon
        {
            get => _ghiChuHoaDon;
            set => SetProperty(ref _ghiChuHoaDon, value);
        }

        public bool HienPopupBan
        {
            get => _hienPopupBan;
            set => SetProperty(ref _hienPopupBan, value);
        }

        public bool HienPopupKhachHang
        {
            get => _hienPopupKhachHang;
            set => SetProperty(ref _hienPopupKhachHang, value);
        }

        public bool HienPopupThanhToan
        {
            get => _hienPopupThanhToan;
            set => SetProperty(ref _hienPopupThanhToan, value);
        }

        public string TuKhoaBan
        {
            get => _tuKhoaBan;
            set
            {
                SetProperty(ref _tuKhoaBan, value);
                LoadBan();
            }
        }

        public string TuKhoaKhachHang
        {
            get => _tuKhoaKhachHang;
            set
            {
                SetProperty(ref _tuKhoaKhachHang, value);
                LoadKhachHang();
            }
        }

        public string PhuongThucThanhToan
        {
            get => _phuongThucThanhToan;
            set => SetProperty(ref _phuongThucThanhToan, value);
        }

        public decimal TienKhachTra
        {
            get => _tienKhachTra;
            set
            {
                SetProperty(ref _tienKhachTra, value);
                OnPropertyChanged(nameof(TienThua));
                OnPropertyChanged(nameof(TienThuaText));
                OnPropertyChanged(nameof(TongTienThanhToanPopup));
                OnPropertyChanged(nameof(TongTienThanhToanPopupText));
            }
        }

        public decimal TongTienThanhToanPopup
        {
            get
            {
                if (_thanhToanHoaDonCu && HoaDonDangChon != null)
                    return HoaDonDangChon.TongTien;

                return TongTien;
            }
            set { }
        }

        public string TongTienThanhToanPopupText
        {
            get { return TongTienThanhToanPopup.ToString("N0") + "đ"; }
            set { }
        }

        public string TieuDePopupThanhToan
        {
            get
            {
                if (_thanhToanHoaDonCu && HoaDonDangChon != null)
                    return "Thanh toán " + HoaDonDangChon.MaHoaDonHienThi;

                return "Thanh toán";
            }
            set { }
        }

        public decimal TienThua => TienKhachTra - TongTienThanhToanPopup;
        public string TienThuaText => TienThua.ToString("N0") + "đ";

        public DateTime? TuNgay
        {
            get => _tuNgay;
            set
            {
                SetProperty(ref _tuNgay, value);
                LoadLichSuHoaDon();
            }
        }

        public DateTime? DenNgay
        {
            get => _denNgay;
            set
            {
                SetProperty(ref _denNgay, value);
                LoadLichSuHoaDon();
            }
        }

        public string TuKhoaHoaDon
        {
            get => _tuKhoaHoaDon;
            set
            {
                SetProperty(ref _tuKhoaHoaDon, value);
                LoadLichSuHoaDon();
            }
        }

        public string TrangThaiHoaDonDangChon
        {
            get => _trangThaiHoaDonDangChon;
            set
            {
                SetProperty(ref _trangThaiHoaDonDangChon, value);
                LoadLichSuHoaDon();
            }
        }

        public PosHoaDonHistoryModel HoaDonDangChon
        {
            get => _hoaDonDangChon;
            set
            {
                SetProperty(ref _hoaDonDangChon, value);
                LoadChiTietHoaDonDangChon();
                OnPropertyChanged(nameof(CoTheHuyHoaDonDangChon));
                OnPropertyChanged(nameof(CoTheThanhToanHoaDonDangChon));
                OnPropertyChanged(nameof(GhiChuHuyHoaDonDangChon));
            }
        }

        public bool CoTheHuyHoaDonDangChon
        {
            get
            {
                if (HoaDonDangChon == null)
                    return false;

                string trangThai = (HoaDonDangChon.TrangThaiHoaDon ?? "").Trim().ToUpper();
                string thanhToan = (HoaDonDangChon.TrangThaiThanhToan ?? "").Trim().ToUpper();

                return trangThai == "DRAFT" && thanhToan != "PAID";
            }
            set { }
        }

        public bool CoTheThanhToanHoaDonDangChon
        {
            get
            {
                return HoaDonDangChon != null
                    && HoaDonDangChon.TrangThaiHoaDon != "COMPLETED"
                    && HoaDonDangChon.TrangThaiHoaDon != "CANCELLED"
                    && HoaDonDangChon.TrangThaiThanhToan != "PAID";
            }
            set { }
        }

        public string GhiChuHuyHoaDonDangChon
        {
            get
            {
                if (HoaDonDangChon == null)
                    return "Chọn hóa đơn để kiểm tra điều kiện hủy.";

                string trangThai = (HoaDonDangChon.TrangThaiHoaDon ?? "").Trim().ToUpper();

                if (trangThai == "DRAFT")
                    return "Có thể hủy vì hóa đơn vẫn đang lưu tạm, chưa gửi pha chế.";

                return "Không thể hủy vì hóa đơn đã gửi pha chế/đang làm/sẵn sàng/đã thanh toán.";
            }
            set { }
        }

        public int TongHoaDon => DanhSachHoaDon.Count;
        public string TongHoaDonText => TongHoaDon.ToString("N0");
        public decimal TongDoanhThu => DanhSachHoaDon.Where(x => x.TrangThaiThanhToan == "PAID" || x.TrangThaiHoaDon == "COMPLETED").Sum(x => x.TongTien);
        public string TongDoanhThuText => TongDoanhThu.ToString("N0") + "đ";
        public int TongDaThanhToan => DanhSachHoaDon.Count(x => x.TrangThaiThanhToan == "PAID" || x.TrangThaiHoaDon == "COMPLETED");
        public int TongChuaThanhToan => DanhSachHoaDon.Count(x => x.TrangThaiThanhToan != "PAID" && x.TrangThaiHoaDon != "COMPLETED" && x.TrangThaiHoaDon != "CANCELLED");

        public ICommand TaoHoaDonMoiCommand { get; set; }
        public ICommand DongThongBaoCommand { get; set; }
        public ICommand ThemVaoGioCommand { get; set; }
        public ICommand TangSoLuongCommand { get; set; }
        public ICommand GiamSoLuongCommand { get; set; }
        public ICommand XoaDongCommand { get; set; }
        public ICommand LamMoiCommand { get; set; }
        public ICommand LuuTamCommand { get; set; }
        public ICommand GuiPhaCheCommand { get; set; }
        public ICommand MoThanhToanCommand { get; set; }
        public ICommand XacNhanThanhToanCommand { get; set; }
        public ICommand DongPopupCommand { get; set; }
        public ICommand ChonHinhThucCommand { get; set; }
        public ICommand ChonDanhMucCommand { get; set; }
        public ICommand MoChonBanCommand { get; set; }
        public ICommand ChonBanCommand { get; set; }
        public ICommand BoChonBanCommand { get; set; }
        public ICommand MoChonKhachHangCommand { get; set; }
        public ICommand ChonKhachHangCommand { get; set; }
        public ICommand BoChonKhachHangCommand { get; set; }
        public ICommand ChonHoaDonCommand { get; set; }
        public ICommand LamMoiLichSuCommand { get; set; }
        public ICommand ThanhToanHoaDonDangChonCommand { get; set; }
        public ICommand HuyHoaDonDangChonCommand { get; set; }
        public ICommand InHoaDonDangChonCommand { get; set; }
        public ICommand ChonHomNayCommand { get; set; }

        public PosViewModel()
        {
            _posService = new PosService();
            _orderService = new OrderService();

            DanhMucs = new ObservableCollection<string>();
            SanPhams = new ObservableCollection<PosProductModel>();
            GioHang = new ObservableCollection<CartItemModel>();
            GioHang.CollectionChanged += GioHang_CollectionChanged;
            DanhSachBan = new ObservableCollection<PosTableModel>();
            DanhSachBanTheoKhuVuc = new ObservableCollection<PosTableGroupModel>();
            DanhSachKhachHang = new ObservableCollection<PosCustomerModel>();
            TrangThaiHoaDonFilters = new ObservableCollection<string>
            {
                "Tất cả",
                "DRAFT",
                "WAITING_KITCHEN",
                "PREPARING",
                "READY",
                "COMPLETED",
                "CANCELLED"
            };
            DanhSachHoaDon = new ObservableCollection<PosHoaDonHistoryModel>();
            ChiTietHoaDonDangChon = new ObservableCollection<PosHoaDonDetailItemModel>();
            PaymentMethods = new ObservableCollection<string> { "CASH", "BANK_TRANSFER", "CARD", "EWALLET" };

            TaoHoaDonMoiCommand = new RelayCommand(p => TaoHoaDonMoi());
            DongThongBaoCommand = new RelayCommand(p => HienPopupThongBao = false);
            ThemVaoGioCommand = new RelayCommand(p => ThemVaoGio(p as PosProductModel));
            TangSoLuongCommand = new RelayCommand(p => TangSoLuong(p as CartItemModel));
            GiamSoLuongCommand = new RelayCommand(p => GiamSoLuong(p as CartItemModel));
            XoaDongCommand = new RelayCommand(p => XoaDong(p as CartItemModel));
            LamMoiCommand = new RelayCommand(p => LamMoiBill());
            LuuTamCommand = new RelayCommand(p => LuuTam(), p => CanThaoTacBill);
            GuiPhaCheCommand = new RelayCommand(p => GuiPhaChe(), p => CanThaoTacBill);
            MoThanhToanCommand = new RelayCommand(p => MoThanhToan(), p => CanThaoTacBill);
            XacNhanThanhToanCommand = new RelayCommand(p => XacNhanThanhToan());
            DongPopupCommand = new RelayCommand(p => DongPopup());
            ChonHinhThucCommand = new RelayCommand(p => ChonHinhThuc(p as string));
            ChonDanhMucCommand = new RelayCommand(p => ChonDanhMuc(p as string));
            MoChonBanCommand = new RelayCommand(p => MoChonBan());
            ChonBanCommand = new RelayCommand(p => ChonBan(p as PosTableModel));
            BoChonBanCommand = new RelayCommand(p => { BanDangChon = null; });
            MoChonKhachHangCommand = new RelayCommand(p => MoChonKhachHang());
            ChonKhachHangCommand = new RelayCommand(p => ChonKhachHang(p as PosCustomerModel));
            BoChonKhachHangCommand = new RelayCommand(p => { KhachHangDangChon = null; });
            ChonHoaDonCommand = new RelayCommand(p => HoaDonDangChon = p as PosHoaDonHistoryModel);
            LamMoiLichSuCommand = new RelayCommand(p => LoadLichSuHoaDon());
            ThanhToanHoaDonDangChonCommand = new RelayCommand(p => MoThanhToanHoaDonDangChon());
            HuyHoaDonDangChonCommand = new RelayCommand(p => HuyHoaDonDangChon());
            InHoaDonDangChonCommand = new RelayCommand(p => InHoaDonDangChon());
            ChonHomNayCommand = new RelayCommand(p => ChonHomNay());

            LoaiHoaDon = "DINE_IN";
            DangLapHoaDon = false;
            PhuongThucThanhToan = "CASH";
            TuNgay = DateTime.Now.Date;
            DenNgay = DateTime.Now.Date;
            TrangThaiHoaDonDangChon = "Tất cả";

            LoadDanhMuc();
            DanhMucDangChon = "Tất cả";
            LoadSanPham();
            LoadBan();
            LoadKhachHang();
            LoadLichSuHoaDon();
        }

        private void LoadDanhMuc()
        {
            DanhMucs.Clear();
            foreach (var item in _posService.GetDanhMuc())
                DanhMucs.Add(item);
        }

        private void LoadSanPham()
        {
            if (SanPhams == null) return;
            SanPhams.Clear();
            foreach (var item in _posService.GetSanPham(DanhMucDangChon, TuKhoa))
                SanPhams.Add(item);
        }

        private void ChonDanhMuc(string danhMuc)
        {
            if (string.IsNullOrWhiteSpace(danhMuc))
                return;

            DanhMucDangChon = danhMuc;
        }

        private void LoadBan()
        {
            if (DanhSachBan == null) return;
            DanhSachBan.Clear();
            if (DanhSachBanTheoKhuVuc != null) DanhSachBanTheoKhuVuc.Clear();

            var data = _posService.GetBanCafes(TuKhoaBan);
            foreach (var item in data)
                DanhSachBan.Add(item);

            if (DanhSachBanTheoKhuVuc != null)
            {
                foreach (var group in data.GroupBy(x => string.IsNullOrWhiteSpace(x.TenKhuVuc) ? "Khu vực khác" : x.TenKhuVuc))
                {
                    var g = new PosTableGroupModel { TenKhuVuc = group.Key };
                    foreach (var item in group.OrderBy(x => x.SoThuTuBan).ThenBy(x => x.TenBan))
                        g.BanTrongKhuVuc.Add(item);
                    DanhSachBanTheoKhuVuc.Add(g);
                }
            }
        }

        private void LoadKhachHang()
        {
            if (DanhSachKhachHang == null) return;
            DanhSachKhachHang.Clear();
            foreach (var item in _posService.GetKhachHangs(TuKhoaKhachHang))
                DanhSachKhachHang.Add(item);
        }

        private void LoadLichSuHoaDon()
        {
            try
            {
                if (DanhSachHoaDon == null || TuNgay == null || DenNgay == null) return;
                if (TuNgay.Value.Date > DenNgay.Value.Date) return;

                DanhSachHoaDon.Clear();
                foreach (var item in _posService.GetLichSuHoaDon(TuNgay.Value, DenNgay.Value, TuKhoaHoaDon, TrangThaiHoaDonDangChon))
                    DanhSachHoaDon.Add(item);

                CapNhatThongKeHoaDon();
            }
            catch (Exception ex)
            {
                ThongBao = "Lỗi tải lịch sử hóa đơn: " + LayLoiChiTiet(ex);
            }
        }

        private void LoadChiTietHoaDonDangChon()
        {
            ChiTietHoaDonDangChon.Clear();
            if (HoaDonDangChon == null) return;

            try
            {
                foreach (var item in _posService.GetChiTietHoaDon(HoaDonDangChon.MaHoaDonBan))
                    ChiTietHoaDonDangChon.Add(item);
            }
            catch (Exception ex)
            {
                ThongBao = "Lỗi tải chi tiết hóa đơn: " + LayLoiChiTiet(ex);
            }
        }

        private void CapNhatThongKeHoaDon()
        {
            OnPropertyChanged(nameof(TongHoaDon));
            OnPropertyChanged(nameof(TongHoaDonText));
            OnPropertyChanged(nameof(TongDoanhThu));
            OnPropertyChanged(nameof(TongDoanhThuText));
            OnPropertyChanged(nameof(TongDaThanhToan));
            OnPropertyChanged(nameof(TongChuaThanhToan));
        }


        private void TaoHoaDonMoi()
        {
            GioHang.Clear();
            TinhTongTien();
            BanDangChon = null;
            KhachHangDangChon = null;
            GhiChuHoaDon = "";
            LoaiHoaDon = "DINE_IN";
            DangLapHoaDon = true;
            ThongBao = "";
        }

        private void BaoLoiDep(string tieuDe, string noiDung)
        {
            IconThongBao = "⚠";
            TieuDeThongBao = string.IsNullOrWhiteSpace(tieuDe) ? "Thông báo" : tieuDe;
            NoiDungThongBao = noiDung;
            HienPopupThongBao = true;
            ThongBao = noiDung;
        }

        private bool KiemTraTonKhoMonTrongBill(int maBienThe, int soLuongCanBan)
        {
            string loi;
            bool hopLe = _posService.KiemTraTonKhoMon(maBienThe, soLuongCanBan, out loi);

            if (!hopLe)
            {
                BaoLoiDep("Không đủ nguyên liệu", loi);
                return false;
            }

            return true;
        }

        private void ThemVaoGio(PosProductModel product)
        {
            if (product == null)
                return;

            if (!DangLapHoaDon)
            {
                BaoLoiDep("Chưa tạo hóa đơn", "Hãy bấm Tạo hóa đơn mới trước khi chọn món.");
                return;
            }

            if (!product.DangBan)
            {
                BaoLoiDep("Món ngưng bán", "Món này hiện đang ngưng bán, không thể thêm vào bill.");
                return;
            }

            int tongSoLuongMonDangCo = GioHang
                .Where(x => x.MaBienThe == product.MaBienThe)
                .Sum(x => x.SoLuong);

            int soLuongMoi = tongSoLuongMonDangCo + 1;

            if (!KiemTraTonKhoMonTrongBill(product.MaBienThe, soLuongMoi))
                return;

            var dongGiongNhau = GioHang.FirstOrDefault(x =>
                x.MaSanPham == product.MaSanPham &&
                x.MaBienThe == product.MaBienThe &&
                x.DonGia == product.GiaBan &&
                ChuanHoaGhiChu(x.GhiChu) == "");

            if (dongGiongNhau != null)
            {
                dongGiongNhau.SoLuong += 1;
            }
            else
            {
                GioHang.Add(new CartItemModel
                {
                    MaSanPham = product.MaSanPham,
                    MaBienThe = product.MaBienThe,
                    TenSanPham = product.TenSanPham,
                    TenSize = product.TenSize,
                    DonGia = product.GiaBan,
                    SoLuong = 1,
                    GhiChu = ""
                });
            }

            HopNhatGioHangTheoGhiChu();
            TinhTongTien();
            ThongBao = "";
        }

        private void TangSoLuong(CartItemModel item)
        {
            if (item == null)
                return;

            int tongKhacDong = GioHang
                .Where(x => x != item && x.MaBienThe == item.MaBienThe)
                .Sum(x => x.SoLuong);

            int soLuongMoi = tongKhacDong + item.SoLuong + 1;

            if (!KiemTraTonKhoMonTrongBill(item.MaBienThe, soLuongMoi))
                return;

            item.SoLuong += 1;
            TinhTongTien();
        }

        private void GiamSoLuong(CartItemModel item)
        {
            if (item == null) return;
            item.SoLuong -= 1;
            if (item.SoLuong <= 0) GioHang.Remove(item);
            TinhTongTien();
        }

        private void XoaDong(CartItemModel item)
        {
            if (item == null) return;
            GioHang.Remove(item);
            TinhTongTien();
        }

        private void ChonHinhThuc(string loai)
        {
            if (!DangLapHoaDon)
            {
                BaoLoiDep("Chưa tạo hóa đơn", "Hãy bấm Tạo hóa đơn mới trước khi chọn hình thức bán hàng.");
                return;
            }

            if (string.IsNullOrWhiteSpace(loai)) return;
            LoaiHoaDon = loai;
        }

        private void MoChonBan()
        {
            if (!DangLapHoaDon)
            {
                BaoLoiDep("Chưa tạo hóa đơn", "Hãy bấm Tạo hóa đơn mới trước khi chọn bàn.");
                return;
            }

            if (LoaiHoaDon != "DINE_IN")
            {
                ThongBao = "Chỉ cần chọn bàn khi hóa đơn là ngồi lại.";
                return;
            }
            HienPopupBan = true;
            TuKhoaBan = "";
            LoadBan();
        }

        private void ChonBan(PosTableModel ban)
        {
            if (ban == null) return;

            if (!ban.CoTheChon)
            {
                BaoLoiDep("Bàn không khả dụng", "Bàn này đang ở trạng thái " + ban.TrangThaiText + ". Vui lòng chọn bàn trống hoặc bàn đã đặt.");
                return;
            }

            BanDangChon = ban;
            HienPopupBan = false;
        }

        private void MoChonKhachHang()
        {
            if (!DangLapHoaDon)
            {
                BaoLoiDep("Chưa tạo hóa đơn", "Hãy bấm Tạo hóa đơn mới trước khi chọn khách hàng.");
                return;
            }

            HienPopupKhachHang = true;
            TuKhoaKhachHang = "";
            LoadKhachHang();
        }

        private void ChonKhachHang(PosCustomerModel khachHang)
        {
            if (khachHang == null) return;
            KhachHangDangChon = khachHang;
            HienPopupKhachHang = false;
        }

        private void LuuTam()
        {
            try
            {
                KiemTraBillTruocKhiLuu(false);
                string maHoaDon = _orderService.LuuTam(LayGioHangDaHopNhat(), BanDangChon != null ? (int?)BanDangChon.MaBan : null, KhachHangDangChon != null ? (int?)KhachHangDangChon.MaKH : null, LoaiHoaDon, GhiChuHoaDon);
                LamMoiBill();
                LoadLichSuHoaDon();
                ThongBao = "Đã lưu tạm hóa đơn: " + maHoaDon;
            }
            catch (Exception ex)
            {
                BaoLoiDep("Lỗi lưu tạm", LayLoiChiTiet(ex));
            }
        }

        private void GuiPhaChe()
        {
            try
            {
                KiemTraBillTruocKhiLuu(true);
                string maHoaDon = _orderService.GuiPhaChe(LayGioHangDaHopNhat(), BanDangChon != null ? (int?)BanDangChon.MaBan : null, KhachHangDangChon != null ? (int?)KhachHangDangChon.MaKH : null, LoaiHoaDon, GhiChuHoaDon);
                LamMoiBill();
                LoadLichSuHoaDon();
                ThongBao = "Đã gửi pha chế: " + maHoaDon;
            }
            catch (Exception ex)
            {
                if (LaLoiHetNguyenLieu(ex))
                {
                    BaoLoiHetNguyenLieu(ex);
                    return;
                }

                BaoLoiDep("Lỗi gửi pha chế", LayLoiChiTiet(ex));
            }
        }

        private void MoThanhToan()
        {
            if (GioHang.Count == 0)
            {
                ThongBao = "Vui lòng chọn món trước khi thanh toán.";
                return;
            }

            try
            {
                KiemTraBillTruocKhiLuu(false);
                _thanhToanHoaDonCu = false;
                TienKhachTra = TongTien;
                PhuongThucThanhToan = "CASH";
                OnPropertyChanged(nameof(TieuDePopupThanhToan));
                OnPropertyChanged(nameof(TongTienThanhToanPopup));
                OnPropertyChanged(nameof(TongTienThanhToanPopupText));
                OnPropertyChanged(nameof(TienThua));
                OnPropertyChanged(nameof(TienThuaText));
                HienPopupThanhToan = true;
            }
            catch (Exception ex)
            {
                BaoLoiDep("Không thể thanh toán", LayLoiChiTiet(ex));
            }
        }

        private void XacNhanThanhToan()
        {
            try
            {
                decimal tongCanThanhToan = TongTienThanhToanPopup;

                if (tongCanThanhToan <= 0)
                {
                    ThongBao = "Tổng tiền không hợp lệ.";
                    return;
                }

                if (TienKhachTra < tongCanThanhToan)
                {
                    ThongBao = "Tiền khách trả chưa đủ.";
                    return;
                }

                if (_thanhToanHoaDonCu)
                {
                    if (HoaDonDangChon == null)
                    {
                        ThongBao = "Vui lòng chọn hóa đơn cần thanh toán.";
                        return;
                    }

                    _orderService.ThanhToanHoaDonDaCo(HoaDonDangChon.MaHoaDonBan, PhuongThucThanhToan, TienKhachTra);
                    HienPopupThanhToan = false;
                    _thanhToanHoaDonCu = false;
                    LoadLichSuHoaDon();
                    LoadChiTietHoaDonDangChon();
                    OnPropertyChanged(nameof(CoTheThanhToanHoaDonDangChon));
                    OnPropertyChanged(nameof(CoTheHuyHoaDonDangChon));
                    ThongBao = "Đã thanh toán hóa đơn.";
                    return;
                }

                string maHoaDon = _orderService.ThanhToanMoi(LayGioHangDaHopNhat(), BanDangChon != null ? (int?)BanDangChon.MaBan : null, KhachHangDangChon != null ? (int?)KhachHangDangChon.MaKH : null, LoaiHoaDon, GhiChuHoaDon, PhuongThucThanhToan, TienKhachTra);
                HienPopupThanhToan = false;
                LamMoiBill();
                LoadLichSuHoaDon();
                ThongBao = "Thanh toán thành công: " + maHoaDon;
            }
            catch (Exception ex)
            {
                BaoLoiDep("Lỗi thanh toán", LayLoiChiTiet(ex));
            }
        }

        private void MoThanhToanHoaDonDangChon()
        {
            try
            {
                if (HoaDonDangChon == null)
                {
                    ThongBao = "Vui lòng chọn hóa đơn cần thanh toán.";
                    return;
                }

                if (HoaDonDangChon.TrangThaiHoaDon == "COMPLETED" || HoaDonDangChon.TrangThaiThanhToan == "PAID")
                {
                    ThongBao = "Hóa đơn này đã thanh toán.";
                    return;
                }

                if (HoaDonDangChon.TrangThaiHoaDon == "CANCELLED")
                {
                    ThongBao = "Hóa đơn đã hủy, không thể thanh toán.";
                    return;
                }

                _thanhToanHoaDonCu = true;
                PhuongThucThanhToan = "CASH";
                TienKhachTra = HoaDonDangChon.TongTien;
                OnPropertyChanged(nameof(TieuDePopupThanhToan));
                OnPropertyChanged(nameof(TongTienThanhToanPopup));
                OnPropertyChanged(nameof(TongTienThanhToanPopupText));
                OnPropertyChanged(nameof(TienThua));
                OnPropertyChanged(nameof(TienThuaText));
                HienPopupThanhToan = true;
            }
            catch (Exception ex)
            {
                BaoLoiDep("Lỗi mở thanh toán hóa đơn", LayLoiChiTiet(ex));
            }
        }

        private void HuyHoaDonDangChon()
        {
            try
            {
                if (HoaDonDangChon == null)
                {
                    ThongBao = "Vui lòng chọn hóa đơn cần hủy.";
                    return;
                }

                if (!CoTheHuyHoaDonDangChon)
                {
                    BaoLoiDep("Không thể hủy bill", "Chỉ được hủy bill khi hóa đơn còn Lưu tạm, chưa gửi pha chế.");
                    return;
                }

                _orderService.HuyHoaDon(HoaDonDangChon.MaHoaDonBan, "Hủy từ POS");
                HoaDonDangChon = null;
                LoadLichSuHoaDon();
                ChiTietHoaDonDangChon.Clear();
                OnPropertyChanged(nameof(CoTheHuyHoaDonDangChon));
                OnPropertyChanged(nameof(CoTheThanhToanHoaDonDangChon));
                OnPropertyChanged(nameof(GhiChuHuyHoaDonDangChon));
                ThongBao = "Đã hủy hóa đơn lưu tạm.";
            }
            catch (Exception ex)
            {
                BaoLoiDep("Lỗi hủy hóa đơn", LayLoiChiTiet(ex));
            }
        }

        private void InHoaDonDangChon()
        {
            try
            {
                if (HoaDonDangChon == null)
                {
                    BaoLoiDep("Chưa chọn hóa đơn", "Vui lòng chọn hóa đơn cần in.");
                    return;
                }

                var report = new CrystalHoaDonService();
                report.XemHoaDon(HoaDonDangChon.MaHoaDonBan);
            }
            catch (Exception ex)
            {
                BaoLoiDep("Lỗi in hóa đơn", LayLoiChiTiet(ex));
            }
        }

        private void GioHang_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (CartItemModel item in e.NewItems)
                    item.PropertyChanged += GioHangItem_PropertyChanged;
            }

            if (e.OldItems != null)
            {
                foreach (CartItemModel item in e.OldItems)
                    item.PropertyChanged -= GioHangItem_PropertyChanged;
            }

            TinhTongTien();
        }

        private void GioHangItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CartItemModel.GhiChu))
            {
                HopNhatGioHangTheoGhiChu();
                TinhTongTien();
            }
        }

        private string ChuanHoaGhiChu(string ghiChu)
        {
            if (string.IsNullOrWhiteSpace(ghiChu))
                return "";

            return string.Join(" ", ghiChu.Trim().Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
        }

        private bool LaCungDongChiTiet(CartItemModel a, CartItemModel b)
        {
            if (a == null || b == null) return false;

            return a.MaSanPham == b.MaSanPham
                && a.MaBienThe == b.MaBienThe
                && a.DonGia == b.DonGia
                && string.Equals(a.TenSize ?? "", b.TenSize ?? "", StringComparison.OrdinalIgnoreCase)
                && string.Equals(ChuanHoaGhiChu(a.GhiChu), ChuanHoaGhiChu(b.GhiChu), StringComparison.OrdinalIgnoreCase);
        }

        private void HopNhatGioHangTheoGhiChu()
        {
            if (_dangHopNhatGioHang || GioHang == null || GioHang.Count < 2)
                return;

            try
            {
                _dangHopNhatGioHang = true;

                for (int i = 0; i < GioHang.Count; i++)
                {
                    var dongGoc = GioHang[i];

                    for (int j = GioHang.Count - 1; j > i; j--)
                    {
                        var dongKiemTra = GioHang[j];
                        if (!LaCungDongChiTiet(dongGoc, dongKiemTra))
                            continue;

                        dongGoc.SoLuong += dongKiemTra.SoLuong;
                        GioHang.RemoveAt(j);
                    }
                }
            }
            finally
            {
                _dangHopNhatGioHang = false;
            }
        }

        private List<CartItemModel> LayGioHangDaHopNhat()
        {
            HopNhatGioHangTheoGhiChu();
            return GioHang.ToList();
        }

        private void KiemTraBillTruocKhiLuu(bool guiPhaChe)
        {
            if (!DangLapHoaDon)
                throw new Exception("Vui lòng bấm Tạo hóa đơn mới trước.");

            if (GioHang.Count == 0)
                throw new Exception("Vui lòng chọn món trước.");

            if (LoaiHoaDon == "DINE_IN" && BanDangChon == null)
                throw new Exception("Vui lòng chọn bàn cho hóa đơn ngồi lại.");

            string loiTonKho;
            if (!_posService.KiemTraTonKhoGioHang(LayGioHangDaHopNhat(), out loiTonKho))
                throw new Exception(loiTonKho);
        }

        private void LamMoiBill()
        {
            GioHang.Clear();
            TinhTongTien();
            BanDangChon = null;
            KhachHangDangChon = null;
            GhiChuHoaDon = "";
            LoaiHoaDon = "DINE_IN";
            DangLapHoaDon = false;
            HienPopupThanhToan = false;
            _thanhToanHoaDonCu = false;
            ThongBao = "";
        }

        private void DongPopup()
        {
            HienPopupBan = false;
            HienPopupKhachHang = false;
            HienPopupThanhToan = false;
            _thanhToanHoaDonCu = false;
        }

        private void ChonHomNay()
        {
            TuNgay = DateTime.Now.Date;
            DenNgay = DateTime.Now.Date;
            LoadLichSuHoaDon();
        }

        private void TinhTongTien()
        {
            TongTien = GioHang.Sum(x => x.ThanhTien);
            CapNhatTrangThaiNutBill();
        }

        private void CapNhatTrangThaiNutBill()
        {
            OnPropertyChanged(nameof(DangLapHoaDon));
            OnPropertyChanged(nameof(ChuaLapHoaDon));
            OnPropertyChanged(nameof(CoMonTrongBill));
            OnPropertyChanged(nameof(CanThaoTacBill));
            OnPropertyChanged(nameof(CoTheChonBan));
            OnPropertyChanged(nameof(GoiYTrangThaiBill));
        }

        private string LayLoiChiTiet(Exception ex)
        {
            if (ex == null) return "";
            string msg = ex.Message;
            if (ex.InnerException != null) msg += " | Inner: " + ex.InnerException.Message;
            if (ex.InnerException != null && ex.InnerException.InnerException != null) msg += " | Inner 2: " + ex.InnerException.InnerException.Message;
            return msg;
        }
        private bool LaLoiHetNguyenLieu(Exception ex)
        {
            string msg = LayLoiChiTiet(ex).ToUpper();

            return msg.Contains("CK_NGUYENLIEUS_SOLUONGHIENTAI")
                || msg.Contains("NGUYENLIEUS")
                || msg.Contains("SOLUONGHIENTAI")
                || msg.Contains("SỐ LƯỢNG HIỆN TẠI")
                || msg.Contains("CHECK CONSTRAINT");
        }

        private void BaoLoiHetNguyenLieu(Exception ex)
        {
            BaoLoiDep(
                "Hết nguyên liệu",
                "Không thể gửi pha chế vì một hoặc nhiều nguyên liệu trong kho không đủ để pha món này.\n\n" +
                "Hãy kiểm tra lại công thức món và số lượng tồn kho nguyên liệu trước khi gửi pha chế.\n\n" +
                "Chi tiết kỹ thuật: " + LayLoiChiTiet(ex)
            );
        }
    }
}
