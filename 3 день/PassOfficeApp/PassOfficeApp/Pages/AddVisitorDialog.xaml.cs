using System;
using System.Windows;

namespace PassOfficeApp.Pages
{
    public partial class AddVisitorDialog : Window
    {
        public string FullName { get; private set; }
        public string LastName { get; private set; }
        public string FirstName { get; private set; }
        public string MiddleName { get; private set; }
        public string Phone { get; private set; }
        public string Email { get; private set; }
        public DateTime BirthDate { get; private set; }
        public string PassportSeries { get; private set; }
        public string PassportNumber { get; private set; }

        public AddVisitorDialog() { InitializeComponent(); BirthDate = DateTime.Now.AddYears(-30); }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLastName.Text) || string.IsNullOrWhiteSpace(txtFirstName.Text))
            { MessageBox.Show("Заполните ФИО"); return; }

            LastName = txtLastName.Text; FirstName = txtFirstName.Text; MiddleName = txtMiddleName.Text;
            FullName = $"{LastName} {FirstName} {MiddleName}".Trim();
            Phone = txtPhone.Text; Email = txtEmail.Text;
            BirthDate = dpBirthDate.SelectedDate ?? DateTime.Now.AddYears(-30);
            PassportSeries = txtPassportSeries.Text; PassportNumber = txtPassportNumber.Text;
            DialogResult = true; Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) { DialogResult = false; Close(); }
    }
}