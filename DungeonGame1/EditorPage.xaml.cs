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

        public int EditorWidth => currentState?.Width ?? 10;
        public int EditorHeight => currentState?.Height ?? 10;

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

            DataContext = this;
            Loaded += EditorPage_Loaded;
        }

        private void EditorPage_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeEditor();
        }


        private void InitializeEditor()
        {
            // Создаем AvailableEntityDTO с символами
            var entities = new List<AvailableEntityDTO>
            {
                new AvailableEntityDTO { Type = EntityVisualType.Player, Name = "Игрок", Symbol = "☻" },
                new AvailableEntityDTO { Type = EntityVisualType.Enemy, Name = "Враг", Symbol = "👹" },
                new AvailableEntityDTO { Type = EntityVisualType.Wall, Name = "Стена", Symbol = "█" },
                new AvailableEntityDTO { Type = EntityVisualType.Trap, Name = "Ловушка", Symbol = "⚠" },
                new AvailableEntityDTO { Type = EntityVisualType.Crystal, Name = "Кристалл", Symbol = "💎" },
                new AvailableEntityDTO { Type = EntityVisualType.Exit, Name = "Выход", Symbol = "🚪" },
                new AvailableEntityDTO { Type = EntityVisualType.Empty, Name = "Пусто", Symbol = "·" }
            };

            EntitiesListBox.ItemsSource = entities;
            UpdateEntitySelection();

            // Устанавливаем размеры
            WidthBox.Text = currentState.Width.ToString();
            HeightBox.Text = currentState.Height.ToString();

            UpdateEditorDisplay();
        }

        private void UpdateEditorDisplay()
        {
            var displayTiles = new List<EditorDisplayTile>();

            // Создаем все клетки
            for (int y = 0; y < currentState.Height; y++)
            {
                for (int x = 0; x < currentState.Width; x++)
                {
                    var tile = currentState.Map.FirstOrDefault(t => t.X == x && t.Y == y);
                    displayTiles.Add(new EditorDisplayTile(tile ?? new TileDTO
                    {
                        X = x,
                        Y = y,
                        EntityType = EntityVisualType.Empty
                    }));
                }
            }

            EditorGrid.ItemsSource = displayTiles;
        }

        private void UpdateEntitySelection()
        {
            var entities = EntitiesListBox.ItemsSource as IEnumerable<AvailableEntityDTO>;
            if (entities != null)
            {
                var selected = entities.FirstOrDefault(e => e.Type == selectedEntity);
                EntitiesListBox.SelectedItem = selected;
            }
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
            if (sender is Border border && border.DataContext is EditorDisplayTile displayTile)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    // ЛКМ - ставим объект
                    currentState = editorService.PlaceEntity(displayTile.X, displayTile.Y, selectedEntity);
                    UpdateEditorDisplay();
                }
                else if (e.RightButton == MouseButtonState.Pressed)
                {
                    // ПКМ - очищаем
                    currentState = editorService.RemoveEntity(displayTile.X, displayTile.Y);
                    UpdateEditorDisplay();
                }
                else if (e.MiddleButton == MouseButtonState.Pressed)
                {
                    // Средняя кнопка - выбираем объект под курсором
                    var tile = currentState.Map.FirstOrDefault(t => t.X == displayTile.X && t.Y == displayTile.Y);
                    if (tile != null && tile.EntityType != EntityVisualType.Empty)
                    {
                        selectedEntity = tile.EntityType;
                        UpdateEntitySelection();
                    }
                }
            }
        }

        private void Page_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Чтобы фокус не терялся
            Focus();
        }

        private void SizeBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (WidthBox == null || HeightBox == null)
                return;

            if (currentState == null)
                return; // ← вот этого у тебя не было

            if (int.TryParse(WidthBox.Text, out int width) &&
                int.TryParse(HeightBox.Text, out int height) &&
                width > 0 && width <= 50 &&
                height > 0 && height <= 50)
            {
                currentState.Width = width;
                currentState.Height = height;

                if (currentState.Map != null)
                    currentState.Map.RemoveAll(t => t.X >= width || t.Y >= height);

                UpdateEditorDisplay();
            }
        }



        private void NewLevelBtn_Click(object sender, RoutedEventArgs e)
        {
            currentState = editorService.CreateNewLevel();

            if (currentState == null)
            {
                MessageBox.Show("Не удалось создать новый уровень.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            LevelNameBox.Text = currentState.LevelName;
            WidthBox.Text = currentState.Width.ToString();
            HeightBox.Text = currentState.Height.ToString();
            UpdateEditorDisplay();
        }


        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LevelNameBox.Text))
            {
                MessageBox.Show("Введите название уровня!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = editorService.SaveLevelAs(LevelNameBox.Text);
            if (result == AppState.MainMenu)
            {
                MessageBox.Show($"Уровень '{LevelNameBox.Text}' сохранен!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                mainWindow.NavigateToMainMenu();
            }
        }

        private void MenuBtn_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Выйти без сохранения?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                mainWindow.NavigateToMainMenu();
            }
        }
    }

    public class EditorDisplayTile
    {
        public int X { get; set; }
        public int Y { get; set; }
        public EntityVisualType EntityType { get; set; }
        public FacingDirection FacingDirection { get; set; }
        public string Symbol { get; private set; }
        public Brush Color { get; private set; }
        public Brush BackgroundColor { get; private set; }
        public string ToolTipText { get; private set; }

        public EditorDisplayTile(TileDTO tile)
        {
            X = tile.X;
            Y = tile.Y;
            EntityType = tile.EntityType;
            FacingDirection = tile.FacingDirection;
            UpdateVisuals();
        }

        public void UpdateVisuals()
        {
            switch (EntityType)
            {
                case EntityVisualType.Player:
                    Symbol = GetPlayerSymbol(FacingDirection);
                    Color = Brushes.White;
                    BackgroundColor = Brushes.DarkBlue;
                    ToolTipText = $"Игрок ({X}, {Y})";
                    break;
                case EntityVisualType.Enemy:
                    Symbol = "👹";
                    Color = Brushes.Red;
                    BackgroundColor = Brushes.DarkRed;
                    ToolTipText = $"Враг ({X}, {Y})";
                    break;
                case EntityVisualType.Wall:
                    Symbol = "█";
                    Color = Brushes.Gray;
                    BackgroundColor = Brushes.DarkSlateGray;
                    ToolTipText = $"Стена ({X}, {Y})";
                    break;
                case EntityVisualType.Trap:
                    Symbol = "⚠";
                    Color = Brushes.Orange;
                    BackgroundColor = Brushes.DarkOrange;
                    ToolTipText = $"Ловушка ({X}, {Y})";
                    break;
                case EntityVisualType.Crystal:
                    Symbol = "💎";
                    Color = Brushes.Gold;
                    BackgroundColor = Brushes.DarkGoldenrod;
                    ToolTipText = $"Кристалл ({X}, {Y})";
                    break;
                case EntityVisualType.Exit:
                    Symbol = "🚪";
                    Color = Brushes.LightGreen;
                    BackgroundColor = Brushes.DarkGreen;
                    ToolTipText = $"Выход ({X}, {Y})";
                    break;
                default:
                    Symbol = "·";
                    Color = Brushes.DimGray;
                    BackgroundColor = Brushes.Transparent;
                    ToolTipText = $"Пусто ({X}, {Y})";
                    break;
            }
        }

        private string GetPlayerSymbol(FacingDirection direction)
        {
            switch (direction)
            {
                case FacingDirection.Up:
                    return "↑";
                case FacingDirection.Down:
                    return "↓";
                case FacingDirection.Left:
                    return "←";
                case FacingDirection.Right:
                    return "→";
                default:
                    return "☻";
            }
        }
    }
}