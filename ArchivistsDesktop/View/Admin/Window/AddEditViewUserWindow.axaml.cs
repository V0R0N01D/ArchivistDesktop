using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ArchivistsDesktop.Contracts;
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
using MessageBox.Avalonia.Models;

namespace ArchivistsDesktop.View.Admin.Window;

public partial class AddEditViewUserWindow : Avalonia.Controls.Window
{
    private EditUserResponse _currentUser = new()
    {
        User = new UserAllDataResponse()
    };

    // true - добавление, false - редактирование, null - просмотр
    private bool? _isAddEditView = true;

    public AddEditViewUserWindow()
    {
        InitializeComponent();

        LoadUserDataToField();

        InitializeEvents();
    }

    public AddEditViewUserWindow(EditUserResponse selectUser, bool isView)
    {
        InitializeComponent();

        _isAddEditView = isView ? null : false;

        _currentUser = selectUser;

        LoadUserDataToField();

        InitializeEvents();
    }

    /// <summary>
    /// Инициализация событий
    /// </summary>
    private void InitializeEvents()
    {
        BackPage.Click += BackPageOnClick;

        if (_isAddEditView is null)
        {
            Title = "Просмотр данных пользователя";
            MainText.Text = "Просмотр данных пользователя";
            
            ChangePassword.IsVisible = false;
            AddRole.IsVisible = false;
            SaveUser.IsVisible = false;

            LastName.IsReadOnly = true;
            FirstName.IsReadOnly = true;
            Login.IsReadOnly = true;

            return;
        }
        
        ChangePassword.Click += ChangePasswordOnClick;
        AddRole.Click += AddRoleOnClick;

        var menItem = new MenuItem()
        {
            Header = "Удалить",
        };
        menItem.Click += RemoveRoleOnClick;
        Roles.ContextMenu = new ContextMenu()
        {
            Items = new List<MenuItem>()
            {
                menItem
            }
        };


        if (_isAddEditView is true)
        {
            SaveUser.Click += AddUserOnClick;
            return;
        }

        Title = "Редактирование данных пользователя";
        MainText.Text = "Редактирование данных пользователя";
        SaveUser.Content = "Редактировать";

        SaveUser.Click += SaveUserOnClick;
    }

