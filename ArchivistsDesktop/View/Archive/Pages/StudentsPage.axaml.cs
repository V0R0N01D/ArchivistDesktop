using ArchivistsDesktop.DataClass;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MessageBox.Avalonia;
using System.Net.Http;
using System;
using System.Net.Http.Json;
using ArchivistsDesktop.Contracts.ResponseClass;
using System.Collections.Generic;
using System.Linq;
using ArchivistsDesktop.View.Archive.Window;
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
        }

        /// <summary>
        /// ������������� �������
        /// </summary>
        private void InitializeEvent()
        {
            BackPage.Click += BackPage_Click;
            Search.Click += SearchOnClick;
            StudentList.DoubleTapped += StudentListOnDoubleTapped;
        }

        /// <summary>
        /// �������� ������ ��������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void StudentListOnDoubleTapped(object? sender, RoutedEventArgs e)
        {
            if (StudentList.SelectedItem is not StudentsResponse selectStudent)
            {
                return;
            }
            
            StudentList.IsEnabled = false;
            // ������ ����������� � api
            var authString = Auth.GetAuth(ConnectData.Login, ConnectData.Password);

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"Students/{selectStudent!.Id}");
                request.Headers.Add("AUTH", authString);
                var response = await ConnectData.Client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
                    {
                        WindowIcon = UserData.currentWindow!.Icon,
                        ContentTitle = "������",
                        CanResize = true,
                        ContentMessage = $"���: {response.StatusCode}, ������: {await response.Content.ReadAsStringAsync()}",
                        ButtonDefinitions = ButtonEnum.Ok
                    }).ShowDialog(UserData.currentWindow!);
                    StudentList.IsEnabled = true;
                    return;
                }
                
                var student = await response.Content.ReadFromJsonAsync<EditStudentResponse>();

                if (student is not null)
                {
                    await new AddEditViewStudentWindow(student, true).ShowDialog(UserData.currentWindow);
                }
            }
            catch (Exception ex)
            {
                await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
                {
                    WindowIcon = UserData.currentWindow!.Icon,
                    ContentTitle = "������",
                    CanResize = true,
                    ContentMessage = $"������ ����������: {ex.Message}",
                    ButtonDefinitions = ButtonEnum.Ok
                }).ShowDialog(UserData.currentWindow!);
            }
            StudentList.IsEnabled = true;
        }

        /// <summary>
        /// ������������� ������� ������� ������� �� �����
        /// </summary>
        private void LoadRoleFunction()
        {
            if (ConnectData.Roles!.FirstOrDefault(role => role.Id == 9) is not null)
            {
                AddStudent.IsVisible = true;
                AddStudent.Click += AddStudentOnClick;
            }
            
            var functionMenu = new List<MenuItem>();
            if (ConnectData.Roles!.FirstOrDefault(role => role.Id == 2) is not null)
            {
                var menuItem = new MenuItem()
                {
                    Header = "�������������"
                };
                menuItem.Click += EditMenu_Click;
                functionMenu.Add(menuItem);
            }

            if (ConnectData.Roles!.FirstOrDefault(role => role.Id == 3) is not null)
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
        /// ���������� ������ ��������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AddStudentOnClick(object? sender, RoutedEventArgs e)
        {
            await new AddEditViewStudentWindow().ShowDialog(UserData.currentWindow);
            LoadStudentData();
        }

        /// <summary>
        /// �������� ��������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RemoveMenu_Click(object? sender, RoutedEventArgs e)
        {
            if (StudentList.SelectedItem is not StudentsResponse selectStudent)
            {
                return;
            }
            
            var res = await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
            {
                WindowIcon = UserData.currentWindow!.Icon,
                ContentTitle = "�����������",
                CanResize = true,
                ContentMessage = "�� �������, ��� ������ ������� ���������� ��������?",
                ButtonDefinitions = ButtonEnum.YesNo
            }).ShowDialog(UserData.currentWindow!);

            if (res != ButtonResult.Yes)
            {
                return;
            }

            // ������ ����������� � api
            var authString = Auth.GetAuth(ConnectData.Login, ConnectData.Password);

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Delete, $"Students/{selectStudent!.Id}");
                request.Headers.Add("AUTH", authString);
                var response = await ConnectData.Client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                { 
                    await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
                    {
                        WindowIcon = UserData.currentWindow!.Icon,
                        ContentTitle = "������",
                        CanResize = true,
                        ContentMessage = $"�������� �� ���������. ���: {response.StatusCode}, ������: {await response.Content.ReadAsStringAsync()}",
                        ButtonDefinitions = ButtonEnum.Ok
                    }).ShowDialog(UserData.currentWindow!);
                    return;
                }
                LoadStudentData();
            }
            catch (Exception ex)
            {
                await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
                {
                    WindowIcon = UserData.currentWindow!.Icon,
                    ContentTitle = "������",
                    CanResize = true,
                    ContentMessage = $"������ ����������: {ex.Message}",
                    ButtonDefinitions = ButtonEnum.Ok
                }).ShowDialog(UserData.currentWindow!);
            }
        }

        /// <summary>
        /// �������������� ��������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void EditMenu_Click(object? sender, RoutedEventArgs e)
        {
            // ������ ����������� � api
            var authString = Auth.GetAuth(ConnectData.Login, ConnectData.Password);

            try
            {
                var selectStudent = StudentList.SelectedItem as StudentsResponse;

                using var request = new HttpRequestMessage(HttpMethod.Get, $"Students/{selectStudent!.Id}");
                request.Headers.Add("AUTH", authString);
                var response = await ConnectData.Client.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
                    {
                        WindowIcon = UserData.currentWindow!.Icon,
                        ContentTitle = "������",
                        CanResize = true,
                        ContentMessage = $"���: {response.StatusCode}, ������: {await response.Content.ReadAsStringAsync()}",
                        ButtonDefinitions = ButtonEnum.Ok
                    }).ShowDialog(UserData.currentWindow!);
                    return;
                }
                
                var student = await response.Content.ReadFromJsonAsync<EditStudentResponse>();

                if (student is not null)
                {
                    await new AddEditViewStudentWindow(student, false).ShowDialog(UserData.currentWindow);
                }

                LoadStudentData();
            }
            catch (Exception ex)
            {
                await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
                {
                    WindowIcon = UserData.currentWindow!.Icon,
                    ContentTitle = "������",
                    CanResize = true,
                    ContentMessage = $"������ ����������: {ex.Message}",
                    ButtonDefinitions = ButtonEnum.Ok
                }).ShowDialog(UserData.currentWindow!);
            }
        }

        /// <summary>
        /// ����� ���������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchOnClick(object? sender, RoutedEventArgs e)
        {
            LoadStudentData();
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
        private async void LoadStudentData()
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
                    await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
                    {
                        WindowIcon = UserData.currentWindow!.Icon,
                        ContentTitle = "������",
                        CanResize = true,
                        ContentMessage = $"���: {response.StatusCode}, ������: {await response.Content.ReadAsStringAsync()}",
                        ButtonDefinitions = ButtonEnum.Ok
                    }).ShowDialog(UserData.currentWindow!);
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
                await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
                {
                    WindowIcon = UserData.currentWindow!.Icon,
                    ContentTitle = "������",
                    CanResize = true,
                    ContentMessage = $"������ ����������: {ex.Message}",
                    ButtonDefinitions = ButtonEnum.Ok
                }).ShowDialog(UserData.currentWindow!);
            }

            // ��������� ����������� ������� �� ������ ������
            Search.IsEnabled = true;
        }
    }
}