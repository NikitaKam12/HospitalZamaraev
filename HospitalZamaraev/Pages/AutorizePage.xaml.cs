using Npgsql;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace HospitalZamaraev.Pages
{
    public partial class AutorizePage : Page
    {
        public AutorizePage()
        {
            InitializeComponent();
        }

        private void Autorize_Click(object sender, RoutedEventArgs e)
        {
            var connection = new Connection();

            try
            {
                string login = Login_Box.Text;
                string password = Psswrd_box.Password;

                // Запросы для проверки в таблицах Doctors и Clients
                string queryDoctor = "SELECT id_doctor FROM Doctors WHERE login = @login AND password_doctor = @password";
                string queryClient = "SELECT id_client FROM Clients WHERE login = @login AND password_client = @password";

                using (var conn = connection.GetConnection())
                {
                    conn.Open(); // Открываем соединение здесь

                    // Проверка на врача
                    using (var cmd = new NpgsqlCommand(queryDoctor, conn))
                    {
                        cmd.Parameters.AddWithValue("@login", login);
                        cmd.Parameters.AddWithValue("@password", password);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var doctorId = reader.GetGuid(0);
                                Session.UserID = doctorId; // Сохраняем id_doctor в сессии
                                NavigationService.Navigate(new DoctorMainPage()); // Переход на страницу врача
                                return;
                            }
                        }
                    }

                    // Проверка на клиента
                    using (var cmd = new NpgsqlCommand(queryClient, conn))
                    {
                        cmd.Parameters.AddWithValue("@login", login);
                        cmd.Parameters.AddWithValue("@password", password);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var clientId = reader.GetGuid(0);
                                Session.UserID = clientId; // Сохраняем id_client в сессии
                                NavigationService.Navigate(new ClientMainPage()); // Переход на страницу клиента
                                return;
                            }
                        }
                    }
                }

                MessageBox.Show("Неверный логин или пароль.");
            }
            catch (Exception ex)
            {
                LogError(ex);
                MessageBox.Show($"Произошла ошибка: {ex.Message}");
            }
        }


        private void LogError(Exception ex)
        {
            try
            {
                string directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Errors");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, $"Error_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                File.WriteAllText(filePath, $"Date: {DateTime.Now}\nException: {ex}\nStackTrace: {ex.StackTrace}");
            }
            catch (Exception logEx)
            {
                MessageBox.Show($"Не удалось записать информацию об ошибке в файл: {logEx.Message}");
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            // Переход на страницу регистрации
            NavigationService.Navigate(new RegisterPage()); // Создайте эту страницу
        }
    }
}
