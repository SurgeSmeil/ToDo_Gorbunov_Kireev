using System;
using System.Windows;
using System.Windows.Controls;

namespace Desktop
{
    public partial class CreateTask : Page
    {
        private int _currentUserId;
        public event EventHandler Returned;

        public CreateTask(int userId)
        {
            InitializeComponent();
            _currentUserId = userId;

            DatePickerControl.SelectedDate = DateTime.Now;
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
                        NavigationService.GoBack();
                }
                else
                {
                    MessageBox.Show("Ошибка при создании задачи", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
                NavigationService.GoBack();
        }
    }
}