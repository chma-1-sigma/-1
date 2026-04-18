using System.Windows;
using HranitelPro.Admin.Services;

namespace HranitelPro.Admin
{
    public partial class MainWindow : Window
    {
        private DatabaseService _db;

        public MainWindow()
        {
            InitializeComponent();
            string connectionString = "Host=localhost;Port=5432;Database=KhranitelPro;Username=postgres;Password=slon12";
            _db = new DatabaseService(connectionString);
        }

        private void BtnGeneralDept_Click(object sender, RoutedEventArgs e)
        {
            var authWindow = new AuthWindow(_db, "GeneralDept");
            authWindow.Show();
            this.Hide();
        }

        private void BtnSecurity_Click(object sender, RoutedEventArgs e)
        {
            var authWindow = new AuthWindow(_db, "Security");
            authWindow.Show();
            this.Hide();
        }

        private void BtnDepartment_Click(object sender, RoutedEventArgs e)
        {
            var authWindow = new AuthWindow(_db, "Department");
            authWindow.Show();
            this.Hide();
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}