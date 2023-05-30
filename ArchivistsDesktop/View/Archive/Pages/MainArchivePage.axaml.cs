using Avalonia.Controls;
using Avalonia.Interactivity;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using MessageBox.Avalonia;
using ArchivistsDesktop.DataClass;
using ArchivistsDesktop.View.Archive.Pages;

namespace ArchivistsDesktop.View.ArchiveWindow.Pages
{
    public partial class MainArchivePage : UserControl
    {
        public MainArchivePage()
        {
            InitializeComponent();

            InitializeEvent();
        }

        /// <summary>
        /// Инициализация событий
        /// </summary>
        private void InitializeEvent()
        {
            ListStudent.Click += ListStudent_Click;
            ListGroup.Click += ListGroup_Click;
        }

        #region События
        /// <summary>
        /// Просмотр списка студентов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListGroup_Click(object? sender, RoutedEventArgs e)
        {
            
        }

        /// <summary>
        /// Просмотр списка групп
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListStudent_Click(object? sender, RoutedEventArgs e)
        {
            UserData.currentWindow!.DisplayPage(new StudentsPage());
        }
        #endregion
    }
}
