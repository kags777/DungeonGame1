using System.IO;
using System.Linq;
using System.Windows;
using Newtonsoft.Json;

namespace DungeonGame1
{
    public partial class SaveSelectionDialog : Window
    {
        public string SelectedSaveId { get; private set; }
        public string SelectedLevelId { get; private set; }
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
                // Загружаем полные данные сохранения
                var savePath = Path.Combine("Saves", $"{selected.Id}.json");
                if (File.Exists(savePath))
                {
                    try
                    {
                        var json = File.ReadAllText(savePath);
                        var save = JsonConvert.DeserializeObject<SaveData>(json);

                        SelectedSaveId = selected.Id;
                        SelectedLevelId = save.LevelId;
                        DialogResult = true;
                    }
                    catch
                    {
                        MessageBox.Show("Ошибка загрузки сохранения", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите сохранение!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
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