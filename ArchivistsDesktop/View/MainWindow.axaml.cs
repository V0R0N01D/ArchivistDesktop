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
        /// ������������� �������
        /// </summary>
        private void InitializeEvents()
        {
            Settings.Click += SettingsOnClick;
            Login.Click += Login_Click;
        }

        /// <summary>
        /// ���������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SettingsOnClick(object? sender, RoutedEventArgs e)
        {
            await new SettingsWindow().ShowDialog(this);
        }

        #region ��������� �������

        /// <summary>
        /// ���������� ������� �� ������ �����������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Login_Click(object? sender, RoutedEventArgs e)
        {
            // ���������� ������ �����������
            Login.IsEnabled = false;

            LoginInput.BorderBrush = new SolidColorBrush(Colors.Black);
            PasswordInput.BorderBrush = new SolidColorBrush(Colors.Black);

            // ��������� ������ ��� �����������
            var login = LoginInput.Text;
            var password = PasswordInput.Text;

            // �������� ������ �� �������
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
                    ContentTitle = "������",
                    ContentMessage = "���� ����� ������",
                    ButtonDefinitions = ButtonEnum.Ok
                }).ShowDialog(this);
                Login.IsEnabled = true;
                return;
            }

            // �������� ������ �� �������
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
                    ContentTitle = "������",
                    ContentMessage = "���� ������ ������",
                    ButtonDefinitions = ButtonEnum.Ok
                }).ShowDialog(this);
                Login.IsEnabled = true;
                return;
            }

            // ������ ����������� � api
            string authString = Auth.GetAuth(login, password);

            // ��������� ���������� �� api
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
                            ContentTitle = "������",
                            ContentMessage =
                                $"���: {response.StatusCode}, ������: {await response.Content.ReadAsStringAsync()}",
                            ButtonDefinitions = ButtonEnum.Ok
                        }).ShowDialog(this);

                        Login.IsEnabled = true;
                        return;
                    }

                    ConnectData.Login = login;
                    ConnectData.Password = password;

                    ConnectData.Roles = await response.Content.ReadFromJsonAsync<List<RoleResponse>>();
                }
            }
            // �������� ������ ����� � api
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
                    ContentTitle = "������",
                    ContentMessage =
                        $"������ ����������: {ex.Message}",
                    ButtonDefinitions = ButtonEnum.Ok
                }).ShowDialog(this);
                Login.IsEnabled = true;
                return;
            }
            
            // �������� ������� ���� ��� �������� ���� �����������������
            if (ConnectData.Roles!.FirstOrDefault(role => role.Id == 15000) is not null)
            {
                UserData.currentWindow = new DefaultWindow(false);
                UserData.currentWindow.Show();
                this.Close();
            }
            // �������� ������� ���� ��� �������� ������
            else if (ConnectData.Roles!.FirstOrDefault(role => role.Id == 1) is not null)
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
                    ContentTitle = "������",
                    ContentMessage =
                        "� ������������ ����������� ���������� � ����� � �������� ����� �����",
                    ButtonDefinitions = ButtonEnum.Ok
                }).ShowDialog(this);
            }
            
            Login.IsEnabled = true;
        }
        #endregion
    }
}