using System.Windows;

namespace HranitelPro.Admin
{
    public partial class RejectionReasonDialog : Window
    {
        public string RejectionReason { get; private set; }

        public RejectionReasonDialog()
        {
            InitializeComponent();

            rbOther.Checked += (s, e) => { txtOtherReason.IsEnabled = true; txtOtherReason.Visibility = Visibility.Visible; };
            rbInvalidData.Checked += (s, e) => { txtOtherReason.IsEnabled = false; txtOtherReason.Visibility = Visibility.Collapsed; };
            rbMissingFiles.Checked += (s, e) => { txtOtherReason.IsEnabled = false; txtOtherReason.Visibility = Visibility.Collapsed; };
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (rbInvalidData.IsChecked == true)
                RejectionReason = "Заявка отклонена в связи с указанием заявителем заведомо недостоверных данных";
            else if (rbMissingFiles.IsChecked == true)
                RejectionReason = "Заявка отклонена в связи с отсутствием или низким качеством прикрепленных файлов";
            else if (rbOther.IsChecked == true)
            {
                if (string.IsNullOrWhiteSpace(txtOtherReason.Text))
                {
                    MessageBox.Show("Укажите причину отклонения");
                    return;
                }
                RejectionReason = txtOtherReason.Text;
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}