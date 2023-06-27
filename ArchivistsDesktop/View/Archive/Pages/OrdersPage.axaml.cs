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

public partial class OrdersPage : UserControl
{
    public OrdersPage()
    {
        InitializeComponent();

        InitializeEvents();

        LoadTypeOrder();
        
        LoadRoleFunction();
    }

    /// <summary>
    /// Загрузка приказов
    /// </summary>
    private async void LoadOrders()
    {
        Search.IsEnabled = false;
        
        var requestAddres = "Order";

        var search = SearchInput.Text;

        if (!string.IsNullOrWhiteSpace(search))
        {
            requestAddres = requestAddres.AddOptionalParam("search", search);
        }

        if (FilterTypeOrder.SelectedItem is not null && FilterTypeOrder.SelectedIndex != 0)
        {
            requestAddres =
                requestAddres.AddOptionalParam("type", (FilterTypeOrder.SelectedItem as TypeOrderResponse)!.Id);
        }

        if (StartDate.SelectedDate.HasValue)
        {
            var selectDate = StartDate.SelectedDate.Value.Date;
            requestAddres = requestAddres.AddOptionalParam("start_date", selectDate.Ticks);
        }

        if (EndDate.SelectedDate.HasValue)
        {
            var selectDate = EndDate.SelectedDate.Value.Date;
            requestAddres = requestAddres.AddOptionalParam("end_date", selectDate.Ticks);
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

            var types = await response.Content.ReadFromJsonAsync<List<OrderAllDataResponse>>();

            NoResult.IsVisible = types is { Count: 0 };
            
            Orders.Items = types;
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
    /// Загрузка типов приказов
    /// </summary>
    private async void LoadTypeOrder()
    {
        // Строка авторизации в api
        var authString = Auth.GetAuth(ConnectData.Login, ConnectData.Password);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "OrderType");
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
                return;
            }

            var types = await response.Content.ReadFromJsonAsync<List<TypeOrderResponse>>();

            types = types ?? new();

            types.Insert(0, new TypeOrderResponse()
            {
                Id = int.MinValue,
                Title = "Все типы"
            });

            FilterTypeOrder.Items = types;

            FilterTypeOrder.SelectedIndex = 0;
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
    }

    /// <summary>
    /// Инициализация функций которые зависят от ролей
    /// </summary>
    private void LoadRoleFunction()
    {
        if (ConnectData.Roles!.FirstOrDefault(role => role.Id == 11) is not null)
        {
            AddOrder.IsVisible = true;
            AddOrder.Click += AddOrderOnClick;
        }

        var functionMenu = new List<MenuItem>();
        if (ConnectData.Roles!.FirstOrDefault(role => role.Id == 12) is not null)
        {
            var menuItem = new MenuItem()
            {
                Header = "Редактировать"
            };
            menuItem.Click += EditGroupClick;
            functionMenu.Add(menuItem);
        }

        if (ConnectData.Roles!.FirstOrDefault(role => role.Id == 13) is not null)
        {
            var menuItem = new MenuItem()
            {
                Header = "Удалить"
            };
            menuItem.Click += DeleteOrderClick;
            functionMenu.Add(menuItem);
        }

        if (functionMenu.Count == 0)
        {
            return;
        }

        Orders.ContextMenu = new ContextMenu()
        {
            Items = functionMenu
        };
    }
    
    /// <summary>
    /// Удаление выбранного приказа
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void DeleteOrderClick(object? sender, RoutedEventArgs e)
    {
        if (Orders.SelectedItem is not OrderAllDataResponse selectOrder)
        {
            return;
        }

        var res = await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
        {
            WindowIcon = UserData.currentWindow!.Icon,
            ContentTitle = "Уведомление",
            CanResize = true,
            ContentMessage = "Вы уверены, что хотите удалить выбранный приказ?",
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
            using var request = new HttpRequestMessage(HttpMethod.Delete, $"Order/{selectOrder.Id}");
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

            LoadOrders();
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
    /// Редактирование выбранного приказа
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void EditGroupClick(object? sender, RoutedEventArgs e)
    {
        if (Orders.SelectedItem is not OrderAllDataResponse selectOrder)
        {
            return;
        }

        await new AddEditViewOrder(selectOrder, false).ShowDialog(UserData.currentWindow);
        LoadOrders();
    }

    /// <summary>
    /// Добавление приказа
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void AddOrderOnClick(object? sender, RoutedEventArgs e)
    {
        await new AddEditViewOrder().ShowDialog(UserData.currentWindow);
        LoadOrders();
    }

    /// <summary>
    /// Инициализация событий
    /// </summary>
    private void InitializeEvents()
    {
        BackPage.Click += BackPage_Click;
        Search.Click += SearchOnClick;
        ClearDate.Click += ClearDateOnClick;
        Orders.DoubleTapped += OrdersOnDoubleTapped;
    }

    /// <summary>
    /// Просмотр данных приказа
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OrdersOnDoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (Orders.SelectedItem is not OrderAllDataResponse selectOrder)
        {
            return;
        }

        await new AddEditViewOrder(selectOrder, true).ShowDialog(UserData.currentWindow);
        LoadOrders();
    }

    /// <summary>
    /// Сброс фильтрации по дате
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ClearDateOnClick(object? sender, RoutedEventArgs e)
    {
        StartDate.SelectedDate = null;
        EndDate.SelectedDate = null;
    }

    /// <summary>
    /// Поиск
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void SearchOnClick(object? sender, RoutedEventArgs e)
    {
        LoadOrders();
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