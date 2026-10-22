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
    /// Логика взаимодействия для RegisterPage.xaml
    /// </summary>    
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new LoginPage());
        }
        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            string lastName = txtLastName.Text.Trim();
            string firstName = txtFirstName.Text.Trim();
            string middleName = txtMiddleName.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string email = txtEmail.Text.Trim();
            string login = txtLogin.Text.Trim();
            string password = pbPassword.Password;
            string passwordRepeat = pbPasswordRepeat.Password;
            if (lastName == "" || firstName == "" || phone == "" || email == "" || login == "" || password == "" || passwordRepeat == "")
            {
                DbConnector.ShowWarning("Заполните все поля, помеченные звездочкой (*)!");
                return;
            }
            if (DbConnector.IsValidName(lastName) == false)
            {
                DbConnector.ShowWarning("Фамилия должна содержать только буквы!");
                return;
            }
            if (DbConnector.IsValidName(firstName) == false)
            {
                DbConnector.ShowWarning("Имя должно содержать только буквы!");
                return;
            }
            if (middleName != "")
            {
                if (DbConnector.IsValidName(middleName) == false)
                {
                    DbConnector.ShowWarning("Отчество должно содержать только буквы!");
                    return;
                }
            }
            if (DbConnector.IsValidPhone(phone) == false)
            {
                DbConnector.ShowWarning("Введите корректный номер телефона, минимум 10 цифр!");
                return;
            }
            if (DbConnector.IsValidEmail(email) == false)
            {
                DbConnector.ShowWarning("Введите корректный адрес электронной почты, например name@mail.ru!");
                return;
            }
            if (DbConnector.IsValidLogin(login) == false)
            {
                DbConnector.ShowWarning("Логин должен содержать не менее 3 символов: латинские буквы, цифры, точка и подчёркивание!");
                return;
            }
            if (password.Length < 6)
            {
                DbConnector.ShowWarning("Пароль должен содержать не менее 6 символов!");
                return;
            }
            if (password != passwordRepeat)
            {
                DbConnector.ShowWarning("Пароли не совпадают!");
                return;
            }
            string fullName = lastName + " " + firstName;
            if (middleName != "")
            {
                fullName = fullName + " " + middleName;
            }
            string passwordHash = DbConnector.ComputeSha256(password);
            try
            {
                using (SqlConnection connection = new SqlConnection(DbConnector.ConnectionString))
                {
                    connection.Open();
                    int loginCount = 0;
                    using (SqlCommand checkLogin = new SqlCommand("SELECT COUNT(*) FROM Users WHERE login = @Login", connection))
                    {
                        checkLogin.Parameters.AddWithValue("@Login", login);
                        loginCount = Convert.ToInt32(checkLogin.ExecuteScalar());
                    }
                    if (loginCount > 0)
                    {
                        DbConnector.ShowWarning("Пользователь с таким логином уже существует!");
                        return;
                    }
                    int emailCount = 0;
                    using (SqlCommand checkEmail = new SqlCommand("SELECT COUNT(*) FROM Clients WHERE email = @Email", connection))
                    {
                        checkEmail.Parameters.AddWithValue("@Email", email);
                        emailCount = Convert.ToInt32(checkEmail.ExecuteScalar());
                    }
                    if (emailCount > 0)
                    {
                        DbConnector.ShowWarning("Клиент с такой электронной почтой уже зарегистрирован!");
                        return;
                    }
                    SqlTransaction transaction = connection.BeginTransaction();
                    try
                    {
                        int newClientId = 0;
                        string clientQuery = "INSERT INTO Clients (client_type, name, inn, phone, email, address) VALUES (@Type, @Name, NULL, @Phone, @Email, NULL); SELECT CAST(SCOPE_IDENTITY() AS INT);";
                        using (SqlCommand clientCommand = new SqlCommand(clientQuery, connection, transaction))
                        {
                            clientCommand.Parameters.AddWithValue("@Type", "Физическое лицо");
                            clientCommand.Parameters.AddWithValue("@Name", fullName);
                            clientCommand.Parameters.AddWithValue("@Phone", phone);
                            clientCommand.Parameters.AddWithValue("@Email", email);
                            newClientId = Convert.ToInt32(clientCommand.ExecuteScalar());
                        }
                        string userQuery = "INSERT INTO Users (role_id, client_id, login, password_hash) VALUES (@RoleId, @ClientId, @Login, @PasswordHash)";
                        using (SqlCommand userCommand = new SqlCommand(userQuery, connection, transaction))
                        {
                            userCommand.Parameters.AddWithValue("@RoleId", Session.RoleClient);
                            userCommand.Parameters.AddWithValue("@ClientId", newClientId);
                            userCommand.Parameters.AddWithValue("@Login", login);
                            userCommand.Parameters.AddWithValue("@PasswordHash", passwordHash);
                            userCommand.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    DbConnector.ShowWarning("Пользователь с такими данными уже существует!");
                }
                else
                {
                    DbConnector.ShowError("Не удалось выполнить регистрацию. Проверьте подключение к базе данных.");
                }
                return;
            }
            catch (Exception ex)
            {
                DbConnector.ShowError("Ошибка при регистрации: " + ex.Message);
                return;
            }
            DbConnector.ShowInfo("Вы успешно зарегистрированы! Теперь можно войти.");
            NavigationService.Navigate(new LoginPage());
        }
    }
}