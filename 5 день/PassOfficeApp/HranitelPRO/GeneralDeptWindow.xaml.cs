using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace HranitelPRO
{
    public partial class GeneralDeptWindow : Window
    {
        private DatabaseService _db;
        private int _employeeId;
        private string _employeeName;

        // Класс для отчета
        public class ReportItem
        {
            public string Department { get; set; }
            public int Count { get; set; }
            public double Percentage { get; set; }
        }

        // Класс для текущих посетителей
        public class CurrentVisitor
        {
            public string Department { get; set; }
            public string FullName { get; set; }
            public string RequestType { get; set; }
            public DateTime? EntryTime { get; set; }
            public string Purpose { get; set; }
        }

        public GeneralDeptWindow(DatabaseService db, int employeeId, string employeeName)
        {
            InitializeComponent();
            _db = db;
            _employeeId = employeeId;
            _employeeName = employeeName;
            userNameText.Text = employeeName;

            // Устанавливаем сегодняшнюю дату для отчета
            reportDatePicker.SelectedDate = DateTime.Now;

            this.Loaded += async (s, e) => await LoadData();
        }

        private async Task LoadData()
        {
            await LoadDepartments();
            await LoadRequests();
        }

        private async Task LoadDepartments()
        {
            var departments = await _db.GetDepartments();
            var list = new ObservableCollection<Department>();
            list.Add(new Department { id = 0, name = "Все" });
            foreach (var d in departments)
                list.Add(d);
            filterDepartmentCombo.ItemsSource = list;
            filterDepartmentCombo.SelectedIndex = 0;
        }

        private async Task LoadRequests()
        {
            try
            {
                string type = null;
                if (filterTypeCombo.SelectedItem is ComboBoxItem typeItem && typeItem.Content.ToString() != "Все")
                    type = typeItem.Content.ToString();

                int deptId = 0;
                if (filterDepartmentCombo.SelectedItem is Department dept && dept.id > 0)
                    deptId = dept.id;

                string status = null;
                if (filterStatusCombo.SelectedItem is ComboBoxItem statusItem && statusItem.Content.ToString() != "Все")
                    status = statusItem.Content.ToString();

                string search = string.IsNullOrWhiteSpace(searchBox.Text) ? null : searchBox.Text;

                var requests = await _db.GetAllRequests(type, deptId > 0 ? deptId : (int?)null, status, search);
                requestsGrid.ItemsSource = requests;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        // Переключение на вкладку заявок
        private void ShowRequests_Click(object sender, RoutedEventArgs e)
        {
            panelRequests.Visibility = Visibility.Visible;
            panelReports.Visibility = Visibility.Collapsed;
            panelCurrentVisitors.Visibility = Visibility.Collapsed;

            btnRequests.Background = System.Windows.Media.Brushes.DodgerBlue;
            btnReports.Background = System.Windows.Media.Brushes.Gray;
            btnCurrentVisitors.Background = System.Windows.Media.Brushes.Gray;
        }

        // Переключение на вкладку отчетов
        private void ShowReports_Click(object sender, RoutedEventArgs e)
        {
            panelRequests.Visibility = Visibility.Collapsed;
            panelReports.Visibility = Visibility.Visible;
            panelCurrentVisitors.Visibility = Visibility.Collapsed;

            btnRequests.Background = System.Windows.Media.Brushes.Gray;
            btnReports.Background = System.Windows.Media.Brushes.DodgerBlue;
            btnCurrentVisitors.Background = System.Windows.Media.Brushes.Gray;
        }

        // Переключение на вкладку текущих посетителей
        private void ShowCurrentVisitors_Click(object sender, RoutedEventArgs e)
        {
            panelRequests.Visibility = Visibility.Collapsed;
            panelReports.Visibility = Visibility.Collapsed;
            panelCurrentVisitors.Visibility = Visibility.Visible;

            btnRequests.Background = System.Windows.Media.Brushes.Gray;
            btnReports.Background = System.Windows.Media.Brushes.Gray;
            btnCurrentVisitors.Background = System.Windows.Media.Brushes.DodgerBlue;

            LoadCurrentVisitors();
        }

        // Загрузка текущих посетителей
        private async void LoadCurrentVisitors()
        {
            try
            {
                var currentVisitors = new ObservableCollection<CurrentVisitor>();

                // Получаем все заявки со статусом "одобрена" и временем входа
                var allRequests = await _db.GetAllRequests(null, null, "одобрена", null);
                var current = allRequests.Where(r => r.visit_start_time.HasValue && !r.visit_end_time.HasValue);

                foreach (var req in current)
                {
                    currentVisitors.Add(new CurrentVisitor
                    {
                        Department = req.department_name,
                        FullName = req.full_name,
                        RequestType = req.request_type,
                        EntryTime = req.visit_start_time,
                        Purpose = req.purpose
                    });
                }

                currentVisitorsGrid.ItemsSource = currentVisitors;

                if (currentVisitors.Count == 0)
                {
                    MessageBox.Show("В настоящее время на территории нет посетителей", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки текущих посетителей: {ex.Message}");
            }
        }

        // Обновление текущих посетителей
        private void RefreshCurrentVisitors_Click(object sender, RoutedEventArgs e)
        {
            LoadCurrentVisitors();
        }

        // Формирование отчета
        private async void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string period = (reportPeriodCombo.SelectedItem as ComboBoxItem)?.Content.ToString();
                DateTime selectedDate = reportDatePicker.SelectedDate ?? DateTime.Now;
                string groupBy = (reportGroupCombo.SelectedItem as ComboBoxItem)?.Content.ToString();

                var reportData = new ObservableCollection<ReportItem>();
                int totalCount = 0;

                // Получаем данные в зависимости от периода
                var allRequests = await _db.GetAllRequests(null, null, "одобрена", null);

                DateTime startDate;
                DateTime endDate;

                switch (period)
                {
                    case "День":
                        startDate = selectedDate.Date;
                        endDate = startDate.AddDays(1);
                        break;
                    case "Месяц":
                        startDate = new DateTime(selectedDate.Year, selectedDate.Month, 1);
                        endDate = startDate.AddMonths(1);
                        break;
                    case "Год":
                        startDate = new DateTime(selectedDate.Year, 1, 1);
                        endDate = startDate.AddYears(1);
                        break;
                    default:
                        startDate = selectedDate.Date;
                        endDate = startDate.AddDays(1);
                        break;
                }

                var filteredRequests = allRequests.Where(r => r.created_at >= startDate && r.created_at < endDate);

                if (groupBy == "По подразделениям")
                {
                    var grouped = filteredRequests.GroupBy(r => r.department_name)
                        .Select(g => new { Department = g.Key, Count = g.Count() })
                        .OrderByDescending(g => g.Count);

                    totalCount = grouped.Sum(g => g.Count);

                    foreach (var g in grouped)
                    {
                        reportData.Add(new ReportItem
                        {
                            Department = g.Department,
                            Count = g.Count,
                            Percentage = totalCount > 0 ? (double)g.Count / totalCount * 100 : 0
                        });
                    }
                }
                else
                {
                    var grouped = filteredRequests.GroupBy(r => r.request_type)
                        .Select(g => new { Department = g.Key, Count = g.Count() });

                    totalCount = grouped.Sum(g => g.Count);

                    foreach (var g in grouped)
                    {
                        reportData.Add(new ReportItem
                        {
                            Department = g.Department,
                            Count = g.Count,
                            Percentage = totalCount > 0 ? (double)g.Count / totalCount * 100 : 0
                        });
                    }
                }

                reportGrid.ItemsSource = reportData;

                if (reportData.Count == 0)
                {
                    MessageBox.Show("Нет данных для формирования отчета", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // Автоматическое формирование отчета за каждые 3 часа
                await GenerateThreeHourReport();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка формирования отчета: {ex.Message}");
            }
        }

        // Автоматическое формирование отчета за каждые 3 часа
        private async Task GenerateThreeHourReport()
        {
            try
            {
                // Определяем текущий час
                int currentHour = DateTime.Now.Hour;
                int reportHour = (currentHour / 3) * 3;

                // Создаем папку для отчетов, если её нет
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string reportsFolder = Path.Combine(documentsPath, "Отчеты ТБ");
                string dailyFolder = Path.Combine(reportsFolder, DateTime.Now.ToString("dd_MM_yyyy"));

                if (!Directory.Exists(reportsFolder))
                    Directory.CreateDirectory(reportsFolder);

                if (!Directory.Exists(dailyFolder))
                    Directory.CreateDirectory(dailyFolder);

                // Получаем данные для отчета
                var departments = await _db.GetDepartments();
                var allRequests = await _db.GetAllRequests(null, null, "одобрена", null);

                // Считаем посетителей за последние 3 часа
                DateTime threeHoursAgo = DateTime.Now.AddHours(-3);
                var recentRequests = allRequests.Where(r => r.visit_start_time.HasValue && r.visit_start_time >= threeHoursAgo);

                // Формируем HTML отчет
                string htmlContent = $@"
                <html>
                <head>
                    <meta charset='utf-8'>
                    <title>Отчет по посетителям</title>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 20px; }}
                        h1 {{ color: #1E3A5F; }}
                        h2 {{ color: #3498DB; }}
                        table {{ border-collapse: collapse; width: 100%; margin-top: 20px; }}
                        th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
                        th {{ background-color: #1E3A5F; color: white; }}
                        tr:nth-child(even) {{ background-color: #f2f2f2; }}
                        .footer {{ margin-top: 30px; font-size: 12px; color: gray; text-align: center; }}
                    </style>
                </head>
                <body>
                    <h1>Отчет о количестве посетителей</h1>
                    <h2>Подразделение: Общий отчет</h2>
                    <p>Период: {threeHoursAgo:dd.MM.yyyy HH:mm} - {DateTime.Now:dd.MM.yyyy HH:mm}</p>
                    <p>Время формирования: {DateTime.Now:dd.MM.yyyy HH:mm:ss}</p>
                    
                    <h3>Количество посетителей по подразделениям</h3>
                    <table>
                        <tr>
                            <th>Подразделение</th>
                            <th>Количество посетителей</th>
                            <th>Процент от общего числа</th>
                        </tr>
                ";

                int totalVisitors = recentRequests.Count();
                var groupedByDept = recentRequests.GroupBy(r => r.department_name);

                foreach (var group in groupedByDept)
                {
                    double percentage = totalVisitors > 0 ? (double)group.Count() / totalVisitors * 100 : 0;
                    htmlContent += $@"
                        <tr>
                            <td>{group.Key}</td>
                            <td>{group.Count()}</td>
                            <td>{percentage:F1}%</td>
                        </tr>
                    ";
                }

                htmlContent += $@"
                    </table>
                    
                    <h3>Список посетителей</h3>
                    <table>
                        <tr>
                            <th>ФИО</th>
                            <th>Подразделение</th>
                            <th>Тип</th>
                            <th>Время входа</th>
                            <th>Цель</th>
                        </tr>
                ";

                foreach (var visitor in recentRequests.OrderBy(r => r.visit_start_time))
                {
                    htmlContent += $@"
                        <tr>
                            <td>{visitor.full_name}</td>
                            <td>{visitor.department_name}</td>
                            <td>{visitor.request_type}</td>
                            <td>{visitor.visit_start_time:HH:mm:ss}</td>
                            <td>{visitor.purpose}</td>
                        </tr>
                    ";
                }

                htmlContent += $@"
                    </table>
                    
                    <div class='footer'>
                        <p>Отчет сформирован автоматически системой ХранительПРО</p>
                        <p>© {DateTime.Now.Year} ХранительПРО - Система контроля доступа</p>
                    </div>
                </body>
                </html>
                ";

                // Сохраняем HTML файл
                string fileName = Path.Combine(dailyFolder, $"report_{reportHour:00}.html");
                File.WriteAllText(fileName, htmlContent);

                // Конвертируем в PDF (опционально)
                // ConvertHtmlToPdf(htmlContent, Path.Combine(dailyFolder, $"report_{reportHour:00}.pdf"));

                System.Diagnostics.Debug.WriteLine($"Создан отчет: {fileName}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка создания отчета: {ex.Message}");
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
                // Проверка черного списка
                var (inBlacklist, reason) = await _db.CheckBlacklist(request.passport_series, request.passport_number);

                if (inBlacklist)
                {
                    await _db.UpdateRequestStatus(request.request_id, request.request_type, 3,
                        "Заявка на посещение объекта КИИ отклонена в связи с нарушением Федерального закона от 26.07.2017 № 187-ФЗ");

                    MessageBox.Show($"ВНИМАНИЕ!\n\nПосетитель находится в черном списке!\nПричина: {reason}\n\nЗаявка автоматически отклонена.",
                        "Черный список", MessageBoxButton.OK, MessageBoxImage.Warning);
                    await LoadRequests();
                    return;
                }

                var dialog = new RequestDetailDialog(_db, request, _employeeId, 0);
                dialog.Owner = this;
                if (dialog.ShowDialog() == true)
                {
                    await LoadRequests();
                    LoadCurrentVisitors();
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