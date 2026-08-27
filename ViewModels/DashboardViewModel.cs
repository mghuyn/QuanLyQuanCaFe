using System;
using System.Collections.ObjectModel;
using QuanLyQuanCaFe.Core;
using QuanLyQuanCaFe.Models;
using QuanLyQuanCaFe.Services;

namespace QuanLyQuanCaFe.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly DashboardService _dashboardService;

        private int _soSanPham;
        private int _soNhanVien;
        private int _soBan;
        private int _soHoaDonHomNay;
        private string _loi;

        public ObservableCollection<DashboardDoanhThuGioModel> DoanhThuTheoGio { get; set; }

        public int SoSanPham
        {
            get => _soSanPham;
            set => SetProperty(ref _soSanPham, value);
        }

        public int SoNhanVien
        {
            get => _soNhanVien;
            set => SetProperty(ref _soNhanVien, value);
        }

        public int SoBan
        {
            get => _soBan;
            set => SetProperty(ref _soBan, value);
        }

        public int SoHoaDonHomNay
        {
            get => _soHoaDonHomNay;
            set => SetProperty(ref _soHoaDonHomNay, value);
        }

        public string Loi
        {
            get => _loi;
            set => SetProperty(ref _loi, value);
        }

        public DashboardViewModel()
        {
            _dashboardService = new DashboardService();
            DoanhThuTheoGio = new ObservableCollection<DashboardDoanhThuGioModel>();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                DashboardSummaryModel summary = _dashboardService.GetSummary();

                SoSanPham = summary.SoSanPham;
                SoNhanVien = summary.SoNhanVien;
                SoBan = summary.SoBan;
                SoHoaDonHomNay = summary.SoHoaDonHomNay;

                DoanhThuTheoGio.Clear();

                foreach (var item in _dashboardService.GetDoanhThuTheoGioHomNay())
                {
                    DoanhThuTheoGio.Add(item);
                }

                Loi = "";
            }
            catch (Exception ex)
            {
                Loi = "Lỗi tải Dashboard: " + LayLoiChiTiet(ex);
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
