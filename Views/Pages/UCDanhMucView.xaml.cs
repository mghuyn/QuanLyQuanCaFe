using System.Windows.Controls;
using QuanLyQuanCaFe.ViewModels;

namespace QuanLyQuanCaFe.Views.Pages
{
    public partial class UCDanhMucView : UserControl
    {
        public UCDanhMucView()
        {
            InitializeComponent();
            DataContext = new DanhMuc_VM();
        }
    }
}
