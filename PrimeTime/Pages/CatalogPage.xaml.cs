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
    /// Логика взаимодействия для CatalogPagexaml.xaml
    /// </summary>
    public class ServiceItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string DisplayPrice
        {
            get
            {
                return Price.ToString("N0") + " руб.";
            }
        }
    }
    public partial class CatalogPage : Page
    {
        private bool isReady = false;
        public CatalogPage()
        {
            InitializeComponent();
            cbSort.Items.Add("Сначала дешёвые");
            cbSort.Items.Add("Сначала дорогие");
            cbSort.Items.Add("По названию");
            cbSort.SelectedIndex = 0;
            if (Session.IsAuthorized() == false)
            {
                brdHint.Visibility = Visibility.Visible;
            }
            isReady = true;
            LoadServices();
        }
        private void SearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (isReady == false)
            {
                return;
            }
            LoadServices();
        }
        private void SortChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isReady == false)
            {
                return;
            }
            LoadServices();
        }
        private void LoadServices()
        {
            string search = txtSearch.Text.Trim();
            decimal minPrice = 0;
            decimal maxPrice = 1000000000;
            if (DbConnector.IsInteger(txtMinPrice.Text.Trim()) == true)
            {
                minPrice = Convert.ToDecimal(txtMinPrice.Text.Trim());
            }
            if (DbConnector.IsInteger(txtMaxPrice.Text.Trim()) == true)
            {
                maxPrice = Convert.ToDecimal(txtMaxPrice.Text.Trim());
            }
            string orderBy = "price ASC";
            if (cbSort.SelectedIndex == 1)
            {
                orderBy = "price DESC";
            }
            if (cbSort.SelectedIndex == 2)
            {
                orderBy = "name ASC";
            }
            List<ServiceItem> services = new List<ServiceItem>();
            try
            {
                using (SqlConnection connection = new SqlConnection(DbConnector.ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT service_id, name, price FROM Services WHERE name LIKE @Search AND price >= @MinPrice AND price <= @MaxPrice ORDER BY " + orderBy;
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Search", "%" + search + "%");
                        command.Parameters.AddWithValue("@MinPrice", minPrice);
                        command.Parameters.AddWithValue("@MaxPrice", maxPrice);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read() == true)
                            {
                                ServiceItem item = new ServiceItem();
                                item.Id = reader.GetInt32(0);
                                item.Name = reader.GetString(1);
                                item.Price = reader.GetDecimal(2);
                                services.Add(item);
                            }
                        }
                    }
                }
            }
            catch (SqlException)
            {
                DbConnector.ShowError("Не удалось загрузить услуги. Проверьте подключение к базе данных.");
                return;
            }
            catch (Exception ex)
            {
                DbConnector.ShowError("Ошибка при загрузке услуг: " + ex.Message);
                return;
            }
            lbServices.ItemsSource = services;
            if (services.Count == 0)
            {
                txtEmpty.Visibility = Visibility.Visible;
            }
            else
            {
                txtEmpty.Visibility = Visibility.Collapsed;
            }
        }
        private void BtnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (Session.IsAuthorized() == false)
            {
                DbConnector.ShowWarning("Чтобы оформить заявку, войдите в личный кабинет.");
                NavigationService.Navigate(new LoginPage());
                return;
            }
            if (Session.ClientId <= 0)
            {
                DbConnector.ShowWarning("Заявки на мероприятия оформляют клиенты.");
                return;
            }
            Button button = sender as Button;
            if (button == null)
            {
                return;
            }
            int serviceId = Convert.ToInt32(button.Tag);
            NavigationService.Navigate(new OrderPage(serviceId));
        }
    }
}