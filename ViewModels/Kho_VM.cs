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
    public class Kho_VM : BaseViewModel
    {
        private readonly KhoService _service;

        private string _tuKhoa;
        private string _danhMucDangChon;
        private string _trangThaiDangChon;

        private KhoItemModel _nguyenLieuDangChon;

        private decimal _soLuongNhapXuat;
        private decimal _donGiaNhap;
        private string _ghiChu;
        private string _thongBao;

        private bool _dangLapPhieu;
        private string _loaiPhieuDangLap;
        private string _tieuDePhieu;

        private DateTime? _ngayLapPhieu;
        private NhaCC_Model _nhaCungCapDangChon;
        private bool _dangChonNguyenLieu;

        private string _tuKhoaPhieu;
        private string _loaiPhieuLoc;
        private PhieuKhoModel _phieuDangChon;

        public ObservableCollection<KhoItemModel> DanhSachKho { get; set; }
        public ObservableCollection<string> DanhMucs { get; set; }
        public ObservableCollection<string> TrangThaiFilters { get; set; }

        public ObservableCollection<KhoPhieuItemModel> ChiTietPhieuTam { get; set; }

        public ObservableCollection<PhieuKhoModel> DanhSachPhieuKho { get; set; }
        public ObservableCollection<string> LoaiPhieuFilters { get; set; }

        public ObservableCollection<NhaCC_Model> NhaCungCaps { get; set; }

        public string TuKhoa
        {
            get => _tuKhoa;
            set
            {
                SetProperty(ref _tuKhoa, value);
                LoadKho();
            }
        }

        public string DanhMucDangChon
        {
            get => _danhMucDangChon;
            set
            {
                SetProperty(ref _danhMucDangChon, value);
                LoadKho();
            }
        }

        public string TrangThaiDangChon
        {
            get => _trangThaiDangChon;
            set
            {
                SetProperty(ref _trangThaiDangChon, value);
                LoadKho();
            }
        }

        public KhoItemModel NguyenLieuDangChon
        {
            get => _nguyenLieuDangChon;
            set
            {
                SetProperty(ref _nguyenLieuDangChon, value);

                if (value != null)
                {
                    SoLuongNhapXuat = 1;
                    DonGiaNhap = value.GiaNhapCuoi;

                    if (!DangLapPhieu)
                    {
                        ThongBao = "Đã chọn nguyên liệu. Hãy tạo phiếu nhập hoặc phiếu xuất trước.";
                    }
                    else
                    {
                        ThongBao = "Đã chọn nguyên liệu: " + value.TenNguyenLieu;
                    }
                }
            }
        }

        public decimal SoLuongNhapXuat
        {
            get => _soLuongNhapXuat;
            set => SetProperty(ref _soLuongNhapXuat, value);
        }

        public decimal DonGiaNhap
        {
            get => _donGiaNhap;
            set => SetProperty(ref _donGiaNhap, value);
        }

        public string GhiChu
        {
            get => _ghiChu;
            set => SetProperty(ref _ghiChu, value);
        }

        public string ThongBao
        {
            get => _thongBao;
            set => SetProperty(ref _thongBao, value);
        }

        public bool DangLapPhieu
        {
            get => _dangLapPhieu;
            set
            {
                SetProperty(ref _dangLapPhieu, value);
                OnPropertyChanged(nameof(TrangThaiLapPhieuText));
            }
        }

        public string LoaiPhieuDangLap
        {
            get => _loaiPhieuDangLap;
            set
            {
                SetProperty(ref _loaiPhieuDangLap, value);
                OnPropertyChanged(nameof(NoiDungNutThemDong));
                OnPropertyChanged(nameof(NoiDungNutLuuPhieu));
                OnPropertyChanged(nameof(TrangThaiLapPhieuText));
            }
        }

        public string TieuDePhieu
        {
            get => _tieuDePhieu;
            set => SetProperty(ref _tieuDePhieu, value);
        }

        public DateTime? NgayLapPhieu
        {
            get => _ngayLapPhieu;
            set => SetProperty(ref _ngayLapPhieu, value);
        }

        public NhaCC_Model NhaCungCapDangChon
        {
            get => _nhaCungCapDangChon;
            set => SetProperty(ref _nhaCungCapDangChon, value);
        }

        public bool DangChonNguyenLieu
        {
            get => _dangChonNguyenLieu;
            set => SetProperty(ref _dangChonNguyenLieu, value);
        }

        public string TuKhoaPhieu
        {
            get => _tuKhoaPhieu;
            set
            {
                SetProperty(ref _tuKhoaPhieu, value);
                LoadLichSuPhieu();
            }
        }

        public string LoaiPhieuLoc
        {
            get => _loaiPhieuLoc;
            set
            {
                SetProperty(ref _loaiPhieuLoc, value);
                LoadLichSuPhieu();
            }
        }

        public PhieuKhoModel PhieuDangChon
        {
            get => _phieuDangChon;
            set => SetProperty(ref _phieuDangChon, value);
        }

        public string NoiDungNutThemDong
        {
            get
            {
                if (LoaiPhieuDangLap == "NHAP")
                    return "+ Thêm vào phiếu nhập";

                if (LoaiPhieuDangLap == "XUAT")
                    return "+ Thêm vào phiếu xuất";

                return "+ Thêm vào phiếu";
            }
            set { }
        }

        public string NoiDungNutLuuPhieu
        {
            get
            {
                if (LoaiPhieuDangLap == "NHAP")
                    return "Lưu phiếu nhập";

                if (LoaiPhieuDangLap == "XUAT")
                    return "Lưu phiếu xuất";

                return "Lưu phiếu";
            }
            set { }
        }

        public string TrangThaiLapPhieuText
        {
            get
            {
                if (!DangLapPhieu)
                    return "Chưa lập phiếu";

                if (LoaiPhieuDangLap == "NHAP")
                    return "Đang lập phiếu nhập kho";

                if (LoaiPhieuDangLap == "XUAT")
                    return "Đang lập phiếu xuất kho";

                return "Đang lập phiếu";
            }
            set { }
        }

        public int TongNguyenLieu
        {
            get => DanhSachKho.Count;
            set { }
        }

        public int SoLuongCanhBao
        {
            get => DanhSachKho.Count(x => x.SoLuongHienTai > 0 && x.SoLuongHienTai <= x.SoLuongToiThieu);
            set { }
        }

        public int SoLuongHetHang
        {
            get => DanhSachKho.Count(x => x.SoLuongHienTai <= 0);
            set { }
        }

        public decimal TongGiaTriTon
        {
            get => DanhSachKho.Sum(x => x.SoLuongHienTai * x.GiaNhapCuoi);
            set { }
        }

        public string TongGiaTriTonText
        {
            get => TongGiaTriTon.ToString("N0") + "đ";
            set { }
        }

        public decimal TongTienPhieu
        {
            get => ChiTietPhieuTam.Sum(x => x.ThanhTien);
            set { }
        }

        public string TongTienPhieuText
        {
            get => TongTienPhieu.ToString("N0") + "đ";
            set { }
        }

        public int TongSoDongPhieu
        {
            get => ChiTietPhieuTam.Count;
            set { }
        }

        public int TongSoPhieu
        {
            get => DanhSachPhieuKho.Count;
            set { }
        }

        public int TongPhieuNhap
        {
            get => DanhSachPhieuKho.Count(x => x.LoaiPhieu == "NHAP");
            set { }
        }

        public int TongPhieuXuat
        {
            get => DanhSachPhieuKho.Count(x => x.LoaiPhieu == "XUAT");
            set { }
        }

        public decimal TongGiaTriPhieu
        {
            get => DanhSachPhieuKho.Sum(x => x.TongTien);
            set { }
        }

        public string TongGiaTriPhieuText
        {
            get => TongGiaTriPhieu.ToString("N0") + "đ";
            set { }
        }
        private bool _hienPopupNguyenLieu;
        private string _tuKhoaPopupNguyenLieu;
        private string _danhMucPopupDangChon;
        public ObservableCollection<KhoItemModel> DanhSachNguyenLieuPopup { get; set; }
        public bool HienPopupNguyenLieu
        {
            get => _hienPopupNguyenLieu;
            set => SetProperty(ref _hienPopupNguyenLieu, value);
        }

        public string TuKhoaPopupNguyenLieu
        {
            get => _tuKhoaPopupNguyenLieu;
            set
            {
                SetProperty(ref _tuKhoaPopupNguyenLieu, value);
                LoadNguyenLieuPopup();
            }
        }

        public string DanhMucPopupDangChon
        {
            get => _danhMucPopupDangChon;
            set
            {
                SetProperty(ref _danhMucPopupDangChon, value);
                LoadNguyenLieuPopup();
            }
        }
        public ICommand DongPopupNguyenLieuCommand { get; set; }

        public ICommand ChonNguyenLieuCommand { get; set; }
        public ICommand MoChonNguyenLieuCommand { get; set; }

        public ICommand TaoPhieuNhapCommand { get; set; }
        public ICommand TaoPhieuXuatCommand { get; set; }

        public ICommand ThemDongPhieuCommand { get; set; }
        public ICommand XoaDongPhieuCommand { get; set; }

        public ICommand LuuPhieuCommand { get; set; }
        public ICommand HuyPhieuCommand { get; set; }

        public ICommand ChonPhieuCommand { get; set; }

        public ICommand LamMoiCommand { get; set; }
        public ICommand LamMoiLichSuCommand { get; set; }

        public Kho_VM()
        {
            _service = new KhoService();

            DanhSachKho = new ObservableCollection<KhoItemModel>();
            DanhMucs = new ObservableCollection<string>();

            TrangThaiFilters = new ObservableCollection<string>
            {
                "Tất cả",
                "Ổn định",
                "Tồn thấp",
                "Hết hàng"
            };
            DanhSachNguyenLieuPopup = new ObservableCollection<KhoItemModel>();

            DongPopupNguyenLieuCommand = new RelayCommand(p => DongPopupNguyenLieu());

            DanhMucPopupDangChon = "Tất cả";
            HienPopupNguyenLieu = false;
            ChiTietPhieuTam = new ObservableCollection<KhoPhieuItemModel>();

            DanhSachPhieuKho = new ObservableCollection<PhieuKhoModel>();

            LoaiPhieuFilters = new ObservableCollection<string>
            {
                "Tất cả",
                "Phiếu nhập",
                "Phiếu xuất"
            };

            NhaCungCaps = new ObservableCollection<NhaCC_Model>();

            ChonNguyenLieuCommand = new RelayCommand(p => ChonNguyenLieu(p as KhoItemModel));
            MoChonNguyenLieuCommand = new RelayCommand(p => MoChonNguyenLieu());

            TaoPhieuNhapCommand = new RelayCommand(p => BatDauLapPhieu("NHAP"));
            TaoPhieuXuatCommand = new RelayCommand(p => BatDauLapPhieu("XUAT"));

            ThemDongPhieuCommand = new RelayCommand(p => ThemDongVaoPhieu());
            XoaDongPhieuCommand = new RelayCommand(p => XoaDongPhieu(p as KhoPhieuItemModel));

            LuuPhieuCommand = new RelayCommand(p => LuuPhieu());
            HuyPhieuCommand = new RelayCommand(p => HuyPhieu());

            ChonPhieuCommand = new RelayCommand(p => ChonPhieu(p as PhieuKhoModel));

            LamMoiCommand = new RelayCommand(p => LoadData());
            LamMoiLichSuCommand = new RelayCommand(p => LoadLichSuPhieu());

            TrangThaiDangChon = "Tất cả";
            LoaiPhieuLoc = "Tất cả";

            TieuDePhieu = "Chưa lập phiếu";
            LoaiPhieuDangLap = "";
            DangLapPhieu = false;

            NgayLapPhieu = DateTime.Now;
            DangChonNguyenLieu = false;

            SoLuongNhapXuat = 1;
            DonGiaNhap = 0;
            GhiChu = "";
            ThongBao = "";

            LoadData();
            LoadLichSuPhieu();
        }

        private void LoadData()
        {
            try
            {
                LoadDanhMuc();
                LoadNhaCungCap();

                if (string.IsNullOrWhiteSpace(DanhMucDangChon))
                    DanhMucDangChon = "Tất cả";

                LoadKho();
            }
            catch (Exception ex)
            {
                ThongBao = "Lỗi tải dữ liệu kho: " + LayLoiChiTiet(ex);
            }
        }

        private void LoadDanhMuc()
        {
            DanhMucs.Clear();

            foreach (var item in _service.GetDanhMucKho())
            {
                DanhMucs.Add(item);
            }
        }

        private void LoadNhaCungCap()
        {
            NhaCungCaps.Clear();

            foreach (var item in _service.GetNhaCungCaps())
            {
                NhaCungCaps.Add(item);
            }

            if (NhaCungCapDangChon == null && NhaCungCaps.Count > 0)
            {
                NhaCungCapDangChon = NhaCungCaps[0];
            }
        }

        private void LoadKho()
        {
            try
            {
                if (DanhSachKho == null)
                    return;

                DanhSachKho.Clear();

                var data = _service.GetDanhSachKho(TuKhoa, DanhMucDangChon);

                if (TrangThaiDangChon == "Ổn định")
                {
                    data = data.Where(x => x.SoLuongHienTai > x.SoLuongToiThieu).ToList();
                }
                else if (TrangThaiDangChon == "Tồn thấp")
                {
                    data = data.Where(x => x.SoLuongHienTai > 0 && x.SoLuongHienTai <= x.SoLuongToiThieu).ToList();
                }
                else if (TrangThaiDangChon == "Hết hàng")
                {
                    data = data.Where(x => x.SoLuongHienTai <= 0).ToList();
                }

                foreach (var item in data)
                {
                    DanhSachKho.Add(item);
                }

                CapNhatThongKeKho();
            }
            catch (Exception ex)
            {
                ThongBao = "Lỗi tải danh sách kho: " + LayLoiChiTiet(ex);
            }
        }

        private void LoadLichSuPhieu()
        {
            try
            {
                if (DanhSachPhieuKho == null)
                    return;

                DanhSachPhieuKho.Clear();

                var data = _service.GetLichSuPhieuKho(TuKhoaPhieu, LoaiPhieuLoc);

                foreach (var item in data)
                {
                    DanhSachPhieuKho.Add(item);
                }

                CapNhatThongKePhieu();
            }
            catch (Exception ex)
            {
                ThongBao = "Lỗi tải lịch sử phiếu kho: " + LayLoiChiTiet(ex);
            }
        }

        private void CapNhatThongKeKho()
        {
            OnPropertyChanged(nameof(TongNguyenLieu));
            OnPropertyChanged(nameof(SoLuongCanhBao));
            OnPropertyChanged(nameof(SoLuongHetHang));
            OnPropertyChanged(nameof(TongGiaTriTon));
            OnPropertyChanged(nameof(TongGiaTriTonText));
        }

        private void CapNhatTongPhieuTam()
        {
            OnPropertyChanged(nameof(TongTienPhieu));
            OnPropertyChanged(nameof(TongTienPhieuText));
            OnPropertyChanged(nameof(TongSoDongPhieu));
        }

        private void CapNhatThongKePhieu()
        {
            OnPropertyChanged(nameof(TongSoPhieu));
            OnPropertyChanged(nameof(TongPhieuNhap));
            OnPropertyChanged(nameof(TongPhieuXuat));
            OnPropertyChanged(nameof(TongGiaTriPhieu));
            OnPropertyChanged(nameof(TongGiaTriPhieuText));
        }

        private void CapNhatTrangThaiLapPhieu()
        {
            OnPropertyChanged(nameof(NoiDungNutThemDong));
            OnPropertyChanged(nameof(NoiDungNutLuuPhieu));
            OnPropertyChanged(nameof(TrangThaiLapPhieuText));
        }

        private void ChonNguyenLieu(KhoItemModel item)
        {
            NguyenLieuDangChon = item;

            if (item != null)
            {
                HienPopupNguyenLieu = false;
                ThongBao = "Đã chọn nguyên liệu: " + item.TenNguyenLieu;
            }
        }
        private void BatDauLapPhieu(string loaiPhieu)
        {
            DangLapPhieu = true;
            LoaiPhieuDangLap = loaiPhieu;
            HienPopupNguyenLieu = false;
            TuKhoaPopupNguyenLieu = "";
            DanhMucPopupDangChon = "Tất cả";
            if (loaiPhieu == "NHAP")
            {
                TieuDePhieu = "Phiếu nhập kho";
                ThongBao = "Đang lập phiếu nhập kho. Chọn ngày lập, nhà cung cấp rồi bấm Add nguyên liệu.";
            }
            else
            {
                TieuDePhieu = "Phiếu xuất kho";
                ThongBao = "Đang lập phiếu xuất kho. Chọn ngày lập rồi bấm Add nguyên liệu.";
            }

            ChiTietPhieuTam.Clear();
            NguyenLieuDangChon = null;

            SoLuongNhapXuat = 1;
            DonGiaNhap = 0;
            GhiChu = "";

            NgayLapPhieu = DateTime.Now;
            DangChonNguyenLieu = false;

            if (NhaCungCaps.Count > 0)
            {
                NhaCungCapDangChon = NhaCungCaps[0];
            }

            CapNhatTongPhieuTam();
            CapNhatTrangThaiLapPhieu();
        }

        private void MoChonNguyenLieu()
        {
            if (!DangLapPhieu || string.IsNullOrWhiteSpace(LoaiPhieuDangLap))
            {
                ThongBao = "Vui lòng chọn Tạo phiếu nhập hoặc Tạo phiếu xuất trước.";
                return;
            }

            if (NgayLapPhieu == null)
            {
                ThongBao = "Vui lòng chọn ngày lập phiếu.";
                return;
            }

            if (LoaiPhieuDangLap == "NHAP" && NhaCungCapDangChon == null)
            {
                ThongBao = "Vui lòng chọn nhà cung cấp trước khi thêm nguyên liệu.";
                return;
            }

            DangChonNguyenLieu = true;
            HienPopupNguyenLieu = true;

            TuKhoaPopupNguyenLieu = "";
            DanhMucPopupDangChon = "Tất cả";

            LoadNguyenLieuPopup();

            if (NhaCungCapDangChon != null)
            {
                ThongBao = "Đã mở nguyên liệu gợi ý theo nhà cung cấp: " + NhaCungCapDangChon.TenNhaCungCap;
            }
            else
            {
                ThongBao = "Đã mở danh sách nguyên liệu.";
            }
        }
        private void DongPopupNguyenLieu()
        {
            HienPopupNguyenLieu = false;
        }

        private void LoadNguyenLieuPopup()
        {
            try
            {
                if (DanhSachNguyenLieuPopup == null)
                    return;

                DanhSachNguyenLieuPopup.Clear();

                string danhMucCanLoc = DanhMucPopupDangChon;
                string tuKhoaCanTim = TuKhoaPopupNguyenLieu;

                /*
                 * Nếu người dùng chưa tự nhập từ khóa và đang chọn "Tất cả",
                 * hệ thống sẽ tự lấy từ khóa gợi ý theo nhà cung cấp.
                 * Ví dụ:
                 * - NCC sữa => ưu tiên milk, sữa, cream...
                 * - NCC trà => ưu tiên tea, trà, oolong...
                 * - NCC bánh => ưu tiên flour, bột, bơ...
                 */
                if (string.IsNullOrWhiteSpace(tuKhoaCanTim) &&
                    (string.IsNullOrWhiteSpace(danhMucCanLoc) || danhMucCanLoc == "Tất cả"))
                {
                    tuKhoaCanTim = LayTuKhoaUuTienTheoNhaCungCap();
                }

                var data = _service.GetDanhSachKho("", danhMucCanLoc);

                if (!string.IsNullOrWhiteSpace(tuKhoaCanTim))
                {
                    string[] keywords = tuKhoaCanTim
                        .ToLower()
                        .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    data = data
                        .Where(x =>
                            keywords.Any(k =>
                                (!string.IsNullOrWhiteSpace(x.TenNguyenLieu) && x.TenNguyenLieu.ToLower().Contains(k)) ||
                                (!string.IsNullOrWhiteSpace(x.TenDanhMuc) && x.TenDanhMuc.ToLower().Contains(k)) ||
                                (!string.IsNullOrWhiteSpace(x.MaCodeNguyenLieu) && x.MaCodeNguyenLieu.ToLower().Contains(k))
                            )
                        )
                        .ToList();
                }

                /*
                 * Nếu lọc thông minh không ra nguyên liệu nào,
                 * fallback về toàn bộ danh sách để người dùng vẫn chọn được.
                 */
                if (data.Count == 0)
                {
                    data = _service.GetDanhSachKho(TuKhoaPopupNguyenLieu, DanhMucPopupDangChon);
                }

                foreach (var item in data)
                {
                    DanhSachNguyenLieuPopup.Add(item);
                }
            }
            catch (Exception ex)
            {
                ThongBao = "Lỗi tải nguyên liệu popup: " + LayLoiChiTiet(ex);
            }
        }
        private void ThemDongVaoPhieu()
        {
            try
            {
                if (!DangLapPhieu || string.IsNullOrWhiteSpace(LoaiPhieuDangLap))
                {
                    ThongBao = "Vui lòng bấm Tạo phiếu nhập hoặc Tạo phiếu xuất trước.";
                    return;
                }

                if (!DangChonNguyenLieu)
                {
                    ThongBao = "Vui lòng bấm Add nguyên liệu trước.";
                    return;
                }

                if (NguyenLieuDangChon == null)
                {
                    ThongBao = "Vui lòng chọn nguyên liệu.";
                    return;
                }

                if (SoLuongNhapXuat <= 0)
                {
                    ThongBao = "Số lượng phải lớn hơn 0.";
                    return;
                }

                if (LoaiPhieuDangLap == "NHAP" && DonGiaNhap <= 0)
                {
                    ThongBao = "Đơn giá nhập phải lớn hơn 0.";
                    return;
                }

                if (LoaiPhieuDangLap == "XUAT" && NguyenLieuDangChon.SoLuongHienTai < SoLuongNhapXuat)
                {
                    ThongBao = "Tồn kho không đủ để thêm vào phiếu xuất.";
                    return;
                }

                decimal donGia = LoaiPhieuDangLap == "NHAP"
                    ? DonGiaNhap
                    : NguyenLieuDangChon.GiaNhapCuoi;

                var itemDaCo = ChiTietPhieuTam
                    .FirstOrDefault(x => x.MaNguyenLieu == NguyenLieuDangChon.MaNguyenLieu);

                if (itemDaCo != null)
                {
                    itemDaCo.SoLuong += SoLuongNhapXuat;
                    itemDaCo.DonGia = donGia;
                }
                else
                {
                    ChiTietPhieuTam.Add(new KhoPhieuItemModel
                    {
                        MaNguyenLieu = NguyenLieuDangChon.MaNguyenLieu,
                        TenNguyenLieu = NguyenLieuDangChon.TenNguyenLieu,
                        DonViTinh = NguyenLieuDangChon.TenDonVi,
                        SoLuong = SoLuongNhapXuat,
                        DonGia = donGia
                    });
                }

                SoLuongNhapXuat = 1;
                CapNhatTongPhieuTam();

                ThongBao = LoaiPhieuDangLap == "NHAP"
                    ? "Đã thêm nguyên liệu vào phiếu nhập."
                    : "Đã thêm nguyên liệu vào phiếu xuất.";
            }
            catch (Exception ex)
            {
                ThongBao = "Lỗi thêm dòng phiếu: " + LayLoiChiTiet(ex);
            }
        }

        private void XoaDongPhieu(KhoPhieuItemModel item)
        {
            if (item == null)
                return;

            ChiTietPhieuTam.Remove(item);

            CapNhatTongPhieuTam();

            if (ChiTietPhieuTam.Count == 0)
            {
                ThongBao = "Đã xóa dòng phiếu. Phiếu hiện đang trống.";
            }
            else
            {
                ThongBao = "Đã xóa 1 dòng khỏi phiếu.";
            }
        }

        private void LuuPhieu()
        {
            try
            {
                if (!DangLapPhieu || string.IsNullOrWhiteSpace(LoaiPhieuDangLap))
                {
                    ThongBao = "Chưa có phiếu nào đang lập.";
                    return;
                }

                if (NgayLapPhieu == null)
                {
                    ThongBao = "Vui lòng chọn ngày lập phiếu.";
                    return;
                }

                if (LoaiPhieuDangLap == "NHAP" && NhaCungCapDangChon == null)
                {
                    ThongBao = "Vui lòng chọn nhà cung cấp.";
                    return;
                }

                if (ChiTietPhieuTam.Count == 0)
                {
                    ThongBao = "Phiếu chưa có nguyên liệu.";
                    return;
                }

                string maPhieu;
                string ghiChuDayDu = TaoGhiChuDayDu();

                if (LoaiPhieuDangLap == "NHAP")
                {
                    maPhieu = _service.TaoPhieuNhapKho(ChiTietPhieuTam.ToList(), ghiChuDayDu);
                    ThongBao = "Đã lưu phiếu nhập: " + maPhieu;
                }
                else if (LoaiPhieuDangLap == "XUAT")
                {
                    maPhieu = _service.TaoPhieuXuatKho(ChiTietPhieuTam.ToList(), ghiChuDayDu);
                    ThongBao = "Đã lưu phiếu xuất: " + maPhieu;
                }
                else
                {
                    ThongBao = "Chưa xác định loại phiếu.";
                    return;
                }

                ResetPhieuSauKhiLuu();
                LoadKho();
                LoadLichSuPhieu();
            }
            catch (Exception ex)
            {
                ThongBao = "Lỗi lưu phiếu: " + LayLoiChiTiet(ex);
            }
        }

        private string TaoGhiChuDayDu()
        {
            string loai = LoaiPhieuDangLap == "NHAP" ? "Phiếu nhập kho" : "Phiếu xuất kho";

            string ngay = NgayLapPhieu.HasValue
                ? NgayLapPhieu.Value.ToString("dd/MM/yyyy")
                : DateTime.Now.ToString("dd/MM/yyyy");

            string nhaCungCap = NhaCungCapDangChon != null
                ? NhaCungCapDangChon.TenNhaCungCap
                : "Không có";

            return loai
                + " | Ngày lập: " + ngay
                + " | Nhà cung cấp: " + nhaCungCap
                + " | Ghi chú: " + (string.IsNullOrWhiteSpace(GhiChu) ? "Không có" : GhiChu);
        }

        private void HuyPhieu()
        {
            DangLapPhieu = false;
            LoaiPhieuDangLap = "";
            TieuDePhieu = "Chưa lập phiếu";
            HienPopupNguyenLieu = false;
            TuKhoaPopupNguyenLieu = "";
            DanhMucPopupDangChon = "Tất cả";
            NguyenLieuDangChon = null;
            ChiTietPhieuTam.Clear();

            SoLuongNhapXuat = 1;
            DonGiaNhap = 0;
            GhiChu = "";

            NgayLapPhieu = DateTime.Now;
            DangChonNguyenLieu = false;

            if (NhaCungCaps.Count > 0)
            {
                NhaCungCapDangChon = NhaCungCaps[0];
            }

            CapNhatTongPhieuTam();
            CapNhatTrangThaiLapPhieu();

            ThongBao = "Đã hủy / reset phiếu.";
        }

        private void ResetPhieuSauKhiLuu()
        {
            DangLapPhieu = false;
            LoaiPhieuDangLap = "";
            TieuDePhieu = "Chưa lập phiếu";
            HienPopupNguyenLieu = false;
            TuKhoaPopupNguyenLieu = "";
            DanhMucPopupDangChon = "Tất cả";

            NguyenLieuDangChon = null;
            ChiTietPhieuTam.Clear();

            SoLuongNhapXuat = 1;
            DonGiaNhap = 0;
            GhiChu = "";

            NgayLapPhieu = DateTime.Now;
            DangChonNguyenLieu = false;

            if (NhaCungCaps.Count > 0)
            {
                NhaCungCapDangChon = NhaCungCaps[0];
            }

            CapNhatTongPhieuTam();
            CapNhatTrangThaiLapPhieu();
        }

        private void ChonPhieu(PhieuKhoModel phieu)
        {
            PhieuDangChon = phieu;

            if (phieu != null)
            {
                ThongBao = "Đã chọn phiếu: " + phieu.MaCodePhieuKho;
            }
        }

        private string LayLoiChiTiet(Exception ex)
        {
            if (ex == null)
                return "";

            string message = ex.Message;

            if (ex.InnerException != null)
            {
                message += " | Inner: " + ex.InnerException.Message;
            }

            if (ex.InnerException != null && ex.InnerException.InnerException != null)
            {
                message += " | Inner 2: " + ex.InnerException.InnerException.Message;
            }

            return message;
        }
        private string LayTuKhoaUuTienTheoNhaCungCap()
        {
            if (NhaCungCapDangChon == null || string.IsNullOrWhiteSpace(NhaCungCapDangChon.TenNhaCungCap))
                return "";

            string ten = NhaCungCapDangChon.TenNhaCungCap.ToLower();

            if (ten.Contains("sữa") || ten.Contains("milk") || ten.Contains("dairy"))
                return "milk sữa cream kem";

            if (ten.Contains("trà") || ten.Contains("tea"))
                return "tea trà oolong đen xanh";

            if (ten.Contains("cà phê") || ten.Contains("coffee") || ten.Contains("roastery"))
                return "coffee cà phê espresso arabica robusta";

            if (ten.Contains("bánh") || ten.Contains("bakery") || ten.Contains("bột"))
                return "flour bột butter bơ cream kem sugar đường baking bánh";

            if (ten.Contains("syrup") || ten.Contains("siro"))
                return "syrup siro sauce sốt";

            if (ten.Contains("topping") || ten.Contains("trân châu") || ten.Contains("pearl"))
                return "topping trân châu jelly thạch pearl";

            if (ten.Contains("trái cây") || ten.Contains("fruit"))
                return "fruit trái cây cam chanh dâu xoài đào";

            if (ten.Contains("đá") || ten.Contains("ice"))
                return "ice đá";

            return "";
        }
    }
}