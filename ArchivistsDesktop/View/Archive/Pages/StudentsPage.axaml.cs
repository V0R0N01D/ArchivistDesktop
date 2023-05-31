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
        /// Èםטצטאכטחאצטÿ סמבûעטי
        /// </summary>
        private void InitializeEvent()
        {
            BackPage.Click += BackPage_Click;
            Search.Click += SearchOnClick;
        }

        /// <summary>
        /// Ïמטסך סעףהוםעמג
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchOnClick(object? sender, RoutedEventArgs e)
        {
            LoadClientData();
        }


        /// <summary>
        /// Îעמבנאזוםטו ןנוהûהףשודמ מךםא
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackPage_Click(object? sender, RoutedEventArgs e)
        {
            UserData.currentWindow!.DisplayBackPage();
        }

        /// <summary>
        /// Çאדנףחךא ט מעבנאזוםטו טםפמנלאצטט מ סעףהוםעאץ
        /// </summary>
        private async void LoadClientData()
        {
            // Ñענמךא אגעמנטחאצטט ג api
            var authString = Auth.GetAuth(ConnectData.Login, ConnectData.Password);

            var searchText = SearchInput.Text;

            // Ïנמגונךא חםאקוםטÿ גגוהוםםמדמ ג ןמכו ןמטסךא
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

            // Îעךכ‏קוםטו גמחלמזםמסעט םאזאעטÿ םא ךםמןךף ןמטסךא
            Search.IsEnabled = false;

            // Ïנמגונךא הכÿ ןנוהןנמסלמענא ףהאכטעüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüüü
            if (ConnectData.Login == "")
            {
                return;
            }

            // Ïמכףקוםטו טםפמנלאצטט מע api
            try
            {
                var requestik = "Students".AddOptionalParam("search", searchText).AddOptionalParam("studing", isStuding);
                using var request = new HttpRequestMessage(HttpMethod.Get, requestik);
                request.Headers.Add("AUTH", authString);
                var response = await ConnectData.Client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    await MessageBoxManager.GetMessageBoxStandardWindow("Îרטבךא",
                            $"Êמה: {response.StatusCode}, מרטבךא: {await response.Content.ReadAsStringAsync()}")
                        .ShowDialog(UserData.currentWindow);
                    Search.IsEnabled = true;
                    return;
                }

                _students = await response.Content.ReadFromJsonAsync<List<StudentsResponse>>();

                NoResult.IsVisible = _students is { Count: 0 };

                StudentList.Items = _students;
            }
            // Ïונוץגאע מרטבמך סגÿחט ס api
            catch (Exception ex)
            {
                await MessageBoxManager.GetMessageBoxStandardWindow("Îרטבךא", $"Îרטבךא סמוהטםוםטÿ: {ex.Message}")
                    .ShowDialog(UserData.currentWindow);
                return;
            }

            // Âךכ‏קוםטו גמחלמזםמסעט םאזאעטÿ םא ךםמןךף ןמטסךא
            Search.IsEnabled = true;
        }
    }
}