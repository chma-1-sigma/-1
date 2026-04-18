using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using HranitelPro.Admin.Models;
using HranitelPro.Admin.Services;
using OfficeOpenXml;

namespace HranitelPro.Admin
{
    public partial class ReportWindow : Window
    {
        private DatabaseService _db;

        public ReportWindow(DatabaseService db)
        {
            InitializeComponent();
            _db = db;
            startDatePicker.SelectedDate = DateTime.Now.AddMonths(-1);
            endDatePicker.SelectedDate = DateTime.Now;
        }

        private async void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            if (!startDatePicker.SelectedDate.HasValue || !endDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите период", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var data = await _db.GetVisitStatistics(
                startDatePicker.SelectedDate.Value,
                endDatePicker.SelectedDate.Value
            );

            reportGrid.ItemsSource = data;

            if (data.Count == 0)
            {
                MessageBox.Show("Нет данных за выбранный период", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            if (reportGrid.ItemsSource == null)
            {
                MessageBox.Show("Сначала сформируйте отчет", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                DefaultExt = "xlsx",
                FileName = $"Отчет_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                var data = reportGrid.ItemsSource as ObservableCollection<ReportData>;

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Отчет");

                    // Заголовки
                    worksheet.Cells[1, 1].Value = "Дата";
                    worksheet.Cells[1, 2].Value = "Подразделение";
                    worksheet.Cells[1, 3].Value = "Количество посещений";

                    // Данные
                    int row = 2;
                    foreach (var item in data)
                    {
                        worksheet.Cells[row, 1].Value = item.Date.ToString("dd.MM.yyyy");
                        worksheet.Cells[row, 2].Value = item.DepartmentName;
                        worksheet.Cells[row, 3].Value = item.VisitCount;
                        row++;
                    }

                    worksheet.Cells.AutoFitColumns();

                    File.WriteAllBytes(saveDialog.FileName, package.GetAsByteArray());
                }

                MessageBox.Show($"Отчет сохранен: {saveDialog.FileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}