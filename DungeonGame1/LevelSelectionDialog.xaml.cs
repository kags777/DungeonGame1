using System.Linq;
using System.Windows;

namespace DungeonGame1
{
    public partial class LevelSelectionDialog : Window
    {
        public string SelectedLevelId { get; private set; }
        private IMainMenuService menuService;
        private bool forGame;

        public LevelSelectionDialog(IMainMenuService service, bool forGame)
        {
            InitializeComponent();
            menuService = service;
            this.forGame = forGame;
            LoadLevels();
        }

        private void LoadLevels()
        {
            var levels = menuService.GetAvailableLevels();
            LevelsListBox.ItemsSource = levels;

            if (levels.Any())
            {
                LevelsListBox.SelectedIndex = 0;
            }
        }

        private void SelectBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = LevelsListBox.SelectedItem as LevelInfoDTO;
            if (selected != null)
            {
                SelectedLevelId = selected.Id;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Выберите уровень!");
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}