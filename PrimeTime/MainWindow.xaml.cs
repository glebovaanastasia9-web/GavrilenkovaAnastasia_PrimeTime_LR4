using PrimeTime.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static PrimeTime.Pages.OrderPage;

namespace PrimeTime
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>           
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            UpdateMenu();
            MainFrame.Navigate(new HomePage());
        }
        public static void RefreshMenu()
        {
            MainWindow window = Application.Current.MainWindow as MainWindow;
            if (window != null)
            {
                window.UpdateMenu();
            }
        }
        public void UpdateMenu()
        {
            if (Session.IsAuthorized() == true)
            {
                txtUser.Text = Session.Login + " (" + Session.RoleName + ")";
                BtnLogout.Visibility = Visibility.Visible;
                BtnLogin.Visibility = Visibility.Collapsed;
                BtnRegister.Visibility = Visibility.Collapsed;
            }
            else
            {
                txtUser.Text = "Гость";
                BtnLogout.Visibility = Visibility.Collapsed;
                BtnLogin.Visibility = Visibility.Visible;
                BtnRegister.Visibility = Visibility.Visible;
            }
            if (Session.RoleId == Session.RoleClient)
            {
                BtnOrder.Visibility = Visibility.Visible;
            }
            else
            {
                BtnOrder.Visibility = Visibility.Collapsed;
            }
            if (Session.IsAuthorized() == true)
            {
                BtnOrders.Visibility = Visibility.Visible;
                if (Session.IsStaff() == true)
                {
                    BtnOrders.Content = "Все заявки";
                }
                else
                {
                    BtnOrders.Content = "Мои заявки";
                }
            }
            else
            {
                BtnOrders.Visibility = Visibility.Collapsed;
            }
        }
        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            while (MainFrame.CanGoBack == true)
            {
                MainFrame.RemoveBackEntry();
            }
        }
        private void BtnHome_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new HomePage());
        }
        private void BtnCatalog_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CatalogPage());
        }
        private void BtnOrder_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new OrderPage());
        }
        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new OrdersPage());
        }
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new LoginPage());
        }
        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new RegisterPage());
        }
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            Session.Clear();
            UpdateMenu();
            MainFrame.Navigate(new HomePage());
        }
    }
}