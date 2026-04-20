using System.Windows;

namespace HranitelPRO
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
                // Для отладки - показываем роль
                MessageBox.Show($"Добро пожаловать, {name}!\nРоль: {role}\nПодразделение: {department}",
                    "Успешный вход", MessageBoxButton.OK, MessageBoxImage.Information);

                if (role == "GeneralDept")
                {
                    var deptWindow = new GeneralDeptWindow(_db, id, name);
                    deptWindow.Show();
                    this.Close();
                }
                else if (role == "Security")
                {
                    var securityWindow = new SecurityWindow(_db, id, name);
                    securityWindow.Show();
                    this.Close();
                }
                else if (role == "DepartmentEmployee")
                {
                    int deptId = await _db.GetEmployeeDepartmentId(id);
                    var departmentWindow = new DepartmentWindow(_db, id, name, deptId, department);
                    departmentWindow.Show();
                    this.Close();
                }
                else
                {
                    errorText.Text = $"Неизвестная роль: {role}";
                }
            }
            else
            {
                errorText.Text = "Неверный код авторизации\n\nТестовые коды: GEN001, GEN002, SEC001, DEP001, DEP002";
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