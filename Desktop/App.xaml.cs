using System;
using System.Windows;

namespace Desktop
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                SQLitePCL.Batteries.Init();
                Console.WriteLine("SQLite успешно инициализирован");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации SQLite: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine($"Ошибка SQLite: {ex.Message}");
            }

            try
            {
                UserRepository.InitializeDatabase();
                Console.WriteLine("База данных успешно инициализирована");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации базы данных: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine($"Ошибка БД: {ex.Message}");
            }

            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                HandleException("Необработанное исключение", args.ExceptionObject as Exception);
            };

            this.DispatcherUnhandledException += (sender, args) =>
            {
                //HandleException("Исключение в UI потоке", args.Exception);
                args.Handled = true;
            };
        }

        private void HandleException(string context, Exception ex)
        {
            string errorMessage = $"{context}: {ex?.Message ?? "Неизвестная ошибка"}";

            Console.WriteLine($"ОШИБКА: {errorMessage}");
            Console.WriteLine($"Стек вызова: {ex?.StackTrace}");

            if (System.Windows.Threading.Dispatcher.FromThread(System.Threading.Thread.CurrentThread) != null)
            {
              //  MessageBox.Show(errorMessage, "Ошибка",
                  //  MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Console.WriteLine("Приложение завершает работу");
            base.OnExit(e);
        }
    }
}