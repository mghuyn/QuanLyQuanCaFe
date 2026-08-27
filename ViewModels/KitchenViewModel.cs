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
    public class KitchenViewModel : BaseViewModel
    {
        private readonly KitchenService _kitchenService;
        private string _thongBao;

        private RecipeProductOptionModel _monCongThucDangChon;
        private KhoItemModel _nguyenLieuCongThucDangChon;
        private RecipeManageItemModel _dongCongThucDangChon;
        private decimal _soLuongCanNhap;
        private decimal _phanTramHaoHutNhap;
        private string _ghiChuCongThucNhap;

        public ObservableCollection<KitchenOrderItemModel> MonMoi { get; set; }
        public ObservableCollection<KitchenOrderItemModel> MonDangPha { get; set; }
        public ObservableCollection<KitchenOrderItemModel> MonHoanTat { get; set; }
        public ObservableCollection<RecipeProductOptionModel> DanhSachMonCongThuc { get; set; }
        public ObservableCollection<KhoItemModel> DanhSachNguyenLieuCongThuc { get; set; }
        public ObservableCollection<RecipeManageItemModel> DanhSachCongThuc { get; set; }

        public string ThongBao
        {
            get => _thongBao;
            set => SetProperty(ref _thongBao, value);
        }


        public RecipeProductOptionModel MonCongThucDangChon
        {
            get => _monCongThucDangChon;
            set
            {
                SetProperty(ref _monCongThucDangChon, value);
                LoadCongThucTheoMon();
            }
        }

        public KhoItemModel NguyenLieuCongThucDangChon
        {
            get => _nguyenLieuCongThucDangChon;
            set => SetProperty(ref _nguyenLieuCongThucDangChon, value);
        }

        public RecipeManageItemModel DongCongThucDangChon
        {
            get => _dongCongThucDangChon;
            set
            {
                SetProperty(ref _dongCongThucDangChon, value);
                if (value != null)
                {
                    MonCongThucDangChon = DanhSachMonCongThuc.FirstOrDefault(x => x.MaBienThe == value.MaBienThe);
                    NguyenLieuCongThucDangChon = DanhSachNguyenLieuCongThuc.FirstOrDefault(x => x.MaNguyenLieu == value.MaNguyenLieu);
                    SoLuongCanNhap = value.SoLuongCan;
                    PhanTramHaoHutNhap = value.PhanTramHaoHut;
                    GhiChuCongThucNhap = value.GhiChu;
                    ThongBao = "Đang chọn công thức: " + value.TenSanPham + " - " + value.TenNguyenLieu;
                }
            }
        }

        public decimal SoLuongCanNhap { get => _soLuongCanNhap; set => SetProperty(ref _soLuongCanNhap, value); }
        public decimal PhanTramHaoHutNhap { get => _phanTramHaoHutNhap; set => SetProperty(ref _phanTramHaoHutNhap, value); }
        public string GhiChuCongThucNhap { get => _ghiChuCongThucNhap; set => SetProperty(ref _ghiChuCongThucNhap, value); }

        public int TongMonCho => MonMoi.Sum(x => x.SoLuong);
        public int TongMonDangLam => MonDangPha.Sum(x => x.SoLuong);
        public int TongMonHoanTat => MonHoanTat.Sum(x => x.SoLuong);
        public int TongPhieuDangXuLy => MonMoi.Concat(MonDangPha).Select(x => x.MaHoaDonBan).Distinct().Count();

        public ICommand LoadCommand { get; set; }
        public ICommand NhanMonCommand { get; set; }
        public ICommand HoanTatCommand { get; set; }
        public ICommand HuyMonCommand { get; set; }
        public ICommand LoadCongThucCommand { get; set; }
        public ICommand LuuCongThucCommand { get; set; }
        public ICommand XoaCongThucCommand { get; set; }
        public ICommand ChonDongCongThucCommand { get; set; }
        public ICommand ResetCongThucFormCommand { get; set; }

        public KitchenViewModel()
        {
            _kitchenService = new KitchenService();

            MonMoi = new ObservableCollection<KitchenOrderItemModel>();
            MonDangPha = new ObservableCollection<KitchenOrderItemModel>();
            MonHoanTat = new ObservableCollection<KitchenOrderItemModel>();
            DanhSachMonCongThuc = new ObservableCollection<RecipeProductOptionModel>();
            DanhSachNguyenLieuCongThuc = new ObservableCollection<KhoItemModel>();
            DanhSachCongThuc = new ObservableCollection<RecipeManageItemModel>();

            LoadCommand = new RelayCommand(p => LoadData());
            NhanMonCommand = new RelayCommand(p => CapNhatTrangThai(p as KitchenOrderItemModel, "DOING"));
            HoanTatCommand = new RelayCommand(p => CapNhatTrangThai(p as KitchenOrderItemModel, "DONE"));
            HuyMonCommand = new RelayCommand(p => HuyMon(p as KitchenOrderItemModel));
            LoadCongThucCommand = new RelayCommand(p => LoadQuanLyCongThuc());
            LuuCongThucCommand = new RelayCommand(p => LuuCongThuc());
            XoaCongThucCommand = new RelayCommand(p => XoaCongThuc());
            ChonDongCongThucCommand = new RelayCommand(p => DongCongThucDangChon = p as RecipeManageItemModel);
            ResetCongThucFormCommand = new RelayCommand(p => ResetCongThucForm());

            SoLuongCanNhap = 1;
            PhanTramHaoHutNhap = 0;
            GhiChuCongThucNhap = "";

            LoadData();
            LoadQuanLyCongThuc();
        }


        private void LoadQuanLyCongThuc()
        {
            try
            {
                DanhSachMonCongThuc.Clear();
                DanhSachNguyenLieuCongThuc.Clear();

                foreach (var item in _kitchenService.GetSanPhamCongThucOptions())
                    DanhSachMonCongThuc.Add(item);

                foreach (var item in _kitchenService.GetNguyenLieuCongThucOptions())
                    DanhSachNguyenLieuCongThuc.Add(item);

                if (MonCongThucDangChon == null && DanhSachMonCongThuc.Count > 0)
                {
                    MonCongThucDangChon = DanhSachMonCongThuc[0];
                }
                else
                {
                    LoadCongThucTheoMon();
                }

                if (NguyenLieuCongThucDangChon == null && DanhSachNguyenLieuCongThuc.Count > 0)
                    NguyenLieuCongThucDangChon = DanhSachNguyenLieuCongThuc[0];
            }
            catch (Exception ex)
            {
                ThongBao = "Lỗi tải công thức: " + ex.Message;
            }
        }

        private void LoadCongThucTheoMon()
        {
            try
            {
                if (DanhSachCongThuc == null) return;
                DanhSachCongThuc.Clear();

                int? maBienThe = MonCongThucDangChon != null ? (int?)MonCongThucDangChon.MaBienThe : null;
                var data = _kitchenService.GetCongThucs(maBienThe);

                foreach (var item in data)
                    DanhSachCongThuc.Add(item);
            }
            catch (Exception ex)
            {
                ThongBao = "Lỗi tải chi tiết công thức: " + ex.Message;
            }
        }

        private void LuuCongThuc()
        {
            try
            {
                if (MonCongThucDangChon == null) { ThongBao = "Vui lòng chọn món trong danh sách món hiện tại."; return; }
                if (NguyenLieuCongThucDangChon == null) { ThongBao = "Vui lòng chọn nguyên liệu."; return; }

                _kitchenService.LuuCongThuc(MonCongThucDangChon.MaBienThe, NguyenLieuCongThucDangChon.MaNguyenLieu, SoLuongCanNhap, PhanTramHaoHutNhap, GhiChuCongThucNhap);
                LoadCongThucTheoMon();
                LoadData();
                ThongBao = "Đã lưu công thức cho món: " + MonCongThucDangChon.HienThi;
            }
            catch (Exception ex)
            {
                ThongBao = "Lỗi lưu công thức: " + ex.Message;
            }
        }

        private void XoaCongThuc()
        {
            try
            {
                if (DongCongThucDangChon == null) { ThongBao = "Vui lòng chọn dòng công thức cần xóa."; return; }
                _kitchenService.XoaCongThuc(DongCongThucDangChon.MaBienThe, DongCongThucDangChon.MaNguyenLieu);
                LoadCongThucTheoMon();
                ResetCongThucForm();
                LoadData();
                ThongBao = "Đã xóa dòng công thức.";
            }
            catch (Exception ex)
            {
                ThongBao = "Lỗi xóa công thức: " + ex.Message;
            }
        }

        private void ResetCongThucForm()
        {
            DongCongThucDangChon = null;
            NguyenLieuCongThucDangChon = DanhSachNguyenLieuCongThuc.FirstOrDefault();
            SoLuongCanNhap = 1;
            PhanTramHaoHutNhap = 0;
            GhiChuCongThucNhap = "";
        }

        private void LoadData()
        {
            try
            {
                MonMoi.Clear();
                MonDangPha.Clear();
                MonHoanTat.Clear();

                var data = _kitchenService.GetMonCanPhaChe();

                foreach (var item in data.Where(x => x.TrangThaiMon == "NEW"))
                    MonMoi.Add(item);

                foreach (var item in data.Where(x => x.TrangThaiMon == "DOING" || x.TrangThaiMon == "PREPARING"))
                    MonDangPha.Add(item);

                foreach (var item in data.Where(x => x.TrangThaiMon == "DONE"))
                    MonHoanTat.Add(item);

                CapNhatThongKe();
                ThongBao = "Dữ liệu pha chế đã được cập nhật theo ngày hôm nay.";
            }
            catch (Exception ex)
            {
                ThongBao = "Lỗi tải pha chế: " + ex.Message;
            }
        }

        private void CapNhatThongKe()
        {
            OnPropertyChanged(nameof(TongMonCho));
            OnPropertyChanged(nameof(TongMonDangLam));
            OnPropertyChanged(nameof(TongMonHoanTat));
            OnPropertyChanged(nameof(TongPhieuDangXuLy));
        }


        private void HuyMon(KitchenOrderItemModel item)
        {
            try
            {
                if (item == null)
                    return;

                _kitchenService.HuyMon(item.MaChiTietHoaDonBanList, "Order nhầm / khách đổi món");
                LoadData();
                ThongBao = "Đã hủy món và cập nhật lại tồn kho nếu cần.";
            }
            catch (Exception ex)
            {
                ThongBao = "Lỗi hủy món: " + ex.Message;
            }
        }

        private void CapNhatTrangThai(KitchenOrderItemModel item, string trangThaiMoi)
        {
            try
            {
                if (item == null)
                    return;

                _kitchenService.CapNhatTrangThaiMon(item.MaChiTietHoaDonBanList, trangThaiMoi);
                LoadData();
            }
            catch (Exception ex)
            {
                ThongBao = "Lỗi cập nhật: " + ex.Message;
            }
        }
    }
}
