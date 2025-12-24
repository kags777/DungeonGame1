using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace DungeonGame1
{
    public partial class MainMenuPage : Page
    {
        private MainWindow mainWindow;
        private IMainMenuService menuService;

        public MainMenuPage(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window;
            menuService = new MainMenuService();
        }

        private void NewGameBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new LevelSelectionDialog(menuService, true);
            if (dialog.ShowDialog() == true)
            {
                mainWindow.StartGame(dialog.SelectedLevelId, true);
            }
        }

        private void ContinueBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveSelectionDialog(menuService);
            if (dialog.ShowDialog() == true)
            {
                // Передаем saveId и false (сохранение)
                mainWindow.StartGame(dialog.SelectedSaveId, false);

            }
        }

        private void EditorBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new LevelSelectionDialog(menuService, false);
            if (dialog.ShowDialog() == true)
            {
                mainWindow.OpenEditor(dialog.SelectedLevelId);
            }
            else
            {
                mainWindow.OpenEditor();
            }
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            menuService.ExitApp();
        }
    }
}