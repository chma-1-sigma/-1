using System;
using System.Linq;
using System.Windows;
using HranitelPro.Admin.Models;
using HranitelPro.Admin.Services;

namespace HranitelPro.Admin
{
    public partial class AddToBlacklistDialog : Window
    {
        private DatabaseService _db;
        private int _employeeId;
        private string _passportSeries;
        private string _passportNumber;
        private string _lastName;
        private string _firstName;
        private string _middleName;

        public AddToBlacklistDialog(DatabaseService db, int employeeId, RequestViewModel request)
        {
            InitializeComponent();
            _db = db;
            _employeeId = employeeId;

            _lastName = request.last_name;
            _firstName = request.first_name;
            _middleName = request.middle_name;
            _passportSeries = request.passport_series;
            _passportNumber = request.passport_number;

            txtLastName.Text = _lastName;
            txtFirstName.Text = _firstName;
            txtMiddleName.Text = _middleName ?? "";
            txtPassport.Text = $"{_passportSeries} {_passportNumber}";
        }

        public AddToBlacklistDialog(DatabaseService db, int employeeId, GroupMember member)
        {
            InitializeComponent();
            _db = db;
            _employeeId = employeeId;

            _lastName = member.last_name;
            _firstName = member.first_name;
            _middleName = member.middle_name;
            _passportSeries = member.passport_series;
            _passportNumber = member.passport_number;

            txtLastName.Text = _lastName;
            txtFirstName.Text = _firstName;
            txtMiddleName.Text = _middleName ?? "";
            txtPassport.Text = $"{_passportSeries} {_passportNumber}";
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtReason.Text))
            {
                MessageBox.Show("Укажите причину добавления", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int result = await _db.AddToBlacklist(
                    _passportSeries,
                    _passportNumber,
                    _lastName,
                    _firstName,
                    _middleName,
                    txtReason.Text,
                    _employeeId
                );

                if (result > 0)
                {
                    MessageBox.Show("Посетитель добавлен в черный список", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Ошибка при добавлении", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}