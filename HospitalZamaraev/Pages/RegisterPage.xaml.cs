using Npgsql;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace HospitalZamaraev.Pages
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

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            var connection = new Connection();
            try
            {
                string username = Username_Box.Text; // Изменили на username
                string login = Login_Box.Text;
                string password = Password_Box.Password;

                // Проверяем, есть ли уже клиент с таким логином
                string checkQueryClient = "SELECT COUNT(*) FROM Clients WHERE login = @login";

                using (var conn = connection.GetConnection())
                {
                    using (var cmd = new NpgsqlCommand(checkQueryClient, conn))
                    {
                        cmd.Parameters.AddWithValue("@login", login);
                        var count = (long)cmd.ExecuteScalar();
                        if (count > 0)
                        {
                            MessageBox.Show("Клиент с таким логином уже существует.");
                            return;
                        }
                    }

                    // Добавляем нового клиента
                    string insertClientQuery = "INSERT INTO Clients (username, login, password_client) VALUES (@username, @login, @password)"; // Изменили на username
                    using (var cmd = new NpgsqlCommand(insertClientQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username); // Изменили на username
                        cmd.Parameters.AddWithValue("@login", login);
                        cmd.Parameters.AddWithValue("@password", password);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Регистрация успешна!");
                    NavigationService.Navigate(new AutorizePage()); // Перенаправляем на страницу авторизации
                }
            }
            catch (Exception ex)
            {
                LogError(ex);
                MessageBox.Show($"Произошла ошибка: {ex.Message}");
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AutorizePage()); // Перенаправляем на страницу авторизации
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
    }
}
