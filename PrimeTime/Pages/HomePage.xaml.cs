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

namespace PrimeTime.Pages
{
    /// <summary>
    /// Логика взаимодействия для HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
            if (Session.IsAuthorized() == true)
            {
                txtGreeting.Text = "Здравствуйте, " + Session.Login + "! Ваша роль: " + Session.RoleName + ".";
                BtnLogin.Visibility = Visibility.Collapsed;
                BtnRegister.Visibility = Visibility.Collapsed;
            }
            else
            {
                txtGreeting.Text = "Вы вошли как гость. Войдите или зарегистрируйтесь, чтобы оформить заявку на мероприятие.";
                BtnLogin.Visibility = Visibility.Visible;
                BtnRegister.Visibility = Visibility.Visible;
            }
        }
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new LoginPage());
        }
        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RegisterPage());
        }
    }
}