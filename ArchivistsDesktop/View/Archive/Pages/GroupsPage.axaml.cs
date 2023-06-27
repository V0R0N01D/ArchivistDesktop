using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using ArchivistsDesktop.Contracts.ResponseClass;
using ArchivistsDesktop.DataClass;
using ArchivistsDesktop.View.Archive.Window;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;

namespace ArchivistsDesktop.View.Archive.Pages;

public partial class GroupsPage : UserControl
{
    public GroupsPage()
    {
        InitializeComponent();

        InitializeEvents();

        LoadRoleFunction();
    }

    /// <summary>
    /// Загрузка групп
    /// </summary>
    private async void LoadGroups()
    {
        Search.IsEnabled = false;

        var requestAddres = "Groups";

        var search = SearchInput.Text;

        if (!string.IsNullOrWhiteSpace(search))
        {
            requestAddres = requestAddres.AddOptionalParam("search", search);
        }

        if (FilterYear.SelectedItem is not null && FilterYear.SelectedIndex != 0)
        {
            requestAddres =
                requestAddres.AddOptionalParam("year", FilterYear.SelectedIndex);
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
                    WindowIcon = UserData.currentWindow.Icon,
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

            var types = await response.Content.ReadFromJsonAsync<List<GroupResponse>>();

            NoResult.IsVisible = types is { Count: 0 };
            
            Groups.Items = types;
        }
        catch (Exception ex)
        {
            await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
            {
                WindowIcon = UserData.currentWindow.Icon,
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
        BackPage.Click += BackPage_Click;
        Search.Click += SearchOnClick;
        Groups.DoubleTapped += GroupsOnDoubleTapped;
    }

    /// <summary>
    /// Инициализация функций которые зависят от ролей
    /// </summary>
    private void LoadRoleFunction()
    {
        if (ConnectData.Roles!.FirstOrDefault(role => role.Id == 6) is not null)
        {
            AddGroup.IsVisible = true;
            AddGroup.Click += AddGroupOnClick;
        }

        var functionMenu = new List<MenuItem>();
        if (ConnectData.Roles!.FirstOrDefault(role => role.Id == 7) is not null)
        {
            var menuItem = new MenuItem()
            {
                Header = "Редактировать"
            };
            menuItem.Click += EditGroupClick;
            functionMenu.Add(menuItem);
        }

        if (ConnectData.Roles!.FirstOrDefault(role => role.Id == 10) is not null)
        {
            var menuItem = new MenuItem()
            {
                Header = "Удалить"
            };
            menuItem.Click += DeleteGroupClick;
            functionMenu.Add(menuItem);
        }

        if (functionMenu.Count == 0)
        {
            return;
        }

        Groups.ContextMenu = new ContextMenu()
        {
            Items = functionMenu
        };
    }

    /// <summary>
    /// Удаление выбранной группы
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void DeleteGroupClick(object? sender, RoutedEventArgs e)
    {
        if (Groups.SelectedItem is not GroupResponse selectGroup)
        {
            return;
        }

        var res = await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
        {
            WindowIcon = UserData.currentWindow!.Icon,
            ContentTitle = "Уведомление",
            CanResize = true,
            ContentMessage = "Вы уверены, что хотите удалить выбранную группу?",
            ButtonDefinitions = ButtonEnum.YesNo
        }).ShowDialog(UserData.currentWindow!);

        if (res != ButtonResult.Yes)
        {
            return;
        }

        // Строка авторизации в api
        var authString = Auth.GetAuth(ConnectData.Login, ConnectData.Password);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"Groups/{selectGroup.Id}");
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
                        $"Удаление не выполнено. Код: {response.StatusCode}, ошибка: {await response.Content.ReadAsStringAsync()}",
                    ButtonDefinitions = ButtonEnum.Ok
                }).ShowDialog(UserData.currentWindow!);
                return;
            }

            LoadGroups();
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
    }

    /// <summary>
    /// Редактирование выбранной группы
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void EditGroupClick(object? sender, RoutedEventArgs e)
    {
        if (Groups.SelectedItem is not GroupResponse selectGroup)
        {
            return;
        }

        await new AddEditViewGroup(selectGroup, false).ShowDialog(UserData.currentWindow);
        LoadGroups();
    }

    /// <summary>
    /// Добавление новой группы
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void AddGroupOnClick(object? sender, RoutedEventArgs e)
    {
        await new AddEditViewGroup().ShowDialog(UserData.currentWindow);
        LoadGroups();
    }

    /// <summary>
    /// Просмотр данных группы
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void GroupsOnDoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (Groups.SelectedItem is not GroupResponse selectGroup)
        {
            return;
        }

        await new AddEditViewGroup(selectGroup, true).ShowDialog(UserData.currentWindow);
        LoadGroups();
    }

    /// <summary>
    /// Поиск
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SearchOnClick(object? sender, RoutedEventArgs e)
    {
        LoadGroups();
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
}