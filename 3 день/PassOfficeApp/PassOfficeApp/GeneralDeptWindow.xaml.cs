using PassOfficeApp.Models;
using PassOfficeApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PassOfficeApp
{
    public partial class GeneralDeptWindow : Window
    {
        private DatabaseService _db;
        private int _employeeId;
        private string _employeeName;

        public GeneralDeptWindow(DatabaseService db, int employeeId, string employeeName)
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

                string status = null;
                if (filterStatusCombo.SelectedItem is ComboBoxItem statusItem && statusItem.Content.ToString() != "Все")
                    status = statusItem.Content.ToString();

                string search = string.IsNullOrWhiteSpace(searchBox.Text) ? null : searchBox.Text;

                var requests = await _db.GetAllRequests(type, deptId > 0 ? deptId : (int?)null, status, search);
                requestsGrid.ItemsSource = requests;
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

        private async void ViewRequest_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var request = button?.Tag as RequestViewModel;

            if (request != null)
            {
                // Проверка черного списка
                var (inBlacklist, reason) = await _db.CheckBlacklist(request.passport_series, request.passport_number);

                if (inBlacklist)
                {
                    await _db.UpdateRequestStatus(request.request_id, request.request_type, 3, reason);
                    MessageBox.Show($"ВНИМАНИЕ!\n\nПосетитель находится в черном списке!\nПричина: {reason}\n\nЗаявка автоматически отклонена.", "Черный список", MessageBoxButton.OK, MessageBoxImage.Warning);
                    await LoadRequests();
                    return;
                }

                // Открытие окна проверки заявки
                var dialog = new RequestReviewDialog(_db, request, _employeeId);
                dialog.Owner = this;
                if (dialog.ShowDialog() == true)
                {
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