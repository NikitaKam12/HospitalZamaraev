using Npgsql;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HospitalZamaraev.Pages
{
    public partial class BookPage : Page
    {
        private Guid _doctorId; // Идентификатор выбранного врача
        private Guid _clientId; // Идентификатор клиента (Session.UserID)

        public BookPage()
        {
            InitializeComponent();
            InitializeTimePicker();
            LoadDoctors(); // Загружаем список врачей из базы данных
        }

        // Инициализация выбора времени с 9:00 до 21:00
        private void InitializeTimePicker()
        {
            for (int hour = 9; hour <= 21; hour++)
            {
                TimePicker.Items.Add(new ComboBoxItem { Content = $"{hour:D2}:00" });
            }
        }

        // Загрузка списка врачей
        private void LoadDoctors()
        {
            var connection = new Connection();
            try
            {
                using (var conn = connection.GetConnection())
                {
                    conn.Open();
                    string query = "SELECT id_doctor, username FROM Doctors";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Guid doctorId = reader.GetGuid(0);
                            string username = reader.GetString(1);

                            ComboBoxItem item = new ComboBoxItem
                            {
                                Content = username,
                                Tag = doctorId
                            };
                            DoctorPicker.Items.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки списка врачей: {ex.Message}");
            }
        }

        // Обработка изменения выбранной даты
        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine("Дата изменена.");
            UpdateAvailableTimeSlots();
        }


        // Обработка изменения выбранного врача
        private void DoctorPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DoctorPicker.SelectedItem is ComboBoxItem selectedItem)
            {
                _doctorId = (Guid)selectedItem.Tag;
                UpdateAvailableTimeSlots(); // Обновляем временные слоты при выборе врача
            }
        }

        // Обновление доступных временных интервалов
        private void UpdateAvailableTimeSlots()
        {
            if (DatePicker.SelectedDate == null || _doctorId == Guid.Empty)
            {
                Console.WriteLine("Дата или врач не выбраны. Выход.");
                return; // Выход, если дата или врач не выбраны
            }

            Console.WriteLine("Обновление доступных временных слотов...");

            var connection = new Connection();
            using (var conn = connection.GetConnection())
            {
                conn.Open();

                // Получаем все занятые временные интервалы для выбранного врача на выбранную дату
                string query = @"
            SELECT appointment_time
            FROM Appointments
            WHERE id_doctor = @id_doctor
            AND appointment_time::date = @appointment_date";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id_doctor", _doctorId);
                    cmd.Parameters.AddWithValue("@appointment_date", DatePicker.SelectedDate.Value.Date);

                    using (var reader = cmd.ExecuteReader())
                    {
                        var busyTimes = new System.Collections.Generic.List<DateTime>();
                        while (reader.Read())
                        {
                            busyTimes.Add(reader.GetDateTime(0)); // Сохраняем все занятые временные интервалы
                        }

                        // Логирование занятых временных интервалов
                        Console.WriteLine("Занятые временные интервалы:");
                        foreach (var busyTime in busyTimes)
                        {
                            Console.WriteLine(busyTime);
                        }

                        // Очищаем ComboBox
                        TimePicker.Items.Clear();

                        // Добавляем временные интервалы с 9:00 до 21:00
                        for (int hour = 9; hour <= 21; hour++)
                        {
                            DateTime timeSlot = new DateTime(DatePicker.SelectedDate.Value.Year,
                                                              DatePicker.SelectedDate.Value.Month,
                                                              DatePicker.SelectedDate.Value.Day,
                                                              hour, 0, 0);

                            if (!busyTimes.Any(busyTime => busyTime == timeSlot)) // Если время не занято
                            {
                                TimePicker.Items.Add(new ComboBoxItem { Content = $"{hour:D2}:00" });
                                Console.WriteLine($"Добавлено свободное время: {timeSlot}");
                            }
                        }
                    }
                }
            }
        }



        // Кнопка для записи на прием
        private void BookAppointment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем, выбраны ли врач, дата и время
                if (DatePicker.SelectedDate == null || TimePicker.SelectedItem == null || DoctorPicker.SelectedItem == null)
                {
                    MessageBox.Show("Пожалуйста, выберите врача, дату и время для записи.");
                    return;
                }

                // Явно задаем дату и время
                DateTime appointmentDate = DatePicker.SelectedDate.Value.Date.Add(TimeSpan.Parse((TimePicker.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "00:00"));

                // Записываем запись с использованием явных данных
                BookAppointment(appointmentDate);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при записи: {ex.Message}");
            }
        }

        // Записываем запись в базу данных
        private void BookAppointment(DateTime appointmentDate)
        {
            var connection = new Connection();
            using (var conn = connection.GetConnection())
            {
                conn.Open();

                string query = @"
                    INSERT INTO Appointments (id_appointment, id_doctor, id_client, appointment_time) 
                    VALUES (gen_random_uuid(), @id_doctor, @id_client, @appointment_time)";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id_doctor", _doctorId);
                    cmd.Parameters.AddWithValue("@id_client", Session.UserID); // Здесь используем ID клиента
                    cmd.Parameters.AddWithValue("@appointment_time", appointmentDate);

                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Вы успешно записались на прием.");
        }
    }
}
