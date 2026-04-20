using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HranitelPRO
{
    public partial class SecurityWindow : Window
    {
        private DatabaseService _db;
        private int _employeeId;
        private string _employeeName;

        public SecurityWindow(DatabaseService db, int employeeId, string employeeName)
        {
            InitializeComponent();
            _db = db;
            _employeeId = employeeId;
            _employeeName = employeeName;
            userNameText.Text = employeeName;
            this.Loaded += async (s, e) => await LoadData();
        }

        private async Task LoadData()
        {
            await LoadDepartments();
            await LoadRequests();
        }

        private async Task LoadDepartments()
        {
            var departments = await _db.GetDepartments();
            var list = new ObservableCollection<Department>();
            list.Add(new Department { id = 0, name = "Все" });
            foreach (var d in departments)
                list.Add(d);
            filterDepartmentCombo.ItemsSource = list;
            filterDepartmentCombo.SelectedIndex = 0;
        }

        private async Task LoadRequests()
        {
            try
            {
                string type = null;
                if (filterTypeCombo.SelectedItem is ComboBoxItem typeItem && typeItem.Content.ToString() != "Все")
                    type = typeItem.Content.ToString();

                int deptId = 0;
                if (filterDepartmentCombo.SelectedItem is Department dept && dept.id > 0)
                    deptId = dept.id;

                DateTime? date = filterDatePicker.SelectedDate;

                // Получаем все заявки со статусом "одобрена"
                var allRequests = await _db.GetAllRequests(type, deptId > 0 ? deptId : (int?)null, "одобрена", null);

                if (date.HasValue)
                {
                    allRequests = allRequests.Where(r => r.start_date.Date == date.Value.Date).ToList();
                }

                foreach (var req in allRequests)
                {
                    req.PassportFull = $"{req.passport_series} {req.passport_number}";
                }

                requestsGrid.ItemsSource = allRequests;

                // Отладка: показываем количество загруженных заявок
                System.Diagnostics.Debug.WriteLine($"Загружено одобренных заявок: {allRequests.Count}");

                if (allRequests.Count == 0)
                {
                    MessageBox.Show("Нет одобренных заявок для отображения.\n\n" +
                        "Процесс:\n" +
                        "1. Посетитель подает заявку\n" +
                        "2. Сотрудник общего отдела (GEN001) одобряет заявку\n" +
                        "3. Затем заявка появится здесь",
                        "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private async void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            await LoadRequests();
        }

        private async void AllowAccess_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var request = button?.Tag as RequestViewModel;

            if (request != null)
            {
                if (request.visit_start_time.HasValue)
                {
                    MessageBox.Show("Пропуск уже был использован", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"Разрешить доступ для {request.full_name}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    await _db.UpdateRequestStatus(request.request_id, request.request_type, 2, null, DateTime.Now);

                    // Системный звук
                    SystemSounds.Beep.Play();

                    MessageBox.Show($"Доступ разрешен!\nВремя: {DateTime.Now:HH:mm:ss}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadRequests();
                }
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}