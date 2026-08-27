using System.Windows;
using System.Windows.Controls;
using QuanLyQuanCaFe.ViewModels;

namespace QuanLyQuanCaFe.Views.Pages
{
    public partial class UCTableMapView : UserControl
    {
        public UCTableMapView()
        {
            InitializeComponent();
        }

        private void MoBanHang_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            var mainVM = window != null ? window.DataContext as MainViewModel : null;
            if (mainVM != null)
            {
                mainVM.Navigate("POS");
            }
        }
    }
}
