using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>    
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }
        private void BtnGoToReg_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RegisterPage());
        }
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = pbPassword.Password;
            if (login == "")
            {
                DbConnector.ShowWarning("Введите логин!");
                return;
            }
            if (password == "")
            {
                DbConnector.ShowWarning("Введите пароль!");
                return;
            }
            string passwordHash = DbConnector.ComputeSha256(password);
            bool isFound = false;
            int userId = 0;
            int roleId = 0;
            int clientId = 0;
            string roleName = "";
            try
            {
                using (SqlConnection connection = new SqlConnection(DbConnector.ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT u.user_id, u.role_id, u.client_id, r.name FROM Users u JOIN Roles r ON r.role_id = u.role_id WHERE u.login = @Login AND u.password_hash = @PasswordHash";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Login", login);
                        command.Parameters.AddWithValue("@PasswordHash", passwordHash);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read() == true)
                            {
                                isFound = true;
                                userId = reader.GetInt32(0);
                                roleId = reader.GetInt32(1);
                                if (reader.IsDBNull(2) == false)
                                {
                                    clientId = reader.GetInt32(2);
                                }
                                roleName = reader.GetString(3);
                            }
                        }
                    }
                }
            }
            catch (SqlException)
            {
                DbConnector.ShowError("Не удалось подключиться к базе данных. Проверьте, что SQL Server запущен и строка подключения указана верно.");
                return;
            }
            catch (Exception ex)
            {
                DbConnector.ShowError("Ошибка при входе: " + ex.Message);
                return;
            }
            if (isFound == false)
            {
                DbConnector.ShowError("Неверный логин или пароль!");
                pbPassword.Password = "";
                return;
            }
            Session.Start(userId, roleId, clientId, login, roleName);
            MainWindow.RefreshMenu();
            NavigationService.Navigate(new HomePage());
        }
    }
}
