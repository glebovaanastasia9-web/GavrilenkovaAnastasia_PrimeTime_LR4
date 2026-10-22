using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.NetworkInformation;
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
    /// Логика взаимодействия для OrdersPagexaml.xaml
    /// </summary>
    public class OrderItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ClientName { get; set; }
        public string TypeName { get; set; }
        public string VenueName { get; set; }
        public DateTime EventDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int Guests { get; set; }
        public string Status { get; set; }
        public decimal TotalCost { get; set; }
        public decimal Paid { get; set; }
        public Visibility EditVisibility { get; set; }
        public Visibility DeleteVisibility { get; set; }
        public string DisplayTitle
        {
            get
            {
                return "№" + Id.ToString() + " " + Title;
            }
        }
        public string DisplayClient
        {
            get
            {
                return "Клиент: " + ClientName;
            }
        }
        public string DisplayInfo
        {
            get
            {
                return TypeName + ", " + VenueName;
            }
        }
        public string DisplayWhen
        {
            get
            {
                return EventDate.ToString("dd.MM.yyyy") + ", " + StartTime.ToString(@"hh\:mm") + " - " + EndTime.ToString(@"hh\:mm") + ", гостей: " + Guests.ToString();
            }
        }
        public string DisplayCost
        {
            get
            {
                return "Стоимость: " + TotalCost.ToString("N0") + " руб.";
            }
        }
        public string DisplayPaid
        {
            get
            {
                return "Оплачено: " + Paid.ToString("N0") + " руб.";
            }
        }
        public string StatusColor
        {
            get
            {
                if (Status == "Заявка")
                {
                    return "#FFF3C4";
                }
                if (Status == "Подтверждено")
                {
                    return "#D2E8F7";
                }
                if (Status == "Проведено")
                {
                    return "#D5EED3";
                }
                if (Status == "Отменено")
                {
                    return "#FBDCE8";
                }
                return "#EEEEEE";
            }
        }
    }
    public partial class OrdersPage : Page
    {
        private bool isReady = false;
        public OrdersPage()
        {
            InitializeComponent();
            if (Session.IsStaff() == true)
            {
                txtTitle.Text = "Все заявки";
            }
            else
            {
                txtTitle.Text = "Мои заявки";
            }
            cbStatus.Items.Add("Все статусы");
            cbStatus.Items.Add("Заявка");
            cbStatus.Items.Add("Подтверждено");
            cbStatus.Items.Add("Проведено");
            cbStatus.Items.Add("Отменено");
            cbStatus.SelectedIndex = 0;
            isReady = true;
            LoadOrders();
        }
        private void SearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (isReady == false)
            {
                return;
            }
            LoadOrders();
        }
        private void StatusChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isReady == false)
            {
                return;
            }
            LoadOrders();
        }
        private void LoadOrders()
        {
            int clientFilter = 0;
            if (Session.IsStaff() == false)
            {
                clientFilter = Session.ClientId;
            }
            string statusFilter = "";
            if (cbStatus.SelectedIndex > 0)
            {
                statusFilter = cbStatus.SelectedItem.ToString();
            }
            string search = "%" + txtSearch.Text.Trim() + "%";
            List<OrderItem> orders = new List<OrderItem>();
            string query = "SELECT e.event_id, e.title, c.name, t.name, v.name, e.event_date, e.start_time, e.end_time, e.guests_count, e.status, " +
                "v.rent_price + ISNULL((SELECT SUM(s.price * es.quantity) FROM EventServices es JOIN Services s ON s.service_id = es.service_id WHERE es.event_id = e.event_id), 0) + ISNULL((SELECT SUM(q.rental_price * ee.quantity) FROM EventEquipment ee JOIN Equipment q ON q.equipment_id = ee.equipment_id WHERE ee.event_id = e.event_id), 0) AS total_cost, " +
                "ISNULL((SELECT SUM(p.amount) FROM Payments p WHERE p.event_id = e.event_id), 0) AS paid " +
                "FROM Events e JOIN Clients c ON c.client_id = e.client_id JOIN EventTypes t ON t.event_type_id = e.event_type_id JOIN Venues v ON v.venue_id = e.venue_id " +
                "WHERE (@ClientId = 0 OR e.client_id = @ClientId) AND (@Status = '' OR e.status = @Status) AND (e.title LIKE @Search OR c.name LIKE @Search) " +
                "ORDER BY e.event_date DESC, e.event_id DESC";
            try
            {
                using (SqlConnection connection = new SqlConnection(DbConnector.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ClientId", clientFilter);
                        command.Parameters.AddWithValue("@Status", statusFilter);
                        command.Parameters.AddWithValue("@Search", search);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read() == true)
                            {
                                OrderItem item = new OrderItem();
                                item.Id = reader.GetInt32(0);
                                item.Title = reader.GetString(1);
                                item.ClientName = reader.GetString(2);
                                item.TypeName = reader.GetString(3);
                                item.VenueName = reader.GetString(4);
                                item.EventDate = reader.GetDateTime(5);
                                item.StartTime = (TimeSpan)reader.GetValue(6);
                                item.EndTime = (TimeSpan)reader.GetValue(7);
                                item.Guests = Convert.ToInt32(reader.GetValue(8));
                                item.Status = reader.GetString(9);
                                item.TotalCost = reader.GetDecimal(10);
                                item.Paid = reader.GetDecimal(11);
                                item.EditVisibility = Visibility.Collapsed;
                                item.DeleteVisibility = Visibility.Collapsed;
                                if (Session.IsStaff() == true)
                                {
                                    item.EditVisibility = Visibility.Visible;
                                }
                                else
                                {
                                    if (item.Status == "Заявка")
                                    {
                                        item.EditVisibility = Visibility.Visible;
                                        item.DeleteVisibility = Visibility.Visible;
                                    }
                                }
                                if (Session.IsAdmin() == true)
                                {
                                    item.DeleteVisibility = Visibility.Visible;
                                }
                                orders.Add(item);
                            }
                        }
                    }
                }
            }
            catch (SqlException)
            {
                DbConnector.ShowError("Не удалось загрузить заявки. Проверьте подключение к базе данных.");
                return;
            }
            catch (Exception ex)
            {
                DbConnector.ShowError("Ошибка при загрузке заявок: " + ex.Message);
                return;
            }
            lbOrders.ItemsSource = orders;
            if (orders.Count == 0)
            {
                txtEmpty.Visibility = Visibility.Visible;
            }
            else
            {
                txtEmpty.Visibility = Visibility.Collapsed;
            }
        }
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null)
            {
                return;
            }
            int eventId = Convert.ToInt32(button.Tag);
            NavigationService.Navigate(new EditOrderPage(eventId));
        }
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null)
            {
                return;
            }
            int eventId = Convert.ToInt32(button.Tag);
            if (Session.IsAdmin() == false && Session.RoleId != Session.RoleClient)
            {
                DbConnector.ShowWarning("У вас нет прав на удаление заявок!");
                return;
            }
            MessageBoxResult answer = MessageBox.Show("Удалить заявку №" + eventId.ToString() + " вместе со связанными данными? Это действие нельзя отменить.", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (answer != MessageBoxResult.Yes)
            {
                return;
            }
            try
            {
                using (SqlConnection connection = new SqlConnection(DbConnector.ConnectionString))
                {
                    connection.Open();
                    if (Session.IsAdmin() == false)
                    {
                        int ownCount = 0;
                        using (SqlCommand ownCommand = new SqlCommand("SELECT COUNT(*) FROM Events WHERE event_id = @EventId AND client_id = @ClientId AND status = @Status", connection))
                        {
                            ownCommand.Parameters.AddWithValue("@EventId", eventId);
                            ownCommand.Parameters.AddWithValue("@ClientId", Session.ClientId);
                            ownCommand.Parameters.AddWithValue("@Status", "Заявка");
                            ownCount = Convert.ToInt32(ownCommand.ExecuteScalar());
                        }
                        if (ownCount == 0)
                        {
                            DbConnector.ShowWarning("Удалить можно только свою заявку со статусом «Заявка»!");
                            LoadOrders();
                            return;
                        }
                    }
                    string[] queries = new string[6];
                    queries[0] = "DELETE FROM Payments WHERE event_id = @EventId";
                    queries[1] = "DELETE FROM Reviews WHERE event_id = @EventId";
                    queries[2] = "DELETE FROM EventStaff WHERE event_id = @EventId";
                    queries[3] = "DELETE FROM EventEquipment WHERE event_id = @EventId";
                    queries[4] = "DELETE FROM EventServices WHERE event_id = @EventId";
                    queries[5] = "DELETE FROM Events WHERE event_id = @EventId";
                    SqlTransaction transaction = connection.BeginTransaction();
                    try
                    {
                        int index = 0;
                        while (index < queries.Length)
                        {
                            using (SqlCommand command = new SqlCommand(queries[index], connection, transaction))
                            {
                                command.Parameters.AddWithValue("@EventId", eventId);
                                command.ExecuteNonQuery();
                            }
                            index = index + 1;
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
            catch (SqlException)
            {
                DbConnector.ShowError("Не удалось удалить заявку. Проверьте подключение к базе данных.");
                return;
            }
            catch (Exception ex)
            {
                DbConnector.ShowError("Ошибка при удалении заявки: " + ex.Message);
                return;
            }
            DbConnector.ShowInfo("Заявка удалена.");
            LoadOrders();
        }
    }
}