using ArchivistsDesktop.View.ArchiveWindow.Pages;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Enums;
using MessageBox.Avalonia.DTO;
using ArchivistsDesktop.DataClass;
using ArchivistsDesktop.View.Admin.Pages;

namespace ArchivistsDesktop.View
{
    public partial class DefaultWindow : Window
    {
        public DefaultWindow()
        {
            InitializeComponent();
        }

        public DefaultWindow(bool isArchive)
        {
            InitializeComponent();

            InitializeEvent();

            SeeMainPage(isArchive);
        }

        /// <summary>
        /// Инициализация событий
        /// </summary>
        private void InitializeEvent()
        {
            ExitItem.Click += ExitItem_Click;
            SettingsItem.Click += SettingsItem_Click;
        }

        /// <summary>
        /// Отображение окна настроек
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SettingsItem_Click(object? sender, RoutedEventArgs e)
        {
            await new SettingsWindow().ShowDialog(this);
        }

        /// <summary>
        /// Выход в окно авторизации
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ExitItem_Click(object? sender, RoutedEventArgs e)
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
                ContentMessage = "Вы уверены, что хотите закрыть данное окно и перейти к авторизации (Все не сохраненные данные исчезнут)?",
                ButtonDefinitions = ButtonEnum.YesNo
            }).ShowDialog(this);

            if (res == ButtonResult.Yes)
            {
                new MainWindow().Show();
                this.Close();
            }
        }

        /// <summary>
        /// Отображение стартовой страницы
        /// </summary>
        private void SeeMainPage(bool isAchive)
        {
            if (isAchive)
            {
                UserData.currentPage = new MainArchivePage();
                PageControl.Content = UserData.currentPage;
                return;
            }

            UserData.currentPage = new UsersPage();
            PageControl.Content = UserData.currentPage;
        }

        /// <summary>
        /// Отображение выбранной страницы
        /// </summary>
        /// <param name="selectPage">Выбранная страница</param>
        internal void DisplayPage(UserControl selectPage)
        {
            UserData.PutPreviousPage(UserData.currentPage!);
            UserData.currentPage = selectPage;
            PageControl.Content = UserData.currentPage;
        }

        /// <summary>
        /// Отображение прошлой страницы
        /// </summary>
        internal void DisplayBackPage()
        {
            var previousPage = UserData.GetPreviousPage();
            if (previousPage != null)
            {
                UserData.currentPage = previousPage;
                PageControl.Content = UserData.currentPage;
            }
        }
    }
}
