using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DungeonGame1
{
    public partial class EditorPage : Page
    {
        private MainWindow mainWindow;
        private ILevelEditorService editorService;
        private EditorStateDTO currentState;
        private EntityVisualType selectedEntity = EntityVisualType.Wall;

        public EditorPage(MainWindow window, string levelId = null)
        {
            InitializeComponent();
            mainWindow = window;
            editorService = new LevelEditorService();

            if (!string.IsNullOrEmpty(levelId))
            {
                currentState = editorService.LoadLevel(levelId);
                LevelNameBox.Text = currentState.LevelName;
            }
            else
            {
                currentState = editorService.CreateNewLevel();
            }

            InitializeEditor();
        }

        private void InitializeEditor()
        {
            // Загружаем список объектов
            var entities = editorService.GetAvailableEntities();
            EntitiesListBox.ItemsSource = entities;
            EntitiesListBox.SelectedItem = entities.FirstOrDefault(e => e.Type == selectedEntity);

            // Отображаем карту
            UpdateEditorDisplay();
        }

        private void UpdateEditorDisplay()
        {
            var displayTiles = new List<DisplayTile>();

            for (int y = 0; y < currentState.Height; y++)
            {
                for (int x = 0; x < currentState.Width; x++)
                {
                    var tile = currentState.Map.FirstOrDefault(t => t.X == x && t.Y == y);
                    if (tile != null)
                    {
                        displayTiles.Add(new DisplayTile(tile));
                    }
                    else
                    {
                        displayTiles.Add(new DisplayTile(new TileDTO
                        {
                            X = x,
                            Y = y,
                            EntityType = EntityVisualType.Empty
                        }));
                    }
                }
            }

            EditorGrid.ItemsSource = displayTiles;
        }

        private void EntitiesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EntitiesListBox.SelectedItem is AvailableEntityDTO selected)
            {
                selectedEntity = selected.Type;
            }
        }

        private void Tile_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is DisplayTile displayTile)
            {
                // Находим координаты
                var index = EditorGrid.Items.IndexOf(displayTile);
                int x = index % currentState.Width;
                int y = index / currentState.Width;

                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    currentState = editorService.PlaceEntity(x, y, selectedEntity);
                }
                else if (e.RightButton == MouseButtonState.Pressed)
                {
                    currentState = editorService.RemoveEntity(x, y);
                }

                UpdateEditorDisplay();
            }
        }

        private void NewLevelBtn_Click(object sender, RoutedEventArgs e)
        {
            currentState = editorService.CreateNewLevel();
            LevelNameBox.Text = currentState.LevelName;
            UpdateEditorDisplay();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            var result = editorService.SaveLevelAs(LevelNameBox.Text);
            if (result == AppState.MainMenu)
            {
                MessageBox.Show("Уровень сохранен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                mainWindow.NavigateToMainMenu();
            }
        }

        private void MenuBtn_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.NavigateToMainMenu();
        }
    }
}