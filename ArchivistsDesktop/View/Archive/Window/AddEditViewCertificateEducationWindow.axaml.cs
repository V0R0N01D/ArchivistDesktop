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

public partial class AddEditViewCertificateEducationWindow : Avalonia.Controls.Window
{
    // true - добавление, false - редактирование, null - просмотр
    private bool? _isAddEditView = null;

    private List<GradeResponse>? _grades = new();

    private List<int> _removeGrades = new();

    private CertificateEducationResponse _certificate = new();

    private CanAdd _isAccepted;

    public AddEditViewCertificateEducationWindow()
    {
        InitializeComponent();
    }

    public AddEditViewCertificateEducationWindow(ref CertificateEducationResponse certificate, bool? isAddEditView,
        ref CanAdd isAccepted)
    {
        InitializeComponent();

        _isAccepted = isAccepted;

        _certificate = certificate;

        _isAddEditView = isAddEditView;

        InitializeEvents();

        LoadCertifiateTypes();

        LoadCertificateDataToField();

        if (_isAddEditView != null)
        {
            var menItem = new MenuItem()
            {
                Header = "Удалить",
            };
            menItem.Click += MenuItem_OnClick;
            GradeList.ContextMenu = new ContextMenu()
            {
                Items = new List<MenuItem>()
                {
                    menItem
                }
            };
        }
    }

    /// <summary>
    /// Загрузка типов аттестатов в comboBox
    /// </summary>
    private async void LoadCertifiateTypes()
    {
        // Строка авторизации в api
        var authString = Auth.GetAuth(ConnectData.Login, ConnectData.Password);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "CertificateType");
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

            var types = await response.Content.ReadFromJsonAsync<List<CertificateEducationTypeReposponse>>();

            TypeEducation.Items = types;

            if (_certificate.Type is not null)
            {
                TypeEducation.SelectedItem = types!.FirstOrDefault(cer => cer.Id == _certificate.Type.Id);
            }
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
    /// Загрузка сведений студента в поля
    /// </summary>
    private void LoadCertificateDataToField()
    {
        if (_isAddEditView is true)
        {
            return;
        }

        Id.Text = _certificate.Id.ToString();
        Identify.Text = _certificate.Identify;
        IsOriginal.IsChecked = _certificate.IsOriginal;
        AverangeGrade.Text = _certificate.AverageGrade.ToString();

        _grades = _certificate.Grades;
        
        GradeList.Items = _grades;

        if (_certificate.DateIssue.HasValue)
        {
            var dateIssue = _certificate.DateIssue.Value.ToDateTime(new TimeOnly(0));
            DateIssue.SelectedDate = dateIssue;
            DateIssue.DisplayDate = dateIssue;
        }
    }

    /// <summary>
    /// Инициализация событий
    /// </summary>
    private void InitializeEvents()
    {
        BackPage.Click += BackPageOnClick;
        AddGrade.Click += AddGradeOnClick;
        if (_isAddEditView is true)
        {
            SaveCertificate.Click += AddCertificateOnClick;
            return;
        }

        if (_isAddEditView is null)
        {
            Title = "Просмотр данных аттестата";
            MainText.Text = "Просмотр данных аттестата";

            SaveCertificate.IsVisible = false;
            AddGrade.IsVisible = false;

            Identify.IsEnabled = false;
            TypeEducation.IsEnabled = false;
            IsOriginal.IsEnabled = false;
            DateIssue.IsEnabled = false;
            GradeList.IsEnabled = false;
            return;
        }

        Title = "Редактирование данных аттестата";
        MainText.Text = "Редактирование данных аттестата";
        SaveCertificate.Content = "Редактировать";

        SaveCertificate.Click += SaveCertificateOnClick;
    }

