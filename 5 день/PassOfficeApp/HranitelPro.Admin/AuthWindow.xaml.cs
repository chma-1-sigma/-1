using System;
using System.Windows;
using HranitelPro.Admin.Services;

namespace HranitelPro.Admin
{
    public partial class AuthWindow : Window
    {
        private DatabaseService _db;
        private string _expectedRole;

        public AuthWindow(DatabaseService db, string expectedRole)
        {
            InitializeComponent();
            _db = db;
            _expectedRole = expectedRole;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string code = authCodeBox.Password;
            if (string.IsNullOrEmpty(code))
            {
                errorText.Text = "Введите код авторизации";
                return;
            }

            try
            {
                var (id, name, role, department) = await _db.AuthEmployee(code);

                if (id > 0)
                {
                    if (role == _expectedRole)
                    {
                        if (role == "GeneralDept")
                        {
                            var window = new GeneralDeptWindow(_db, id, name);
                            window.Show();
                            this.Close();
                        }
                        else if (role == "Security")
                        {
                            var window = new SecurityWindow(_db, id, name);
                            window.Show();
                            this.Close();
                        }
                        else if (role == "Department")
                        {
                            var window = new DepartmentWindow(_db, id, name);
                            window.Show();
                            this.Close();
                        }
                        else
                        {
                            errorText.Text = "Неизвестная роль пользователя";
                        }
                    }
                    else
                    {
                        errorText.Text = $"У вас нет доступа к этому терминалу.\nВаша роль: {role}\nТребуется: {_expectedRole}";
                    }
                }
                else
                {
                    errorText.Text = "Неверный код авторизации\n\nТестовые коды:\nGEN001, GEN002 - общий отдел\nSEC001 - охрана\nDEP001, DEP002 - подразделение";
                }
            }
            catch (Exception ex)
            {
                errorText.Text = $"Ошибка: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Auth error: {ex.Message}");
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}