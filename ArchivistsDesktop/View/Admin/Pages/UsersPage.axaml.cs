using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using ArchivistsDesktop.Contracts.ResponseClass;
using ArchivistsDesktop.DataClass;
using ArchivistsDesktop.View.Admin.Window;
using ArchivistsDesktop.View.Archive.Window;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DocumentFormat.OpenXml.Spreadsheet;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;

namespace ArchivistsDesktop.View.Admin.Pages;

public partial class UsersPage : UserControl
{
    public UsersPage()
    {
        InitializeComponent();

        InitializeEvents();

        LoadRoleFunction();
    }

    /// <summary>
    /// Загрузка пользователей
    /// </summary>
    private async void LoadUsers()
    {
        Search.IsEnabled = false;

        var requestAddres = "Users";

        var search = SearchInput.Text;

        if (!string.IsNullOrWhiteSpace(search))
        {
            requestAddres = requestAddres.AddOptionalParam("search", search);
        }

        // Строка авторизации в api
        var authString = Auth.GetAuth(ConnectData.Login, ConnectData.Password);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, requestAddres);
            request.Headers.Add("AUTH", authString);
            var response = await ConnectData.Client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
                {
                    WindowIcon = UserData.currentWindow!.Icon,
                    CanResize = true,
                    MinWidth = 300,
                    MaxWidth = 1920,
                    MinHeight = 100,
                    MaxHeight = 300,
                    FontFamily = this.FontFamily,
                    ContentTitle = "Ошибка",
                    ContentMessage =
                        $"Ошибка. Код: {response.StatusCode}, ошибка: {await response.Content.ReadAsStringAsync()}",
                    ButtonDefinitions = ButtonEnum.Ok
                }).ShowDialog(UserData.currentWindow);
                Search.IsEnabled = true;
                return;
            }

            var users = await response.Content.ReadFromJsonAsync<List<UserAllDataResponse>>();

            NoResult.IsVisible = users is { Count: 0 };

            Users.Items = users;
        }
        catch (Exception ex)
        {
            await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
            {
                WindowIcon = UserData.currentWindow!.Icon,
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
            }).ShowDialog(UserData.currentWindow);
        }

        Search.IsEnabled = true;
    }

    /// <summary>
    /// Инициализация событий
    /// </summary>
    private void InitializeEvents()
    {
        Search.Click += SearchOnClick;
        Users.DoubleTapped += UsersOnDoubleTapped;
    }

    /// <summary>
    /// Инициализация функций которые зависят от ролей
    /// </summary>
    private void LoadRoleFunction()
    {
        if (ConnectData.Roles!.FirstOrDefault(role => role.Id == 15001) is not null)
        {
            AddUser.IsVisible = true;
            AddUser.Click += AddUserOnClick;
        }

        var functionMenu = new List<MenuItem>();
        if (ConnectData.Roles!.FirstOrDefault(role => role.Id == 15002) is not null)
        {
            var menuItem = new MenuItem()
            {
                Header = "Редактировать"
            };
            menuItem.Click += EditUserClick;
            functionMenu.Add(menuItem);
        }

        if (functionMenu.Count == 0)
        {
            return;
        }

        Users.ContextMenu = new ContextMenu()
        {
            Items = functionMenu
        };
    }

    /// <summary>
    /// Редактирование выбранного пользователя
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void EditUserClick(object? sender, RoutedEventArgs e)
    {
        if (Users.SelectedItem is not UserAllDataResponse selectUser)
        {
            return;
        }
        
        Users.IsEnabled = false;
        // Строка авторизации в api
        var authString = Auth.GetAuth(ConnectData.Login, ConnectData.Password);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"Users/{selectUser.Id}");
            request.Headers.Add("AUTH", authString);
            var response = await ConnectData.Client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
                {
                    WindowIcon = UserData.currentWindow!.Icon,
                    ContentTitle = "Ошибка",
                    CanResize = true,
                    ContentMessage =
                        $"Код: {response.StatusCode}, ошибка: {await response.Content.ReadAsStringAsync()}",
                    ButtonDefinitions = ButtonEnum.Ok
                }).ShowDialog(UserData.currentWindow!);
                Users.IsEnabled = true;
                return;
            }

            var user = await response.Content.ReadFromJsonAsync<EditUserResponse>();

            if (user is not null)
            {
                await new AddEditViewUserWindow(user, false).ShowDialog(UserData.currentWindow);
                LoadUsers();
            }
        }
        catch (Exception ex)
        {
            await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
            {
                WindowIcon = UserData.currentWindow!.Icon,
                ContentTitle = "Ошибка",
                CanResize = true,
                ContentMessage = $"Ошибка соединения: {ex.Message}",
                ButtonDefinitions = ButtonEnum.Ok
            }).ShowDialog(UserData.currentWindow!);
        }
        Users.IsEnabled = true;
    }

    /// <summary>
    /// Добавление нового пользователя
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void AddUserOnClick(object? sender, RoutedEventArgs e)
    {
        await new AddEditViewUserWindow().ShowDialog(UserData.currentWindow);
        LoadUsers();
    }

    /// <summary>
    /// Просмотр данных пользователя
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void UsersOnDoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (Users.SelectedItem is not UserAllDataResponse selectUser)
        {
            return;
        }
        
        Users.IsEnabled = false;
        // Строка авторизации в api
        var authString = Auth.GetAuth(ConnectData.Login, ConnectData.Password);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"Users/{selectUser.Id}");
            request.Headers.Add("AUTH", authString);
            var response = await ConnectData.Client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
                {
                    WindowIcon = UserData.currentWindow!.Icon,
                    ContentTitle = "Ошибка",
                    CanResize = true,
                    ContentMessage =
                        $"Код: {response.StatusCode}, ошибка: {await response.Content.ReadAsStringAsync()}",
                    ButtonDefinitions = ButtonEnum.Ok
                }).ShowDialog(UserData.currentWindow!);
                Users.IsEnabled = true;
                return;
            }

            var user = await response.Content.ReadFromJsonAsync<EditUserResponse>();

            if (user is not null)
            {
                await new AddEditViewUserWindow(user, true).ShowDialog(UserData.currentWindow);
                LoadUsers();
            }
        }
        catch (Exception ex)
        {
            await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
            {
                WindowIcon = UserData.currentWindow!.Icon,
                ContentTitle = "Ошибка",
                CanResize = true,
                ContentMessage = $"Ошибка соединения: {ex.Message}",
                ButtonDefinitions = ButtonEnum.Ok
            }).ShowDialog(UserData.currentWindow!);
        }
        Users.IsEnabled = true;
    }

    /// <summary>
    /// Поиск
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SearchOnClick(object? sender, RoutedEventArgs e)
    {
        LoadUsers();
    }
}