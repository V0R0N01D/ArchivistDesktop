using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ArchivistsDesktop.Contracts.ResponseClass;
using ArchivistsDesktop.DataClass;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DocumentFormat.OpenXml.Bibliography;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;

namespace ArchivistsDesktop.View.Archive.Window;

public partial class AddEditViewSpeciality : Avalonia.Controls.Window
{
    private SpecialityResponse _currentSpeciality = new();

    // true - добавление, false - редактирование, null - просмотр
    private bool? _isAddEditView = true;

    public AddEditViewSpeciality()
    {
        InitializeComponent();

        InitializeEvents();
    }

    public AddEditViewSpeciality(SpecialityResponse selectSpeciality, bool isView)
    {
        InitializeComponent();

        _isAddEditView = isView ? null : false;

        _currentSpeciality = selectSpeciality;

        InitializeEvents();
    }

    /// <summary>
    /// Добавление специальности
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void AddSpecialityOnClick(object? sender, RoutedEventArgs e)
    {
        var res = await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
        {
            WindowIcon = this.Icon,
            ContentTitle = "Уведомление",
            CanResize = true,
            ContentMessage = "Вы уверены, что хотите добавить специальность?",
            ButtonDefinitions = ButtonEnum.YesNo
        }).ShowDialog(this);

        if (res != ButtonResult.Yes)
        {
            return;
        }

        SaveSpeciality.IsEnabled = false;

        if (!await IsNotNullProperty())
        {
            SaveSpeciality.IsEnabled = true;
            return;
        }

        // Строка авторизации в api
        var authString = Auth.GetAuth(ConnectData.Login, ConnectData.Password);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"Speciality");
            request.Content = JsonContent.Create(_currentSpeciality);
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
                SaveSpeciality.IsEnabled = true;
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
                    "Специальность успешно добавлена",
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
            SaveSpeciality.IsEnabled = true;
        }
    }

    /// <summary>
    /// Редактирование специальности
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void EditSpecialityOnClick(object? sender, RoutedEventArgs e)
    {
        var res = await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
        {
            WindowIcon = this.Icon,
            ContentTitle = "Уведомление",
            CanResize = true,
            ContentMessage = "Вы уверены, что хотите редактировать специальность?",
            ButtonDefinitions = ButtonEnum.YesNo
        }).ShowDialog(this);

        if (res != ButtonResult.Yes)
        {
            return;
        }

        SaveSpeciality.IsEnabled = false;

        if (!await IsNotNullProperty())
        {
            SaveSpeciality.IsEnabled = true;
            return;
        }

        // Строка авторизации в api
        var authString = Auth.GetAuth(ConnectData.Login, ConnectData.Password);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"Speciality");
            request.Content = JsonContent.Create(_currentSpeciality);
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
                SaveSpeciality.IsEnabled = true;
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
                ContentMessage = "Специальность успешно отредактирована",
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
                ContentMessage = $"Ошибка соединения: {ex.Message}",
                ButtonDefinitions = ButtonEnum.Ok
            }).ShowDialog(this);
            SaveSpeciality.IsEnabled = true;
        }
    }

    /// <summary>
    /// Инициализация событий
    /// </summary>
    private void InitializeEvents()
    {
        DataContext = _currentSpeciality;
        BackPage.Click += BackPageOnClick;

        if (_isAddEditView is true)
        {
            SaveSpeciality.Click += AddSpecialityOnClick;
            return;
        }

        if (_isAddEditView is null)
        {
            Title = "Просмотр данных специальности";
            MainText.Text = "Просмотр данных специальности";

            SaveSpeciality.IsVisible = false;
            FGOS.IsReadOnly = true;
            SpecialityTitle.IsReadOnly = true;
            return;
        }

        SaveSpeciality.Click += EditSpecialityOnClick;
        Title = "Редактирование данных cпецильности";
        MainText.Text = "Редактирование данных специальности";
        SaveSpeciality.Content = "Редактировать";
    }

    /// <summary>
    /// Проверка, что обязательные поля (ФГОС и название) заполнены
    /// </summary>
    /// <returns>Заполнены поля или нет</returns>
    private async Task<bool> IsNotNullProperty()
    {
        if (string.IsNullOrWhiteSpace(FGOS.Text))
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
                ContentMessage = "Не заполнено поле ФГОС",
                ButtonDefinitions = ButtonEnum.Ok
            }).ShowDialog(this);
            return false;
        }

        if (string.IsNullOrWhiteSpace(SpecialityTitle.Text))
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
                ContentMessage = "Не заполнено поле название",
                ButtonDefinitions = ButtonEnum.Ok
            }).ShowDialog(this);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Переход обратно к списку групп
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void BackPageOnClick(object? sender, RoutedEventArgs e)
    {
        if (_isAddEditView == null)
        {
            Close();
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
                "Вы уверены, что хотите закрыть данное окно и перейти обратно к просмотру специальностей? (Все не сохраненные данные исчезнут)",
            ButtonDefinitions = ButtonEnum.YesNo
        }).ShowDialog(this);

        if (res == ButtonResult.Yes)
        {
            Close();
        }
    }
}