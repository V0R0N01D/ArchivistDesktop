using System;
using System.Threading.Tasks;
using ArchivistsDesktop.Contracts;
using ArchivistsDesktop.Contracts.ResponseClass;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;

namespace ArchivistsDesktop.View.Archive.Window;

public partial class AddEditViewDiplom : Avalonia.Controls.Window
{
    private DiplomResponse _currentDiplom = new();

    // true - добавление, false - редактирование, null - просмотр
    private bool? _isAddEditView = true;
    
    private CanAdd _canAdd = new();

    
    public AddEditViewDiplom()
    {
        InitializeComponent();
    }
    
    public AddEditViewDiplom(ref DiplomResponse diplom,  ref CanAdd isAccepted)
    {
        InitializeComponent();

        _canAdd = isAccepted;

        _currentDiplom = diplom;

        InitializeEvents();
    }
    
    public AddEditViewDiplom(ref DiplomResponse diplom, bool isView,  ref CanAdd isAccepted)
    {
        InitializeComponent();

        _canAdd = isAccepted;

        _isAddEditView = isView ? null : false;

        _currentDiplom = diplom;

        InitializeEvents();
    }
    
    
    /// <summary>
    /// Инициализация событий
    /// </summary>
    private void InitializeEvents()
    {
        DataContext = _currentDiplom;
        BackPage.Click += BackPageOnClick;

        if (_isAddEditView is true)
        {
            SaveDiplom.Click += SaveDiplomOnClick;
            return;
        }

        DateDiplom.SelectedDate = _currentDiplom.DateIssue.HasValue
            ? new DateTimeOffset(_currentDiplom.DateIssue.Value.ToDateTime(new TimeOnly(0)), new TimeSpan(3, 0, 0))
            : null;

        if (_isAddEditView is null)
        {
            Title = "Просмотр данных диплома";
            MainText.Text = "Просмотр данных диплома";

            DiplomSerial.IsReadOnly = true;
            DiplomNumber.IsReadOnly = true;
            DateDiplom.IsHitTestVisible = false;

            SaveDiplom.IsVisible = false;

            return;
        }

        SaveDiplom.Click += EditDiplomOnClick;
        Title = "Редактирование данных диплома";
        MainText.Text = "Редактирование данных диплома";
        SaveDiplom.Content = "Редактировать";
    }

     /// <summary>
    /// Загрузка значений из datepicker в поле и проверка что введено обязательное поле номер диплома 
    /// </summary>
    private async Task<bool> LoadCurrentValToDiplom()
    {
        if (string.IsNullOrWhiteSpace(DiplomNumber.Text))
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
                ContentMessage = "Не введен номер диплома",
                ButtonDefinitions = ButtonEnum.Ok
            }).ShowDialog(this);
            return false;
        }

        if (DateDiplom.SelectedDate.HasValue)
        {
            _currentDiplom.DateIssue = DateOnly.FromDateTime(DateDiplom.SelectedDate.Value.DateTime);
        }

        return true;
    }
    
    /// <summary>
    /// Добавление диплома
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void SaveDiplomOnClick(object? sender, RoutedEventArgs e)
    {
        var res = await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
        {
            WindowIcon = this.Icon,
            ContentTitle = "Уведомление",
            CanResize = true,
            ContentMessage = "Вы уверены, что хотите добавить диплом?",
            ButtonDefinitions = ButtonEnum.YesNo
        }).ShowDialog(this);

        if (res != ButtonResult.Yes)
        {
            return;
        }
        
        if (!await LoadCurrentValToDiplom())
        {
            return;
        }
        
        _canAdd.Accept();
        
        Close();
    }

    /// <summary>
    /// Редактирование диплома
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void EditDiplomOnClick(object? sender, RoutedEventArgs e)
    {
        var res = await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
        {
            WindowIcon = this.Icon,
            ContentTitle = "Уведомление",
            CanResize = true,
            ContentMessage = "Вы уверены, что хотите редактировать диплом?",
            ButtonDefinitions = ButtonEnum.YesNo
        }).ShowDialog(this);

        if (res != ButtonResult.Yes)
        {
            return;
        }
        
        if (!await LoadCurrentValToDiplom())
        {
            return;
        }
        
        _canAdd.Accept();
        
        Close();
    }
    
    /// <summary>
    /// Переход обратно к студенту
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
                "Вы уверены, что хотите закрыть данное окно и перейти обратно к данным студента? (Все не сохраненные данные исчезнут)",
            ButtonDefinitions = ButtonEnum.YesNo
        }).ShowDialog(this);

        if (res == ButtonResult.Yes)
        {
            Close();
        }
    }
}