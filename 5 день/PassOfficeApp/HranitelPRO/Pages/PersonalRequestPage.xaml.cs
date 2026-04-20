using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace HranitelPRO.Pages
{
    public partial class PersonalRequestPage : Page
    {
        private DatabaseService _db;
        private int _userId;
        private string _selectedPhotoPath;
        private string _selectedScanPath;

        public PersonalRequestPage(DatabaseService db, int userId)
        {
            InitializeComponent();
            _db = db;
            _userId = userId;
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

        private void SelectPhoto_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = "JPEG|*.jpg;*.jpeg" };
            if (dialog.ShowDialog() == true)
            {
                _selectedPhotoPath = dialog.FileName;
                photoPathBox.Text = Path.GetFileName(dialog.FileName);
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
            if (!startDatePicker.SelectedDate.HasValue) { errorText.Text = "Выберите дату начала"; return false; }
            if (!endDatePicker.SelectedDate.HasValue) { errorText.Text = "Выберите дату окончания"; return false; }
            if (purposeCombo.SelectedItem == null) { errorText.Text = "Выберите цель посещения"; return false; }
            if (departmentCombo.SelectedItem == null) { errorText.Text = "Выберите подразделение"; return false; }
            if (employeeCombo.SelectedItem == null) { errorText.Text = "Выберите сотрудника"; return false; }
            if (string.IsNullOrWhiteSpace(lastNameBox.Text)) { errorText.Text = "Введите фамилию"; return false; }
            if (string.IsNullOrWhiteSpace(firstNameBox.Text)) { errorText.Text = "Введите имя"; return false; }
            if (string.IsNullOrWhiteSpace(emailBox.Text) || !IsValidEmail(emailBox.Text)) { errorText.Text = "Введите корректный email"; return false; }
            if (string.IsNullOrWhiteSpace(commentBox.Text)) { errorText.Text = "Введите примечание"; return false; }
            if (!birthDatePicker.SelectedDate.HasValue) { errorText.Text = "Выберите дату рождения"; return false; }
            if (passportSeriesBox.Text.Length != 4 || !passportSeriesBox.Text.All(char.IsDigit)) { errorText.Text = "Серия паспорта - 4 цифры"; return false; }
            if (passportNumberBox.Text.Length != 6 || !passportNumberBox.Text.All(char.IsDigit)) { errorText.Text = "Номер паспорта - 6 цифр"; return false; }
            if (string.IsNullOrEmpty(_selectedScanPath)) { errorText.Text = "Прикрепите скан паспорта"; return false; }

            errorText.Text = "";
            return true;
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm()) return;

            var request = new PersonalRequest
            {
                user_id = _userId,
                status_id = await _db.GetPendingStatusId(),
                purpose_id = (int)purposeCombo.SelectedValue,
                department_id = (int)departmentCombo.SelectedValue,
                employee_id = (int)employeeCombo.SelectedValue,
                start_date = startDatePicker.SelectedDate.Value,
                end_date = endDatePicker.SelectedDate.Value,
                visit_comment = commentBox.Text,
                last_name = lastNameBox.Text,
                first_name = firstNameBox.Text,
                middle_name = string.IsNullOrWhiteSpace(middleNameBox.Text) ? null : middleNameBox.Text,
                phone = string.IsNullOrWhiteSpace(phoneBox.Text) ? null : phoneBox.Text,
                email_visitor = emailBox.Text,
                organization = string.IsNullOrWhiteSpace(organizationBox.Text) ? null : organizationBox.Text,
                birth_date = birthDatePicker.SelectedDate.Value,
                passport_series = passportSeriesBox.Text,
                passport_number = passportNumberBox.Text,
                photo_path = _selectedPhotoPath,
                passport_scan_path = _selectedScanPath,
                created_at = DateTime.Now
            };

            try
            {
                int id = await _db.CreatePersonalRequest(request);
                MessageBox.Show($"Заявка №{id} успешно создана!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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
            lastNameBox.Text = "";
            firstNameBox.Text = "";
            middleNameBox.Text = "";
            phoneBox.Text = "+7 ";
            emailBox.Text = "";
            organizationBox.Text = "";
            commentBox.Text = "";
            passportSeriesBox.Text = "";
            passportNumberBox.Text = "";
            _selectedPhotoPath = null;
            _selectedScanPath = null;
            photoPathBox.Text = "";
            scanPathBox.Text = "";
            departmentCombo.SelectedIndex = -1;
            purposeCombo.SelectedIndex = -1;
            employeeCombo.ItemsSource = null;
            employeeCombo.IsEnabled = false;
            startDatePicker.SelectedDate = DateTime.Now.AddDays(1);
            endDatePicker.SelectedDate = DateTime.Now.AddDays(2);
            birthDatePicker.SelectedDate = DateTime.Now.AddYears(-30);
        }

        private bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }
    }
}