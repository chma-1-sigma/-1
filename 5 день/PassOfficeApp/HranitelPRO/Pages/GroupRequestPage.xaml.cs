using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HranitelPRO.Pages
{
    public partial class GroupRequestPage : Page
    {
        private DatabaseService _db;
        private int _userId;
        private string _selectedScanPath;
        private ObservableCollection<VisitorItem> _visitors;
        private int _nextNumber = 1;

        public GroupRequestPage(DatabaseService db, int userId)
        {
            InitializeComponent();
            _db = db;
            _userId = userId;
            _visitors = new ObservableCollection<VisitorItem>();
            visitorsGrid.ItemsSource = _visitors;
            this.Loaded += async (s, e) => await LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                var departments = await _db.GetDepartments();
                departmentCombo.ItemsSource = departments;
                departmentCombo.DisplayMemberPath = "name";
                departmentCombo.SelectedValuePath = "id";

                var purposes = await _db.GetVisitPurposes();
                purposeCombo.ItemsSource = purposes;
                purposeCombo.DisplayMemberPath = "purpose_name";
                purposeCombo.SelectedValuePath = "id";

                startDatePicker.SelectedDate = DateTime.Now.AddDays(1);
                endDatePicker.SelectedDate = DateTime.Now.AddDays(2);
                birthDatePicker.SelectedDate = DateTime.Now.AddYears(-30);
            }
            catch (Exception ex)
            {
                errorText.Text = $"Ошибка: {ex.Message}";
            }
        }

        private async void DepartmentCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (departmentCombo.SelectedItem != null)
            {
                int deptId = (int)departmentCombo.SelectedValue;
                var employees = await _db.GetEmployeesByDepartment(deptId);
                employeeCombo.ItemsSource = employees;
                employeeCombo.DisplayMemberPath = "full_name";
                employeeCombo.SelectedValuePath = "id";
                employeeCombo.IsEnabled = employees.Any();
            }
        }

        private void AddVisitorButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Window
            {
                Title = "Добавление посетителя",
                Width = 450,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = System.Windows.Media.Brushes.White
            };

            var stackPanel = new StackPanel { Margin = new Thickness(20) };

            stackPanel.Children.Add(new TextBlock { Text = "Фамилия:*", Margin = new Thickness(0, 0, 0, 5) });
            var txtLastName = new TextBox { Height = 35, Margin = new Thickness(0, 0, 0, 10) };
            stackPanel.Children.Add(txtLastName);

            stackPanel.Children.Add(new TextBlock { Text = "Имя:*", Margin = new Thickness(0, 0, 0, 5) });
            var txtFirstName = new TextBox { Height = 35, Margin = new Thickness(0, 0, 0, 10) };
            stackPanel.Children.Add(txtFirstName);

            stackPanel.Children.Add(new TextBlock { Text = "Отчество:", Margin = new Thickness(0, 0, 0, 5) });
            var txtMiddleName = new TextBox { Height = 35, Margin = new Thickness(0, 0, 0, 10) };
            stackPanel.Children.Add(txtMiddleName);

            stackPanel.Children.Add(new TextBlock { Text = "Телефон:", Margin = new Thickness(0, 0, 0, 5) });
            var txtPhone = new TextBox { Height = 35, Text = "+7 ", Margin = new Thickness(0, 0, 0, 10) };
            stackPanel.Children.Add(txtPhone);

            stackPanel.Children.Add(new TextBlock { Text = "Email:*", Margin = new Thickness(0, 0, 0, 5) });
            var txtEmail = new TextBox { Height = 35, Margin = new Thickness(0, 0, 0, 10) };
            stackPanel.Children.Add(txtEmail);

            stackPanel.Children.Add(new TextBlock { Text = "Дата рождения:*", Margin = new Thickness(0, 0, 0, 5) });
            var dpBirthDate = new DatePicker { Height = 35, Margin = new Thickness(0, 0, 0, 10), SelectedDate = DateTime.Now.AddYears(-30) };
            stackPanel.Children.Add(dpBirthDate);

            stackPanel.Children.Add(new TextBlock { Text = "Серия паспорта:* (4 цифры)", Margin = new Thickness(0, 0, 0, 5) });
            var txtPassportSeries = new TextBox { Height = 35, MaxLength = 4, Margin = new Thickness(0, 0, 0, 10) };
            stackPanel.Children.Add(txtPassportSeries);

            stackPanel.Children.Add(new TextBlock { Text = "Номер паспорта:* (6 цифр)", Margin = new Thickness(0, 0, 0, 5) });
            var txtPassportNumber = new TextBox { Height = 35, MaxLength = 6, Margin = new Thickness(0, 0, 0, 20) };
            stackPanel.Children.Add(txtPassportNumber);

            var addButton = new Button { Content = "Добавить", Height = 40, Background = System.Windows.Media.Brushes.Green, Foreground = System.Windows.Media.Brushes.White, Margin = new Thickness(0, 0, 0, 10) };
            stackPanel.Children.Add(addButton);

            var cancelButton = new Button { Content = "Отмена", Height = 40, Background = System.Windows.Media.Brushes.Gray, Foreground = System.Windows.Media.Brushes.White };
            stackPanel.Children.Add(cancelButton);

            dialog.Content = stackPanel;

            addButton.Click += (s, args) =>
            {
                if (string.IsNullOrWhiteSpace(txtLastName.Text) || string.IsNullOrWhiteSpace(txtFirstName.Text))
                {
                    MessageBox.Show("Заполните фамилию и имя");
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtEmail.Text))
                {
                    MessageBox.Show("Введите email");
                    return;
                }
                if (txtPassportSeries.Text.Length != 4 || !txtPassportSeries.Text.All(char.IsDigit))
                {
                    MessageBox.Show("Серия паспорта - 4 цифры");
                    return;
                }
                if (txtPassportNumber.Text.Length != 6 || !txtPassportNumber.Text.All(char.IsDigit))
                {
                    MessageBox.Show("Номер паспорта - 6 цифр");
                    return;
                }

                _visitors.Add(new VisitorItem
                {
                    Number = _nextNumber++,
                    FullName = $"{txtLastName.Text} {txtFirstName.Text} {txtMiddleName.Text}".Trim(),
                    LastName = txtLastName.Text,
                    FirstName = txtFirstName.Text,
                    MiddleName = txtMiddleName.Text,
                    Phone = txtPhone.Text,
                    Email = txtEmail.Text,
                    BirthDate = dpBirthDate.SelectedDate ?? DateTime.Now.AddYears(-30),
                    PassportSeries = txtPassportSeries.Text,
                    PassportNumber = txtPassportNumber.Text
                });

                visitorCountText.Text = $"Посетителей: {_visitors.Count} (нужно 5+)";
                visitorCountText.Foreground = _visitors.Count >= 5 ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;
                dialog.Close();
            };

            cancelButton.Click += (s, args) => dialog.Close();

            dialog.Owner = Window.GetWindow(this);
            dialog.ShowDialog();
        }

        private void RemoveVisitor_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var visitor = button?.Tag as VisitorItem;
            if (visitor != null)
            {
                _visitors.Remove(visitor);
                visitorCountText.Text = $"Посетителей: {_visitors.Count} (нужно 5+)";
                visitorCountText.Foreground = _visitors.Count >= 5 ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;
            }
        }

        private void SelectScan_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = "PDF|*.pdf" };
            if (dialog.ShowDialog() == true)
            {
                _selectedScanPath = dialog.FileName;
                scanPathBox.Text = Path.GetFileName(dialog.FileName);
            }
        }

        private bool ValidateForm()
        {
            if (_visitors.Count < 5) { errorText.Text = "Добавьте минимум 5 посетителей"; return false; }
            if (string.IsNullOrEmpty(_selectedScanPath)) { errorText.Text = "Прикрепите скан паспорта руководителя"; return false; }
            if (string.IsNullOrWhiteSpace(lastNameBox.Text)) { errorText.Text = "Введите фамилию руководителя"; return false; }
            if (string.IsNullOrWhiteSpace(firstNameBox.Text)) { errorText.Text = "Введите имя руководителя"; return false; }
            if (string.IsNullOrWhiteSpace(emailBox.Text)) { errorText.Text = "Введите email руководителя"; return false; }
            if (passportSeriesBox.Text.Length != 4 || !passportSeriesBox.Text.All(char.IsDigit)) { errorText.Text = "Серия паспорта - 4 цифры"; return false; }
            if (passportNumberBox.Text.Length != 6 || !passportNumberBox.Text.All(char.IsDigit)) { errorText.Text = "Номер паспорта - 6 цифр"; return false; }
            if (purposeCombo.SelectedItem == null) { errorText.Text = "Выберите цель посещения"; return false; }
            if (departmentCombo.SelectedItem == null) { errorText.Text = "Выберите подразделение"; return false; }
            if (employeeCombo.SelectedItem == null) { errorText.Text = "Выберите сотрудника"; return false; }

            errorText.Text = "";
            return true;
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm()) return;

            var request = new GroupRequest
            {
                user_id = _userId,
                status_id = await _db.GetPendingStatusId(),
                purpose_id = (int)purposeCombo.SelectedValue,
                department_id = (int)departmentCombo.SelectedValue,
                employee_id = (int)employeeCombo.SelectedValue,
                start_date = startDatePicker.SelectedDate.Value,
                end_date = endDatePicker.SelectedDate.Value,
                visit_comment = commentBox.Text,
                group_leader_last_name = lastNameBox.Text,
                group_leader_first_name = firstNameBox.Text,
                group_leader_middle_name = string.IsNullOrWhiteSpace(middleNameBox.Text) ? null : middleNameBox.Text,
                group_leader_phone = string.IsNullOrWhiteSpace(phoneBox.Text) ? null : phoneBox.Text,
                group_leader_email = emailBox.Text,
                group_leader_organization = string.IsNullOrWhiteSpace(organizationBox.Text) ? null : organizationBox.Text,
                group_leader_birth_date = birthDatePicker.SelectedDate.Value,
                group_leader_passport_series = passportSeriesBox.Text,
                group_leader_passport_number = passportNumberBox.Text,
                passport_scan_path = _selectedScanPath,
                created_at = DateTime.Now
            };

            try
            {
                int id = await _db.CreateGroupRequest(request, _visitors.ToList());
                MessageBox.Show($"Групповая заявка №{id} создана!\nПосетителей: {_visitors.Count}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
            }
            catch (Exception ex)
            {
                errorText.Text = $"Ошибка: {ex.Message}";
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            _visitors.Clear();
            _nextNumber = 1;
            lastNameBox.Text = "";
            firstNameBox.Text = "";
            middleNameBox.Text = "";
            phoneBox.Text = "+7 ";
            emailBox.Text = "";
            organizationBox.Text = "";
            commentBox.Text = "";
            passportSeriesBox.Text = "";
            passportNumberBox.Text = "";
            _selectedScanPath = null;
            scanPathBox.Text = "";
            departmentCombo.SelectedIndex = -1;
            purposeCombo.SelectedIndex = -1;
            employeeCombo.ItemsSource = null;
            employeeCombo.IsEnabled = false;
            visitorCountText.Text = "Посетителей: 0 (нужно 5+)";
            visitorCountText.Foreground = System.Windows.Media.Brushes.Red;
            startDatePicker.SelectedDate = DateTime.Now.AddDays(1);
            endDatePicker.SelectedDate = DateTime.Now.AddDays(2);
            birthDatePicker.SelectedDate = DateTime.Now.AddYears(-30);
        }
    }
}