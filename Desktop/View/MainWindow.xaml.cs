using System;
using System.Windows;
using System.Windows.Controls;

namespace Desktop
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            UserRepository.InitializeDatabase();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string email = LoginTextBox.Text;
            string password = PasswordTextBox.Text;

            if (string.IsNullOrEmpty(email) || email == "Введите почту" ||
                string.IsNullOrEmpty(password) || password == "Введите пароль")
            {
                ErrorMessageLabel.Content = "Введите почту и пароль";
                return;
            }

            var user = UserRepository.AuthenticateUser(email, password);
            if (user != null)
            {
                bool hasTasks = TaskRepository.UserHasTasks(user.Id);

                if (hasTasks)
                {
                    MainFrame.Navigate(new MainTasks(user.Id, user.Name));
                }
                else
                {
                    MainFrame.Navigate(new Main_Empty(user.Id, user.Name));
                }
            }
            else
            {
                ErrorMessageLabel.Content = "Неверная почта или пароль";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Regist());
        }
    }
}