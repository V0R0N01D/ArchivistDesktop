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

public partial class AddOrderWindow : Avalonia.Controls.Window
{
    private CanAdd _canAdd = new();
    private OrderAllDataResponse _order = new();
    
    public AddOrderWindow()
    {
        InitializeComponent();
    }
    
    public AddOrderWindow(ref OrderAllDataResponse order, ref CanAdd isAccepted)
    {
        InitializeComponent();

        _order = order;
        _canAdd = isAccepted;
        
        InitializeEvents();
        
        LoadOrders();
    }

    /// <summary>
    /// Загрузка списка приказов
    /// </summary>
    private async void LoadOrders()
    {
        // Строка авторизации в api
        var authString = Auth.GetAuth(ConnectData.Login, ConnectData.Password);

        try
        {
            var requestUri = "Order";
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

            var types = await response.Content.ReadFromJsonAsync<List<OrderAllDataResponse>>();

            Orders.Items = types;
            Orders.SelectedIndex = 0;
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
        SaveOrder.Click += SaveOrderOnClick;
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
    /// Сохранение приказа
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void SaveOrderOnClick(object? sender, RoutedEventArgs e)
    {
        // Проверка, что выбран приказ
        if (Orders.SelectedItem is null)
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
                    "Не выбран приказ",
                ButtonDefinitions = ButtonEnum.Ok
            }).ShowDialog(this);
            return;
        }

        var selectGrade = Orders.SelectedItem as OrderAllDataResponse;

        _order.Id = selectGrade.Id;
        _order.Number = selectGrade.Number;
        _order.OrderDate = selectGrade.OrderDate;
        _order.Group = selectGrade.Group;
        _order.DateStartOrder = selectGrade.DateStartOrder;
        _order.TypeOrder = selectGrade.TypeOrder;

        _canAdd.Accept();
        
        Close();
    }

    /// <summary>
    /// Переход обратно к аттестату
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
                "Вы уверены, что хотите закрыть данное окно и перейти обратно к окну студента? (Все не сохраненные данные исчезнут)",
            ButtonDefinitions = ButtonEnum.YesNo
        }).ShowDialog(this);

        if (res == ButtonResult.Yes)
        {
            Close();
        }
    }
}