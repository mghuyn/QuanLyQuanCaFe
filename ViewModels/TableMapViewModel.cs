using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows;
using System.Data.Entity.Validation;
using System.Data.Entity.Infrastructure;
using QuanLyQuanCaFe.Commands;
using QuanLyQuanCaFe.Core;
using QuanLyQuanCaFe.Models;
using QuanLyQuanCaFe.Services;

namespace QuanLyQuanCaFe.ViewModels
{
    public class TableMapViewModel : BaseViewModel
    {
        private readonly TableService _tableService;
        private string _khuVucDangChon;
        private TableCardModel _banDangChon;
        private string _thongBao;
        private string _tenBanMoi;
        private int _soGheMoi;
        private string _ghiChuBanMoi;
        private string _trangThaiMoi;
        private bool _hienPopupChuyenBan;
        private bool _xacNhanHoaDonDaThanhToan;
        private TableCardModel _banChuyenDen;

        public ObservableCollection<string> KhuVucs { get; set; }
        public ObservableCollection<TableCardModel> BanCafes { get; set; }
        public ObservableCollection<KhuVucBanGroupModel> NhomBanCafes { get; set; }
        public ObservableCollection<TableCardModel> DanhSachBanTrongDeChuyen { get; set; }
        public ObservableCollection<string> TrangThaiOptions { get; set; }

        public string KhuVucDangChon
        {
            get => _khuVucDangChon;
            set { SetProperty(ref _khuVucDangChon, value); LoadBan(); }
        }

        public TableCardModel BanDangChon
        {
            get => _banDangChon;
            set
            {
                SetProperty(ref _banDangChon, value);
                TrangThaiMoi = value != null ? value.TrangThaiBan : "AVAILABLE";
                XacNhanHoaDonDaThanhToan = false;
                OnPropertyChanged(nameof(CoBanDangChon));
                OnPropertyChanged(nameof(CoHoaDonDangPhucVu));
                OnPropertyChanged(nameof(CoTheMoBanHang));
                OnPropertyChanged(nameof(CoTheChuyenBan));
            }
        }

        public bool CoBanDangChon => BanDangChon != null;
        public bool CoHoaDonDangPhucVu => BanDangChon != null && BanDangChon.CoHoaDonDangPhucVu;
        public bool CoTheMoBanHang => BanDangChon != null && (BanDangChon.TrangThaiBan == "AVAILABLE" || BanDangChon.TrangThaiBan == "RESERVED");
        public bool CoTheChuyenBan => BanDangChon != null && BanDangChon.CoHoaDonDangPhucVu;

        public string ThongBao { get => _thongBao; set => SetProperty(ref _thongBao, value); }
        public string TenBanMoi { get => _tenBanMoi; set => SetProperty(ref _tenBanMoi, value); }
        public int SoGheMoi { get => _soGheMoi; set => SetProperty(ref _soGheMoi, value); }
        public string GhiChuBanMoi { get => _ghiChuBanMoi; set => SetProperty(ref _ghiChuBanMoi, value); }
        public string TrangThaiMoi
        {
            get => _trangThaiMoi;
            set
            {
                if (SetProperty(ref _trangThaiMoi, value))
                {
                    OnPropertyChanged(nameof(TrangThaiLaTrong));
                    OnPropertyChanged(nameof(TrangThaiDaDat));
                    OnPropertyChanged(nameof(TrangThaiDangPhucVu));
                    OnPropertyChanged(nameof(TrangThaiCanDon));
                }
            }
        }

        public bool TrangThaiLaTrong
        {
            get => TrangThaiMoi == "AVAILABLE";
            set { if (value) TrangThaiMoi = "AVAILABLE"; }
        }

        public bool TrangThaiDaDat
        {
            get => TrangThaiMoi == "RESERVED";
            set { if (value) TrangThaiMoi = "RESERVED"; }
        }

        public bool TrangThaiDangPhucVu
        {
            get => TrangThaiMoi == "OCCUPIED";
            set { if (value) TrangThaiMoi = "OCCUPIED"; }
        }

