using System.Linq;
using System.Windows;

namespace DungeonGame1
{
    public partial class LevelSelectionDialog : Window
    {
        public string SelectedLevelId { get; private set; }
        private readonly IMainMenuService menuService;
        private readonly bool forGame;

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
            if (LevelsListBox.SelectedItem is LevelInfoDTO selected)
            {
                SelectedLevelId = selected.Id;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Выберите уровень!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (LevelsListBox.SelectedItem is LevelInfoDTO selected)
            {
                // Запрещаем удаление дефолтного уровня
                if (selected.Id == "default")
                {
                    MessageBox.Show("Нельзя удалить стандартный уровень!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы действительно хотите удалить уровень '{selected.Name}'?\n\n" +
                    $"ID: {selected.Id}\n" +
                    $"Размер: {selected.Width}×{selected.Height}",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    bool deleted = menuService.DeleteLevel(selected.Id);
                    if (deleted)
                    {
                        MessageBox.Show($"Уровень '{selected.Name}' успешно удален!",
                            "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadLevels(); // Перезагружаем список
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите уровень для удаления!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}