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

public partial class AddEditViewOrder : Avalonia.Controls.Window
{
    private OrderAllDataResponse _currentOrder = new();

    // true - добавление, false - редактирование, null - просмотр
    private bool? _isAddEditView = true;

    public AddEditViewOrder()
    {
        InitializeComponent();

        LoadComboBoxes();
        
        InitializeEvents();
    }

    public AddEditViewOrder(OrderAllDataResponse order, bool isView)
    {
        InitializeComponent();

        _isAddEditView = isView ? null : false;

        _currentOrder = order;

        LoadComboBoxes();

        InitializeEvents();
    }

    /// <summary>
    /// Загрузка типов приказов и групп
    /// </summary>
    private async void LoadComboBoxes()
    {
        // Строка авторизации в api
        var authString = Auth.GetAuth(ConnectData.Login, ConnectData.Password);

        try
        {
            using var requestOrderTypes = new HttpRequestMessage(HttpMethod.Get, "OrderType");
            requestOrderTypes.Headers.Add("AUTH", authString);
            var responseOrderTypesTask = ConnectData.Client.SendAsync(requestOrderTypes);

            using var requestGroups = new HttpRequestMessage(HttpMethod.Get, "Groups");
            requestGroups.Headers.Add("AUTH", authString);
            var responseGroupsTask = ConnectData.Client.SendAsync(requestGroups);

            var responseOrderTypes = await responseOrderTypesTask;

            if (!responseOrderTypes.IsSuccessStatusCode)
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
                        $"Ошибка. Код: {responseOrderTypes.StatusCode}, ошибка: {await responseOrderTypes.Content.ReadAsStringAsync()}",
                    ButtonDefinitions = ButtonEnum.Ok
                }).ShowDialog(this);
                return;
            }

            var types = await responseOrderTypes.Content.ReadFromJsonAsync<List<TypeOrderResponse>>();

            TypeOrder.Items = types;

            var responseGroups = await responseGroupsTask;

            if (!responseGroups.IsSuccessStatusCode)
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
                        $"Ошибка. Код: {responseGroups.StatusCode}, ошибка: {await responseGroups.Content.ReadAsStringAsync()}",
                    ButtonDefinitions = ButtonEnum.Ok
                }).ShowDialog(this);
                return;
            }

            var groups = await responseGroups.Content.ReadFromJsonAsync<List<GroupResponse>>();

            Group.Items = groups;

            if (_isAddEditView == true)
            {
                return;
            }

            if (_currentOrder.Group is not null)
            {
                Group.SelectedIndex = groups!.FindIndex(s => s.Id == _currentOrder.Group.Id);
            }
            
            TypeOrder.SelectedIndex = types!.FindIndex(t => t.Id == _currentOrder.TypeOrder.Id);
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
        DataContext = _currentOrder;
        BackPage.Click += BackPageOnClick;

        if (_isAddEditView is true)
        {
            Group.SelectedIndex = 0;
            TypeOrder.SelectedIndex = 0;
            SaveOrder.Click += SaveOrderOnClick;
            return;
        }

        DateOrder.SelectedDate = new DateTimeOffset(_currentOrder.OrderDate, new TimeSpan(3, 0, 0));
        DateStartOrder.SelectedDate = _currentOrder.DateStartOrder.HasValue
            ? new DateTimeOffset(_currentOrder.DateStartOrder.Value.ToDateTime(new TimeOnly(0)), new TimeSpan(3, 0, 0))
            : null;

        if (_isAddEditView is null)
        {
            Title = "Просмотр данных приказа";
            MainText.Text = "Просмотр данных приказа";

            SaveOrder.IsVisible = false;
            Group.IsHitTestVisible = false;
            OrderNumber.IsReadOnly = true;
            DateOrder.IsHitTestVisible = false;
            DateStartOrder.IsHitTestVisible = false;
            TypeOrder.IsHitTestVisible = false;
            return;
        }

        SaveOrder.Click += EditOrderOnClick;
        Title = "Редактирование данных приказа";
        MainText.Text = "Редактирование данных приказа";
        SaveOrder.Content = "Редактировать";
    }

    /// <summary>
    /// Загрузка значений из comboBox и datepicker в поля приказа
    /// </summary>
    private async Task<bool> LoadCurrentValToGroup()
    {
        if (string.IsNullOrWhiteSpace(OrderNumber.Text))
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
                ContentMessage = "Не введен номер приказа",
                ButtonDefinitions = ButtonEnum.Ok
            }).ShowDialog(this);
            return false;
        }

        if (!DateOrder.SelectedDate.HasValue)
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
                ContentMessage = "Не выбрана дата приказа",
                ButtonDefinitions = ButtonEnum.Ok
            }).ShowDialog(this);
            return false;
        }

        if (!DateStartOrder.SelectedDate.HasValue)
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
                ContentMessage = "Не выбрана дата начала действия приказа",
                ButtonDefinitions = ButtonEnum.Ok
            }).ShowDialog(this);
            return false;
        }

        if (TypeOrder.SelectedItem is null)
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
                ContentMessage = "Не выбран тип приказа",
                ButtonDefinitions = ButtonEnum.Ok
            }).ShowDialog(this);
            return false;
        }

        _currentOrder.OrderDate = DateOrder.SelectedDate.Value.DateTime;
        _currentOrder.DateStartOrder = DateOnly.FromDateTime(DateStartOrder.SelectedDate.Value.DateTime);
        _currentOrder.TypeOrder = (TypeOrder.SelectedItem as TypeOrderResponse)!;
        _currentOrder.Group = Group.SelectedItem as GroupResponse;

        return true;
    }

    /// <summary>
    /// Добавление приказа
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void SaveOrderOnClick(object? sender, RoutedEventArgs e)
    {
        var res = await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
        {
            WindowIcon = this.Icon,
            ContentTitle = "Уведомление",
            CanResize = true,
            ContentMessage = "Вы уверены, что хотите добавить приказ?",
            ButtonDefinitions = ButtonEnum.YesNo
        }).ShowDialog(this);

        if (res != ButtonResult.Yes)
        {
            return;
        }

        SaveOrder.IsEnabled = false;

        if (!await LoadCurrentValToGroup())
        {
            SaveOrder.IsEnabled = true;
            return;
        }

        // Строка авторизации в api
        var authString = Auth.GetAuth(ConnectData.Login, ConnectData.Password);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"Order");
            request.Content = JsonContent.Create(_currentOrder);
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
                SaveOrder.IsEnabled = true;
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
                    "Приказ успешно добавлен",
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
            SaveOrder.IsEnabled = true;
        }
    }

    /// <summary>
    /// Редактирование приказа
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void EditOrderOnClick(object? sender, RoutedEventArgs e)
    {
        var res = await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
        {
            WindowIcon = this.Icon,
            ContentTitle = "Уведомление",
            CanResize = true,
            ContentMessage = "Вы уверены, что хотите редактировать приказ?",
            ButtonDefinitions = ButtonEnum.YesNo
        }).ShowDialog(this);

        if (res != ButtonResult.Yes)
        {
            return;
        }

        SaveOrder.IsEnabled = false;

        if (!await LoadCurrentValToGroup())
        {
            SaveOrder.IsEnabled = true;
            return;
        }

        // Строка авторизации в api
        var authString = Auth.GetAuth(ConnectData.Login, ConnectData.Password);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"Order");
            request.Content = JsonContent.Create(_currentOrder);
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
                SaveOrder.IsEnabled = true;
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
                    "Приказ успешно отредактирован",
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
            SaveOrder.IsEnabled = true;
        }
    }

    /// <summary>
    /// Переход обратно к списку приказов
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
                "Вы уверены, что хотите закрыть данное окно и перейти обратно к просмотру приказов? (Все не сохраненные данные исчезнут)",
            ButtonDefinitions = ButtonEnum.YesNo
        }).ShowDialog(this);

        if (res == ButtonResult.Yes)
        {
            Close();
        }
    }
}