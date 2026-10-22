using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
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
using static PrimeTime.Pages.OrderPage;

namespace PrimeTime.Pages
{
    /// <summary>
    /// Логика взаимодействия для EditOrderPage.xaml
    /// </summary>
    public partial class EditOrderPage : Page
    {
            private int eventId = 0;
            private List<OrderServiceItem> serviceItems = new List<OrderServiceItem>();
            public EditOrderPage(int id)
            {
                InitializeComponent();
                eventId = id;
                txtHeader.Text = "Редактирование заявки №" + id.ToString();
                cbStatus.Items.Add("Заявка");
                cbStatus.Items.Add("Подтверждено");
                cbStatus.Items.Add("Проведено");
                cbStatus.Items.Add("Отменено");
                if (Session.IsStaff() == false)
                {
                    spStatus.Visibility = Visibility.Collapsed;
                    dpDate.DisplayDateStart = DateTime.Today;
                }
                LoadData();
            }
            private void LoadData()
            {
                List<TypeItem> types = new List<TypeItem>();
                List<VenueItem> venues = new List<VenueItem>();
                int currentTypeId = 0;
                int currentVenueId = 0;
                string currentStatus = "Заявка";
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
                        string eventQuery = "SELECT title, event_type_id, venue_id, event_date, start_time, end_time, guests_count, status FROM Events WHERE event_id = @EventId";
                        using (SqlCommand eventCommand = new SqlCommand(eventQuery, connection))
                        {
                            eventCommand.Parameters.AddWithValue("@EventId", eventId);
                            using (SqlDataReader reader = eventCommand.ExecuteReader())
                            {
                                if (reader.Read() == false)
                                {
                                    DbConnector.ShowError("Заявка не найдена. Возможно, она была удалена.");
                                    return;
                                }
                                txtTitle.Text = reader.GetString(0);
                                currentTypeId = reader.GetInt32(1);
                                currentVenueId = reader.GetInt32(2);
                                dpDate.SelectedDate = reader.GetDateTime(3);
                                TimeSpan startTime = (TimeSpan)reader.GetValue(4);
                                TimeSpan endTime = (TimeSpan)reader.GetValue(5);
                                txtStart.Text = startTime.ToString(@"hh\:mm");
                                txtEnd.Text = endTime.ToString(@"hh\:mm");
                                txtGuests.Text = Convert.ToInt32(reader.GetValue(6)).ToString();
                                currentStatus = reader.GetString(7);
                            }
                        }
                        string serviceQuery = "SELECT s.service_id, s.name, s.price, es.quantity FROM Services s LEFT JOIN EventServices es ON es.service_id = s.service_id AND es.event_id = @EventId ORDER BY s.name";
                        using (SqlCommand serviceCommand = new SqlCommand(serviceQuery, connection))
                        {
                            serviceCommand.Parameters.AddWithValue("@EventId", eventId);
                            using (SqlDataReader reader = serviceCommand.ExecuteReader())
                            {
                                while (reader.Read() == true)
                                {
                                    OrderServiceItem item = new OrderServiceItem();
                                    item.Id = reader.GetInt32(0);
                                    item.Name = reader.GetString(1);
                                    item.Price = reader.GetDecimal(2);
                                    if (reader.IsDBNull(3) == true)
                                    {
                                        item.IsSelected = false;
                                        item.QuantityText = "1";
                                    }
                                    else
                                    {
                                        item.IsSelected = true;
                                        item.QuantityText = Convert.ToInt32(reader.GetValue(3)).ToString();
                                    }
                                    serviceItems.Add(item);
                                }
                            }
                        }
                    }
                }
                catch (SqlException)
                {
                    DbConnector.ShowError("Не удалось загрузить заявку. Проверьте подключение к базе данных.");
                    return;
                }
                catch (Exception ex)
                {
                    DbConnector.ShowError("Ошибка при загрузке заявки: " + ex.Message);
                    return;
                }
                cbType.ItemsSource = types;
                cbVenue.ItemsSource = venues;
                icServices.ItemsSource = serviceItems;
                int index = 0;
                while (index < types.Count)
                {
                    if (types[index].Id == currentTypeId)
                    {
                        cbType.SelectedIndex = index;
                    }
                    index = index + 1;
                }
                index = 0;
                while (index < venues.Count)
                {
                    if (venues[index].Id == currentVenueId)
                    {
                        cbVenue.SelectedIndex = index;
                    }
                    index = index + 1;
                }
                index = 0;
                while (index < cbStatus.Items.Count)
                {
                    if (cbStatus.Items[index].ToString() == currentStatus)
                    {
                        cbStatus.SelectedIndex = index;
                    }
                    index = index + 1;
                }
            }
            private bool CheckServices()
            {
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
                        if (Convert.ToInt32(quantityText) <= 0)
                        {
                            DbConnector.ShowWarning("Количество для услуги «" + item.Name + "» должно быть больше нуля!");
                            return false;
                        }
                    }
                    index = index + 1;
                }
                return true;
            }
            private void BtnCancel_Click(object sender, RoutedEventArgs e)
            {
                NavigationService.Navigate(new OrdersPage());
            }
            private void BtnSave_Click(object sender, RoutedEventArgs e)
            {
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
                if (Session.IsStaff() == false && eventDate < DateTime.Today)
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
                if (CheckServices() == false)
                {
                    return;
                }
                string status = "Заявка";
                if (Session.IsStaff() == true)
                {
                    if (cbStatus.SelectedItem == null)
                    {
                        DbConnector.ShowWarning("Выберите статус заявки!");
                        return;
                    }
                    status = cbStatus.SelectedItem.ToString();
                }
                try
                {
                    using (SqlConnection connection = new SqlConnection(DbConnector.ConnectionString))
                    {
                        connection.Open();
                        if (status != "Отменено")
                        {
                            int busyCount = 0;
                            string busyQuery = "SELECT COUNT(*) FROM Events WHERE venue_id = @VenueId AND event_date = @EventDate AND event_id <> @EventId AND status <> @Cancelled AND start_time < @EndTime AND end_time > @StartTime";
                            using (SqlCommand busyCommand = new SqlCommand(busyQuery, connection))
                            {
                                busyCommand.Parameters.AddWithValue("@VenueId", venue.Id);
                                busyCommand.Parameters.AddWithValue("@EventDate", eventDate);
                                busyCommand.Parameters.AddWithValue("@EventId", eventId);
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
                        }
                        int isStaff = 0;
                        if (Session.IsStaff() == true)
                        {
                            isStaff = 1;
                        }
                        string updateQuery = "UPDATE Events SET title = @Title, event_type_id = @TypeId, venue_id = @VenueId, event_date = @EventDate, start_time = @StartTime, end_time = @EndTime, guests_count = @Guests";
                        if (isStaff == 1)
                        {
                            updateQuery = updateQuery + ", status = @Status";
                        }
                        updateQuery = updateQuery + " WHERE event_id = @EventId AND (@IsStaff = 1 OR (client_id = @ClientId AND status = @Initial))";
                        SqlTransaction transaction = connection.BeginTransaction();
                        try
                        {
                            int updatedRows = 0;
                            using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection, transaction))
                            {
                                updateCommand.Parameters.AddWithValue("@Title", title);
                                updateCommand.Parameters.AddWithValue("@TypeId", eventType.Id);
                                updateCommand.Parameters.AddWithValue("@VenueId", venue.Id);
                                updateCommand.Parameters.AddWithValue("@EventDate", eventDate);
                                updateCommand.Parameters.AddWithValue("@StartTime", startTime);
                                updateCommand.Parameters.AddWithValue("@EndTime", endTime);
                                updateCommand.Parameters.AddWithValue("@Guests", guests);
                                updateCommand.Parameters.AddWithValue("@EventId", eventId);
                                updateCommand.Parameters.AddWithValue("@IsStaff", isStaff);
                                updateCommand.Parameters.AddWithValue("@ClientId", Session.ClientId);
                                updateCommand.Parameters.AddWithValue("@Initial", "Заявка");
                                if (isStaff == 1)
                                {
                                    updateCommand.Parameters.AddWithValue("@Status", status);
                                }
                                updatedRows = updateCommand.ExecuteNonQuery();
                            }
                            if (updatedRows == 0)
                            {
                                transaction.Rollback();
                                DbConnector.ShowWarning("Заявку нельзя изменить: она не найдена или у вас нет прав на её редактирование.");
                                return;
                            }
                            using (SqlCommand deleteCommand = new SqlCommand("DELETE FROM EventServices WHERE event_id = @EventId", connection, transaction))
                            {
                                deleteCommand.Parameters.AddWithValue("@EventId", eventId);
                                deleteCommand.ExecuteNonQuery();
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
                                        serviceCommand.Parameters.AddWithValue("@EventId", eventId);
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
                    DbConnector.ShowError("Не удалось сохранить изменения. Проверьте введённые данные и подключение к базе данных.");
                    return;
                }
                catch (Exception ex)
                {
                    DbConnector.ShowError("Ошибка при сохранении заявки: " + ex.Message);
                    return;
                }
                DbConnector.ShowInfo("Изменения сохранены.");
                NavigationService.Navigate(new OrdersPage());
            }
        }
    }
