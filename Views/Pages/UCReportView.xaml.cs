using System.Windows.Controls;
using QuanLyQuanCaFe.ViewModels;

namespace QuanLyQuanCaFe.Views.Pages
{
    public partial class UCReportView : UserControl
    {
        public UCReportView()
        {
            InitializeComponent();
            DataContext = new BaoCao_VM();
        }
    }
}