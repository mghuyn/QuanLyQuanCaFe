using System;
using System.Windows;
using System.Windows.Input;

using QuanLyQuanCaFe.Core;
using QuanLyQuanCaFe.Commands;

using QuanLyQuanCaFe.Services;

namespace QuanLyQuanCaFe.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly AuthService _authService;

        private string _username;
        private string _errorMessage;
        private bool _isLoading;

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand LoginCommand { get; }

        public event Action LoginSucceeded;

        public LoginViewModel()
        {
            _authService = new AuthService();
            LoginCommand = new RelayCommand(Login);
        }

        private void Login(object parameter)
        {
            var passwordBox = parameter as System.Windows.Controls.PasswordBox;
            string password = passwordBox?.Password;

            IsLoading = true;
            ErrorMessage = "";

            try
            {
                string error;
                bool ok = _authService.Login(Username, password, out error);

                if (!ok)
                {
                    ErrorMessage = error;
                    return;
                }

                LoginSucceeded?.Invoke();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Lỗi đăng nhập: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}