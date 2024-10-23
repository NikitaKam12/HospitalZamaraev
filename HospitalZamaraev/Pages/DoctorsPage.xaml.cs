using Npgsql;
using System;
using System.Windows;
using System.Windows.Controls;

namespace HospitalZamaraev.Pages
{
    public partial class DoctorsPage : Page
    {
        public DoctorsPage()
        {
            InitializeComponent();
            LoadDoctors();
        }

        private void LoadDoctors()
        {
            var connection = new Connection();

            try
            {
                using (var conn = connection.GetConnection())
                {
                    conn.Open(); // Открываем соединение здесь

                    // Исправленный запрос для одной таблицы Doctors
                    string query = @"SELECT username, specializations FROM Doctors";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string username = reader.GetString(0);        // Получаем имя пользователя
                            string specializations = reader.GetString(1); // Получаем специализацию
                            CreateDoctorButton(username, specializations);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки докторов: {ex.Message}");
            }
        }

        private void CreateDoctorButton(string username, string specializations)
        {
            Button doctorButton = new Button
            {
                Content = $"{username}\nСпециализация: {specializations}",
                Margin = new Thickness(30, 50, 10, 5), // Отступы сверху и снизу
                Width = 300,
                Height = 100,
                Style = (Style)FindResource("RoundButtonStyle"), // Используйте стиль кнопки
                ToolTip = specializations
            };

            DoctorsWrapPanel.Children.Add(doctorButton);
        }

    }
}
