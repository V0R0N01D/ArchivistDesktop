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
        /// ������������� �������
        /// </summary>
        private void InitializeEvents()
        {
            Login.Click += Login_Click;

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
                await MessageBoxManager.GetMessageBoxStandardWindow("������", "���� ����� ������", MessageBox.Avalonia.Enums.ButtonEnum.Ok).ShowDialog(this);
                Login.IsEnabled = true;
                return;
            }

            // �������� ������ �� �������
            if (string.IsNullOrWhiteSpace(password))
            {
                PasswordInput.BorderBrush = new SolidColorBrush(Colors.Red);
                await MessageBoxManager.GetMessageBoxStandardWindow("������", "���� ������ ������").ShowDialog(this);
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
                        await MessageBoxManager.GetMessageBoxStandardWindow("������", $"���: {response.StatusCode}, ������: {await response.Content.ReadAsStringAsync()}").ShowDialog(this);
                        Login.IsEnabled = true;
                        return;
                    }

                    ConnectData.Login = login;
                    ConnectData.Password = password;

                    ConnectData.Roles = await response.Content.ReadFromJsonAsync<List<UserRoleResponse>>();
                }
            }
            // �������� ������ ����� � api
            catch (Exception ex)
            {
                await MessageBoxManager.GetMessageBoxStandardWindow("������", $"������ ����������: {ex.Message}").ShowDialog(this);
                Login.IsEnabled = true;
                return;
            }

            // ��������, ��� � ������������ ���� ����
            if (ConnectData.Roles is null
                && ConnectData.Roles!.Count == 0)
            {
                await MessageBoxManager.GetMessageBoxStandardWindow("������", "� ������������ ����������� ���������� � �����").ShowDialog(this);
                Login.IsEnabled = true;
                return;
            }

            // �������� �� ���������� ������ (next)

            // �������� �� ������� ����� ��� ������ ����� ������ (�������� �������� � �����)
            if (ConnectData.Roles.FirstOrDefault(role => role.Id == 1) is not null
                && ConnectData.Roles.FirstOrDefault(role => role.Id == 6) is not null)
            {
                // ��������� ���� � ��������, � ����� ������� ����� ����� ������������
                var selectSystem = await MessageBoxManager.GetMessageBoxCustomWindow(new MessageBoxCustomParams
                {
                    ContentMessage = "� ����� ������� �� ������ �����?",
                    ButtonDefinitions = new[]
                    {
                        new ButtonDefinition
                        {
                            Name = "�������� ��������"
                        },
                        new ButtonDefinition
                        {
                            Name = "�����"
                        }
                    },
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                }).ShowDialog(this);

                // ������� ������� ������
                if (selectSystem == "�����")
                {
                    UserData.currentWindow = new DefaultWindow(true);
                    UserData.currentWindow.Show();
                    this.Close();
                }
                // ������� ������� �������� ��������
                else if (selectSystem == "�������� ��������")
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
            // �������� ������� ���� ��� �������� ������
            else if (ConnectData.Roles.FirstOrDefault(role => role.Id == 1) is not null)
            {
                UserData.currentWindow = new DefaultWindow(true);
                UserData.currentWindow.Show();
                this.Close();
            }
            // �������� ������� ���� ��� �������� �������� ��������
            else if (ConnectData.Roles.FirstOrDefault(role => role.Id == 6) is not null)
            {
                UserData.currentWindow = new DefaultWindow(false);
                UserData.currentWindow.Show();
                this.Close();
            }
            else 
            {
                await MessageBoxManager.GetMessageBoxStandardWindow("������", "� ������������ ����������� ���������� � ����� � �������� ����� �����").ShowDialog(this);
            }
        }
        #endregion
    }
}