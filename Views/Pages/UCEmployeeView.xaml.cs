using System.Windows.Controls;
using QuanLyQuanCaFe.ViewModels;

namespace QuanLyQuanCaFe.Views.Pages
{
    public partial class UCEmployeeView : UserControl
    {
        public UCEmployeeView()
        {
            InitializeComponent();
            DataContext = new NhanVien_VM();
        }
    }
}