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

        public void StartGame(string levelId, bool isNewGame = true)
        {
            MainFrame.Navigate(new GamePage(this, levelId, isNewGame));
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