using System.Windows.Input;
using QuanLyQuanCaFe.Core;

namespace QuanLyQuanCaFe.Core
{
    public class NavigationItem : BaseViewModel
    {
        private bool _isActive;

        public string TieuDe { get; set; }
        public string BieuTuong { get; set; }
        public string MaManHinh { get; set; }
        public string MaQuyen { get; set; }
        public ICommand Command { get; set; }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (SetProperty(ref _isActive, value))
                {
                    OnPropertyChanged(nameof(ActiveBackground));
                    OnPropertyChanged(nameof(ActiveForeground));
                    OnPropertyChanged(nameof(ActiveIconBackground));
                }
            }
        }

        public string ActiveBackground => IsActive ? "#EFF6FF" : "Transparent";
        public string ActiveForeground => IsActive ? "#0B63F6" : "#334155";
        public string ActiveIconBackground => IsActive ? "#DBEAFE" : "Transparent";
    }
}
