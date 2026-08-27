using System.Windows.Controls;
using QuanLyQuanCaFe.ViewModels;

namespace QuanLyQuanCaFe.Views.Pages
{
    public partial class UCKho_View : UserControl
    {
        public UCKho_View()
        {
            InitializeComponent();
            DataContext = new Kho_VM();
        }
    }
}