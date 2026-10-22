using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace PrimeTime
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>    
    public partial class App : Application
    {
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Произошла непредвиденная ошибка: " + e.Exception.Message, "Prime Time", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
        public static class Session
        {
            public const int RoleAdmin = 1;
            public const int RoleManager = 2;
            public const int RoleClient = 3;
            public static int UserId = 0;
            public static int RoleId = 0;
            public static int ClientId = 0;
            public static string Login = "";
            public static string RoleName = "";

            public static void Start(int userId, int roleId, int clientId, string login, string roleName)
            {
                UserId = userId;
                RoleId = roleId;
                ClientId = clientId;
                Login = login;
                RoleName = roleName;
            }
            public static void Clear()
            {
                UserId = 0;
                RoleId = 0;
                ClientId = 0;
                Login = "";
                RoleName = "";
            }
            public static bool IsAuthorized()
            {
                return UserId > 0;
            }
            public static bool IsStaff()
            {
                return RoleId == RoleAdmin || RoleId == RoleManager;
            }
            public static bool IsAdmin()
            {
                return RoleId == RoleAdmin;
            }
        }
        public static class DbConnector
        {
            public static string ConnectionString = @"Server=.\SQLEXPRESS01;Database=PrimeTime;Trusted_Connection=True;Connect Timeout=5;";

            public static void ShowWarning(string text)
            {
                MessageBox.Show(text, "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            public static void ShowError(string text)
            {
                MessageBox.Show(text, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            public static void ShowInfo(string text)
            {
                MessageBox.Show(text, "Prime Time", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            public static string ComputeSha256(string plainText)
            {
                SHA256 sha = SHA256.Create();
                byte[] textBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] hashBytes = sha.ComputeHash(textBytes);
                StringBuilder hexBuilder = new StringBuilder();
                int counter = 0;
                while (counter < hashBytes.Length)
                {
                    byte currentByte = hashBytes[counter];
                    hexBuilder.Append(currentByte.ToString("x2"));
                    counter = counter + 1;
                }
                sha.Dispose();
                return hexBuilder.ToString();
            }
            public static bool IsInteger(string text)
            {
                if (text == "")
                {
                    return false;
                }
                int index = 0;
                while (index < text.Length)
                {
                    char symbol = text[index];
                    if (symbol < '0' || symbol > '9')
                    {
                        return false;
                    }
                    index = index + 1;
                }
                return true;
            }
            public static bool IsValidName(string text)
            {
                if (text == "")
                {
                    return false;
                }
                int index = 0;
                while (index < text.Length)
                {
                    char symbol = text[index];
                    bool isAllowedSymbol = false;
                    if (symbol >= 'а' && symbol <= 'я')
                    {
                        isAllowedSymbol = true;
                    }
                    if (symbol >= 'А' && symbol <= 'Я')
                    {
                        isAllowedSymbol = true;
                    }
                    if (symbol >= 'a' && symbol <= 'z')
                    {
                        isAllowedSymbol = true;
                    }
                    if (symbol >= 'A' && symbol <= 'Z')
                    {
                        isAllowedSymbol = true;
                    }
                    if (symbol == 'ё' || symbol == 'Ё')
                    {
                        isAllowedSymbol = true;
                    }
                    if (symbol == '-' || symbol == ' ')
                    {
                        isAllowedSymbol = true;
                    }
                    if (isAllowedSymbol == false)
                    {
                        return false;
                    }
                    index = index + 1;
                }
                return true;
            }
            public static bool IsValidPhone(string text)
            {
                if (text == "")
                {
                    return false;
                }
                int index = 0;
                int digitCount = 0;
                while (index < text.Length)
                {
                    char symbol = text[index];
                    bool isAllowedSymbol = false;
                    if (symbol >= '0' && symbol <= '9')
                    {
                        isAllowedSymbol = true;
                        digitCount = digitCount + 1;
                    }
                    if (symbol == '+' || symbol == '-' || symbol == '(' || symbol == ')' || symbol == ' ')
                    {
                        isAllowedSymbol = true;
                    }
                    if (isAllowedSymbol == false)
                    {
                        return false;
                    }
                    index = index + 1;
                }
                if (digitCount < 10)
                {
                    return false;
                }
                return true;
            }
            public static bool IsValidEmail(string text)
            {
                if (text == "")
                {
                    return false;
                }
                if (text.IndexOf(' ') >= 0)
                {
                    return false;
                }
                int atPosition = text.IndexOf('@');
                if (atPosition < 1)
                {
                    return false;
                }
                if (text.IndexOf('@', atPosition + 1) >= 0)
                {
                    return false;
                }
                int dotPosition = text.LastIndexOf('.');
                if (dotPosition <= atPosition + 1)
                {
                    return false;
                }
                if (dotPosition >= text.Length - 1)
                {
                    return false;
                }
                return true;
            }
            public static bool IsValidLogin(string text)
            {
                if (text.Length < 3)
                {
                    return false;
                }
                int index = 0;
                while (index < text.Length)
                {
                    char symbol = text[index];
                    bool isAllowedSymbol = false;
                    if (symbol >= 'a' && symbol <= 'z')
                    {
                        isAllowedSymbol = true;
                    }
                    if (symbol >= 'A' && symbol <= 'Z')
                    {
                        isAllowedSymbol = true;
                    }
                    if (symbol >= '0' && symbol <= '9')
                    {
                        isAllowedSymbol = true;
                    }
                    if (symbol == '_' || symbol == '.')
                    {
                        isAllowedSymbol = true;
                    }
                    if (isAllowedSymbol == false)
                    {
                        return false;
                    }
                    index = index + 1;
                }
                return true;
            }
        }
    }