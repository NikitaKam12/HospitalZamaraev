using Npgsql;
using System;
using System.Windows;
using System.Windows.Controls;

namespace HospitalZamaraev.Pages
{
    public partial class BookPage : Page
    {
        private Guid _doctorId;  // Идентификатор выбранного врача

        public BookPage()
        {
            InitializeComponent();
            InitializeTimePicker();
            LoadDoctors();  // Загружаем список врачей из базы данных
        }

        // Инициализация выбора времени с 9:00 до 21:00
        private void InitializeTimePicker()
        {
            for (int hour = 9; hour <= 21; hour++)
            {
                TimePicker.Items.Add(new ComboBoxItem { Content = new DateTime(1, 1, 1, hour, 0, 0).ToString("HH:mm") });
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
                    string query = "SELECT id_doctor, username, specializations FROM Doctors";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Guid doctorId = reader.GetGuid(0);
                            string doctorName = reader.GetString(1);
                            string specializations = reader.GetString(2);

                            ComboBoxItem item = new ComboBoxItem
                            {
                                Content = $"{doctorName} ({specializations})",
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

        // Обработка изменения выбранного врача
        private void DoctorPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DoctorPicker.SelectedItem is ComboBoxItem selectedItem)
            {
                _doctorId = (Guid)selectedItem.Tag;
                DoctorInfo.Text = selectedItem.Content.ToString();
            }
        }

        // Кнопка для записи на прием
        private void BookAppointment_Click(object sender, RoutedEventArgs e)
        {
            var connection = new Connection();

            try
            {
                if (DoctorPicker.SelectedItem == null || DatePicker.SelectedDate == null || TimePicker.SelectedItem == null)
                {
                    MessageBox.Show("Пожалуйста, выберите врача, дату и время для записи.");
                    return;
                }

                DateTime selectedTime = DateTime.Parse((TimePicker.SelectedItem as ComboBoxItem).Content.ToString());
                DateTime appointmentDate = DatePicker.SelectedDate.Value.Date.Add(selectedTime.TimeOfDay);

                // Проверка времени с 9:00 до 21:00
                if (appointmentDate.Hour < 9 || appointmentDate.Hour > 21)
                {
                    MessageBox.Show("Пожалуйста, выберите время между 9:00 и 21:00.");
                    return;
                }

                // Проверяем доступно ли время
                if (IsTimeSlotAvailable(appointmentDate))
                {
                    // Записываем запись
                    BookAppointment(appointmentDate);
                }
                else
                {
                    MessageBox.Show("Выбранное время уже занято. Пожалуйста, выберите другое время.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при записи: {ex.Message}");
            }
        }

        // Проверка доступности времени для записи
        private bool IsTimeSlotAvailable(DateTime appointmentDate)
        {
            var connection = new Connection();
            using (var conn = connection.GetConnection())
            {
                conn.Open();

                DateTime start_time = appointmentDate;
                DateTime end_time = appointmentDate.AddHours(1);

                string query = @"SELECT COUNT(*)
                                 FROM Appointments
                                 WHERE id_doctor = @id_doctor
                                 AND appointment_time >= @start_time
                                 AND appointment_time < @end_time";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id_doctor", _doctorId);
                    cmd.Parameters.AddWithValue("@start_time", start_time);
                    cmd.Parameters.AddWithValue("@end_time", end_time);

                    int count = (int)cmd.ExecuteScalar();
                    return count == 0;
                }
            }
        }

        // Записываем запись в базу данных
        private void BookAppointment(DateTime appointmentDate)
        {
            var connection = new Connection();
            using (var conn = connection.GetConnection())
            {
                conn.Open();

                string query = @"INSERT INTO Appointments (id_appointment, id_doctor, id_client, appointment_time) 
                                 VALUES (gen_random_uuid(), @id_doctor, @id_client, @appointment_time)";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id_doctor", _doctorId);
                    cmd.Parameters.AddWithValue("@id_client", Session.UserID); // Используем Session.UserID
                    cmd.Parameters.AddWithValue("@appointment_time", appointmentDate);

                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Вы успешно записались на прием.");
        }
    }
}
