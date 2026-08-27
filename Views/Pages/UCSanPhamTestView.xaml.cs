using System.Windows.Controls;
using QuanLyQuanCaFe.ViewModels;

namespace QuanLyQuanCaFe.Views.Pages
{
    public partial class UCSanPhamTestView : UserControl
    {
        public UCSanPhamTestView()
        {
            InitializeComponent();
            DataContext = new SanPhamTestViewModel();
        }
    }
}