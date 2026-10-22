using PrimeTime.Entity;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
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

namespace PrimeTime.Pages
{
    /// <summary>
    /// Логика взаимодействия для OrderPage.xaml
    /// </summary>
    public class TypeItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class VenueItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Capacity { get; set; }
        public decimal Rent { get; set; }
        public string Title
        {
            get
            {
                return Name + " (до " + Capacity.ToString() + " гостей, " + Rent.ToString("N0") + " руб.)";
            }
        }
    }
    public class OrderServiceItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public bool IsSelected { get; set; }
        public string QuantityText { get; set; }
        public string DisplayPrice
        {
            get
            {
                return Price.ToString("N0") + " руб.";
            }
        }
    }
    public partial class OrderPage : Page
    {
        private List<OrderServiceItem> serviceItems = new List<OrderServiceItem>();
        public OrderPage() : this(0)
        {
        }
        public OrderPage(int preselectedServiceId)
        {
            InitializeComponent();
            dpDate.DisplayDateStart = DateTime.Today;
            LoadData(preselectedServiceId);
        }
        private void LoadData(int preselectedServiceId)
        {
            List<TypeItem> types = new List<TypeItem>();
            List<VenueItem> venues = new List<VenueItem>();
            try
            {
                using (SqlConnection connection = new SqlConnection(DbConnector.ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand typeCommand = new SqlCommand("SELECT event_type_id, name FROM EventTypes ORDER BY name", connection))
                    {
                        using (SqlDataReader reader = typeCommand.ExecuteReader())
                        {
                            while (reader.Read() == true)
                            {
                                TypeItem item = new TypeItem();
                                item.Id = reader.GetInt32(0);
                                item.Name = reader.GetString(1);
                                types.Add(item);
                            }
                        }
                    }
                    using (SqlCommand venueCommand = new SqlCommand("SELECT venue_id, name, capacity, rent_price FROM Venues ORDER BY name", connection))
                    {
                        using (SqlDataReader reader = venueCommand.ExecuteReader())
                        {
                            while (reader.Read() == true)
                            {
                                VenueItem item = new VenueItem();
                                item.Id = reader.GetInt32(0);
                                item.Name = reader.GetString(1);
                                item.Capacity = Convert.ToInt32(reader.GetValue(2));
                                item.Rent = reader.GetDecimal(3);
                                venues.Add(item);
                            }
                        }
                    }
                    using (SqlCommand serviceCommand = new SqlCommand("SELECT service_id, name, price FROM Services ORDER BY name", connection))
                    {
                        using (SqlDataReader reader = serviceCommand.ExecuteReader())
                        {
                            while (reader.Read() == true)
                            {
                                OrderServiceItem item = new OrderServiceItem();
                                item.Id = reader.GetInt32(0);
                                item.Name = reader.GetString(1);
                                item.Price = reader.GetDecimal(2);
                                item.QuantityText = "1";
                                if (item.Id == preselectedServiceId)
                                {
                                    item.IsSelected = true;
                                }
                                serviceItems.Add(item);
                            }
                        }
                    }
                }
            }
            catch (SqlException)
            {
                DbConnector.ShowError("Не удалось загрузить данные для заявки. Проверьте подключение к базе данных.");
                return;
            }
            catch (Exception ex)
            {
                DbConnector.ShowError("Ошибка при загрузке данных: " + ex.Message);
                return;
            }
            cbType.ItemsSource = types;
            cbVenue.ItemsSource = venues;
            icServices.ItemsSource = serviceItems;
        }
        private bool TryCalculate(out decimal total)
        {
            total = 0;
            VenueItem venue = cbVenue.SelectedItem as VenueItem;
            if (venue == null)
            {
                DbConnector.ShowWarning("Выберите площадку!");
                return false;
            }
            total = venue.Rent;
            int index = 0;
            while (index < serviceItems.Count)
            {
                OrderServiceItem item = serviceItems[index];
                if (item.IsSelected == true)
                {
                    string quantityText = "";
                    if (item.QuantityText != null)
                    {
                        quantityText = item.QuantityText.Trim();
                    }
                    if (DbConnector.IsInteger(quantityText) == false || quantityText.Length > 5)
                    {
                        DbConnector.ShowWarning("Количество для услуги «" + item.Name + "» должно быть целым числом больше нуля!");
                        return false;
                    }
                    int quantity = Convert.ToInt32(quantityText);
                    if (quantity <= 0)
                    {
                        DbConnector.ShowWarning("Количество для услуги «" + item.Name + "» должно быть больше нуля!");
                        return false;
                    }
                    total = total + item.Price * quantity;
                }
                index = index + 1;
            }
            return true;
        }
        private void BtnCalculate_Click(object sender, RoutedEventArgs e)
        {
            decimal total = 0;
            if (TryCalculate(out total) == false)
            {
                return;
            }
            txtTotal.Text = "Предварительная стоимость: " + total.ToString("N0") + " руб.";
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new CatalogPage());
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (Session.ClientId <= 0)
            {
                DbConnector.ShowWarning("Заявки на мероприятия оформляют клиенты!");
                return;
            }
            string title = txtTitle.Text.Trim();
            if (title == "")
            {
                DbConnector.ShowWarning("Введите название мероприятия!");
                return;
            }
            TypeItem eventType = cbType.SelectedItem as TypeItem;
            if (eventType == null)
            {
                DbConnector.ShowWarning("Выберите тип мероприятия!");
                return;
            }
            VenueItem venue = cbVenue.SelectedItem as VenueItem;
            if (venue == null)
            {
                DbConnector.ShowWarning("Выберите площадку!");
                return;
            }
            if (dpDate.SelectedDate.HasValue == false)
            {
                DbConnector.ShowWarning("Выберите дату мероприятия!");
                return;
            }
            DateTime eventDate = dpDate.SelectedDate.Value.Date;
            if (eventDate < DateTime.Today)
            {
                DbConnector.ShowWarning("Дата мероприятия не может быть в прошлом!");
                return;
            }
            TimeSpan startTime;
            TimeSpan endTime;
            if (TimeSpan.TryParseExact(txtStart.Text.Trim(), @"h\:mm", CultureInfo.InvariantCulture, out startTime) == false)
            {
                DbConnector.ShowWarning("Время начала введите в формате чч:мм, например 18:00!");
                return;
            }
            if (TimeSpan.TryParseExact(txtEnd.Text.Trim(), @"h\:mm", CultureInfo.InvariantCulture, out endTime) == false)
            {
                DbConnector.ShowWarning("Время окончания введите в формате чч:мм, например 23:00!");
                return;
            }
            if (endTime <= startTime)
            {
                DbConnector.ShowWarning("Время окончания должно быть позже времени начала!");
                return;
            }
            string guestsText = txtGuests.Text.Trim();
            if (DbConnector.IsInteger(guestsText) == false)
            {
                DbConnector.ShowWarning("Количество гостей должно быть целым числом!");
                return;
            }
            int guests = Convert.ToInt32(guestsText);
            if (guests <= 0)
            {
                DbConnector.ShowWarning("Количество гостей должно быть больше нуля!");
                return;
            }
            if (guests > venue.Capacity)
            {
                DbConnector.ShowWarning("Выбранная площадка вмещает не более " + venue.Capacity.ToString() + " гостей!");
                return;
            }
            decimal total = 0;
            if (TryCalculate(out total) == false)
            {
                return;
            }
            MessageBoxResult answer = MessageBox.Show("Предварительная стоимость: " + total.ToString("N0") + " руб. Оформить заявку?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (answer != MessageBoxResult.Yes)
            {
                return;
            }
            int newEventId = 0;
            try
            {
                using (SqlConnection connection = new SqlConnection(DbConnector.ConnectionString))
                {
                    connection.Open();
                    int busyCount = 0;
                    string busyQuery = "SELECT COUNT(*) FROM Events WHERE venue_id = @VenueId AND event_date = @EventDate AND status <> @Cancelled AND start_time < @EndTime AND end_time > @StartTime";
                    using (SqlCommand busyCommand = new SqlCommand(busyQuery, connection))
                    {
                        busyCommand.Parameters.AddWithValue("@VenueId", venue.Id);
                        busyCommand.Parameters.AddWithValue("@EventDate", eventDate);
                        busyCommand.Parameters.AddWithValue("@Cancelled", "Отменено");
                        busyCommand.Parameters.AddWithValue("@StartTime", startTime);
                        busyCommand.Parameters.AddWithValue("@EndTime", endTime);
                        busyCount = Convert.ToInt32(busyCommand.ExecuteScalar());
                    }
                    if (busyCount > 0)
                    {
                        DbConnector.ShowWarning("Эта площадка уже занята в выбранное время. Выберите другое время или площадку!");
                        return;
                    }
                    SqlTransaction transaction = connection.BeginTransaction();
                    try
                    {
                        string eventQuery = "INSERT INTO Events (client_id, event_type_id, venue_id, title, event_date, start_time, end_time, guests_count, status) VALUES (@ClientId, @TypeId, @VenueId, @Title, @EventDate, @StartTime, @EndTime, @Guests, @Status); SELECT CAST(SCOPE_IDENTITY() AS INT);";
                        using (SqlCommand eventCommand = new SqlCommand(eventQuery, connection, transaction))
                        {
                            eventCommand.Parameters.AddWithValue("@ClientId", Session.ClientId);
                            eventCommand.Parameters.AddWithValue("@TypeId", eventType.Id);
                            eventCommand.Parameters.AddWithValue("@VenueId", venue.Id);
                            eventCommand.Parameters.AddWithValue("@Title", title);
                            eventCommand.Parameters.AddWithValue("@EventDate", eventDate);
                            eventCommand.Parameters.AddWithValue("@StartTime", startTime);
                            eventCommand.Parameters.AddWithValue("@EndTime", endTime);
                            eventCommand.Parameters.AddWithValue("@Guests", guests);
                            eventCommand.Parameters.AddWithValue("@Status", "Заявка");
                            newEventId = Convert.ToInt32(eventCommand.ExecuteScalar());
                        }
                        int index = 0;
                        while (index < serviceItems.Count)
                        {
                            OrderServiceItem item = serviceItems[index];
                            if (item.IsSelected == true)
                            {
                                string serviceQuery = "INSERT INTO EventServices (event_id, service_id, quantity) VALUES (@EventId, @ServiceId, @Quantity)";
                                using (SqlCommand serviceCommand = new SqlCommand(serviceQuery, connection, transaction))
                                {
                                    serviceCommand.Parameters.AddWithValue("@EventId", newEventId);
                                    serviceCommand.Parameters.AddWithValue("@ServiceId", item.Id);
                                    serviceCommand.Parameters.AddWithValue("@Quantity", Convert.ToInt32(item.QuantityText.Trim()));
                                    serviceCommand.ExecuteNonQuery();
                                }
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
                DbConnector.ShowError("Не удалось сохранить заявку. Проверьте введённые данные и подключение к базе данных.");
                return;
            }
            catch (Exception ex)
            {
                DbConnector.ShowError("Ошибка при оформлении заявки: " + ex.Message);
                return;
            }
            DbConnector.ShowInfo("Заявка №" + newEventId.ToString() + " успешно оформлена. Менеджер свяжется с вами для подтверждения.");
            NavigationService.Navigate(new OrdersPage());
        }
    }
}