    /// <summary>
    /// Изменение пароля
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ChangePasswordOnClick(object? sender, RoutedEventArgs e)
    {
        if (_currentUser.User.Password is not null || _isAddEditView is false)
        {
            var res = await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
            {
                WindowIcon = this.Icon,
                ContentTitle = "Уведомление",
                CanResize = true,
                ContentMessage = "Вы уверены, что хотите изменить пароль пользователя?",
                ButtonDefinitions = ButtonEnum.YesNo
            }).ShowDialog(this);
            if (res != ButtonResult.Yes)
            {
                return;
            }
        }
        var chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
        var random = new Random();
        var password = "";
        for (var i = 0; i < 8; i++)
        {
            // Выбираем случайный символ из массива
            var c = chars[random.Next(chars.Length)];
            // Добавляем его к паролю
            password += c;
        }
        await MessageBoxManager.GetMessageBoxCustomWindow(new MessageBoxInputParams()
        {
            WindowIcon = this.Icon,
            ContentTitle = "Пароль:",
            ContentHeader = "Пароль",
            CanResize = true,
            MaxHeight = 150,
            Width = 250,
            MaxWidth = 250,
            ContentMessage = password
        }).ShowDialog(this);
        _currentUser.User.Password = password;
    }

    /// <summary>
    /// Удаление роли
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void RemoveRoleOnClick(object? sender, RoutedEventArgs e)
    {
        if (Roles.SelectedItem is not UserRoleResponse selectRole)
        {
            return;
        }

        var res = await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
        {
            WindowIcon = UserData.currentWindow!.Icon,
            ContentTitle = "Уведомление",
            CanResize = true,
            ContentMessage = "Вы уверены, что хотите удалить выбранную роль?",
            ButtonDefinitions = ButtonEnum.YesNo
        }).ShowDialog(UserData.currentWindow!);

        if (res != ButtonResult.Yes)
        {
            return;
        }

        _currentUser.UserRoles!.Remove(selectRole);
        Roles.Items = _currentUser.UserRoles.ToList();
        
        if (selectRole.Id is null)
        {
            return;
        }

        _currentUser.RemoveRoles ??= new();
        _currentUser.RemoveRoles.Add((int)selectRole.Id);
    }

    /// <summary>
    /// Добавление роли к пользователю
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void AddRoleOnClick(object? sender, RoutedEventArgs e)
    {
        // проверка, что роль сохранили
        CanAdd isAcceptedRoleAdd = new();
        
        var selectRoleAdd = new RoleResponse();
        
        await new AddRole(ref selectRoleAdd, ref isAcceptedRoleAdd).ShowDialog(this);
        
        if (isAcceptedRoleAdd.IsAccepted)
        {
            _currentUser.UserRoles ??= new();
            _currentUser.UserRoles.Add(new UserRoleResponse()
            {
                Role = selectRoleAdd
            });

            Roles.Items = _currentUser.UserRoles.ToList();
        }
    }

    /// <summary>
    /// Переход обратно к просмотру пользователей
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void BackPageOnClick(object? sender, RoutedEventArgs e)
    {
        if (_isAddEditView == null)
        {
            this.Close();
            return;
        }

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
                "Вы уверены, что хотите закрыть данное окно и перейти обратно к просмотру пользователей? (Все не сохраненные данные исчезнут)",
            ButtonDefinitions = ButtonEnum.YesNo
        }).ShowDialog(this);

        if (res == ButtonResult.Yes)
        {
            Close();
        }
    }

    /// <summary>
    /// Добавление пользователя
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void AddUserOnClick(object? sender, RoutedEventArgs e)
    {
        var res = await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
        {
            WindowIcon = this.Icon,
            ContentTitle = "Уведомление",
            CanResize = true,
            ContentMessage = "Вы уверены, что хотите добавить пользователя?",
            ButtonDefinitions = ButtonEnum.YesNo
        }).ShowDialog(this);

        if (res != ButtonResult.Yes)
        {
            return;
        }

        SaveUser.IsEnabled = false;

        if (!await CheckAllDataRight())
        {
            SaveUser.IsEnabled = true;
            return;
        }

        // Строка авторизации в api
        var authString = Auth.GetAuth(ConnectData.Login, ConnectData.Password);
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"Users/");
            request.Content = JsonContent.Create(_currentUser);
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
                SaveUser.IsEnabled = true;
                return;
            }

            await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
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
                    "Пользователь успешно добавлен",
                ButtonDefinitions = ButtonEnum.Ok
            }).ShowDialog(this);
            Close();
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
            SaveUser.IsEnabled = true;
        }
    }

    /// <summary>
    /// Сохранение изменения данных пользователя
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void SaveUserOnClick(object? sender, RoutedEventArgs e)
    {
        var res = await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
        {
            WindowIcon = UserData.currentWindow!.Icon,
            ContentTitle = "Уведомление",
            CanResize = true,
            ContentMessage = "Вы уверены, что хотите сохранить изменения пользователя?",
            ButtonDefinitions = ButtonEnum.YesNo
        }).ShowDialog(this);

        if (res != ButtonResult.Yes)
        {
            return;
        }

        SaveUser.IsEnabled = false;

        if (!await CheckAllDataRight())
        {
            SaveUser.IsEnabled = true;
            return;
        }

        // Строка авторизации в api
        var authString = Auth.GetAuth(ConnectData.Login, ConnectData.Password);
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"Users/");
            request.Content = JsonContent.Create(_currentUser);
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
                SaveUser.IsEnabled = true;
                return;
            }

            await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
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
                    "Пользователь успешно отредактирован",
                ButtonDefinitions = ButtonEnum.Ok
            }).ShowDialog(this);
            Close();
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
            SaveUser.IsEnabled = true;
        }
    }

    /// <summary>
    /// Загрузка сведений студента в поля
    /// </summary>
    private void LoadUserDataToField()
    {
        DataContext = _currentUser.User;

        if (_isAddEditView is true)
        {
            return;
        }

        Roles.Items = _currentUser.UserRoles;
        
        DateReg.SelectedDate = _currentUser.User.DateReg;
    }

    /// <summary>
    /// Проверка, что введены все обязательные поля
    /// </summary>
    /// <returns></returns>
    private async Task<bool> CheckAllDataRight()
    {
        if (string.IsNullOrWhiteSpace(Login.Text))
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
                    "Не введен логин",
                ButtonDefinitions = ButtonEnum.Ok
            }).ShowDialog(this);
            return false;
        }
        
        if (_isAddEditView == true && string.IsNullOrWhiteSpace(_currentUser.User.Password))
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
                    "Не создан пароль",
                ButtonDefinitions = ButtonEnum.Ok
            }).ShowDialog(this);
            return false;
        }

        if (_currentUser.User.Password is not null)
        {
            _currentUser.User.Password = (_currentUser.User.Password + _currentUser.User.Login).GetSha256();
        }

        return true;
    }
}