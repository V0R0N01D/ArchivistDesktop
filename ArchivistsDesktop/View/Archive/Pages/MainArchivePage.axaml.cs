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
            Specialities.Click += SpecialitiesOnClick;
            Students.Click += ListStudent_Click;
            Groups.Click += ListGroup_Click;
            Orders.Click += OrdersOnClick;
        }

        #region События
        /// <summary>
        /// Просмотр списка групп
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListGroup_Click(object? sender, RoutedEventArgs e)
        {
            UserData.currentWindow!.DisplayPage(new GroupsPage());
        }

        /// <summary>
        /// Просмотр списка студентов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListStudent_Click(object? sender, RoutedEventArgs e)
        {
            UserData.currentWindow!.DisplayPage(new StudentsPage());
        }
        
        /// <summary>
        /// Просмотр списка приказов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OrdersOnClick(object? sender, RoutedEventArgs e)
        {
            UserData.currentWindow!.DisplayPage(new OrdersPage());
        }
        
        /// <summary>
        /// Просмотр списка специальностей
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SpecialitiesOnClick(object? sender, RoutedEventArgs e)
        {
            UserData.currentWindow!.DisplayPage(new SpecialitiesPage());
        }
        #endregion
    }
}
