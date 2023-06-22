using System;
using System.Collections.Generic;
using System.Linq;
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

public partial class AddGradeWindow : Avalonia.Controls.Window
{
    private GradeResponse _grade;
    private CanAdd _isAccepted = new CanAdd();

    public AddGradeWindow()
    {
        InitializeComponent();
        _grade = new();
    }

    public AddGradeWindow(ref GradeResponse grade, ref CanAdd isAccepted)
    {
        InitializeComponent();

        _grade = grade;

        _isAccepted = isAccepted;

        LoadLessons();

        InitializeEvents();
    }

    /// <summary>
    /// Загрузка уроков в comboBox
    /// </summary>
    private async void LoadLessons()
    {
        // Строка авторизации в api
        var authString = Auth.GetAuth(ConnectData.Login, ConnectData.Password);

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "Lesson");
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

            var types = await response.Content.ReadFromJsonAsync<List<LessonResponse>>();

            Lessons.Items = types;
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
        SaveScore.Click += SaveScoreOnClick;
        BackPage.Click += BackPageOnClick;
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
                "Вы уверены, что хотите закрыть данное окно и перейти обратно к окну аттестата? (Все не сохраненные данные исчезнут)",
            ButtonDefinitions = ButtonEnum.YesNo
        }).ShowDialog(this);

        if (res == ButtonResult.Yes)
        {
            Close();
        }
    }

    /// <summary>
    /// Сохранение оценки
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void SaveScoreOnClick(object? sender, RoutedEventArgs e)
    {
        // Проверка, что выбран урок
        if (Lessons.SelectedItem is null)
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
                    "Не выбран предмет",
                ButtonDefinitions = ButtonEnum.Ok
            }).ShowDialog(this);
            return;
        }

        // Проверка, что все оценки в диапазоне от 2 до 5
        if (!short.TryParse(Score.Text, out var res) || res is < 2 or > 5)
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
                    "Оценка введена не верно, она должна быть в диапазоне от 2 до 5",
                ButtonDefinitions = ButtonEnum.Ok
            }).ShowDialog(this);
            return;
        }

        _grade.Score = res;
        _grade.Lesson = Lessons.SelectedItem as LessonResponse;

        _isAccepted.Accept();

        Close();
    }
}