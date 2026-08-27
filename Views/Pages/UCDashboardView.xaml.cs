using System.Windows;
using System.Windows.Controls;
using QuanLyQuanCaFe.ViewModels;

namespace QuanLyQuanCaFe.Views.Pages
{
    public partial class UCDashboardView : UserControl
    {
        public UCDashboardView()
        {
            InitializeComponent();
        }

        private void ChuyenTrang(string maManHinh)
        {
            var window = Window.GetWindow(this);

            if (window == null)
                return;

            var mainVM = window.DataContext as MainViewModel;

            if (mainVM == null)
                return;

            mainVM.Navigate(maManHinh);
        }

        private void MoBanHang_Click(object sender, RoutedEventArgs e)
        {
            ChuyenTrang("POS");
        }

        private void MoSanPham_Click(object sender, RoutedEventArgs e)
        {
            ChuyenTrang("ProductTest");
        }

        private void MoKho_Click(object sender, RoutedEventArgs e)
        {
            ChuyenTrang("Inventory");
        }

        private void MoNhanVien_Click(object sender, RoutedEventArgs e)
        {
            ChuyenTrang("Employees");
        }
    }
}