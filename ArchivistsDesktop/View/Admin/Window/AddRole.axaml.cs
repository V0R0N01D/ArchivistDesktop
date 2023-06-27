using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using ArchivistsDesktop.Contracts;
using ArchivistsDesktop.Contracts.ResponseClass;
using ArchivistsDesktop.DataClass;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;

namespace ArchivistsDesktop.View.Admin.Window;

public partial class AddRole : Avalonia.Controls.Window
{
    private CanAdd _canAdd = new();
    private RoleResponse _role = new();
    
    public AddRole()
    {
        InitializeComponent();
    }

    public AddRole(ref RoleResponse role, ref CanAdd isAccepted)
    {
        InitializeComponent();

        _role = role;
        _canAdd = isAccepted;
        
        InitializeEvents();
        
        LoadRoles();
    }

    /// <summary>
    /// Загрузка списка ролей
    /// </summary>
    private async void LoadRoles()
    {
        // Строка авторизации в api
        var authString = Auth.GetAuth(ConnectData.Login, ConnectData.Password);

        try
        {
            var requestUri = "Roles/all";
            var search = InputSearch.Text;
            if (!string.IsNullOrWhiteSpace(search))
            {
                requestUri = requestUri.AddOptionalParam("search", search);
            }
            
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
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
                        $"Ошибка. Код: {response.StatusCode}, ошибка: {await response.Content.ReadAsStringAsync()}",
                    ButtonDefinitions = ButtonEnum.Ok
                }).ShowDialog(this);
                return;
            }

            var roles = await response.Content.ReadFromJsonAsync<List<RoleResponse>>();

            Roles.Items = roles;
        }
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
        }
    }
    
    /// <summary>
    /// Инициализация событий
    /// </summary>
    private void InitializeEvents()
    {
        SaveRole.Click += SaveRoleOnClick;
        BackPage.Click += BackPageOnClick;
        Search.Click += SearchOnClick;
    }

    /// <summary>
    /// Поиск ролей
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SearchOnClick(object? sender, RoutedEventArgs e)
    {
        LoadRoles();
    }

    /// <summary>
    /// Сохранение роли
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void SaveRoleOnClick(object? sender, RoutedEventArgs e)
    {
        // Проверка, что выбрана роль
        if (Roles.SelectedItem is not RoleResponse selectRole)
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
                    "Не выбрана роль",
                ButtonDefinitions = ButtonEnum.Ok
            }).ShowDialog(this);
            return;
        }

        _role.Id = selectRole.Id;
        _role.Title = selectRole.Title;
        _role.Description = selectRole.Description;

        _canAdd.Accept();
        
        Close();
    }

    /// <summary>
    /// Переход обратно к пользователю
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void BackPageOnClick(object? sender, RoutedEventArgs e)
    {
        var res = await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
        {
            WindowIcon = this.Icon,
            CanResize = true,
            MinWidth = 300,
            MaxWidth = 1920,
            MinHeight = 100,
            MaxHeight = 300,
            FontFamily = this.FontFamily,
            ContentTitle = "Уведомление",
            ContentMessage =
                "Вы уверены, что хотите закрыть данное окно и перейти обратно к окну пользователя? (Все не сохраненные данные исчезнут)",
            ButtonDefinitions = ButtonEnum.YesNo
        }).ShowDialog(this);

        if (res == ButtonResult.Yes)
        {
            Close();
        }
    }
}