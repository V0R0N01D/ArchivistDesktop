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
using MessageBox.Avalonia.Enums;

using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO;

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
            Settings.Click += SettingsOnClick;
            Login.Click += Login_Click;
        }

        /// <summary>
        /// Настройки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SettingsOnClick(object? sender, RoutedEventArgs e)
        {
            await new SettingsWindow().ShowDialog(this);
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
                await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
                {
                    WindowIcon = this.Icon,
                    CanResize = true,
                    MinWidth = 300,
                    MaxWidth = 1920,
                    MinHeight = 100,
                    MaxHeight = 300,
                    FontFamily = this.FontFamily,
                    ContentTitle = "Ошибка",
                    ContentMessage = "Поле логин пустое",
                    ButtonDefinitions = ButtonEnum.Ok
                }).ShowDialog(this);
                Login.IsEnabled = true;
                return;
            }

            // Проверка пароля на пустоту
            if (string.IsNullOrWhiteSpace(password))
            {
                PasswordInput.BorderBrush = new SolidColorBrush(Colors.Red);
                await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
                {
                    WindowIcon = this.Icon,
                    CanResize = true,
                    MinWidth = 300,
                    MaxWidth = 1920,
                    MinHeight = 100,
                    MaxHeight = 300,
                    FontFamily = this.FontFamily,
                    ContentTitle = "Ошибка",
                    ContentMessage = "Поле пароль пустое",
                    ButtonDefinitions = ButtonEnum.Ok
                }).ShowDialog(this);
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
                        await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
                        {
                            WindowIcon = this.Icon,
                            CanResize = true,
                            MinWidth = 300,
                            MaxWidth = 1920,
                            MinHeight = 100,
                            MaxHeight = 300,
                            FontFamily = this.FontFamily,
                            ContentTitle = "Ошибка",
                            ContentMessage =
                                $"Код: {response.StatusCode}, ошибка: {await response.Content.ReadAsStringAsync()}",
                            ButtonDefinitions = ButtonEnum.Ok
                        }).ShowDialog(this);

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
                await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
                {
                    WindowIcon = this.Icon,
                    CanResize = true,
                    MinWidth = 300,
                    MaxWidth = 1920,
                    MinHeight = 100,
                    MaxHeight = 300,
                    FontFamily = this.FontFamily,
                    ContentTitle = "Ошибка",
                    ContentMessage =
                        $"Ошибка соединения: {ex.Message}",
                    ButtonDefinitions = ButtonEnum.Ok
                }).ShowDialog(this);
                Login.IsEnabled = true;
                return;
            }

            // Проверка на обновление пароля (next)
            
            // Проверка наличия роли для открытия архива
            if (ConnectData.Roles!.FirstOrDefault(role => role.Id == 1) is not null)
            {
                UserData.currentWindow = new DefaultWindow(true);
                UserData.currentWindow.Show();
                this.Close();
            }
            else
            {
                await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
                {
                    WindowIcon = this.Icon,
                    CanResize = true,
                    MinWidth = 300,
                    MaxWidth = 1920,
                    MinHeight = 100,
                    MaxHeight = 300,
                    FontFamily = this.FontFamily,
                    ContentTitle = "Ошибка",
                    ContentMessage =
                        "У пользователя отсутствует информация о ролях с которыми можно зайти",
                    ButtonDefinitions = ButtonEnum.Ok
                }).ShowDialog(this);
            }
            
            Login.IsEnabled = true;
        }
        #endregion
    }
}