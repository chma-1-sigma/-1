using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HranitelPRO
{
    public partial class RequestDetailDialog : Window
    {
        private DatabaseService _db;
        private RequestViewModel _request;
        private int _employeeId;
        private int _departmentId;
        private ObservableCollection<VisitorDetail> _visitors;

        public RequestDetailDialog(DatabaseService db, RequestViewModel request, int employeeId, int departmentId)
        {
            InitializeComponent();
            _db = db;
            _request = request;
            _employeeId = employeeId;
            _departmentId = departmentId;

            LoadData();
            LoadMembers();
        }

        private void LoadData()
        {
            txtRequestId.Text = _request.request_id.ToString();
            txtRequestType.Text = _request.request_type;
            txtVisitDate.Text = _request.start_date.ToString("dd.MM.yyyy");
            txtStatus.Text = _request.status;
            txtPurpose.Text = _request.purpose ?? "—";
        }

        private void LoadMembers()
        {
            _visitors = new ObservableCollection<VisitorDetail>();

            if (_request.request_type == "Личная")
            {
                _visitors.Add(new VisitorDetail
                {
                    Id = 1,
                    FullName = _request.full_name,
                    LastName = _request.last_name,
                    FirstName = _request.first_name,
                    MiddleName = _request.middle_name,
                    PassportSeries = _request.passport_series,
                    PassportNumber = _request.passport_number,
                    ArrivalTime = _request.visit_start_time,
                    DepartureTime = _request.visit_end_time,
                    IsGroupLeader = true
                });
            }
            else
            {
                _visitors.Add(new VisitorDetail
                {
                    Id = 1,
                    FullName = _request.group_leader_name ?? _request.full_name,
                    LastName = _request.last_name,
                    FirstName = _request.first_name,
                    MiddleName = _request.middle_name,
                    PassportSeries = _request.passport_series,
                    PassportNumber = _request.passport_number,
                    ArrivalTime = _request.visit_start_time,
                    DepartureTime = _request.visit_end_time,
                    IsGroupLeader = true
                });
            }

            visitorsListBox.ItemsSource = _visitors;
        }

        private void Visitor_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var textBlock = sender as TextBlock;
            var visitor = textBlock?.Tag as VisitorDetail;

            if (visitor != null)
            {
                var contextMenu = new ContextMenu();
                var menuItem = new MenuItem { Header = "Черный список..." };
                menuItem.Click += (s, args) => AddToBlacklist(visitor);
                contextMenu.Items.Add(menuItem);
                contextMenu.PlacementTarget = textBlock;
                contextMenu.IsOpen = true;
            }
        }

        private async void AddToBlacklist(VisitorDetail visitor)
        {
            var dialog = new AddToBlacklistDialog(_db, visitor, _employeeId);
            dialog.Owner = this;
            if (dialog.ShowDialog() == true)
            {
                MessageBox.Show($"Посетитель {visitor.FullName} добавлен в черный список", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void ConfirmArrival_Click(object sender, RoutedEventArgs e)
        {
            if (!_request.visit_start_time.HasValue)
            {
                MessageBox.Show("Доступ еще не разрешен сотрудником охраны", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Подтвердить прибытие посетителей?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show("Прибытие подтверждено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
        }

        private async void ConfirmDeparture_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Подтвердить убытие посетителей?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await _db.SetDepartureTime(_request.request_id, _request.request_type);
                MessageBox.Show("Убытие подтверждено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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