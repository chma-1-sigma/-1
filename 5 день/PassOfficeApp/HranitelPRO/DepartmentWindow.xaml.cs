using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HranitelPRO
{
    public partial class DepartmentWindow : Window
    {
        private DatabaseService _db;
        private int _employeeId;
        private int _departmentId;
        private string _employeeName;
        private string _departmentName;

        public DepartmentWindow(DatabaseService db, int employeeId, string employeeName, int departmentId, string departmentName)
        {
            InitializeComponent();
            _db = db;
            _employeeId = employeeId;
            _employeeName = employeeName;
            _departmentId = departmentId;
            _departmentName = departmentName;

            userNameText.Text = employeeName;
            departmentNameText.Text = departmentName;

            this.Loaded += async (s, e) => await LoadRequests();
        }

        private async Task LoadRequests()
        {
            try
            {
                string type = null;
                if (filterTypeCombo.SelectedItem is ComboBoxItem typeItem && typeItem.Content.ToString() != "Все")
                    type = typeItem.Content.ToString();

                DateTime? date = filterDatePicker.SelectedDate;

                var allRequests = await _db.GetAllRequests(type, _departmentId, "одобрена", null);

                if (date.HasValue)
                {
                    allRequests = allRequests.Where(r => r.start_date.Date == date.Value.Date).ToList();
                }

                foreach (var req in allRequests)
                {
                    req.PassportFull = $"{req.passport_series} {req.passport_number}";
                }

                requestsGrid.ItemsSource = allRequests;
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
                var dialog = new RequestDetailDialog(_db, request, _employeeId, _departmentId);
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