using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HranitelPRO
{
    public partial class MainWindow : Window
    {
        private DatabaseService _db;
        private User _currentUser;

        public MainWindow()
        {
            InitializeComponent();

            // Исправленная строка подключения с параметром Include Error Detail
            string connectionString = "Host=localhost;Port=5432;Database=KhranitelPro;Username=postgres;Password=slon12;Include Error Detail=true";
            _db = new DatabaseService(connectionString);
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = loginEmail.Text.Trim();
            string password = loginPassword.Password;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                loginErrorText.Text = "Заполните все поля";
                return;
            }

            var user = await _db.LoginUser(email, password);
            if (user != null)
            {
                _currentUser = user;
                loginPanel.Visibility = Visibility.Collapsed;
                appPanel.Visibility = Visibility.Visible;
                userPanel.Visibility = Visibility.Visible;
                userEmailText.Text = user.email;
                PersonalButton_Click(null, null);
            }
            else
            {
                loginErrorText.Text = "Неверный email или пароль\n\nТест: test@example.com / Test123!";
            }
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string email = regEmail.Text.Trim();
            string password = regPassword.Password;
            string confirm = regConfirmPassword.Password;

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                registerErrorText.Text = "Неверный email";
                return;
            }

            if (password.Length < 8 || !Regex.IsMatch(password, @"[A-Z]") || !Regex.IsMatch(password, @"[a-z]") || !Regex.IsMatch(password, @"[0-9]") || !Regex.IsMatch(password, @"[!@#$%^&*()]"))
            {
                registerErrorText.Text = "Пароль: 8+ символов, Aa, 1, @";
                return;
            }

            if (password != confirm)
            {
                registerErrorText.Text = "Пароли не совпадают";
                return;
            }

            bool success = await _db.RegisterUser(email, password);
            if (success)
            {
                MessageBox.Show("Регистрация успешна!", "Успех");
                ShowLoginPanel();
            }
            else
            {
                registerErrorText.Text = "Email уже существует";
            }
        }

        private void EmployeeLoginButton_Click(object sender, RoutedEventArgs e)
        {
            var authWindow = new AuthWindow(_db);
            authWindow.Show();
            this.Hide();
        }

        private void ShowLoginPanel()
        {
            loginPanel.Visibility = Visibility.Visible;
            registerPanel.Visibility = Visibility.Collapsed;
            appPanel.Visibility = Visibility.Collapsed;
            userPanel.Visibility = Visibility.Collapsed;
        }

        private void RegisterLink_Click(object sender, RoutedEventArgs e)
        {
            loginPanel.Visibility = Visibility.Collapsed;
            registerPanel.Visibility = Visibility.Visible;
        }

        private void BackToLoginLink_Click(object sender, RoutedEventArgs e)
        {
            ShowLoginPanel();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            _currentUser = null;
            ShowLoginPanel();
        }

        private void PersonalButton_Click(object sender, RoutedEventArgs e)
        {
            contentFrame.Navigate(new Pages.PersonalRequestPage(_db, _currentUser.id));
        }

        private void GroupButton_Click(object sender, RoutedEventArgs e)
        {
            contentFrame.Navigate(new Pages.GroupRequestPage(_db, _currentUser.id));
        }

        private async void MyRequestsButton_Click(object sender, RoutedEventArgs e)
        {
            var requests = await _db.GetUserRequests(_currentUser.id);
            contentFrame.Navigate(new Pages.MyRequestsPage(requests));
        }
    }
}