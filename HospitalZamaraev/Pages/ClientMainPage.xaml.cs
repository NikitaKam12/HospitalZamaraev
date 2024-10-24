using System.Windows;
using System.Windows.Controls;

namespace HospitalZamaraev.Pages
{
    public partial class ClientMainPage : Page
    {
        public ClientMainPage()
        {
            InitializeComponent();
            // Загружаем первую страницу при инициализации
            MainFrame.Navigate(new ServicesPage());
        }

        private void ServicesButton_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ServicesPage());
        }

        private void DoctorsButton_Click(object sender, RoutedEventArgs e)
        {
            
            MainFrame.Navigate(new DoctorsPage());
        }

        private void BookButton_Click(object sender, RoutedEventArgs e)
        {
           
             MainFrame.Navigate(new BookPage());
        }

        private void MyRecordsButton_Click(object sender, RoutedEventArgs e)
        {
            
             MainFrame.Navigate(new MyRecordsPage());
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
