using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Desktop
{
    public partial class MainTasks : Page
    {
        private int _currentUserId;
        private string _userName;
        private List<Task> _tasks;
        private Task _selectedTask;
        private bool _showCompletedOnly = false;

        public MainTasks(int userId, string userName)
        {
            InitializeComponent();
            _currentUserId = userId;
            _userName = userName;

            this.Loaded += OnPageLoaded;
            

            LoadUserData();
            LoadTasks();
        }
        private void OnNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Content == this)
            {
                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
                this.BeginAnimation(OpacityProperty, fadeIn);

                LoadTasks();
            }
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            StartPageAnimation();
            this.Loaded -= OnPageLoaded;
        }

        private void StartPageAnimation()
        {
            try
            {
                var slideIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(400));
                slideIn.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut };
                this.BeginAnimation(OpacityProperty, slideIn);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка анимации входа: {ex.Message}");
            }
        }

        private void LoadUserData()
        {
            string firstLetter = _userName.Length > 0 ? _userName[0].ToString().ToUpper() : "U";
            AvatarTextBlock.Text = firstLetter;
            UserNameTextBlock.Text = _userName;
        }

        private void LoadTasks()
        {
            try
            {
                _tasks = TaskRepository.GetTasksByUser(_currentUserId);
                DisplayCategories();
                DisplayTasks();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки задач: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisplayCategories()
        {
            CategoriesPanel.Children.Clear();
            var allButton = CreateCategoryButton("Все");
            allButton.Click += (s, e) => DisplayTasks();
            CategoriesPanel.Children.Add(allButton);

            var tasksToShow = _showCompletedOnly
                ? _tasks.Where(t => t.IsCompleted).ToList()
                : _tasks.Where(t => !t.IsCompleted).ToList();

            var categories = tasksToShow
                .Select(t => t.Category)
                .Distinct()
                .ToList();

            if (categories.Count == 0)
            {
                categories = new List<string> { "Дом", "Работа", "Учеба", "Отдых" };
            }

            foreach (var category in categories)
            {
                var button = CreateCategoryButton(category);
                button.Click += (s, e) => DisplayTasks(category);
                CategoriesPanel.Children.Add(button);
            }
        }

        private Button CreateCategoryButton(string text)
        {
            var button = new Button
            {
                Content = text,
                Background = Brushes.Transparent,
                Foreground = Brushes.White,
                BorderBrush = Brushes.Transparent,
                FontSize = 14,
                Margin = new Thickness(0, 0, 20, 0),
                Padding = new Thickness(10, 5, 10, 5),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            button.RenderTransformOrigin = new Point(0.5, 0.5);
            button.RenderTransform = new ScaleTransform(1, 1);

            button.MouseEnter += (s, e) =>
            {
                var animation = new DoubleAnimation(1.1, TimeSpan.FromMilliseconds(200));
                button.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
                button.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
            };

            button.MouseLeave += (s, e) =>
            {
                var animation = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(200));
                button.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
                button.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
            };

            return button;
        }

        private void DisplayTasks(string category = null)
        {
            TasksPanel.Children.Clear();

            var filteredTasks = string.IsNullOrEmpty(category) || category == "Все"
                ? (_showCompletedOnly
                    ? _tasks.Where(t => t.IsCompleted).ToList()
                    : _tasks.Where(t => !t.IsCompleted).ToList())
                : (_showCompletedOnly
                    ? _tasks.Where(t => t.Category == category && t.IsCompleted).ToList()
                    : _tasks.Where(t => t.Category == category && !t.IsCompleted).ToList());

            if (_showCompletedOnly)
            {
                filteredTasks = filteredTasks.OrderByDescending(t => t.CreatedDate).ToList();
            }
            else
            {
                filteredTasks = filteredTasks.OrderBy(t => t.DueDate ?? DateTime.MaxValue).ToList();
            }

            if (filteredTasks.Count == 0)
            {
                var messageText = _showCompletedOnly
                    ? "Нет выполненных задач"
                    : "Нет активных задач";

                var messageBlock = new TextBlock
                {
                    Text = messageText,
                    Foreground = Brushes.Gray,
                    FontSize = 16,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 50, 0, 0)
                };
                TasksPanel.Children.Add(messageBlock);
            }
            else
            {
                foreach (var task in filteredTasks)
                {
                    TasksPanel.Children.Add(CreateTaskControl(task));
                }
            }
        }

        private Border CreateTaskControl(Task task)
        {
            var taskBorder = new Border
            {
                Background = Brushes.Transparent,
                BorderBrush = new SolidColorBrush(Color.FromRgb(85, 85, 85)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Margin = new Thickness(0, 0, 0, 10),
                Padding = new Thickness(10),
                Tag = task.Id,
                Cursor = System.Windows.Input.Cursors.Hand,
                Opacity = 0,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform(1, 1)
            };

            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
            taskBorder.BeginAnimation(Border.OpacityProperty, fadeIn);

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            if (_showCompletedOnly)
            {
                var checkIcon = new TextBlock
                {
                    Text = "✓",
                    Foreground = Brushes.Green,
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 10, 0)
                };
                Grid.SetColumn(checkIcon, 0);
                grid.Children.Add(checkIcon);
            }
            else
            {
                var checkBox = new CheckBox
                {
                    IsChecked = task.IsCompleted,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 10, 0),
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    RenderTransform = new ScaleTransform(1, 1)
                };

                checkBox.Checked += (s, e) =>
                {
                    var scaleAnimation = new DoubleAnimation(1.3, TimeSpan.FromMilliseconds(100));
                    scaleAnimation.AutoReverse = true;
                    checkBox.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
                    checkBox.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);

                    UpdateTaskStatus(task.Id, true);
                };

                checkBox.Unchecked += (s, e) => UpdateTaskStatus(task.Id, false);

                Grid.SetColumn(checkBox, 0);
                grid.Children.Add(checkBox);
            }

            var titleText = new TextBlock
            {
                Text = task.Title,
                Foreground = task.IsCompleted ?
                    new SolidColorBrush(Color.FromRgb(128, 128, 128)) :
                    Brushes.Black,
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center,
                TextDecorations = task.IsCompleted ? TextDecorations.Strikethrough : null
            };

            var dateText = new TextBlock
            {
                Text = task.DueDate.HasValue ?
                    task.DueDate.Value.ToString("HH:mm dd MMMM yyyy") :
                    "Нет даты",
                Foreground = new SolidColorBrush(Color.FromRgb(128, 128, 128)),
                FontSize = 12,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(20, 0, 0, 0)
            };

            Grid.SetColumn(titleText, 1);
            Grid.SetColumn(dateText, 2);

            grid.Children.Add(titleText);
            grid.Children.Add(dateText);

            taskBorder.Child = grid;

            taskBorder.MouseEnter += (s, e) =>
            {
                taskBorder.Background = new SolidColorBrush(Color.FromArgb(30, 0, 122, 204));
                var scaleAnimation = new DoubleAnimation(1.02, TimeSpan.FromMilliseconds(200));
                taskBorder.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
                taskBorder.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
            };

            taskBorder.MouseLeave += (s, e) =>
            {
                taskBorder.Background = Brushes.Transparent;
                var scaleAnimation = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(200));
                taskBorder.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
                taskBorder.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
            };

            taskBorder.MouseLeftButtonDown += (s, e) => SelectTask(task);

            return taskBorder;
        }

        private void SelectTask(Task task)
        {
            _selectedTask = task;
            SelectedTaskTitle.Text = task.Title;
            SelectedTaskDate.Text = task.DueDate.HasValue
                ? task.DueDate.Value.ToString("HH:mm dd MMMM yyyy")
                : "Нет даты";
            SelectedTaskDescription.Text = string.IsNullOrEmpty(task.Description)
                ? "Нет описания"
                : task.Description;

            CompleteButton.IsEnabled = !task.IsCompleted && !_showCompletedOnly;
            DeleteButton.IsEnabled = true;

            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
            SelectedTaskTitle.BeginAnimation(TextBlock.OpacityProperty, fadeIn);
            SelectedTaskDate.BeginAnimation(TextBlock.OpacityProperty, fadeIn);
            SelectedTaskDescription.BeginAnimation(TextBlock.OpacityProperty, fadeIn);
        }

        private void UpdateTaskStatus(int taskId, bool isCompleted)
        {
            try
            {
                if (TaskRepository.UpdateTaskStatus(taskId, isCompleted))
                {
                    var task = _tasks.FirstOrDefault(t => t.Id == taskId);
                    if (task != null)
                    {
                        task.IsCompleted = isCompleted;
                        if (_selectedTask?.Id == taskId)
                        {
                            SelectTask(task);
                        }
                        if (_showCompletedOnly)
                        {
                            DisplayTasks();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления статуса: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            StartPageTransition(() =>
            {
                CreateTask createTaskPage = new CreateTask(_currentUserId, _userName);
                createTaskPage.Returned += (s, args) =>
                {
                    LoadTasks();
                };

                NavigationService.Navigate(createTaskPage);

            });
        }

        private void CompleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTask != null && !_selectedTask.IsCompleted)
            {
                var button = sender as Button;
                var scaleAnimation = new DoubleAnimation(0.9, TimeSpan.FromMilliseconds(100));
                scaleAnimation.AutoReverse = true;
                button.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
                button.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);

                UpdateTaskStatus(_selectedTask.Id, true);
                CompleteButton.IsEnabled = false;
                LoadTasks();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTask != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить эту задачу?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (TaskRepository.DeleteTask(_selectedTask.Id))
                        {
                            _tasks.Remove(_selectedTask);
                            _selectedTask = null;

                            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
                            SelectedTaskTitle.BeginAnimation(TextBlock.OpacityProperty, fadeOut);
                            SelectedTaskDate.BeginAnimation(TextBlock.OpacityProperty, fadeOut);
                            SelectedTaskDescription.BeginAnimation(TextBlock.OpacityProperty, fadeOut);

                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                SelectedTaskTitle.Text = "Заголовок";
                                SelectedTaskDate.Text = "Нет даты";
                                SelectedTaskDescription.Text = "Выберите задачу для просмотра деталей";

                                CompleteButton.IsEnabled = false;
                                DeleteButton.IsEnabled = false;

                                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
                                SelectedTaskTitle.BeginAnimation(TextBlock.OpacityProperty, fadeIn);
                                SelectedTaskDate.BeginAnimation(TextBlock.OpacityProperty, fadeIn);
                                SelectedTaskDescription.BeginAnimation(TextBlock.OpacityProperty, fadeIn);
                            }), TimeSpan.FromMilliseconds(300));

                            LoadTasks();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            _showCompletedOnly = !_showCompletedOnly;

            var button = sender as Button;
            var rotateTransform = button.RenderTransform as RotateTransform;
            if (rotateTransform == null)
            {
                rotateTransform = new RotateTransform();
                button.RenderTransform = rotateTransform;
            }

            var rotateAnimation = new DoubleAnimation(_showCompletedOnly ? 180 : 0,
                TimeSpan.FromMilliseconds(300));
            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);

            if (_showCompletedOnly)
            {
                HistoryButton.Content = "←";
                HistoryButton.Background = new SolidColorBrush(Color.FromRgb(108, 117, 125));
                HistoryButton.BorderBrush = new SolidColorBrush(Color.FromRgb(108, 117, 125));
                HistoryButton.ToolTip = "Вернуться к активным задачам";
            }
            else
            {
                HistoryButton.Content = "✓";
                HistoryButton.Background = new SolidColorBrush(Color.FromRgb(40, 167, 69));
                HistoryButton.BorderBrush = new SolidColorBrush(Color.FromRgb(40, 167, 69));
                HistoryButton.ToolTip = "Показать историю выполненных задач";
            }

            if (_selectedTask != null)
            {
                _selectedTask = null;
                SelectedTaskTitle.Text = "Заголовок";
                SelectedTaskDate.Text = "18:00 01 Января 2022";
                SelectedTaskDescription.Text = "Lorem ipsum dolor sit amet, consectetur adipiscing.";
                CompleteButton.IsEnabled = false;
                DeleteButton.IsEnabled = false;
            }
            DisplayCategories();
            DisplayTasks();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти из профиля?",
                "Выход из профиля", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                StartPageTransition(() =>
                {
                    MainWindow loginPage = new MainWindow();
                    NavigationService.Navigate(loginPage);
                });
            }
        }

        private void StartPageTransition(Action navigationAction)
        {
            try
            {
                var slideOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
                slideOut.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut };

                this.BeginAnimation(OpacityProperty, slideOut);

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    navigationAction.Invoke();
                }), System.Windows.Threading.DispatcherPriority.Normal, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка анимации: {ex.Message}");
                navigationAction.Invoke();
            }
        }
    }
}