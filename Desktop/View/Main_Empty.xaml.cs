using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Desktop
{
    public partial class Main_Empty : Page
    {
        private int _userId;
        private string _userName;

        public Main_Empty(int userId, string userName)
        {
            InitializeComponent();
            _userId = userId;
            _userName = userName;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null)
            {
                CreateTask createTaskPage = new CreateTask(_userId, _userName);
                NavigationService.Navigate(createTaskPage);
            }
            else
            {
                MessageBox.Show("Ошибка навигации", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ChangePhoto_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                Title = "Выберите новое фото профиля"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFilePath = openFileDialog.FileName;
                MessageBox.Show($"Выбрано новое фото: {System.IO.Path.GetFileName(selectedFilePath)}");
                avatarImage.Source = new BitmapImage(new Uri(selectedFilePath));
            }
        }

        public void LogOut_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Выход из профиля",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show("Выход выполнен успешно!");
                if (NavigationService != null)
                {
                    Window parentWindow = Window.GetWindow(this);

                    if (parentWindow != null)
                    {
                        Frame mainFrame = new Frame();
                        parentWindow.Content = mainFrame;
                        mainFrame.Navigate(new Main_Empty(_userId, _userName));
                    }
                }
            }
        }
    }
}