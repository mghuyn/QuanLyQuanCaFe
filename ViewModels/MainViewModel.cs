using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QuanLyQuanCaFe.Commands;
using QuanLyQuanCaFe.Core;
using QuanLyQuanCaFe.Views.Pages;

namespace QuanLyQuanCaFe.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private UserControl _currentView;
        private string _pageTitle;
        private string _appName = "CafeManager";
        private string _currentMenuKey;

        public ObservableCollection<NavigationItem> MenuItems { get; set; }

        public UserControl CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public string PageTitle
        {
            get => _pageTitle;
            set => SetProperty(ref _pageTitle, value);
        }

        public string AppName
        {
            get => _appName;
            set => SetProperty(ref _appName, value);
        }


        public string CurrentMenuKey
        {
            get => _currentMenuKey;
            set
            {
                if (SetProperty(ref _currentMenuKey, value))
                {
                    OnPropertyChanged(nameof(TopBarVisibility));
                    OnPropertyChanged(nameof(TopBarHeight));
                }
            }
        }

        public Visibility TopBarVisibility => CurrentMenuKey == "Dashboard" ? Visibility.Visible : Visibility.Collapsed;

        public GridLength TopBarHeight => CurrentMenuKey == "Dashboard" ? new GridLength(86) : new GridLength(0);

        public string TenNhanVien => AppSession.HoTenNhanVien ?? "Nhân viên";

        public string TenVaiTro => AppSession.TenVaiTro ?? "Đang đăng nhập";

        public ICommand NavigateCommand { get; set; }
        public ICommand LogoutCommand { get; set; }

        public MainViewModel()
        {
            NavigateCommand = new RelayCommand(p => Navigate(p?.ToString()));
            LogoutCommand = new RelayCommand(p => Logout());

            MenuItems = new ObservableCollection<NavigationItem>();

            AddMenu("Tổng quan", "☕", "Dashboard", "REPORT_BASIC");
            AddMenu("Bán hàng", "🧾", "POS", "ORDER_SELL");
            AddMenu("Sơ đồ bàn", "▦", "Tables", "TABLE_MAP_VIEW");
            AddMenu("Pha chế", "🍵", "Kitchen", "KITCHEN_VIEW");
            AddMenu("Sản phẩm", "🧋", "ProductTest", "PRODUCT_MANAGE");
            AddMenu("Danh mục", "🗂", "DanhMuc", "CATEGORY_MANAGE");
            AddMenu("Kho", "📦", "Inventory", "INVENTORY_MANAGE");
            AddMenu("Khách hàng", "👤", "Customers", "CUSTOMER_MANAGE");
            AddMenu("Báo cáo", "📊", "Reports", "REPORT_BASIC");
            AddMenu("Nhân viên", "👥", "Employees", "EMPLOYEE_MANAGE");
            

            if (MenuItems.Count > 0)
                Navigate(MenuItems[0].MaManHinh);
            else
                Navigate("NoPermission");
        }

        private void AddMenu(string tieuDe, string bieuTuong, string maManHinh, string maQuyen)
        {
            if (!AppSession.HasPermission(maQuyen))
                return;

            MenuItems.Add(new NavigationItem
            {
                TieuDe = tieuDe,
                BieuTuong = bieuTuong,
                MaManHinh = maManHinh,
                MaQuyen = maQuyen,
                Command = NavigateCommand
            });
        }


        private void Logout()
        {
            AppSession.Clear();
            var login = new QuanLyQuanCaFe.Views.LoginWindow();
            var current = Application.Current.MainWindow;
            Application.Current.MainWindow = login;
            login.Show();
            current?.Close();
        }

        public void Navigate(string key)
        {
            CurrentMenuKey = key;
            foreach (var item in MenuItems)
                item.IsActive = item.MaManHinh == key;

            switch (key)
            {
                case "Dashboard":
                    PageTitle = "Tổng quan";
                    CurrentView = new UCDashboardView();
                    break;

                case "POS":
                    PageTitle = "Bán hàng POS";
                    CurrentView = new UCPosView();
                    break;

                case "Tables":
                    PageTitle = "Sơ đồ bàn";
                    CurrentView = new UCTableMapView();
                    break;

                case "Kitchen":
                    PageTitle = "Pha chế";
                    CurrentView = new UCKitchenView();
                    break;

                case "Inventory":
                    PageTitle = "Quản lý kho";
                    CurrentView = new UCKho_View();
                    break;

                case "Customers":
                    PageTitle = "Khách hàng";
                    CurrentView = new UCCustomerView();
                    break;

                case "Reports":
                    PageTitle = "Báo cáo";
                    CurrentView = new UCReportView();
                    break;

                case "Employees":
                    PageTitle = "Nhân viên";
                    CurrentView = new UCEmployeeView();
                    break;

                case "ProductTest":
                    PageTitle = "Sản phẩm";
                    CurrentView = new UCSanPhamTestView();
                    break;

                case "DanhMuc":
                    PageTitle = "Danh mục";
                    CurrentView = new UCDanhMucView();
                    break;

                default:
                    PageTitle = "Không có quyền";
                    CurrentView = new UCDashboardView();
                    break;
            }
        }
    }
}