using System.Windows.Controls;
using QuanLyQuanCaFe.ViewModels;

namespace QuanLyQuanCaFe.Views.Pages
{
    public partial class UCCustomerView : UserControl
    {
        public UCCustomerView()
        {
            InitializeComponent();
            DataContext = new KhachHang_VM();
        }
    }
}