using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using HranitelPro.Admin.Models;
using HranitelPro.Admin.Services;

namespace HranitelPro.Admin
{
    public partial class DepartmentWindow : Window
    {
        private DatabaseService _db;
        private int _employeeId;
        private string _employeeName;
        private int _departmentId;

        public DepartmentWindow(DatabaseService db, int employeeId, string employeeName)
        {
            InitializeComponent();
            _db = db;
            _employeeId = employeeId;
            _employeeName = employeeName;
            userNameText.Text = employeeName;

            // Получаем ID подразделения сотрудника
            _departmentId = GetDepartmentId().Result;

            this.Loaded += async (s, e) => await LoadRequests();
        }

        private async Task<int> GetDepartmentId()
        {
            var departments = await _db.GetDepartments();
            return departments.Count > 0 ? departments[0].id : 1;
        }

        private async Task LoadRequests()
        {
            try
            {
                string type = null;
                if (filterTypeCombo.SelectedItem is ComboBoxItem typeItem && typeItem.Content.ToString() != "Все")
                    type = typeItem.Content.ToString();

                DateTime? date = filterDatePicker.SelectedDate;

                var requests = await _db.GetApprovedRequestsForDepartment(_departmentId, date);

                // Фильтрация по типу
                if (type != null)
                {
                    requests = requests.FindAll(r => r.request_type == type);
                }

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

        private async void SetArrival_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var request = button?.Tag as RequestViewModel;

            if (request != null)
            {
                if (!request.visit_start_time.HasValue)
                {
                    MessageBox.Show("Сотрудник охраны еще не разрешил доступ", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (request.visit_arrival_time.HasValue)
                {
                    MessageBox.Show("Прибытие уже зафиксировано", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"Зафиксировать прибытие для {request.full_name}?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    await _db.SetArrivalTime(request.request_id, request.request_type);

                    MessageBox.Show($"Прибытие зафиксировано!\nВремя: {DateTime.Now:HH:mm:ss}", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    await LoadRequests();
                }
            }
        }

        private async void AddToBlacklist_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var request = button?.Tag as RequestViewModel;

            if (request != null)
            {
                // Для групповой заявки показываем список участников
                if (request.request_type == "Групповая" && request.GroupMembers.Count > 0)
                {
                    // Преобразуем List в ObservableCollection для диалога
                    var membersObservable = new System.Collections.ObjectModel.ObservableCollection<GroupMember>(request.GroupMembers);
                    var memberDialog = new SelectMemberDialog(membersObservable);
                    memberDialog.Owner = this;
                    if (memberDialog.ShowDialog() == true && memberDialog.SelectedMember != null)
                    {
                        var addDialog = new AddToBlacklistDialog(_db, _employeeId, memberDialog.SelectedMember);
                        addDialog.Owner = this;
                        if (addDialog.ShowDialog() == true)
                        {
                            await LoadRequests();
                        }
                    }
                }
                else
                {
                    var addDialog = new AddToBlacklistDialog(_db, _employeeId, request);
                    addDialog.Owner = this;
                    if (addDialog.ShowDialog() == true)
                    {
                        await LoadRequests();
                    }
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