using System.Collections.Generic;
using System.Windows.Controls;

namespace HranitelPRO.Pages
{
    public partial class MyRequestsPage : Page
    {
        public MyRequestsPage(List<RequestViewModel> requests)
        {
            InitializeComponent();
            if (requests != null && requests.Count > 0)
            {
                requestsGrid.ItemsSource = requests;
                emptyText.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                emptyText.Visibility = System.Windows.Visibility.Visible;
            }
        }
    }
}