using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
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

public partial class AddEditViewGroup : Avalonia.Controls.Window
{
    private GroupResponse _currentGroup = new();

    // true - добавление, false - редактирование, null - просмотр
    private bool? _isAddEditView = true;

    public AddEditViewGroup()
    {
        InitializeComponent();

        LoadComboBoxes();

        InitializeEvents();
    }

    public AddEditViewGroup(GroupResponse selectGroup, bool isView)
    {
        InitializeComponent();

        _isAddEditView = isView ? null : false;

        _currentGroup = selectGroup;

        LoadComboBoxes();

        InitializeEvents();
    }

    /// <summary>
    /// Загрузка типов обучения в comboBox
    /// </summary>
    private async void LoadComboBoxes()
    {
        // Строка авторизации в api
        var authString = Auth.GetAuth(ConnectData.Login, ConnectData.Password);

        try
        {
            using var requestTypeEducation = new HttpRequestMessage(HttpMethod.Get, "TypeEducation");
            requestTypeEducation.Headers.Add("AUTH", authString);
            var responseTypeEducationTask = ConnectData.Client.SendAsync(requestTypeEducation);

            var responseTypeEducation = await responseTypeEducationTask;

            if (!responseTypeEducation.IsSuccessStatusCode)
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
                        $"Ошибка. Код: {responseTypeEducation.StatusCode}, ошибка: {await responseTypeEducation.Content.ReadAsStringAsync()}",
                    ButtonDefinitions = ButtonEnum.Ok
                }).ShowDialog(this);
                return;
            }

            var types = await responseTypeEducation.Content.ReadFromJsonAsync<List<TypeEducationResponse>>();

            TypeEducation.Items = types;

            if (_isAddEditView == true)
            {
                return;
            }
            
            TypeEducation.SelectedIndex = types!.FindIndex(t => t.Id == _currentGroup.TypeEducation.Id);
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
    /// Загрузка значений из comboBox и datepicker в поля группы
    /// </summary>
    private async Task<bool> LoadCurrentValToGroup()
    {
        if (DateEndEducation.SelectedDate.HasValue)
        {
            _currentGroup.DateEndEducation = DateOnly.FromDateTime(DateEndEducation.SelectedDate.Value.DateTime);
        }

        if (Year.SelectedItem is null)
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
                ContentMessage = "Не выбран курс",
                ButtonDefinitions = ButtonEnum.Ok
            }).ShowDialog(this);
            return false;
        }

        if (_currentGroup.Speciality is null)
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
                ContentMessage = "Не выбрана специальность",
                ButtonDefinitions = ButtonEnum.Ok
            }).ShowDialog(this);
            return false;
        }

        if (TypeEducation.SelectedItem is null)
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
                ContentMessage = "Не выбран тип обучения",
                ButtonDefinitions = ButtonEnum.Ok
            }).ShowDialog(this);
            return false;
        }
        
        _currentGroup.Year = (short)(Year.SelectedIndex == 5 ? 100 : Year.SelectedIndex + 1);
        _currentGroup.TypeEducation = (TypeEducation.SelectedItem as TypeEducationResponse)!;

        return true;
    }

    /// <summary>
    /// Добавление группы
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void AddGroupOnClick(object? sender, RoutedEventArgs e)
    {
        var res = await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
        {
            WindowIcon = this.Icon,
            ContentTitle = "Уведомление",
            CanResize = true,
            ContentMessage = "Вы уверены, что хотите добавить группу?",
            ButtonDefinitions = ButtonEnum.YesNo
        }).ShowDialog(this);

        if (res != ButtonResult.Yes)
        {
            return;
        }

        SaveGroup.IsEnabled = false;

        if (!await LoadCurrentValToGroup())
        {
            SaveGroup.IsEnabled = true;
            return;
        }

        // Строка авторизации в api
        var authString = Auth.GetAuth(ConnectData.Login, ConnectData.Password);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"Groups");
            request.Content = JsonContent.Create(_currentGroup);
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
                SaveGroup.IsEnabled = true;
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
                    "Группа успешно добавлена",
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
            SaveGroup.IsEnabled = true;
        }
    }

    /// <summary>
    /// Редактирование группы
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void EditGroupOnClick(object? sender, RoutedEventArgs e)
    {
        var res = await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
        {
            WindowIcon = this.Icon,
            ContentTitle = "Уведомление",
            CanResize = true,
            ContentMessage = "Вы уверены, что хотите редактировать группу?",
            ButtonDefinitions = ButtonEnum.YesNo
        }).ShowDialog(this);

        if (res != ButtonResult.Yes)
        {
            return;
        }

        SaveGroup.IsEnabled = false;

        if (!await LoadCurrentValToGroup())
        {
            SaveGroup.IsEnabled = true;
            return;
        }

        // Строка авторизации в api
        var authString = Auth.GetAuth(ConnectData.Login, ConnectData.Password);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"Groups");
            request.Content = JsonContent.Create(_currentGroup);
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
                SaveGroup.IsEnabled = true;
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
                    "Группа успешно отредактирована",
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
            SaveGroup.IsEnabled = true;
        }
    }

    /// <summary>
    /// Инициализация событий
    /// </summary>
    private void InitializeEvents()
    {
        DataContext = _currentGroup;
        BackPage.Click += BackPageOnClick;
        ChangeSpeciality.Click += ChangeSpecialityOnClick;

        if (_isAddEditView is true)
        {
            TypeEducation.SelectedIndex = 0;
            SaveGroup.Click += AddGroupOnClick;
            Speciality.Text = "Не выбрана";
            return;
        }

        DateEndEducation.SelectedDate = _currentGroup.DateEndEducation.HasValue
            ? new DateTimeOffset(_currentGroup.DateEndEducation.Value.ToDateTime(new TimeOnly(0)),
                new TimeSpan(3, 0, 0))
            : null;
        Year.SelectedIndex = _currentGroup.Year == 100 ? 4 : _currentGroup.Year - 1;
        
        Speciality.Text = $"{_currentGroup.Speciality.Fgos} {_currentGroup.Speciality.Title}";

        if (_isAddEditView is null)
        {
            Title = "Просмотр данных группы";
            MainText.Text = "Просмотр данных группы";

            ChangeSpeciality.IsVisible = false;
            SaveGroup.IsVisible = false;
            GroupNumber.IsReadOnly = true;
            Year.IsHitTestVisible = false;
            DateEndEducation.IsHitTestVisible = false;
            TypeEducation.IsHitTestVisible = false;
            Speciality.IsHitTestVisible = false;
            return;
        }

        SaveGroup.Click += EditGroupOnClick;
        Title = "Редактирование данных группы";
        MainText.Text = "Редактирование данных группы";
        SaveGroup.Content = "Редактировать";
    }

    /// <summary>
    /// Изменение специальности
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ChangeSpecialityOnClick(object? sender, RoutedEventArgs e)
    {
        // проверка, что специальность сохранили
        CanAdd isAcceptedSpeciality = new();

        var selectSpeciality = new SpecialityResponse();

        await new SelectSpeciality(ref selectSpeciality, ref isAcceptedSpeciality).ShowDialog(this);

        if (isAcceptedSpeciality.IsAccepted)
        {
            _currentGroup.Speciality = selectSpeciality;
            Speciality.Text = $"{_currentGroup.Speciality.Fgos} {_currentGroup.Speciality.Title}";
        }
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
                "Вы уверены, что хотите закрыть данное окно и перейти обратно к просмотру группы? (Все не сохраненные данные исчезнут)",
            ButtonDefinitions = ButtonEnum.YesNo
        }).ShowDialog(this);

        if (res == ButtonResult.Yes)
        {
            Close();
        }
    }
}