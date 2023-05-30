using ArchivistsDesktop.DataClass;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MessageBox.Avalonia;
using System.Net.Http;
using System;
using System.Net.Http.Json;
using ArchivistAPI.Contracts.ResponseClass;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace ArchivistsDesktop.View.Archive.Pages
{
    public partial class StudentsPage : UserControl
    {
        private List<StudentsResponse>? _students = new(); 

        public StudentsPage()
        {
            InitializeComponent();

            InitializeEvent();

            LoadClientData();
        }

        /// <summary>
        /// Инициализация событий
        /// </summary>
        private void InitializeEvent()
        {
            BackPage.Click += BackPage_Click;
        }


        /// <summary>
        /// Отображение предыдущего окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackPage_Click(object? sender, RoutedEventArgs e)
        {
            UserData.currentWindow!.DisplayBackPage();
        }

        private async void LoadClientData()
        {
            // Строка авторизации в api
            string authString = Auth.GetAuth(ConnectData.Login, ConnectData.Password);

            // findbutton is enabled false

            // Получение информации от api
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, "Students"))
                {
                    request.Headers.Add("AUTH", authString);
                    var response = await ConnectData.Client.SendAsync(request);
                    if (!response.IsSuccessStatusCode)
                    {
                        await MessageBoxManager.GetMessageBoxStandardWindow("Ошибка", $"Код: {response.StatusCode}, ошибка: {await response.Content.ReadAsStringAsync()}").ShowDialog(UserData.currentWindow);
                        return;
                    }

                    _students = await response.Content.ReadFromJsonAsync<List<StudentsResponse>>();

                    StudentList.Items = _students;


                    var k = StudentList.ItemCount;
                }
            }
            // Перехват ошибок связи с api
            catch (Exception ex)
            {
                await MessageBoxManager.GetMessageBoxStandardWindow("Ошибка", $"Ошибка соединения: {ex.Message}").ShowDialog(UserData.currentWindow);
                return;
            }

            

            // findbutton is enabled true
        }
    }
}
