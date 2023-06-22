using System;
using System.IO;
using System.Net.Http;
using ArchivistsDesktop.DataClass;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;

namespace ArchivistsDesktop.View;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        
        LoadSettingToField();
        
        InitializeEvent();
    }

    /// <summary>
    /// Загрузка настроек в поля 
    /// </summary>
    private async void LoadSettingToField()
    {
        if (!File.Exists("./server_address.txt"))
        {
            return;
        }
        using (StreamReader reader = new StreamReader("./server_address.txt"))
        {
            string address = await reader.ReadToEndAsync();
            Address.Text = address;
        }
    }

    /// <summary>
    /// Инициализация событий
    /// </summary>
    private void InitializeEvent()
    {
        BackPage.Click += BackPageOnClick;
        SaveSetting.Click += SaveSettingOnClick;
    }

    /// <summary>
    /// Переход обратно к окну приложения
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void BackPageOnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    /// Сохранение настроек
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void SaveSettingOnClick(object? sender, RoutedEventArgs e)
    {
        var res = await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
        {
            WindowIcon = this.Icon,
            ContentTitle = "Уведомление",
            ContentMessage = "Вы уверены, что хотите сохранить настройки?",
            ButtonDefinitions = ButtonEnum.YesNo
        }).ShowDialog(this);

        if (res != ButtonResult.Yes)
        {
            return;
        }

        var address = Address.Text;

        if (string.IsNullOrWhiteSpace(address))
        {
            await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
            {
                WindowIcon = this.Icon,
                ContentTitle = "Уведомление",
                ContentMessage = "Строка адреса пустая",
                ButtonDefinitions = ButtonEnum.Ok
            }).ShowDialog(this);
            return;
        }

        if (!Uri.IsWellFormedUriString(address, UriKind.Absolute))
        {
            await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
            {
                WindowIcon = this.Icon,
                ContentTitle = "Уведомление",
                ContentMessage = "Адрес заполнен неверно",
                ButtonDefinitions = ButtonEnum.Ok
            }).ShowDialog(this);
            return;
        }
        
        // полная перезапись файла 
        using (StreamWriter writer = new StreamWriter("server_address.txt", false))
        {
            await writer.WriteAsync(address);
        }

        // обновление клиента с учетом нового адреса
        ConnectData.Client = new HttpClient()
        {
            BaseAddress = new Uri(address)
        };
        
        await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
        {
            WindowIcon = this.Icon,
            ContentTitle = "Уведомление",
            ContentMessage = "Настройки успешно сохранены",
            ButtonDefinitions = ButtonEnum.Ok
        }).ShowDialog(this);
        
        Close();
    }
}