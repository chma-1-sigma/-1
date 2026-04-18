using PassOfficeApp.Models;
using PassOfficeApp.Services;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace PassOfficeApp
{
    public partial class RequestReviewDialog : Window
    {
        private DatabaseService _db;
        private RequestViewModel _request;
        private int _employeeId;
        private bool _isLocked;

        public RequestReviewDialog(DatabaseService db, RequestViewModel request, int employeeId)
        {
            InitializeComponent();
            _db = db;
            _request = request;
            _employeeId = employeeId;
            LoadData();
        }

        private void LoadData()
        {
            txtId.Text = _request.request_id.ToString();
            txtType.Text = _request.request_type;
            txtStatus.Text = _request.status;
            txtDate.Text = _request.created_at.ToString("dd.MM.yyyy HH:mm");
            txtFullName.Text = _request.full_name;
            txtEmail.Text = _request.visitor_email;
            txtPhone.Text = _request.phone;
            txtPassport.Text = $"{_request.passport_series} {_request.passport_number}";
            txtOrg.Text = _request.organization ?? "—";
            txtBirthDate.Text = _request.birth_date?.ToString("dd.MM.yyyy") ?? "—";
            dpVisitDate.SelectedDate = _request.start_date;

            if (_request.status == "не одобрена" && _request.rejection_reason?.Contains("187-ФЗ") == true)
            {
                _isLocked = true;
                btnApprove.IsEnabled = false;
                btnReject.IsEnabled = false;
                txtRejectionReason.Text = _request.rejection_reason;
                txtRejectionReason.IsEnabled = true;
                MessageBox.Show("Посетитель в черном списке! Заявка отклонена автоматически.", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void Approve_Click(object sender, RoutedEventArgs e)
        {
            if (_isLocked)
            {
                MessageBox.Show("Невозможно одобрить - посетитель в черном списке!");
                return;
            }

            if (!dpVisitDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Укажите дату посещения");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtVisitTime.Text))
            {
                MessageBox.Show("Укажите время посещения");
                return;
            }

            DateTime visitTime;
            if (!DateTime.TryParse($"{dpVisitDate.SelectedDate.Value:yyyy-MM-dd} {txtVisitTime.Text}", out visitTime))
            {
                MessageBox.Show("Неверный формат времени");
                return;
            }

            await _db.UpdateRequestStatus(_request.request_id, _request.request_type, 2, null, visitTime);
            MessageBox.Show($"Заявка №{_request.request_id} одобрена!\nДата: {visitTime:dd.MM.yyyy HH:mm}", "Успех");
            DialogResult = true;
            Close();
        }

        private async void Reject_Click(object sender, RoutedEventArgs e)
        {
            if (_isLocked)
            {
                MessageBox.Show("Невозможно отклонить - уже отклонена системой");
                return;
            }

            var dialog = new RejectionReasonDialog();
            if (dialog.ShowDialog() == true)
            {
                await _db.UpdateRequestStatus(_request.request_id, _request.request_type, 3, dialog.RejectionReason);
                MessageBox.Show($"Заявка №{_request.request_id} отклонена!", "Отклонено");
                DialogResult = true;
                Close();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}