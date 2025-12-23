using System;
using System.Windows;
using System.Windows.Controls;

namespace DungeonGame1
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            NavigateToMainMenu();
        }

        public void NavigateToMainMenu()
        {
            MainFrame.Navigate(new MainMenuPage(this));
        }

        public void StartGame(string id, bool isNewGame = true)
        {
            try
            {
                if (isNewGame)
                {
                    Console.WriteLine($"Starting NEW game with levelId: {id}");
                }
                else
                {
                    Console.WriteLine($"Loading SAVE with saveId: {id}");
                }

                MainFrame.Navigate(new GamePage(this, id, isNewGame));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при запуске игры: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                NavigateToMainMenu();
            }
        }

        public void OpenEditor(string levelId = null)
        {
            MainFrame.Navigate(new EditorPage(this, levelId));
        }

        // Метод для возврата на предыдущую страницу (если нужно)
        public void GoBack()
        {
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
            }
        }
    }
}