using Microsoft.Extensions.Configuration;
using PassOfficeApp.Models;
using PassOfficeApp.Pages;
using PassOfficeApp.Services;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PassOfficeApp
{
    public partial class MainWindow : Window
    {
        private PostgresService _postgresService;
        private User _currentUser;
        private string _connectionString;

        public MainWindow()
        {
            InitializeComponent();
            InitializeServices();
        }

        private void InitializeServices()
        {
            try
            {
                // Загрузка конфигурации
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .Build();

                _connectionString = configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrEmpty(_connectionString))
                {
                    // Строка подключения по умолчанию
                    _connectionString = "Host=localhost;Port=5432;Database=KhranitelPro;Username=postgres;Password=123456";
                }

                _postgresService = new PostgresService(_connectionString);

                // Проверка подключения при запуске
                this.Loaded += async (s, e) => await TestDatabaseConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task TestDatabaseConnection()
        {
            bool isConnected = await _postgresService.TestConnection();
            if (!isConnected)
            {
                MessageBox.Show(
                    "Не удалось подключиться к базе данных.\n\n" +
                    "Проверьте:\n" +
                    "1. Запущен ли PostgreSQL сервер\n" +
                    "2. Правильно ли указаны параметры в appsettings.json\n" +
                    "3. Существует ли база данных KhranitelPro\n\n" +
                    $"Используется строка: {_connectionString}\n\n" +
                    "Вы можете войти с тестовыми данными после настройки БД.",
                    "Ошибка подключения",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string email = loginEmail.Text.Trim();
            string password = loginPassword.Password;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                loginErrorText.Text = "Заполните email и пароль";
                return;
            }

            string method = (authMethodCombo.SelectedItem as ComboBoxItem)?.Content.ToString();
            User user = null;

            try
            {
                switch (method)
                {
                    case "📝 SQL запрос":
                        user = await _postgresService.LoginUserSQL(email, password);
                        break;
                    case "🗄️ Хранимая процедура":
                        user = await _postgresService.LoginUserSP(email, password);
                        break;
                    case "🔷 ORM (Entity Framework)":
                        // Для ORM используем тот же SQL метод (можно добавить позже)
                        user = await _postgresService.LoginUserSQL(email, password);
                        break;
                }

                if (user != null)
                {
                    _currentUser = user;
                    loginErrorText.Text = "";
                    ShowAppPanel(user.email);
                }
                else
                {
                    loginErrorText.Text = "Неверный email или пароль\n\nТестовый вход: test@example.com / Test123!";
                }
            }
            catch (Exception ex)
            {
                loginErrorText.Text = $"Ошибка: {ex.Message}";
            }
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string email = regEmail.Text.Trim();
            string password = regPassword.Password;
            string confirmPassword = regConfirmPassword.Password;

            // Валидация email
            if (!IsValidEmail(email))
            {
                registerErrorText.Text = "Введите корректный email";
                return;
            }

            // Валидация пароля
            if (!IsValidPassword(password))
            {
                registerErrorText.Text = "Пароль должен содержать минимум 8 символов, заглавные/строчные буквы, цифры и спецсимволы";
                return;
            }

            if (password != confirmPassword)
            {
                registerErrorText.Text = "Пароли не совпадают";
                return;
            }

            string method = (regMethodCombo.SelectedItem as ComboBoxItem)?.Content.ToString();
            bool success = false;

            try
            {
                switch (method)
                {
                    case "📝 SQL запрос":
                        success = await _postgresService.RegisterUserSQL(email, password);
                        break;
                    case "🗄️ Хранимая процедура":
                        success = await _postgresService.RegisterUserSP(email, password);
                        break;
                    case "🔷 ORM (Entity Framework)":
                        success = await _postgresService.RegisterUserSQL(email, password);
                        break;
                }

                if (success)
                {
                    registerErrorText.Text = "";
                    MessageBox.Show("Регистрация успешна! Теперь войдите в систему.", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    ShowLoginPanel();
                }
                else
                {
                    registerErrorText.Text = "Пользователь с таким email уже существует";
                }
            }
            catch (Exception ex)
            {
                registerErrorText.Text = $"Ошибка: {ex.Message}";
            }
        }

        private void ShowAppPanel(string userEmail)
        {
            loginPanel.Visibility = Visibility.Collapsed;
            registerPanel.Visibility = Visibility.Collapsed;
            appPanel.Visibility = Visibility.Visible;
            userPanel.Visibility = Visibility.Visible;
            userEmailText.Text = userEmail;

            NewRequestButton_Click(null, null);
        }

        private void ShowLoginPanel()
        {
            loginPanel.Visibility = Visibility.Visible;
            registerPanel.Visibility = Visibility.Collapsed;
            appPanel.Visibility = Visibility.Collapsed;
            userPanel.Visibility = Visibility.Collapsed;

            loginEmail.Text = "";
            loginPassword.Password = "";
            loginErrorText.Text = "";
        }

        private void RegisterLink_Click(object sender, RoutedEventArgs e)
        {
            loginPanel.Visibility = Visibility.Collapsed;
            registerPanel.Visibility = Visibility.Visible;
            registerErrorText.Text = "";
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

        private void NewRequestButton_Click(object sender, RoutedEventArgs e)
        {
            var page = new PersonalRequestPage(_postgresService, _currentUser.id);
            contentFrame.Navigate(page);
        }

        private async void MyRequestsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var requests = await _postgresService.GetUserRequestsSQL(_currentUser.id);
                var page = new MyRequestsPage(requests);
                contentFrame.Navigate(page);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заявок: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPassword(string password)
        {
            if (password.Length < 8) return false;
            if (!Regex.IsMatch(password, @"[A-Z]")) return false;
            if (!Regex.IsMatch(password, @"[a-z]")) return false;
            if (!Regex.IsMatch(password, @"[0-9]")) return false;
            if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?"":{}|<>]")) return false;
            return true;
        }
    }
}