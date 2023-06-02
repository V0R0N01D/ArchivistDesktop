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
using System.Linq;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;

namespace ArchivistsDesktop.View.Archive.Pages
{
    public partial class StudentsPage : UserControl
    {
        private List<StudentsResponse>? _students = new();

        public StudentsPage()
        {
            InitializeComponent();

            LoadRoleFunction();

            InitializeEvent();

            LoadClientData();
        }

        /// <summary>
        /// ������������� �������
        /// </summary>
        private void InitializeEvent()
        {
            BackPage.Click += BackPage_Click;
            Search.Click += SearchOnClick;
        }

        /// <summary>
        /// ������������� ������� ������� ������� �� �����
        /// </summary>
        private void LoadRoleFunction()
        {
            var functionMenu = new List<MenuItem>();
            if (ConnectData.Roles.FirstOrDefault(role => role.Id == 2) is not null)
            {
                var menuItem = new MenuItem()
                {
                    Header = "�������������"
                };
                menuItem.Click += EditMenu_Click;
                functionMenu.Add(menuItem);
            }

            if (ConnectData.Roles.FirstOrDefault(role => role.Id == 3) is not null)
            {
                var menuItem = new MenuItem()
                {
                    Header = "�������"
                };
                menuItem.Click += RemoveMenu_Click;
                functionMenu.Add(menuItem);
            }

            StudentList.ContextMenu = new ContextMenu()
            {
                Items = functionMenu
            };
        }

        /// <summary>
        /// �������� ��������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RemoveMenu_Click(object? sender, RoutedEventArgs e)
        {
            var res = await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
            {
                WindowIcon = UserData.currentWindow!.Icon,
                ContentTitle = "�����������",
                ContentMessage = "�� �������, ��� ������ ������� ���������� ��������?",
                ButtonDefinitions = ButtonEnum.YesNo
            }).ShowDialog(UserData.currentWindow!);

            if (res == ButtonResult.No)
            {
                return;
            }

            // ������ ����������� � api
            var authString = Auth.GetAuth(ConnectData.Login, ConnectData.Password);

            try
            {
                var selectStudent = StudentList.SelectedItem as StudentsResponse;

                using var request = new HttpRequestMessage(HttpMethod.Delete, $"Students/{selectStudent!.Id}");
                request.Headers.Add("AUTH", authString);
                var response = await ConnectData.Client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    await MessageBoxManager.GetMessageBoxStandardWindow("������",
                            $"�������� �� ���������. ���: {response.StatusCode}, ������: {await response.Content.ReadAsStringAsync()}")
                        .ShowDialog(UserData.currentWindow);
                    return;
                }
                LoadClientData();
            }
            catch (Exception ex)
            {
                await MessageBoxManager.GetMessageBoxStandardWindow("������", $"������ ����������: {ex.Message}")
                    .ShowDialog(UserData.currentWindow);
                return;
            }

        }

        /// <summary>
        /// �������������� ��������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditMenu_Click(object? sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// ����� ���������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchOnClick(object? sender, RoutedEventArgs e)
        {
            LoadClientData();
        }

        /// <summary>
        /// ����������� ����������� ����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackPage_Click(object? sender, RoutedEventArgs e)
        {
            UserData.currentWindow!.DisplayBackPage();
        }

        /// <summary>
        /// �������� � ���������� ���������� � ���������
        /// </summary>
        private async void LoadClientData()
        {
            var searchText = SearchInput.Text;

            // �������� �������� ���������� � ���� ������
            if (string.IsNullOrWhiteSpace(searchText))
            {
                searchText = null;
            }

            bool? isStuding = FilterIsStudent.SelectedIndex switch
            {
                2 => false,
                1 => true,
                _ => null
            };

            // ���������� ����������� ������� �� ������ ������
            Search.IsEnabled = false;

            // �������� ��� ������������� �������������������������������������������������������������������������������������������������������������������������������������������������������������������������������������
            if (ConnectData.Login == "")
            {
                return;
            }

            // ������ ����������� � api
            var authString = Auth.GetAuth(ConnectData.Login, ConnectData.Password);

            // ��������� ���������� �� api
            try
            {
                var requestik = "Students".AddOptionalParam("search", searchText).AddOptionalParam("studing", isStuding);
                using var request = new HttpRequestMessage(HttpMethod.Get, requestik);
                request.Headers.Add("AUTH", authString);
                var response = await ConnectData.Client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    await MessageBoxManager.GetMessageBoxStandardWindow("������",
                            $"���: {response.StatusCode}, ������: {await response.Content.ReadAsStringAsync()}")
                        .ShowDialog(UserData.currentWindow);
                    Search.IsEnabled = true;
                    return;
                }

                _students = await response.Content.ReadFromJsonAsync<List<StudentsResponse>>();

                NoResult.IsVisible = _students is { Count: 0 };

                StudentList.Items = _students;
            }
            // �������� ������ ����� � api
            catch (Exception ex)
            {
                await MessageBoxManager.GetMessageBoxStandardWindow("������", $"������ ����������: {ex.Message}")
                    .ShowDialog(UserData.currentWindow);
                return;
            }

            // ��������� ����������� ������� �� ������ ������
            Search.IsEnabled = true;
        }
    }
}