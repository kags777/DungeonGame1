using System.IO;
using System.Linq;
using System.Windows;

namespace DungeonGame1
{
    public partial class SaveSelectionDialog : Window
    {
        public string SelectedSaveId { get; private set; }
        private IMainMenuService menuService;

        public SaveSelectionDialog(IMainMenuService service)
        {
            InitializeComponent();
            menuService = service;
            LoadSaves();
        }

        private void LoadSaves()
        {
            var saves = menuService.GetAvailableSaves();
            SavesListBox.ItemsSource = saves;

            if (saves.Any())
            {
                SavesListBox.SelectedIndex = 0;
            }
        }

        private void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = SavesListBox.SelectedItem as SaveInfoDTO;
            if (selected != null)
            {
                SelectedSaveId = selected.Id;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Выберите сохранение!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = SavesListBox.SelectedItem as SaveInfoDTO;
            if (selected != null)
            {
                var result = MessageBox.Show($"Удалить сохранение от {selected.SaveTime}?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var savePath = Path.Combine("Saves", $"{selected.Id}.json");
                    if (File.Exists(savePath))
                    {
                        File.Delete(savePath);
                        LoadSaves();
                    }
                }
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}