using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DungeonGame1
{
    public partial class GamePage : Page
    {
        private MainWindow mainWindow;
        private IGameSession gameSession;
        private GameStateDTO currentState;
        private int mapWidth = 10;
        private int mapHeight = 10;

        public int MapWidth => mapWidth;
        public int MapHeight => mapHeight;

        public GamePage(MainWindow window, string levelId, bool isNewGame)
        {
            InitializeComponent();
            mainWindow = window;
            gameSession = new GameSession(levelId, isNewGame);

            // Устанавливаем DataContext для привязок
            DataContext = this;

            Loaded += (s, e) => UpdateGameDisplay();
            Focus();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateGameDisplay();
        }

        private void UpdateGameDisplay()
        {
            currentState = gameSession.GetGameState();

            // Обновляем статус
            HealthText.Text = $"{currentState.Health}/{currentState.MaxHealth}";
            ScoreText.Text = currentState.Score.ToString();
            CrystalsText.Text = $"{currentState.CrystalsCollected}/{currentState.TotalCrystals}";

            // Находим размеры карты
            if (currentState.Map.Any())
            {
                mapWidth = currentState.Map.Max(t => t.X) + 1;
                mapHeight = currentState.Map.Max(t => t.Y) + 1;
            }

            // Создаем отображаемые тайлы
            var displayTiles = new List<DisplayTile>();

            // Сначала заполняем все клетки пустотой
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    displayTiles.Add(new DisplayTile
                    {
                        X = x,
                        Y = y,
                        EntityType = EntityVisualType.Empty
                    });
                }
            }

            // Затем накладываем реальные объекты
            foreach (var tile in currentState.Map)
            {
                var displayTile = displayTiles.FirstOrDefault(t => t.X == tile.X && t.Y == tile.Y);
                if (displayTile != null)
                {
                    displayTile.EntityType = tile.EntityType;
                    displayTile.FacingDirection = tile.FacingDirection;
                    displayTile.UpdateVisuals();
                }
                else if (tile.X >= 0 && tile.X < mapWidth && tile.Y >= 0 && tile.Y < mapHeight)
                {
                    // Добавляем новый тайл, если его нет
                    displayTiles.Add(new DisplayTile(tile));
                }
            }

            GameGrid.ItemsSource = displayTiles.OrderBy(t => t.Y).ThenBy(t => t.X);

            // Проверяем состояние игры
            if (currentState.Status == GameStatus.Victory)
            {
                MessageBox.Show($"🎉 Победа! Вы набрали {currentState.Score} очков!",
                    "Поздравляем!", MessageBoxButton.OK, MessageBoxImage.Information);
                mainWindow.NavigateToMainMenu();
            }
            else if (currentState.Status == GameStatus.Defeat)
            {
                MessageBox.Show("💀 Вы погибли! Попробуйте еще раз.",
                    "Игра окончена", MessageBoxButton.OK, MessageBoxImage.Warning);
                mainWindow.NavigateToMainMenu();
            }
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (currentState == null || currentState.Status != GameStatus.Playing)
                return;

            FacingDirection direction = FacingDirection.None;

            switch (e.Key)
            {
                case Key.W:
                case Key.Up:
                    direction = FacingDirection.Up;
                    break;
                case Key.S:
                case Key.Down:
                    direction = FacingDirection.Down;
                    break;
                case Key.A:
                case Key.Left:
                    direction = FacingDirection.Left;
                    break;
                case Key.D:
                case Key.Right:
                    direction = FacingDirection.Right;
                    break;
                case Key.Space:
                    gameSession.PlayerAttack();
                    UpdateGameDisplay();
                    return;
            }

            if (direction != FacingDirection.None)
            {
                gameSession.MovePlayer(direction);
                UpdateGameDisplay();
            }
        }

        private void PauseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentState.Status == GameStatus.Playing)
            {
                gameSession.PauseGame();
                MessageBox.Show("Игра на паузе\nНажмите ПРОДОЛЖИТЬ чтобы вернуться",
                    "Пауза", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (currentState.Status == GameStatus.Paused)
            {
                gameSession.ResumeGame();
            }
            UpdateGameDisplay();
            Focus();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            gameSession.SaveAndExit();
            MessageBox.Show("Игра сохранена!", "Сохранение",
                MessageBoxButton.OK, MessageBoxImage.Information);
            mainWindow.NavigateToMainMenu();
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Выйти в меню без сохранения?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                mainWindow.NavigateToMainMenu();
            }
        }
    }

    public class DisplayTile
    {
        public int X { get; set; }
        public int Y { get; set; }
        public EntityVisualType EntityType { get; set; }
        public FacingDirection FacingDirection { get; set; }
        public string Symbol { get; private set; }
        public Brush Color { get; private set; }
        public Brush BackgroundColor { get; private set; }

        public DisplayTile() { }

        public DisplayTile(TileDTO tile)
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
                    break;
                case EntityVisualType.Enemy:
                    Symbol = "👹";
                    Color = Brushes.Red;
                    BackgroundColor = Brushes.DarkRed;
                    break;
                case EntityVisualType.Wall:
                    Symbol = "█";
                    Color = Brushes.Gray;
                    BackgroundColor = Brushes.DarkSlateGray;
                    break;
                case EntityVisualType.Trap:
                    Symbol = "⚠";
                    Color = Brushes.Orange;
                    BackgroundColor = Brushes.DarkOrange;
                    break;
                case EntityVisualType.Crystal:
                    Symbol = "💎";
                    Color = Brushes.Gold;
                    BackgroundColor = Brushes.DarkGoldenrod;
                    break;
                case EntityVisualType.Exit:
                    Symbol = "🚪";
                    Color = Brushes.LightGreen;
                    BackgroundColor = Brushes.DarkGreen;
                    break;
                default:
                    Symbol = "·";
                    Color = Brushes.DimGray;
                    BackgroundColor = Brushes.Transparent;
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