    /// <summary>
    /// Добавление оценки в аттестат
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void AddGradeOnClick(object? sender, RoutedEventArgs e)
    {
        var grade = new GradeResponse();
        var accept = new CanAdd();
        await new AddGradeWindow(ref grade, ref accept).ShowDialog(this);

        if (!accept.IsAccepted)
        {
            return;
        }
        
        var grades = GradeList.Items as List<GradeResponse> ?? new();
        
        if (grades.FirstOrDefault(g => g.Lesson.Id == grade.Lesson.Id) is not null)
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
                    "Нельзя добавить предмет, который уже есть в аттестате",
                ButtonDefinitions = ButtonEnum.Ok
            }).ShowDialog(this);
            return;
        }
        
        grades.Add(grade);

        GradeList.Items = grades.ToList();
        
        AverangeGrade.Text = decimal.Round(
            ((decimal)grades.Sum(g => g.Score)) /
            grades.Count, 2).ToString();
    }
    
    /// <summary>
    /// Добавление нового сертификата
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void AddCertificateOnClick(object? sender, RoutedEventArgs e)
    {
        if (!await LoadDataToCurrent())
        {
            return;
        }
        _isAccepted.Accept();
        Close();
    }

    /// <summary>
    /// Сохранение изменений в сертификате 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void SaveCertificateOnClick(object? sender, RoutedEventArgs e)
    {
        if (!await LoadDataToCurrent())
        {
            return;
        }
        _isAccepted.Accept();
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
                "Вы уверены, что хотите закрыть данное окно и перейти обратно к окну студента? (Все не сохраненные данные исчезнут)",
            ButtonDefinitions = ButtonEnum.YesNo
        }).ShowDialog(this);

        if (res == ButtonResult.Yes)
        {
            Close();
        }
    }

    /// <summary>
    /// Получение данных из полей в класс аттестата
    /// </summary>
    /// <returns>Все ли сведения получены или нет</returns>
    private async Task<bool> LoadDataToCurrent()
    {
        // Проверка, что поле identify заполнено
        if (string.IsNullOrWhiteSpace(Identify.Text))
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
                    "Поле идентификатора не заполнено",
                ButtonDefinitions = ButtonEnum.Ok
            }).ShowDialog(this);
            return false;
        }
        
        // Проверка, что выбран тип аттестата
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
                ContentMessage =
                    "Не выбран тип аттестата",
                ButtonDefinitions = ButtonEnum.Ok
            }).ShowDialog(this);
            return false;
        }
        
        // Проверка оценок
        var grades = GradeList.Items as List<GradeResponse>;

        if (grades is not null)
        {
            foreach (var grade in grades.Where(grade => grade.Score is < 2 or > 5))
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
                        $"Оценка должна быть в диапазоне от 2 до 5, ошибка в предмете: {grade.Lesson.Title}",
                    ButtonDefinitions = ButtonEnum.Ok
                }).ShowDialog(this);
                return false;
            }
        }

        if (_removeGrades.Count > 0)
        {
            _certificate.RemoveGrades = _certificate.RemoveGrades ?? new();
            _certificate.RemoveGrades.AddRange(_removeGrades);
            _certificate.RemoveGrades = _certificate.RemoveGrades.Distinct().ToList();
        }

        _certificate.Type = (CertificateEducationTypeReposponse)TypeEducation.SelectedItem;

        _certificate.Grades = (grades ?? new()).ToList();

        _certificate.Identify = Identify.Text;
        _certificate.IsOriginal = IsOriginal.IsChecked ?? false;

        if (DateIssue.SelectedDate.HasValue)
        {
            _certificate.DateIssue = DateOnly.FromDateTime(DateIssue.SelectedDate.Value);
        }

        return true;
    }

    /// <summary>
    /// Удаление оценки
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        var selectGrade = GradeList.SelectedItem as GradeResponse;
        var grades = GradeList.Items as List<GradeResponse>;

        if (selectGrade!.Id is not null)
        {
            _removeGrades.Add((int)selectGrade.Id);
        }

        grades!.Remove(selectGrade);
        
        GradeList.Items = grades.ToList();
        
        AverangeGrade.Text = decimal.Round(
            ((decimal)grades.Sum(g => g.Score)) /
            grades.Count, 2).ToString();
    }
}