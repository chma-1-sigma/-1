using System.Collections.ObjectModel;
using System.Windows;
using HranitelPro.Admin.Models;

namespace HranitelPro.Admin
{
    public partial class SelectMemberDialog : Window
    {
        public GroupMember SelectedMember { get; private set; }

        public SelectMemberDialog(ObservableCollection<GroupMember> members)
        {
            InitializeComponent();
            membersList.ItemsSource = members;
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedMember = membersList.SelectedItem as GroupMember;
            if (SelectedMember != null)
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Выберите посетителя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}