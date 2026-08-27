using System.Windows;
using QuanLyQuanCaFe.ViewModels;

namespace QuanLyQuanCaFe.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            var vm = DataContext as LoginViewModel;
            if (vm != null)
            {
                vm.LoginSucceeded += OnLoginSucceeded;
            }
        }

        private void OnLoginSucceeded()
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            Close();
        }
    }
}