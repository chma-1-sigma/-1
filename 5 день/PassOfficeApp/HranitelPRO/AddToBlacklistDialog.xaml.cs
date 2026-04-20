using System;
using System.Windows;

namespace HranitelPRO
{
    public partial class AddToBlacklistDialog : Window
    {
        private DatabaseService _db;
        private VisitorDetail _visitor;
        private int _employeeId;

        public AddToBlacklistDialog(DatabaseService db, VisitorDetail visitor, int employeeId)
        {
            InitializeComponent();
            _db = db;
            _visitor = visitor;
            _employeeId = employeeId;

            txtFullName.Text = visitor.FullName;
            txtPassport.Text = $"{visitor.PassportSeries} {visitor.PassportNumber}";
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtReason.Text))
            {
                MessageBox.Show("Укажите причину добавления", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (txtReason.Text.Length > 5000)
            {
                MessageBox.Show("Причина не должна превышать 5000 символов", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int result = await _db.AddToBlacklist(
                    _visitor.PassportSeries,
                    _visitor.PassportNumber,
                    _visitor.LastName,
                    _visitor.FirstName,
                    _visitor.MiddleName,
                    txtReason.Text,
                    _employeeId
                );

                if (result > 0)
                {
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