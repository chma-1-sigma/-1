using Microsoft.Win32;
using PassOfficeApp.Models;
using PassOfficeApp.Services;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PassOfficeApp.Pages
{
    public partial class PersonalRequestPage : Page
    {
        private PostgresService _postgresService;
        private int _userId;
        private string _selectedPhotoPath;
        private string _selectedScanPath;

        public PersonalRequestPage(PostgresService postgresService, int userId)
        {
            InitializeComponent();
            _postgresService = postgresService;
            _userId = userId;

            this.Loaded += async (s, e) => await LoadInitialData();
        }

        private async Task LoadInitialData()
        {
            try
            {
                // Загрузка подразделений
                var departments = await _postgresService.GetDepartments();
                departmentCombo.ItemsSource = departments;

                // Загрузка целей посещения
                var purposes = await _postgresService.GetVisitPurposes();
                purposeCombo.ItemsSource = purposes;

                // Установка минимальных дат
                startDatePicker.DisplayDateStart = DateTime.Now.AddDays(1);
                startDatePicker.DisplayDateEnd = DateTime.Now.AddDays(15);
                startDatePicker.SelectedDate = DateTime.Now.AddDays(1);

                endDatePicker.DisplayDateStart = DateTime.Now.AddDays(1);
                endDatePicker.DisplayDateEnd = DateTime.Now.AddDays(16);
                endDatePicker.SelectedDate = DateTime.Now.AddDays(2);

                startDatePicker.SelectedDateChanged += (s, e) =>
                {
                    if (startDatePicker.SelectedDate.HasValue)
                    {
                        endDatePicker.DisplayDateStart = startDatePicker.SelectedDate.Value;
                        endDatePicker.DisplayDateEnd = startDatePicker.SelectedDate.Value.AddDays(15);

                        if (!endDatePicker.SelectedDate.HasValue ||
                            endDatePicker.SelectedDate.Value < startDatePicker.SelectedDate.Value)
                        {
                            endDatePicker.SelectedDate = startDatePicker.SelectedDate.Value.AddDays(1);
                        }
                    }
                };

                // Установка максимальной даты рождения
                birthDatePicker.DisplayDateEnd = DateTime.Now.AddYears(-16);
                birthDatePicker.SelectedDate = DateTime.Now.AddYears(-30);
            }
            catch (Exception ex)
            {
                errorText.Text = $"Ошибка загрузки данных: {ex.Message}";
            }
        }

        private async void DepartmentCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (departmentCombo.SelectedItem != null)
            {
                int deptId = ((Department)departmentCombo.SelectedItem).id;
                var employees = await _postgresService.GetEmployeesByDepartment(deptId);
                employeeCombo.ItemsSource = employees;
                employeeCombo.IsEnabled = employees.Any();
            }
        }

        private void SelectPhoto_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "JPEG файлы (*.jpg;*.jpeg)|*.jpg;*.jpeg",
                Title = "Выберите фотографию (3×4)"
            };

            if (dialog.ShowDialog() == true)
            {
                var fileInfo = new FileInfo(dialog.FileName);
                if (fileInfo.Length > 4 * 1024 * 1024)
                {
                    MessageBox.Show("Файл слишком большой. Максимальный размер 4 Мб.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _selectedPhotoPath = dialog.FileName;
                photoPathBox.Text = fileInfo.Name;
            }
        }

        private void SelectScan_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "PDF файлы (*.pdf)|*.pdf",
                Title = "Выберите скан паспорта"
            };

            if (dialog.ShowDialog() == true)
            {
                var fileInfo = new FileInfo(dialog.FileName);
                if (fileInfo.Length > 10 * 1024 * 1024)
                {
                    MessageBox.Show("Файл слишком большой. Максимальный размер 10 Мб.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _selectedScanPath = dialog.FileName;
                scanPathBox.Text = fileInfo.Name;
            }
        }

        private bool ValidateForm()
        {
            // Проверка дат
            if (!startDatePicker.SelectedDate.HasValue)
            {
                errorText.Text = "Выберите дату начала";
                return false;
            }

            if (!endDatePicker.SelectedDate.HasValue)
            {
                errorText.Text = "Выберите дату окончания";
                return false;
            }

            if (endDatePicker.SelectedDate.Value < startDatePicker.SelectedDate.Value)
            {
                errorText.Text = "Дата окончания не может быть раньше даты начала";
                return false;
            }

            // Проверка цели посещения
            if (purposeCombo.SelectedItem == null)
            {
                errorText.Text = "Выберите цель посещения";
                return false;
            }

            // Проверка принимающей стороны
            if (departmentCombo.SelectedItem == null)
            {
                errorText.Text = "Выберите подразделение";
                return false;
            }

            if (employeeCombo.SelectedItem == null)
            {
                errorText.Text = "Выберите сотрудника";
                return false;
            }

            // Проверка информации о посетителе
            if (string.IsNullOrWhiteSpace(lastNameBox.Text))
            {
                errorText.Text = "Введите фамилию";
                return false;
            }

            if (string.IsNullOrWhiteSpace(firstNameBox.Text))
            {
                errorText.Text = "Введите имя";
                return false;
            }

            if (string.IsNullOrWhiteSpace(emailBox.Text) || !IsValidEmail(emailBox.Text))
            {
                errorText.Text = "Введите корректный email";
                return false;
            }

            if (string.IsNullOrWhiteSpace(commentBox.Text))
            {
                errorText.Text = "Введите примечание";
                return false;
            }

            if (!birthDatePicker.SelectedDate.HasValue)
            {
                errorText.Text = "Выберите дату рождения";
                return false;
            }

            if (birthDatePicker.SelectedDate.Value > DateTime.Now.AddYears(-16))
            {
                errorText.Text = "Посетитель должен быть не младше 16 лет";
                return false;
            }

            // Проверка паспортных данных
            if (passportSeriesBox.Text.Length != 4 || !passportSeriesBox.Text.All(char.IsDigit))
            {
                errorText.Text = "Серия паспорта должна содержать 4 цифры";
                return false;
            }

            if (passportNumberBox.Text.Length != 6 || !passportNumberBox.Text.All(char.IsDigit))
            {
                errorText.Text = "Номер паспорта должен содержать 6 цифр";
                return false;
            }

            // Проверка скана паспорта
            if (string.IsNullOrEmpty(_selectedScanPath))
            {
                errorText.Text = "Прикрепите скан паспорта";
                return false;
            }

            errorText.Text = "";
            return true;
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            var request = new PersonalRequest
            {
                user_id = _userId,
                status_id = await _postgresService.GetPendingStatusId(),
                purpose_id = ((VisitPurpose)purposeCombo.SelectedItem).id,
                department_id = ((Department)departmentCombo.SelectedItem).id,
                employee_id = ((Employee)employeeCombo.SelectedItem).id,
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
                int requestId = await _postgresService.CreatePersonalRequestSQL(request);
                MessageBox.Show($"Заявка №{requestId} успешно создана!\nСтатус: проверка", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                ClearButton_Click(null, null);
            }
            catch (Exception ex)
            {
                errorText.Text = $"Ошибка при создании заявки: {ex.Message}";
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
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

            startDatePicker.SelectedDate = DateTime.Now.AddDays(1);
            endDatePicker.SelectedDate = DateTime.Now.AddDays(2);
            birthDatePicker.SelectedDate = DateTime.Now.AddYears(-30);
            departmentCombo.SelectedIndex = -1;
            employeeCombo.ItemsSource = null;
            employeeCombo.IsEnabled = false;
            purposeCombo.SelectedIndex = -1;
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
    }
}