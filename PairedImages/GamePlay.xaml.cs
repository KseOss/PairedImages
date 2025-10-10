using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace PairedImages
{
    public partial class GamePlay : Window
    {
        private int movesCount = 0; //Счетчик ходов
        private int foundPairs = 0; //Количество найденных пар
        private const int totalPairs = 9; //Общее кол-во пар для поиска
        private Image firstSelected; //Первая выбранная карточка
        private Image secondSelected; //Вторая выбранная карточка
        private bool isProcessing = false;
        private DispatcherTimer gameTimer; //Таймер игры
        private TimeSpan elapsedTime; //Прошедшее время
        private Dictionary<Image, string> imagePairs = new Dictionary<Image, string>(); //Соответсвие карточек и их изображение
        private List<string> imageNames = new List<string> //Список имен изображений для карточек
        {
            "dotted line face", "face with peeking eye", "face with spiral eyes",
            "money mouth face", "sleeping face", "smiling face with hearts",
            "smiling face with horns", "smiling face with sunglasses", "star struck"
        };
        public GamePlay()
        {
            InitializeComponent();
            Height += 20;
            Width += 20;
            InitializeGame();
            SetupTimer();
        }
        private void InitializeGame()
        { //Сброс всех счетчиков и времени
            movesCount = 0; 
            foundPairs = 0;
            elapsedTime = TimeSpan.Zero;
            //Создаем список всех изображений
            var allImages = new List<string>();
            allImages.AddRange(imageNames); //Добавление оригинального списка
            allImages.AddRange(imageNames); //Дублирование списка для создания пар
            allImages = allImages.OrderBy(x => Guid.NewGuid()).ToList(); //Перемешиваем случайным образом
            //Нахождение всех элементов Image в основном Grid
            var imageControls = new List<Image>(); 
            FindVisualChildren<Image>(MainGrid, imageControls);
            imageControls = imageControls.Where(img => img.Name.StartsWith("Card")).OrderBy(img => img.Name).ToList(); //Фильтр карточек (элементы с именем начинается на "Card") и сортируем по имени
            //Очистка словаря соответствий для новой игры
            imagePairs.Clear();
            //Назначем изображение для каждой карточки
            for (int i = 0; i < allImages.Count && i < imageControls.Count; i++)
            {
                string imagePath = $"/Images/{allImages[i]}.png"; //Формируем путь к изображению
                imagePairs[imageControls[i]] = imagePath; //Сохраняем соотвтсвие карточки и ее изображение
                //Устанавливаем рубашку карточки (изначальное состояние)
                imageControls[i].Source = new BitmapImage(new Uri($"pack://application:,,,/Images/Квадрат.png"));
                imageControls[i].Tag = "hidden"; //установка состояния "скрыто"
                imageControls[i].MouseDown += Card_MouseDown; //Добавляем обработчик клика
            }
            UpdateCounters(); //обновление счетчика
            UpdateTimerDisplay(); //обновление таймера
            if (gameTimer != null) //перезапуска таймера если он существует
                gameTimer.Start();
        }
        private void SetupTimer() //настройка игрового таймера
        {
            elapsedTime = TimeSpan.Zero; //сбрасываем время
            gameTimer = new DispatcherTimer(); //создаем новый таймер
            gameTimer.Interval = TimeSpan.FromSeconds(1); //интервал 1 секунда
            gameTimer.Tick += Timer_Tick; //Добавляем обработчик тика
            gameTimer.Start(); //запускаем таймер
            UpdateTimerDisplay(); //Обновляем отображение времени
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            elapsedTime = elapsedTime.Add(TimeSpan.FromSeconds(1)); //увеличиваем время на 1 секунду
            UpdateTimerDisplay(); //обновление отображения
        }
        private void UpdateTimerDisplay()
        {
            TimerLabel.Content = $"{elapsedTime:mm\\:ss}";
        }

        private void Card_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isProcessing) return; //Если идет обработка предыдущего хода - игнорируем клик
            Image clickedImage = sender as Image;
            if (clickedImage == null || clickedImage.Tag?.ToString() == "matched" || clickedImage == firstSelected) //проверка - карточка существует, не найдена  и не является уже выбранной
                return;
            string imagePath = imagePairs[clickedImage]; // получение пути к изображению для карточки
            clickedImage.Source = new BitmapImage(new Uri($"pack://application:,,,{imagePath}")); //отображение карточки
            clickedImage.Tag = "revealed"; //установка состояния "открыто"
            if (firstSelected == null)
            {
                firstSelected = clickedImage; //если это первая карточка - сохраняем ее
            }
            else
            {
                secondSelected = clickedImage; //если вторая карточка
                isProcessing = true; //блокируем обработку новых кликов
                movesCount++; //увеличиваем счетчик ходов
                if (imagePairs[firstSelected] == imagePairs[secondSelected]) //проверка совпадения карточек
                {
                    firstSelected.Tag = "matched";
                    secondSelected.Tag = "matched";
                    foundPairs++;
                    // Сначала обновляем интерфейс, потом проверяем завершение
                    UpdateCounters();
                    ResetSelection();
                    CheckGameCompletion();
                }
                else
                { //если карточки не совпали
                    UpdateCounters();
                    var timer = new DispatcherTimer(); //создаем таймер для переворота карточек обратно
                    timer.Interval = TimeSpan.FromMilliseconds(1000); //задержка 1 секунда
                    timer.Tick += (s, args) =>
                    {
                        timer.Stop(); //остановка таймер
                        firstSelected.Source = new BitmapImage(new Uri("pack://application:,,,/Images/Квадрат.png"));
                        secondSelected.Source = new BitmapImage(new Uri("pack://application:,,,/Images/Квадрат.png"));
                        firstSelected.Tag = "hidden";
                        secondSelected.Tag = "hidden";
                        ResetSelection(); // сбрасываем выбор
                    };
                    timer.Start();
                }
            }
        }
        private void ResetSelection() //сброс выбранных карточек
        {
            firstSelected = null;
            secondSelected = null;
            isProcessing = false;
        }
        private void UpdateCounters() //обновление счетчиков на интерфейсе
        {
            MovesLabel.Content = movesCount.ToString();
            PairsLabel.Content = $"{foundPairs}/{totalPairs}";
        }
        private void CheckGameCompletion() //провекрка завершения игры (все пары найдены)
        {
            if (foundPairs >= totalPairs) // Используем >= для надежности
            {
                gameTimer.Stop();
                ShowGameCompletionDialog();
            }
        }
        private void ShowGameCompletionDialog() //диалоговое окно при завершении игры
        {
            var dialog = new Window()
            {
                Title = "Игра завершена",
                Height = 400,
                Width = 600,
                WindowStyle = WindowStyle.None, //убираем стандартныеп рамки окна 
                ResizeMode = ResizeMode.NoResize, //запрещаем измеренение размера
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = (Brush)new BrushConverter().ConvertFrom("#16172B"),
                BorderBrush = (Brush)new BrushConverter().ConvertFrom("#7E83F0"),
                BorderThickness = new Thickness(2),
                Owner = this
            };
            var grid = new Grid();
            dialog.Content = grid;
            var backgroundImage = new Image() //добавление фонового изображения
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/Images/Зерно.png")),
                Stretch = Stretch.Fill,
                Opacity = 0.7
            };
            grid.Children.Add(backgroundImage);
            var border = new Border() //рамка диалога
            {
                Background = (Brush)new BrushConverter().ConvertFrom("#16172B"),
                BorderBrush = (Brush)new BrushConverter().ConvertFrom("#7E83F0"),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(20),
                CornerRadius = new CornerRadius(10)
            };
            grid.Children.Add(border);
            var innerGrid = new Grid();
            border.Child = innerGrid;
            innerGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            innerGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            innerGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            innerGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            var titleLabel = new Label()
            {
                Content = "ПОЗДРАВЛЯЕМ!",
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = (Brush)new BrushConverter().ConvertFrom("#7E83F0"),
                FontSize = 28,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 20, 0, 10)
            };
            Grid.SetRow(titleLabel, 0);
            innerGrid.Children.Add(titleLabel);

            var messageLabel = new Label()
            {
                Content = "Вы нашли все парные картинки!",
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = Brushes.White,
                FontSize = 20,
                Margin = new Thickness(20, 10, 20, 5)
            };
            Grid.SetRow(messageLabel, 1);
            innerGrid.Children.Add(messageLabel);

            var statsLabel = new Label()
            {
                Content = $"Ходов: {movesCount}\nВремя: {elapsedTime:mm\\:ss}",
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = (Brush)new BrushConverter().ConvertFrom("#BEBEBE"),
                FontSize = 18,
                Margin = new Thickness(20, 5, 20, 10)
            };
            Grid.SetRow(statsLabel, 2);
            innerGrid.Children.Add(statsLabel);

            var buttonPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 30)
            };
            Grid.SetRow(buttonPanel, 3);
            innerGrid.Children.Add(buttonPanel);

            var playAgainButton = new Button()
            {
                Content = "ИГРАТЬ СНОВА",
                FontSize = 16,
                Background = Brushes.Transparent,
                BorderBrush = (Brush)new BrushConverter().ConvertFrom("#7E83F0"),
                BorderThickness = new Thickness(2),
                Foreground = (Brush)new BrushConverter().ConvertFrom("#D7D7D7"),
                Margin = new Thickness(5),
                Padding = new Thickness(15, 5, 15, 5),
                Cursor = Cursors.Hand
            };
            playAgainButton.Click += (s, e) => { dialog.DialogResult = true; };
            buttonPanel.Children.Add(playAgainButton);

            var mainMenuButton = new Button()
            {
                Content = "ГЛАВНОЕ МЕНЮ",
                FontSize = 16,
                Background = Brushes.Transparent,
                BorderBrush = (Brush)new BrushConverter().ConvertFrom("#7E83F0"),
                BorderThickness = new Thickness(2),
                Foreground = (Brush)new BrushConverter().ConvertFrom("#D7D7D7"),
                Margin = new Thickness(5),
                Padding = new Thickness(15, 5, 15, 5),
                Cursor = Cursors.Hand
            };
            mainMenuButton.Click += (s, e) => { dialog.DialogResult = false; };
            buttonPanel.Children.Add(mainMenuButton);

            var exitButton = new Button()
            {
                Content = "ВЫЙТИ",
                FontSize = 16,
                Background = Brushes.Transparent,
                BorderBrush = (Brush)new BrushConverter().ConvertFrom("#7E83F0"),
                BorderThickness = new Thickness(2),
                Foreground = (Brush)new BrushConverter().ConvertFrom("#D7D7D7"),
                Margin = new Thickness(5),
                Padding = new Thickness(15, 5, 15, 5),
                Cursor = Cursors.Hand
            };
            exitButton.Click += (s, e) => { Application.Current.Shutdown(); };
            buttonPanel.Children.Add(exitButton);

            var result = dialog.ShowDialog();

            if (result == true)
            {
                InitializeGame();
            }
            else if (result == false)
            {
                ReturnToMainMenu();
            }
        }
        private void ReturnToMainMenu() //возврат в главное меню
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e) //обработка клика по кнопке выхода
        {
            ReturnToMainMenu();
        }
        private static void FindVisualChildren<T>(DependencyObject depObj, List<T> children) where T : DependencyObject //метод для поиска дочерних элементов определенного типа (рекурсивный)
        {
            if (depObj != null)
            { //прходка по всем дочерним элементам
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        children.Add((T)child); //еслт элеиегь гужного типа - добавляем в список
                    }
                    FindVisualChildren(child, children);  //рекурсивно ищемм в дочерних элементах
                }
            }
        }
    }
}
