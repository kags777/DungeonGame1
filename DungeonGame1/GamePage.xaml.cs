using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DungeonGame1
{
    public partial class GamePage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private MainWindow mainWindow;
        private IGameSession gameSession;
        private GameStateDTO currentState;

        private int mapWidth = 10;
        private int mapHeight = 10;

        public int MapWidth
        {
            get => mapWidth;
            set
            {
                if (mapWidth != value)
                {
                    mapWidth = value;
                    OnPropertyChanged(nameof(MapWidth));
                }
            }
        }

        public int MapHeight
        {
            get => mapHeight;
            set
            {
                if (mapHeight != value)
                {
                    mapHeight = value;
                    OnPropertyChanged(nameof(MapHeight));
                }
            }
        }

        public GamePage(MainWindow window, string levelId, bool isNewGame)
        {
            InitializeComponent();
            mainWindow = window;
            gameSession = new GameSession(levelId, isNewGame);

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
            if (gameSession == null) return;

            currentState = gameSession.GetGameState();

            // Обновляем статус
            HealthText.Text = $"{currentState.Health}/{currentState.MaxHealth}";
            ScoreText.Text = currentState.Score.ToString();
            CrystalsText.Text = $"{currentState.CrystalsCollected}/{currentState.TotalCrystals}";

            // Определяем размеры карты
            if (currentState.Map.Any())
            {
                MapWidth = currentState.Map.Max(t => t.X) + 1;
                MapHeight = currentState.Map.Max(t => t.Y) + 1;
            }
            else
            {
                MapWidth = 10;
                MapHeight = 10;
            }

            // Создаем сетку
            var displayGrid = new DisplayTile[MapHeight, MapWidth];

            // Инициализируем пустыми клетками
            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    displayGrid[y, x] = new DisplayTile
                    {
                        X = x,
                        Y = y,
                        EntityType = EntityVisualType.Empty
                    };
                    displayGrid[y, x].UpdateVisuals();
                }
            }

            // Заполняем реальными объектами
            foreach (var tile in currentState.Map)
            {
                if (tile.X >= 0 && tile.X < MapWidth &&
                    tile.Y >= 0 && tile.Y < MapHeight)
                {
                    displayGrid[tile.Y, tile.X] = new DisplayTile(tile);
                }
            }

            // Преобразуем в список
            var displayTiles = new List<DisplayTile>();
            for (int y = 0; y < MapHeight; y++)
                for (int x = 0; x < MapWidth; x++)
                    displayTiles.Add(displayGrid[y, x]);

            GameGrid.ItemsSource = displayTiles;

            // Проверка состояния игры
            if (currentState.Status == GameStatus.Victory)
            {
                MessageBox.Show($" Победа! Вы набрали {currentState.Score} очков!",
                    "Поздравляем!", MessageBoxButton.OK, MessageBoxImage.Information);
                mainWindow.NavigateToMainMenu();
            }
            else if (currentState.Status == GameStatus.Defeat)
            {
                MessageBox.Show(" Вы погибли! Попробуйте еще раз.",
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
                case EntityVisualType.Empty:
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
