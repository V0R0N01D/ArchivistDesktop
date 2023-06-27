using System;
using System.Collections.Generic;
using System.IO;
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
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using NameCaseLib;

namespace ArchivistsDesktop.View.Archive.Window;

public partial class AddEditViewStudentWindow : Avalonia.Controls.Window
{
    private EditStudentResponse _currentStudent = new()
    {
        Student = new StudentAllDataResponse()
    };

    // true - добавление, false - редактирование, null - просмотр
    private bool? _isAddEditView = true;

    public AddEditViewStudentWindow()
    {
        InitializeComponent();

        LoadStudentDataToField();

        InitializeEvents();

        LoadCertificateTypes();
    }

    public AddEditViewStudentWindow(EditStudentResponse selectStudent, bool isView)
    {
        InitializeComponent();

        _isAddEditView = isView ? null : false;

        _currentStudent = selectStudent;

        LoadStudentDataToField();

        InitializeEvents();

        LoadCertificateTypes();
    }

    /// <summary>
    /// Загрузка типов аттестатов в comboBox
    /// </summary>
    private async void LoadCertificateTypes()
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

            TypeCertificateEducation.Items = types;

            if (_isAddEditView != true && _currentStudent.Student.TypeCertificate is not null && types is not null)
            {
                var index = types.FindIndex(x => x.Id == _currentStudent.Student.TypeCertificate.Id);
                TypeCertificateEducation.SelectedIndex = index;
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
    /// Инициализация событий
    /// </summary>
    private void InitializeEvents()
    {
        BackPage.Click += BackPageOnClick;
        CheckDiplom.Click += CheckDiplomOnClick;
        AddOrder.Click += AddOrderOnClick;
        Orders.DoubleTapped += OrdersOnDoubleTapped;

        if (_isAddEditView is null)
        {
            Title = "Просмотр данных студента";
            MainText.Text = "Просмотр данных студента";
            CheckDiplom.Content = "Посмотреть документ об образовании";

            MakeDocx.IsVisible = true;
            MakeDocx.Click += MakeDocxOnClick;

            AddOrder.IsVisible = false;
            SaveStudent.IsVisible = false;

            LastName.IsReadOnly = true;
            FirstName.IsReadOnly = true;
            Patronymic.IsReadOnly = true;
            PassportSerial.IsReadOnly = true;
            PassportNumber.IsReadOnly = true;
            DocumentPlace.IsReadOnly = true;
            IsReleased.IsHitTestVisible = false;
            DateBirthday.IsHitTestVisible = false;
            TypeCertificateEducation.IsHitTestVisible = false;

            if (_currentStudent.Student.Diploma is null)
            {
                CheckDiplom.IsVisible = false;
            }

            return;
        }


        var menItem = new MenuItem()
        {
            Header = "Удалить",
        };
        menItem.Click += MenItemOnClick;
        Orders.ContextMenu = new ContextMenu()
        {
            Items = new List<MenuItem>()
            {
                menItem
            }
        };


        if (_isAddEditView is true)
        {
            SaveStudent.Click += AddStudentOnClick;
            return;
        }

        Title = "Редактирование данных студента";
        MainText.Text = "Редактирование данных студента";
        SaveStudent.Content = "Редактировать";

        SaveStudent.Click += SaveStudentOnClick;
    }

    /// <summary>
    /// Просмотр данных приказа
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void OrdersOnDoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (Orders.SelectedItem is not StudentOrderResponse selectOrder)
        {
            return;
        }

        await new AddEditViewOrder(selectOrder.Order, true).ShowDialog(this);
    }

    /// <summary>
    /// Создание документа
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void MakeDocxOnClick(object? sender, RoutedEventArgs e)
    {
        // Путь до папки на рабочем столе
        var pathDocuments = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "documents");
        // Путь до шаблонов
        var pathTemplate = Path.Combine(pathDocuments, "templates");

        if (!Directory.Exists(pathDocuments))
        {
            Directory.CreateDirectory(pathDocuments);
        }

        if (!Directory.Exists(pathTemplate))
        {
            Directory.CreateDirectory(pathTemplate);
            return;
        }

