using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Desktop
{
    public partial class CreateTask : Page
    {
        private string _currentUserName;
        private int _currentUserId;
        public event EventHandler Returned;

        public CreateTask(int userId, string userName)
        {
            InitializeComponent();
            _currentUserId = userId;
            _currentUserName = userName;

            DatePickerControl.SelectedDate = DateTime.Now;
            TimePickerControl.SelectedIndex = 8;

            StartFadeInAnimation();
            
        }

        private void StartFadeInAnimation()
        {
            this.Opacity = 0;
            var animation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
            animation.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut };
            this.BeginAnimation(Page.OpacityProperty, animation);
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите название задачи", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Task task = new Task
                {
                    Title = TitleTextBox.Text,
                    Category = (CategoryComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Дом",
                    Description = DescriptionTextBox.Text,
                    UserId = _currentUserId
                };

                if (DatePickerControl.SelectedDate.HasValue && TimePickerControl.SelectedItem != null)
                {
                    string timeString = (TimePickerControl.SelectedItem as ComboBoxItem)?.Content?.ToString();

                    if (!string.IsNullOrEmpty(timeString))
                    {
                        var dateTime = DatePickerControl.SelectedDate.Value;

                        if (timeString.Contains("AM") || timeString.Contains("PM"))
                        {
                            timeString = timeString.Replace(" AM", "").Replace(" PM", "");
                            if (DateTime.TryParse(timeString, out DateTime time))
                            {
                                task.DueDate = dateTime.Date.Add(time.TimeOfDay);
                            }
                        }
                        else
                        {
                            if (TimeSpan.TryParse(timeString, out TimeSpan timeSpan))
                            {
                                task.DueDate = dateTime.Date.Add(timeSpan);
                            }
                        }
                    }
                }

                if (TaskRepository.CreateTask(task))
                {
                    MessageBox.Show("Задача успешно создана!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    Returned?.Invoke(this, EventArgs.Empty);
                    NavigateBack();
                }
                else
                {
                    //MessageBox.Show("Ошибка при создании задачи", "Ошибка",
                        //MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                   // MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateBack();
        }


        private void NavigateBack()
        {
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
            fadeOut.EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn };

            fadeOut.Completed += (s, e) =>
            {
                this.BeginAnimation(Page.OpacityProperty, null);
                this.Opacity = 1;

                Returned?.Invoke(this, EventArgs.Empty);

                Dispatcher.BeginInvoke(new Action(() =>
                {
                            NavigationService?.Navigate(new MainTasks(_currentUserId, _currentUserName));
                }), System.Windows.Threading.DispatcherPriority.Normal);
            };

            this.BeginAnimation(Page.OpacityProperty, fadeOut);
        }

    }
}