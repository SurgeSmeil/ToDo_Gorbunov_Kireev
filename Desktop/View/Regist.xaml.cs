using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Desktop
{
    public partial class Regist : Page
    {
        public Regist()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {

                NavigationService.GoBack();
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text;
            string password = PasswordBox1.Text;
            string password1 = PasswordBox2.Text;
            string name = NameTextBox.Text;

            if (ValidateEmail(email) && ValidatePassword(password) && ValidateName(name) && password == password1)
            {
                if (UserRepository.RegisterUser(email, password, name))
                {
                    MessageBox.Show("Регистрация успешно проведена!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    var user = UserRepository.GetUserByEmail(email);
                    if (user != null)
                    {
                        bool hasTasks = TaskRepository.UserHasTasks(user.Id);

                        if (hasTasks)
                        {
                            NavigationService.Navigate(new MainTasks(user.Id, user.Name));
                        }
                        else
                        {
                            NavigationService.Navigate(new Main_Empty(user.Id, user.Name));
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Пользователь с таким email уже существует",
                        "Ошибка регистрации", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                string errorMessage = "";

                if (!ValidateEmail(email))
                {
                    errorMessage += "Неверный формат почты\n";
                }

                if (!ValidatePassword(password))
                {
                    errorMessage += "Пароль меньше 6 симв.\n";
                }

                if (!ValidateName(name))
                {
                    errorMessage += "Имя короче 3 симв.\n";
                }

                if (password != password1)
                {
                    errorMessage += "Пароли не совпадают\n";
                }

                MessageBox.Show(errorMessage, "Ошибка регистрации",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateEmail(string email)
        {
            if (string.IsNullOrEmpty(email) || email == "exam@yandex.ru") return false;

            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            return Regex.IsMatch(email, pattern);
        }

        private bool ValidatePassword(string password)
        {
            return !string.IsNullOrEmpty(password) && password.Length >= 6 && password != "Введите пароль";
        }

        private bool ValidateName(string name)
        {
            return !string.IsNullOrEmpty(name) && name.Length >= 3 && name != "Введите имя пользователя";
        }
        private void NameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (NameTextBox.Text == "Введите имя пользователя")
            {
                NameTextBox.Text = "";
                NameTextBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void NameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                NameTextBox.Text = "Введите имя пользователя";
                NameTextBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void EmailTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (EmailTextBox.Text == "exam@yandex.ru")
            {
                EmailTextBox.Text = "";
                EmailTextBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void EmailTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                EmailTextBox.Text = "exam@yandex.ru";
                EmailTextBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void PasswordBox1_GotFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordBox1.Text == "Введите пароль")
            {
                PasswordBox1.Text = "";
                PasswordBox1.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void PasswordBox1_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PasswordBox1.Text))
            {
                PasswordBox1.Text = "Введите пароль";
                PasswordBox1.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void PasswordBox2_GotFocus(object sender, RoutedEventArgs e)
        {
            if (PasswordBox2.Text == "Повторите пароль")
            {
                PasswordBox2.Text = "";
                PasswordBox2.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void PasswordBox2_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PasswordBox2.Text))
            {
                PasswordBox2.Text = "Повторите пароль";
                PasswordBox2.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }
    }
}