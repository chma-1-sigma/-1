using PassOfficeApp.Services;
using System.Windows;

namespace PassOfficeApp
{
    public partial class AuthWindow : Window
    {
        private DatabaseService _db;

        public AuthWindow(DatabaseService db)
        {
            InitializeComponent();
            _db = db;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string code = authCodeBox.Password;
            if (string.IsNullOrEmpty(code))
            {
                errorText.Text = "Введите код авторизации";
                return;
            }

            var (id, name, role, department) = await _db.AuthEmployee(code);

            if (id > 0)
            {
                if (role == "GeneralDept")
                {
                    var deptWindow = new GeneralDeptWindow(_db, id, name);
                    deptWindow.Show();
                    this.Close();
                }
                else if (role == "Security")
                {
                    // TODO: Открыть окно сотрудника охраны
                    MessageBox.Show($"Добро пожаловать, {name}!\nРоль: Сотрудник охраны\nПодразделение: {department}",
                        "Успешный вход", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    errorText.Text = "У вас нет доступа к этому терминалу";
                }
            }
            else
            {
                errorText.Text = "Неверный код авторизации\n\nТестовые коды: GEN001, GEN002, SEC001";
            }
        }
    }
}