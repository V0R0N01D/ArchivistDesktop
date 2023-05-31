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
        /// ������������� �������
        /// </summary>
        private void InitializeEvent()
        {
            BackPage.Click += BackPage_Click;
            Search.Click += SearchOnClick;
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
            // ������ ����������� � api
            var authString = Auth.GetAuth(ConnectData.Login, ConnectData.Password);

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