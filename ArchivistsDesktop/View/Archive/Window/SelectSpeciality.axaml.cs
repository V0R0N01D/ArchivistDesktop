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

namespace ArchivistsDesktop.View.Archive.Window;

public partial class SelectSpeciality : Avalonia.Controls.Window
{
    private CanAdd _canAdd = new();
    private SpecialityResponse _speciality = new();
    
    public SelectSpeciality()
    {
        InitializeComponent();
    }
    
    public SelectSpeciality(ref SpecialityResponse speciality, ref CanAdd isAccepted)
    {
        InitializeComponent();

        _speciality = speciality;
        _canAdd = isAccepted;
        
        InitializeEvents();
        
        LoadOrders();
    }

    /// <summary>
    /// Загрузка списка специальностей
    /// </summary>
    private async void LoadOrders()
    {
        // Строка авторизации в api
        var authString = Auth.GetAuth(ConnectData.Login, ConnectData.Password);

        try
        {
            var requestUri = "Speciality";
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

            var specialities = await response.Content.ReadFromJsonAsync<List<SpecialityResponse>>();

            Speciality.Items = specialities;
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
        SaveSpeciality.Click += SaveSpecialityOnClick;
        BackPage.Click += BackPageOnClick;
        Search.Click += SearchOnClick;
    }

    /// <summary>
    /// Поиск приказов
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SearchOnClick(object? sender, RoutedEventArgs e)
    {
        LoadOrders();
    }

    /// <summary>
    /// Сохранение специальности
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void SaveSpecialityOnClick(object? sender, RoutedEventArgs e)
    {
        // Проверка, что выбран приказ
        if (Speciality.SelectedItem is not SpecialityResponse selectSpeciality)
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
                    "Не выбрана специальность",
                ButtonDefinitions = ButtonEnum.Ok
            }).ShowDialog(this);
            return;
        }

        _speciality.Id = selectSpeciality.Id;
        _speciality.Fgos = selectSpeciality.Fgos;
        _speciality.Title = selectSpeciality.Title;

        _canAdd.Accept();
        
        Close();
    }

    /// <summary>
    /// Переход обратно к группе
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
                "Вы уверены, что хотите закрыть данное окно и перейти обратно к окну группы? (Все не сохраненные данные исчезнут)",
            ButtonDefinitions = ButtonEnum.YesNo
        }).ShowDialog(this);

        if (res == ButtonResult.Yes)
        {
            Close();
        }
    }
}