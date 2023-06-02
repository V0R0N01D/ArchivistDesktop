using ArchivistsDesktop.DataClass;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using ArchivistsDesktop.Contracts.ResponseClass;
using System.Linq;
using MessageBox.Avalonia.Models;
using ArchivistsDesktop.View;

namespace ArchivistsDesktop
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            ConnectData.ClearUserData();

            InitializeComponent();

            InitializeEvents();
        }

        /// <summary>
        /// Инициализация событий
        /// </summary>
        private void InitializeEvents()
        {
            Login.Click += Login_Click;

        }

        #region Обработка событий
        /// <summary>
        /// Обработчик нажатия на кнопку авторизации
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Login_Click(object? sender, RoutedEventArgs e)
        {
            // блокировка кнопки авторизации
            Login.IsEnabled = false;

            LoginInput.BorderBrush = new SolidColorBrush(Colors.Black);
            PasswordInput.BorderBrush = new SolidColorBrush(Colors.Black);

            // Получение данных для авторизации
            var login = LoginInput.Text;
            var password = PasswordInput.Text;

            // Проверка логина на пустоту
            if (string.IsNullOrWhiteSpace(login))
            {
                LoginInput.BorderBrush = new SolidColorBrush(Colors.Red);
                await MessageBoxManager.GetMessageBoxStandardWindow("Ошибка", "Поле логин пустое", MessageBox.Avalonia.Enums.ButtonEnum.Ok).ShowDialog(this);
                Login.IsEnabled = true;
                return;
            }

            // Проверка пароля на пустоту
            if (string.IsNullOrWhiteSpace(password))
            {
                PasswordInput.BorderBrush = new SolidColorBrush(Colors.Red);
                await MessageBoxManager.GetMessageBoxStandardWindow("Ошибка", "Поле пароль пустое").ShowDialog(this);
                Login.IsEnabled = true;
                return;
            }

            // Строка авторизации в api
            string authString = Auth.GetAuth(login, password);

            // Получение информации от api
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, "Roles"))
                {
                    request.Headers.Add("AUTH", authString);
                    var response = await ConnectData.Client.SendAsync(request);
                    if (!response.IsSuccessStatusCode)
                    {
                        await MessageBoxManager.GetMessageBoxStandardWindow("Ошибка", $"Код: {response.StatusCode}, ошибка: {await response.Content.ReadAsStringAsync()}").ShowDialog(this);
                        Login.IsEnabled = true;
                        return;
                    }

                    ConnectData.Login = login;
                    ConnectData.Password = password;

                    ConnectData.Roles = await response.Content.ReadFromJsonAsync<List<UserRoleResponse>>();
                }
            }
            // Перехват ошибок связи с api
            catch (Exception ex)
            {
                await MessageBoxManager.GetMessageBoxStandardWindow("Ошибка", $"Ошибка соединения: {ex.Message}").ShowDialog(this);
                Login.IsEnabled = true;
                return;
            }

            // Проверка, что у пользователя есть роли
            if (ConnectData.Roles is null
                && ConnectData.Roles!.Count == 0)
            {
                await MessageBoxManager.GetMessageBoxStandardWindow("Ошибка", "У пользователя отсутствует информация о ролях").ShowDialog(this);
                Login.IsEnabled = true;
                return;
            }

            // Проверка на обновление пароля (next)

            // Проверка на наличие ролей для выбора сферы работы (приемная комиссия и архив)
            if (ConnectData.Roles.FirstOrDefault(role => role.Id == 1) is not null
                && ConnectData.Roles.FirstOrDefault(role => role.Id == 6) is not null)
            {
                // Модальное окно с вопросом, в какую систему хочет зайти пользователь
                var selectSystem = await MessageBoxManager.GetMessageBoxCustomWindow(new MessageBoxCustomParams
                {
                    ContentMessage = "В какую системы вы хотите зайти?",
                    ButtonDefinitions = new[]
                    {
                        new ButtonDefinition
                        {
                            Name = "Приемная комиссия"
                        },
                        new ButtonDefinition
                        {
                            Name = "Архив"
                        }
                    },
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                }).ShowDialog(this);

                // Выбрана система архива
                if (selectSystem == "Архив")
                {
                    UserData.currentWindow = new DefaultWindow(true);
                    UserData.currentWindow.Show();
                    this.Close();
                }
                // Выбрана система приемной комиссии
                else if (selectSystem == "Приемная комиссия")
                {
                    UserData.currentWindow = new DefaultWindow(false);
                    UserData.currentWindow.Show();
                    this.Close();
                }
                else
                {
                    ConnectData.ClearUserData();
                    Login.IsEnabled = true;
                    return;
                }
            }
            // Проверка наличия роли для открытия архива
            else if (ConnectData.Roles.FirstOrDefault(role => role.Id == 1) is not null)
            {
                UserData.currentWindow = new DefaultWindow(true);
                UserData.currentWindow.Show();
                this.Close();
            }
            // Проверка наличия роли для открытия приемной комиссии
            else if (ConnectData.Roles.FirstOrDefault(role => role.Id == 6) is not null)
            {
                UserData.currentWindow = new DefaultWindow(false);
                UserData.currentWindow.Show();
                this.Close();
            }
            else 
            {
                await MessageBoxManager.GetMessageBoxStandardWindow("Ошибка", "У пользователя отсутствует информация о ролях с которыми можно зайти").ShowDialog(this);
            }
        }
        #endregion
    }
}