        public bool TrangThaiCanDon
        {
            get => TrangThaiMoi == "CLEANING";
            set { if (value) TrangThaiMoi = "CLEANING"; }
        }
        public bool HienPopupChuyenBan { get => _hienPopupChuyenBan; set => SetProperty(ref _hienPopupChuyenBan, value); }

        /// <summary>
        /// Checkbox xử lý tình huống dữ liệu bị kẹt: bàn còn hóa đơn mở dù thực tế đã thanh toán.
        /// Khi bật, nếu chuyển bàn sang Trống/Cần dọn thì hệ thống sẽ đánh dấu hóa đơn đang mở là PAID/COMPLETED và cắt MaBan.
        /// </summary>
        public bool XacNhanHoaDonDaThanhToan
        {
            get => _xacNhanHoaDonDaThanhToan;
            set => SetProperty(ref _xacNhanHoaDonDaThanhToan, value);
        }

        public TableCardModel BanChuyenDen { get => _banChuyenDen; set => SetProperty(ref _banChuyenDen, value); }

        public int TongBan => BanCafes.Count;
        public int BanTrong => BanCafes.Count(x => x.TrangThaiBan == "AVAILABLE");
        public int BanDangPhucVu => BanCafes.Count(x => x.TrangThaiBan == "OCCUPIED");
        public int BanCanDon => BanCafes.Count(x => x.TrangThaiBan == "CLEANING");

        public ICommand ChonBanCommand { get; set; }
        public ICommand LamMoiCommand { get; set; }
        public ICommand ThemBanCommand { get; set; }
        public ICommand DoiTrangThaiCommand { get; set; }
        public ICommand DatBanCommand { get; set; }
        public ICommand BanTrongCommand { get; set; }
        public ICommand CanDonCommand { get; set; }
        public ICommand NgungDungCommand { get; set; }
        public ICommand XoaBanCommand { get; set; }
        public ICommand MoChuyenBanCommand { get; set; }
        public ICommand XacNhanChuyenBanCommand { get; set; }
        public ICommand DongChuyenBanCommand { get; set; }

        public TableMapViewModel()
        {
            _tableService = new TableService();
            KhuVucs = new ObservableCollection<string>();
            BanCafes = new ObservableCollection<TableCardModel>();
            NhomBanCafes = new ObservableCollection<KhuVucBanGroupModel>();
            DanhSachBanTrongDeChuyen = new ObservableCollection<TableCardModel>();
            TrangThaiOptions = new ObservableCollection<string> { "AVAILABLE", "RESERVED", "OCCUPIED", "CLEANING", "INACTIVE" };

            ChonBanCommand = new RelayCommand(p => ChonBan(p as TableCardModel));
            LamMoiCommand = new RelayCommand(p => LoadData());
            ThemBanCommand = new RelayCommand(p => ThemBan());
            DoiTrangThaiCommand = new RelayCommand(p => DoiTrangThai(p != null ? p.ToString() : TrangThaiMoi));
            DatBanCommand = new RelayCommand(p => DoiTrangThai("RESERVED"));
            BanTrongCommand = new RelayCommand(p => DoiTrangThai("AVAILABLE"));
            CanDonCommand = new RelayCommand(p => DoiTrangThai("CLEANING"));
            NgungDungCommand = new RelayCommand(p => DoiTrangThai("INACTIVE"));
            XoaBanCommand = new RelayCommand(p => XoaBan());
            MoChuyenBanCommand = new RelayCommand(p => MoChuyenBan());
            XacNhanChuyenBanCommand = new RelayCommand(p => XacNhanChuyenBan());
            DongChuyenBanCommand = new RelayCommand(p => HienPopupChuyenBan = false);

            SoGheMoi = 2;
            TrangThaiMoi = "AVAILABLE";
            LoadData();
        }


        private string LayLoiDayDu(Exception ex)
        {
            if (ex == null) return "Không rõ lỗi.";

            var parts = new System.Collections.Generic.List<string>();
            var cur = ex;
            while (cur != null)
            {
                if (!string.IsNullOrWhiteSpace(cur.Message))
                    parts.Add(cur.Message);
                cur = cur.InnerException;
            }

            return string.Join(" -> ", parts.Distinct());
        }

