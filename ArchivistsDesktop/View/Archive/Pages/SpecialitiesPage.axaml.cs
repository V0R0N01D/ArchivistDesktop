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
using DocumentFormat.OpenXml.Spreadsheet;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;

namespace ArchivistsDesktop.View.Archive.Pages;

public partial class SpecialitiesPage : UserControl
{
    public SpecialitiesPage()
    {
        InitializeComponent();

        InitializeEvents();

        LoadRoleFunction();
    }

    /// <summary>
    /// Загрузка специальностей
    /// </summary>
    private async void LoadSpecialities()
    {
        Search.IsEnabled = false;

        var requestAddres = "Speciality";

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

            var types = await response.Content.ReadFromJsonAsync<List<SpecialityResponse>>();

            NoResult.IsVisible = types is { Count: 0 };

            Specialities.Items = types;
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
        BackPage.Click += BackPage_Click;
        Search.Click += SearchOnClick;
        Specialities.DoubleTapped += SpecialitiesOnDoubleTapped;
    }

    /// <summary>
    /// Инициализация функций которые зависят от ролей
    /// </summary>
    private void LoadRoleFunction()
    {
        if (ConnectData.Roles!.FirstOrDefault(role => role.Id == 4) is not null)
        {
            AddSpeciality.IsVisible = true;
            AddSpeciality.Click += AddGroupOnClick;
        }

        var functionMenu = new List<MenuItem>();
        if (ConnectData.Roles!.FirstOrDefault(role => role.Id == 5) is not null)
        {
            var menuItem = new MenuItem()
            {
                Header = "Редактировать"
            };
            menuItem.Click += EditSpecialityClick;
            functionMenu.Add(menuItem);
        }

        if (ConnectData.Roles!.FirstOrDefault(role => role.Id == 15) is not null)
        {
            var menuItem = new MenuItem()
            {
                Header = "Удалить"
            };
            menuItem.Click += DeleteSpecialityClick;
            functionMenu.Add(menuItem);
        }

        if (functionMenu.Count == 0)
        {
            return;
        }

        Specialities.ContextMenu = new ContextMenu()
        {
            Items = functionMenu
        };
    }

    /// <summary>
    /// Удаление выбранной специальности
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void DeleteSpecialityClick(object? sender, RoutedEventArgs e)
    {
        if (Specialities.SelectedItem is not SpecialityResponse selectSpeciality)
        {
            return;
        }

        var res = await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
        {
            WindowIcon = UserData.currentWindow!.Icon,
            ContentTitle = "Уведомление",
            CanResize = true,
            ContentMessage = "Вы уверены, что хотите удалить выбранную специальность?",
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
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"Speciality/{selectSpeciality.Id}");
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

            LoadSpecialities();
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
    /// Редактирование выбранной специальности
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void EditSpecialityClick(object? sender, RoutedEventArgs e)
    {
        if (Specialities.SelectedItem is not SpecialityResponse selectSpeciality)
        {
            return;
        }

        await new AddEditViewSpeciality(selectSpeciality, false).ShowDialog(UserData.currentWindow);
        LoadSpecialities();
    }

    /// <summary>
    /// Добавление новой специальности
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void AddGroupOnClick(object? sender, RoutedEventArgs e)
    {
        await new AddEditViewSpeciality().ShowDialog(UserData.currentWindow);
        LoadSpecialities();
    }

    /// <summary>
    /// Просмотр данных специальности
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void SpecialitiesOnDoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (Specialities.SelectedItem is not SpecialityResponse selectSpeciality)
        {
            return;
        }

        await new AddEditViewSpeciality(selectSpeciality, true).ShowDialog(UserData.currentWindow);
        LoadSpecialities();
    }

    /// <summary>
    /// Поиск
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SearchOnClick(object? sender, RoutedEventArgs e)
    {
        LoadSpecialities();
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