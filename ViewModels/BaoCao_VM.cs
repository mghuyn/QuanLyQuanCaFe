using QuanLyQuanCaFe.Commands;
using QuanLyQuanCaFe.Core;
using QuanLyQuanCaFe.Models;
using QuanLyQuanCaFe.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace QuanLyQuanCaFe.ViewModels
{
    public class BaoCao_VM : BaseViewModel
    {
        private readonly BaoCaoService _service;

        private DateTime? _tuNgay;
        private DateTime? _denNgay;
        private string _thongBao;
        private BaoCaoTongQuanModel _tongQuan;

        public ObservableCollection<BaoCaoDoanhThuNgayModel> DoanhThuTheoNgay { get; set; }
        public ObservableCollection<BaoCaoSanPhamBanChayModel> SanPhamBanChay { get; set; }

        public DateTime? TuNgay
        {
            get => _tuNgay;
            set => SetProperty(ref _tuNgay, value);
        }

        public DateTime? DenNgay
        {
            get => _denNgay;
            set => SetProperty(ref _denNgay, value);
        }

        public string ThongBao
        {
            get => _thongBao;
            set => SetProperty(ref _thongBao, value);
        }

        public BaoCaoTongQuanModel TongQuan
        {
            get => _tongQuan;
            set => SetProperty(ref _tongQuan, value);
        }

        public string BaoCaoCapNhatText
        {
            get
            {
                if (TongQuan == null) return "Chưa tải báo cáo.";
                return "Đã tải " + TongQuan.SoHoaDonText + " hóa đơn, doanh thu " + TongQuan.DoanhThuText + ".";
            }
            set { }
        }
        
        public ICommand XemBaoCaoCommand { get; set; }
        public ICommand HomNayCommand { get; set; }
        public ICommand ThangNayCommand { get; set; }
        public ICommand LamMoiCommand { get; set; }
        public ICommand XuatBaoCaoCommand { get; set; }

        public BaoCao_VM()
        {
            _service = new BaoCaoService();

            DoanhThuTheoNgay = new ObservableCollection<BaoCaoDoanhThuNgayModel>();
            SanPhamBanChay = new ObservableCollection<BaoCaoSanPhamBanChayModel>();

            XemBaoCaoCommand = new RelayCommand(p => LoadBaoCao());
            HomNayCommand = new RelayCommand(p => ChonHomNay());
            ThangNayCommand = new RelayCommand(p => ChonThangNay());
            LamMoiCommand = new RelayCommand(p => LoadBaoCao());
            XuatBaoCaoCommand = new RelayCommand(p => XuatBaoCao());

            TongQuan = new BaoCaoTongQuanModel();

            ChonThangNay();
        }

        private void ChonHomNay()
        {
            TuNgay = DateTime.Now.Date;
            DenNgay = DateTime.Now.Date;
            LoadBaoCao();
        }

        private void ChonThangNay()
        {
            DateTime now = DateTime.Now;
            TuNgay = new DateTime(now.Year, now.Month, 1);
            DenNgay = DateTime.Now.Date;
            LoadBaoCao();
        }

        private void LoadBaoCao()
        {
            try
            {
                if (TuNgay == null || DenNgay == null)
                {
                    ThongBao = "Vui lòng chọn khoảng thời gian.";
                    return;
                }

                if (TuNgay.Value.Date > DenNgay.Value.Date)
                {
                    ThongBao = "Từ ngày không được lớn hơn đến ngày.";
                    return;
                }

                TongQuan = _service.GetTongQuan(TuNgay.Value, DenNgay.Value);

                DoanhThuTheoNgay.Clear();
                foreach (var item in _service.GetDoanhThuTheoNgay(TuNgay.Value, DenNgay.Value))
                {
                    DoanhThuTheoNgay.Add(item);
                }

                SanPhamBanChay.Clear();
                foreach (var item in _service.GetSanPhamBanChay(TuNgay.Value, DenNgay.Value))
                {
                    SanPhamBanChay.Add(item);
                }

                OnPropertyChanged(nameof(BaoCaoCapNhatText));
                ThongBao = "Đã tải báo cáo theo khoảng ngày đã chọn.";
            }
            catch (Exception ex)
            {
                ThongBao = "Lỗi tải báo cáo: " + LayLoiChiTiet(ex);
            }
        }

        private void XuatBaoCao()
        {
            try
            {
                if (TuNgay == null || DenNgay == null)
                {
                    ThongBao = "Vui lòng chọn khoảng thời gian trước khi xem report nâng cao.";
                    return;
                }

                var service = new CrystalBaoCaoService();
                service.XemReportDoanhThuNangCao(TuNgay.Value, DenNgay.Value);

                ThongBao = "Đã mở Crystal Report nâng cao doanh thu.";
            }
            catch (Exception ex)
            {
                ThongBao = "Lỗi mở report nâng cao: " + LayLoiChiTiet(ex);
            }
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