        private void BaoThanhCong(string noiDung)
        {
            ThongBao = noiDung;
            MessageBox.Show(noiDung, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BaoLoi(string noiDung)
        {
            ThongBao = noiDung;
            MessageBox.Show(noiDung, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void LoadData()
        {
            KhuVucs.Clear();
            foreach (var item in _tableService.GetKhuVuc()) KhuVucs.Add(item);
            if (string.IsNullOrWhiteSpace(KhuVucDangChon)) KhuVucDangChon = "Tất cả";
            LoadBan();
        }

        private void LoadBan()
        {
            if (BanCafes == null) return;

            int? maBanDangChon = BanDangChon != null ? (int?)BanDangChon.MaBan : null;

            BanCafes.Clear();
            NhomBanCafes.Clear();

            foreach (var item in _tableService.GetBan(KhuVucDangChon)) BanCafes.Add(item);

            var groups = BanCafes
                .GroupBy(x => new { x.ThuTuKhuVuc, x.TenKhuVuc, x.TenTang })
                .OrderBy(g => g.Key.ThuTuKhuVuc)
                .ThenBy(g => g.Key.TenKhuVuc);

            foreach (var g in groups)
            {
                var group = new KhuVucBanGroupModel
                {
                    ThuTuKhuVuc = g.Key.ThuTuKhuVuc,
                    TenKhuVuc = g.Key.TenKhuVuc,
                    TenTang = g.Key.TenTang
                };

                foreach (var ban in g.OrderBy(x => x.SoThuTuBan).ThenBy(x => x.TenBan))
                    group.BanTrongKhuVuc.Add(ban);

                NhomBanCafes.Add(group);
            }

            foreach (var item in BanCafes) item.IsSelected = false;

            var banCanChonLai = maBanDangChon.HasValue
                ? BanCafes.FirstOrDefault(x => x.MaBan == maBanDangChon.Value)
                : BanCafes.FirstOrDefault();

            BanDangChon = banCanChonLai;

            if (BanDangChon != null)
            {
                BanDangChon.IsSelected = true;
                TrangThaiMoi = BanDangChon.TrangThaiBan;
            }

            CapNhatThongKe();
        }

        private void CapNhatThongKe()
        {
            OnPropertyChanged(nameof(TongBan));
            OnPropertyChanged(nameof(BanTrong));
            OnPropertyChanged(nameof(BanDangPhucVu));
            OnPropertyChanged(nameof(BanCanDon));
        }

        private void ChonBan(TableCardModel ban)
        {
            if (ban == null) return;
            foreach (var item in BanCafes) item.IsSelected = false;
            ban.IsSelected = true;
            BanDangChon = ban;
        }

        private void ThemBan()
        {
            try
            {
                _tableService.ThemBan(KhuVucDangChon, TenBanMoi, SoGheMoi, GhiChuBanMoi);
                TenBanMoi = "";
                SoGheMoi = 2;
                GhiChuBanMoi = "";
                LoadData();
                BaoThanhCong("Đã thêm bàn mới.");
            }
            catch (Exception ex)
            {
                BaoLoi("Lỗi thêm bàn: " + ex.Message);
            }
        }

        private void DoiTrangThai(string trangThai)
        {
            try
            {
                if (BanDangChon == null)
                {
                    BaoLoi("Vui lòng chọn bàn cần đổi trạng thái.");
                    return;
                }

                if (BanDangChon.CoHoaDonDangPhucVu && trangThai == "INACTIVE")
                {
                    BaoLoi("Bàn đang có hóa đơn phục vụ. Hãy thanh toán/hủy/chuyển bàn trước khi ngưng dùng.");
                    LoadBan();
                    return;
                }

                // Chỉ chặn chuyển thẳng Đang phục vụ -> Trống nếu chưa tick xác nhận.
                // Trường hợp bill bị kẹt: tick checkbox để hệ thống đánh dấu bill là đã thanh toán và cắt liên kết với bàn.
                bool dungCheDoXacNhanThanhToan = XacNhanHoaDonDaThanhToan
                    && BanDangChon.CoHoaDonDangPhucVu
                    && (trangThai == "AVAILABLE" || trangThai == "CLEANING");

                if (BanDangChon.CoHoaDonDangPhucVu
                    && trangThai == "AVAILABLE"
                    && BanDangChon.TrangThaiBan != "CLEANING"
                    && !dungCheDoXacNhanThanhToan)
                {
                    BaoLoi("Bàn đang có hóa đơn chưa thanh toán. Nếu hóa đơn này thực tế đã thanh toán, hãy tick 'Xác nhận hóa đơn này đã được thanh toán' rồi chọn Trống/Cần dọn lại.");
                    LoadBan();
                    return;
                }

                int maBanVuaChon = BanDangChon.MaBan;
                _tableService.DoiTrangThaiBan(BanDangChon.MaBan, trangThai, dungCheDoXacNhanThanhToan);
                XacNhanHoaDonDaThanhToan = false;
                LoadBan();

                var banChonLai = BanCafes.FirstOrDefault(x => x.MaBan == maBanVuaChon);
                if (banChonLai != null)
                    ChonBan(banChonLai);

                BaoThanhCong("Đã cập nhật trạng thái bàn.");
            }
            catch (DbEntityValidationException ex)
            {
                var loiChiTiet = string.Join("; ",
                    ex.EntityValidationErrors
                        .SelectMany(e => e.ValidationErrors)
                        .Select(e => e.PropertyName + ": " + e.ErrorMessage));

                BaoLoi("Lỗi đổi trạng thái bàn: " + loiChiTiet);
            }
            catch (DbUpdateException ex)
            {
                BaoLoi("Lỗi đổi trạng thái bàn: " + LayLoiDayDu(ex));
            }
            catch (Exception ex)
            {
                BaoLoi("Lỗi đổi trạng thái bàn: " + LayLoiDayDu(ex));
            }
        }


        private void XoaBan()
        {
            try
            {
                if (BanDangChon == null)
                {
                    BaoLoi("Vui lòng chọn bàn cần xóa/ngừng dùng.");
                    return;
                }

                _tableService.XoaBan(BanDangChon.MaBan);
                LoadData();
                BaoThanhCong("Đã xóa/ngừng dùng bàn khỏi sơ đồ.");
            }
            catch (Exception ex)
            {
                BaoLoi("Lỗi xóa bàn: " + ex.Message);
            }
        }

        private void MoChuyenBan()
        {
            try
            {
                if (BanDangChon == null || !BanDangChon.CoHoaDonDangPhucVu)
                {
                    BaoLoi("Chỉ chuyển bàn khi bàn đang có hóa đơn phục vụ.");
                    return;
                }

                DanhSachBanTrongDeChuyen.Clear();
                foreach (var item in _tableService.GetBanTrongDeChuyen(BanDangChon.MaBan))
                    DanhSachBanTrongDeChuyen.Add(item);

                BanChuyenDen = DanhSachBanTrongDeChuyen.FirstOrDefault();
                HienPopupChuyenBan = true;
            }
            catch (Exception ex)
            {
                BaoLoi("Lỗi mở chuyển bàn: " + ex.Message);
            }
        }

        private void XacNhanChuyenBan()
        {
            try
            {
                if (BanDangChon == null || !BanDangChon.CoHoaDonDangPhucVu)
                    throw new Exception("Không có hóa đơn đang phục vụ để chuyển bàn.");

                if (BanChuyenDen == null)
                    throw new Exception("Vui lòng chọn bàn muốn chuyển đến.");

                _tableService.ChuyenBan(BanDangChon.MaHoaDonBanDangPhucVu.Value, BanChuyenDen.MaBan);
                HienPopupChuyenBan = false;
                LoadBan();
                BaoThanhCong("Đã chuyển bàn thành công.");
            }
            catch (Exception ex)
            {
                BaoLoi("Lỗi chuyển bàn: " + ex.Message);
            }
        }
    }
}
