﻿using Npgsql;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace HospitalZamaraev.Pages
{
    public partial class MyRecordsPage : Page
    {
        private Guid? _selectedAppointmentId; // Для хранения выбранного ID записи

        public MyRecordsPage()
        {
            InitializeComponent();
            LoadRecords(); // Загружаем записи при инициализации страницы
        }

        private void LoadRecords()
        {
            var connection = new Connection();
            using (var conn = connection.GetConnection())
            {
                try
                {
                    conn.Open();

                    
                    

                    // SQL-запрос для получения записей по UserID и имен врачей
                    string query = @"
                        SELECT a.id_appointment, d.username, a.appointment_time
                        FROM Appointments a
                        INNER JOIN Doctors d ON a.id_doctor = d.id_doctor
                        WHERE a.id_client = @id_client 
                        ORDER BY a.appointment_time";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id_client", Session.UserID);

                        using (var adapter = new NpgsqlDataAdapter(cmd))
                        {
                            DataTable recordsTable = new DataTable();
                            adapter.Fill(recordsTable);

                            // Проверяем, были ли получены записи
                            if (recordsTable.Rows.Count > 0)
                            {
                                RecordsDataGrid.ItemsSource = recordsTable.DefaultView; // Заполняем DataGrid
                            }
                            else
                            {
                                MessageBox.Show("Записи не найдены.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки записей: {ex.Message}");
                }
            }
        }

        private void RecordsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RecordsDataGrid.SelectedItem is DataRowView selectedRow)
            {
                // Получаем ID выбранной записи
                _selectedAppointmentId = (Guid)selectedRow["id_appointment"];
            }
            else
            {
                _selectedAppointmentId = null; // Если ничего не выбрано
            }
        }

        private void DeleteRecord_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedAppointmentId == null)
            {
                MessageBox.Show("Пожалуйста, выберите запись для удаления.");
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить эту запись?", "Подтверждение", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                DeleteAppointment((Guid)_selectedAppointmentId);
                LoadRecords(); // Обновляем записи после удаления
            }
        }

        private void DeleteAppointment(Guid appointmentId)
        {
            var connection = new Connection();
            using (var conn = connection.GetConnection())
            {
                try
                {
                    conn.Open();

                    string query = "DELETE FROM Appointments WHERE id_appointment = @id_appointment";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id_appointment", appointmentId);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Запись успешно удалена.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении записи: {ex.Message}");
                }
            }
        }
    }
}