        // Выбор файла template -----------------------------------------------------------------------------------------------------------------------------------------------

        var selectTypeDoc = "SpravkaObychenie";

        var pathTemplateDoc = Path.Combine(pathTemplate, $"{selectTypeDoc}.docx");

        if (!File.Exists(pathTemplateDoc))
        {
            return;
        }

        using (var document = WordprocessingDocument.CreateFromTemplate(pathTemplateDoc))
        {
            var body = document.MainDocumentPart!.Document.Body;
            var textElements = body!.Descendants<Text>();

            var lastStudentOrder = _currentStudent.StudentOrders?.MaxBy(x => x.Order.OrderDate)?.Order;

            var inTitleArg = false;
            string currentArg = "";

            foreach (var text in textElements)
            {
                if (!inTitleArg && !text.Text.Contains('<'))
                {
                    continue;
                }

                if (text.Text.Contains('<'))
                {
                    inTitleArg = true;
                    text.Text = text.Text.Replace("<", "");
                    continue;
                }

                if (!text.Text.Contains('>'))
                {
                    currentArg += text.Text;
                    text.Text = "";
                    continue;
                }

                // Замена ФИО
                if (currentArg.Contains("fio"))
                {
                    var nc = new NameCaseLib.Ru();

                    var lastName = nc.Q(_currentStudent.Student.LastName);
                    var firstName = nc.QName(_currentStudent.Student.FirstName);
                    var patronomic = nc.QPatrName(_currentStudent.Student.Patronymic);

                    var padesh = 2;

                    var studentFio = $"{lastName[padesh]} {firstName[padesh]} {patronomic[padesh]}";

                    text.Text = text.Text.Replace(">", studentFio);
                }

                // замена курса
                if (currentArg.Contains("years"))
                {
                    var titleCourse =
                        _currentStudent.StudentOrders?.MaxBy(so => so.Order.OrderDate)?.Order.Group?.Year switch
                        {
                            1 => "первого",
                            2 => "второго",
                            3 => "третьего",
                            4 => "четвертого",
                            5 => "пятого",
                            _ => ""
                        };

                    text.Text = text.Text.Replace(">", titleCourse);
                }

                // замена даты рождения
                if (currentArg.Contains("date_birthday"))
                {
                    text.Text = text.Text.Replace(">", _currentStudent.Student.DateBirthday.ToString("d"));
                }

                if (currentArg.Contains("speciality"))
                {
                    var speciality = lastStudentOrder?.Group?.Speciality;
                    text.Text = text.Text.Replace(">", $"{speciality?.Fgos} {speciality?.Title}");
                }

                if (currentArg.Contains("type_start_education"))
                {
                    text.Text = text.Text.Replace(">", _currentStudent.Student.TypeCertificate?.Title);
                }

                if (currentArg.Contains("type_education"))
                {
                    var typeEducation = lastStudentOrder?.Group?.TypeEducation.Title.ToLower();
                    text.Text = text.Text.Replace(">", typeEducation);
                }

                if (currentArg.Contains("date_order"))
                {
                    text.Text = text.Text.Replace(">", lastStudentOrder?.OrderDate.ToString("d"));
                }

                if (currentArg.Contains("number_order"))
                {
                    text.Text = text.Text.Replace(">", lastStudentOrder?.Number);
                }

                if (currentArg.Contains("years"))
                {
                    var yearTitle = lastStudentOrder?.Group?.Year switch
                    {
                        1 => "первого",
                        2 => "второго",
                        3 => "третьего",
                        4 => "четвертого",
                        _ => "пятого"
                    };

                    text.Text = text.Text.Replace(">", yearTitle);
                }

                if (currentArg.Contains("date_start_order"))
                {
                    if ((bool)lastStudentOrder?.DateStartOrder.HasValue!)
                    {
                        text.Text = text.Text.Replace(">", lastStudentOrder.DateStartOrder!.Value.ToString("d"));
                    }
                }

                if (currentArg.Contains("date_end_education"))
                {
                    if ((bool)lastStudentOrder?.Group?.DateEndEducation.HasValue!)
                    {
                        text.Text = text.Text.Replace(">",
                            lastStudentOrder.Group!.DateEndEducation!.Value.ToString("d"));
                    }
                }


                // если не заменилось значение, то изменение его на пустое место
                if (text.Text.Contains('>'))
                {
                    text.Text = text.Text.Replace(">", "");
                }

                currentArg = "";
                inTitleArg = false;
            }

            document.SaveAs(Path.Combine(pathDocuments,
                                $"{selectTypeDoc}-{_currentStudent.Student.LastName}-{_currentStudent.Student.FirstName}") +
                            ".docx")
                .Close();
        }
    }

    /// <summary>
    /// Удаление приказа
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void MenItemOnClick(object? sender, RoutedEventArgs e)
    {
        if (Orders.SelectedItem is not StudentOrderResponse selectOrder)
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

        _currentStudent.StudentOrders!.Remove(selectOrder);
        Orders.Items = _currentStudent.StudentOrders.ToList();

        if (selectOrder.Id is null)
        {
            return;
        }

        _currentStudent.RemoveOrders = _currentStudent.RemoveOrders ?? new();
        _currentStudent.RemoveOrders.Add((int)selectOrder.Id!);

        Orders.Items = _currentStudent.StudentOrders.ToList();
    }

    /// <summary>
    /// Добавление приказа к пользователю
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void AddOrderOnClick(object? sender, RoutedEventArgs e)
    {
        // проверка, что приказ сохранили
        CanAdd isAcceptedOrderAdd = new();

        var selectOrderAdd = new OrderAllDataResponse();

        await new AddOrderWindow(ref selectOrderAdd, ref isAcceptedOrderAdd).ShowDialog(this);

        if (isAcceptedOrderAdd.IsAccepted)
        {
            _currentStudent.StudentOrders = _currentStudent.StudentOrders ?? new();
            _currentStudent.StudentOrders.Add(new StudentOrderResponse()
            {
                Order = selectOrderAdd
            });
            Orders.Items = _currentStudent.StudentOrders.ToList();
        }
    }

    /// <summary>
    /// Просмотр диплома
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void CheckDiplomOnClick(object? sender, RoutedEventArgs e)
    {
        if (_currentStudent.Student.Diploma is null)
        {
            if (_isAddEditView is null)
            {
                // нельзя открыть того чего нет в режиме просмотра
                return;
            }

            // проверка, что сертификат сохранили
            CanAdd isAcceptedDiplomAdd = new();

            var selectDiplomAdd = new DiplomResponse();

            await new AddEditViewDiplom(ref selectDiplomAdd, ref isAcceptedDiplomAdd).ShowDialog(this);

            if (isAcceptedDiplomAdd.IsAccepted)
            {
                _currentStudent.Student.Diploma = selectDiplomAdd;
            }

            LoadStudentDataToField();

            return;
        }

        CanAdd isAcceptedDiplomEdit = new();

        var selectDiplom = new DiplomResponse()
        {
            Id = _currentStudent.Student.Diploma.Id,
            DateIssue = _currentStudent.Student.Diploma.DateIssue,
            Number = _currentStudent.Student.Diploma.Number,
            Serial = _currentStudent.Student.Diploma.Serial
        };

        await new AddEditViewDiplom(ref selectDiplom,
            _isAddEditView is true ? false : _isAddEditView is null ? true : false,
            ref isAcceptedDiplomEdit).ShowDialog(this);

        if (_isAddEditView is null)
        {
            return;
        }

        if (isAcceptedDiplomEdit.IsAccepted)
        {
            _currentStudent.Student.Diploma = selectDiplom;
        }

        LoadStudentDataToField();
    }

    /// <summary>
    /// Переход обратно к просмотру студентов
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
                "Вы уверены, что хотите закрыть данное окно и перейти обратно к просмотру студентов? (Все не сохраненные данные исчезнут)",
            ButtonDefinitions = ButtonEnum.YesNo
        }).ShowDialog(this);

        if (res == ButtonResult.Yes)
        {
            Close();
        }
    }

    /// <summary>
    /// Добавление студента
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void AddStudentOnClick(object? sender, RoutedEventArgs e)
    {
        var res = await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
        {
            WindowIcon = this.Icon,
            ContentTitle = "Уведомление",
            CanResize = true,
            ContentMessage = "Вы уверены, что хотите добавить студента?",
            ButtonDefinitions = ButtonEnum.YesNo
        }).ShowDialog(this);

        if (res != ButtonResult.Yes)
        {
            return;
        }

        SaveStudent.IsEnabled = false;
        if (!await LoadDataToCurrent())
        {
            SaveStudent.IsEnabled = true;
            return;
        }

        if (!await CheckAllDataRight())
        {
            SaveStudent.IsEnabled = true;
            return;
        }

        // Строка авторизации в api
        var authString = Auth.GetAuth(ConnectData.Login, ConnectData.Password);
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"Students/");
            request.Content = JsonContent.Create(_currentStudent);
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
                SaveStudent.IsEnabled = true;
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
                    "Студент успешно добавлен",
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
            SaveStudent.IsEnabled = true;
        }
    }

    /// <summary>
    /// Сохранение изменения данных студента 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void SaveStudentOnClick(object? sender, RoutedEventArgs e)
    {
        var res = await MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams()
        {
            WindowIcon = UserData.currentWindow!.Icon,
            ContentTitle = "Уведомление",
            CanResize = true,
            ContentMessage = "Вы уверены, что хотите сохранить изменения студента?",
            ButtonDefinitions = ButtonEnum.YesNo
        }).ShowDialog(this);

        if (res != ButtonResult.Yes)
        {
            return;
        }

        SaveStudent.IsEnabled = false;
        if (!await LoadDataToCurrent())
        {
            SaveStudent.IsEnabled = true;
            return;
        }

        if (!await CheckAllDataRight())
        {
            SaveStudent.IsEnabled = true;
            return;
        }

        // Строка авторизации в api
        var authString = Auth.GetAuth(ConnectData.Login, ConnectData.Password);
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, $"Students/");
            request.Content = JsonContent.Create(_currentStudent);
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
                SaveStudent.IsEnabled = true;
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
                    "Студент успешно отредактирован",
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
            SaveStudent.IsEnabled = true;
        }
    }

    /// <summary>
    /// Загрузка сведений студента в поля
    /// </summary>
    private void LoadStudentDataToField()
    {
        DataContext = _currentStudent.Student;

        if (_currentStudent.Student.Diploma is not null)
        {
            CheckDiplom.Content = "Посмотреть документ об образовании";
        }

        if (_isAddEditView is true)
        {
            return;
        }

        Orders.Items = _currentStudent.StudentOrders;

        var birthday = _currentStudent.Student.DateBirthday.ToDateTime(new TimeOnly(0));
        DateBirthday.SelectedDate = birthday;
    }

    /// <summary>
    /// Получение не забинденных данных из полей в класс студента
    /// </summary>
    /// <returns>Все ли сведения получены или нет</returns>
    private async Task<bool> LoadDataToCurrent()
    {
        if (!DateBirthday.SelectedDate.HasValue)
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
                    "Не выбрана дата рождения",
                ButtonDefinitions = ButtonEnum.Ok
            }).ShowDialog(this);
            return false;
        }

        if (TypeCertificateEducation.SelectedItem is not null)
        {
            _currentStudent.Student.TypeCertificate =
                TypeCertificateEducation.SelectedItem as CertificateEducationTypeReposponse;
        }

        _currentStudent.Student.DateBirthday = DateOnly.FromDateTime(DateBirthday.SelectedDate.Value.Date);

        return true;
    }

    /// <summary>
    /// Проверка, что введены все обязательные поля
    /// </summary>
    /// <returns></returns>
    private async Task<bool> CheckAllDataRight()
    {
        if (string.IsNullOrWhiteSpace(LastName.Text))
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
                    "Не введена фамилия",
                ButtonDefinitions = ButtonEnum.Ok
            }).ShowDialog(this);
            return false;
        }

        if (string.IsNullOrWhiteSpace(FirstName.Text))
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
                    "Не введено имя",
                ButtonDefinitions = ButtonEnum.Ok
            }).ShowDialog(this);
            return false;
        }

        return true;
    }
}