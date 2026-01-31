using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
            LoadUserData();
            LoadTasks();
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
            return new Button
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
                Cursor = System.Windows.Input.Cursors.Hand
            };

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
                    Margin = new Thickness(0, 0, 10, 0)
                };
                checkBox.Checked += (s, e) => UpdateTaskStatus(task.Id, true);
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
            CreateTask createTaskPage = new CreateTask(_currentUserId);
            createTaskPage.Returned += (s, args) =>
            {
                LoadTasks();
            };

            NavigationService.Navigate(createTaskPage);
        }

        private void CompleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedTask != null && !_selectedTask.IsCompleted)
            {
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

                            SelectedTaskTitle.Text = "Заголовок";
                            SelectedTaskDate.Text = "Нет даты";
                            SelectedTaskDescription.Text = "Выберите задачу для просмотра деталей";

                            CompleteButton.IsEnabled = false;
                            DeleteButton.IsEnabled = false;

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
                MainWindow loginPage = new MainWindow();
                NavigationService.Navigate(loginPage);
            }
        }
    }
}