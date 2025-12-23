using DungeonGame1;
using Newtonsoft.Json; // ← ИЗМЕНИЛ ЗДЕСЬ
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace DungeonGame1{
    // Интерфейсы остаются те же...
    public interface IMainMenuService
    {
        List<LevelInfoDTO> GetAvailableLevels();
        List<SaveInfoDTO> GetAvailableSaves();
        AppState StartNewGame(string levelId);
        AppState ContinueGame(string saveId);
        AppState OpenEditor(string levelId);
        AppState ExitApp();
    }

    public interface IGameSession
    {
        GameStateDTO MovePlayer(FacingDirection direction);
        GameStateDTO PlayerAttack();
        GameStateDTO PauseGame();
        GameStateDTO ResumeGame();
        AppState SaveAndExit();
        GameStateDTO GetGameState();
    }

    public interface ILevelEditorService
    {
        EditorStateDTO CreateNewLevel();
        EditorStateDTO LoadLevel(string levelId);
        List<AvailableEntityDTO> GetAvailableEntities();
        EditorStateDTO PlaceEntity(int x, int y, EntityVisualType entityType);
        EditorStateDTO RemoveEntity(int x, int y);
        AppState SaveLevelAs(string name);
        EditorStateDTO GetEditorState();
    }

    public class MainMenuService : IMainMenuService
    {
        private readonly string levelsPath = "Levels";
        private readonly string savesPath = "Saves";

        public MainMenuService()
        {
            Directory.CreateDirectory(levelsPath);
            Directory.CreateDirectory(savesPath);
            CreateDefaultLevelIfNeeded();
        }

        private void CreateDefaultLevelIfNeeded()
        {
            var defaultLevelPath = Path.Combine(levelsPath, "default.json");
            if (!File.Exists(defaultLevelPath))
            {
                var level = new LevelData
                {
                    Id = "default",
                    Name = "Подземелье новичка",
                    Width = 10,
                    Height = 10,
                    Tiles = GenerateDefaultLevel(10, 10)
                };
                SaveLevel(level);
            }
        }

        private List<TileDTO> GenerateDefaultLevel(int width, int height)
        {
            var tiles = new List<TileDTO>();

            // ТОЛЬКО границы карты - стены по краям
            for (int x = 0; x < width; x++)
            {
                tiles.Add(new TileDTO { X = x, Y = 0, EntityType = EntityVisualType.Wall });
                tiles.Add(new TileDTO { X = x, Y = height - 1, EntityType = EntityVisualType.Wall });
            }
            for (int y = 1; y < height - 1; y++)
            {
                tiles.Add(new TileDTO { X = 0, Y = y, EntityType = EntityVisualType.Wall });
                tiles.Add(new TileDTO { X = width - 1, Y = y, EntityType = EntityVisualType.Wall });
            }

            // Игрок в центре - ХОРОШО ВИДНО
            tiles.Add(new TileDTO
            {
                X = width / 2,
                Y = height / 2,
                EntityType = EntityVisualType.Player,
                FacingDirection = FacingDirection.Right
            });

            // Враги вокруг
            tiles.Add(new TileDTO { X = width / 2 + 2, Y = height / 2, EntityType = EntityVisualType.Enemy });
            tiles.Add(new TileDTO { X = width / 2, Y = height / 2 + 2, EntityType = EntityVisualType.Enemy });

            // Кристаллы - МАЛО
            tiles.Add(new TileDTO { X = width / 2 + 1, Y = height / 2 - 1, EntityType = EntityVisualType.Crystal });
            tiles.Add(new TileDTO { X = width / 2 - 1, Y = height / 2 + 1, EntityType = EntityVisualType.Crystal });

            // Ловушки - МАЛО
            tiles.Add(new TileDTO { X = width / 2 - 2, Y = height / 2, EntityType = EntityVisualType.Trap });

            // Выход в углу
            tiles.Add(new TileDTO { X = width - 2, Y = height - 2, EntityType = EntityVisualType.Exit });

            return tiles;
        }

        public List<LevelInfoDTO> GetAvailableLevels()
        {
            var levels = new List<LevelInfoDTO>();
            foreach (var file in Directory.GetFiles(levelsPath, "*.json"))
            {
                var level = LoadLevelFromFile(file);
                if (level != null)
                {
                    levels.Add(new LevelInfoDTO
                    {
                        Id = level.Id,
                        Name = level.Name,
                        Width = level.Width,
                        Height = level.Height
                    });
                }
            }
            return levels;
        }

        public List<SaveInfoDTO> GetAvailableSaves()
        {
            var saves = new List<SaveInfoDTO>();
            foreach (var file in Directory.GetFiles(savesPath, "*.json"))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var save = JsonConvert.DeserializeObject<SaveData>(json); // ← ИЗМЕНИЛ ЗДЕСЬ
                    saves.Add(new SaveInfoDTO
                    {
                        Id = save.Id,
                        LevelName = save.LevelId,
                        SaveTime = save.SaveTime,
                        Score = save.GameState.Score
                    });
                }
                catch { }
            }
            return saves;
        }

        public AppState StartNewGame(string levelId)
        {
            return AppState.Game;
        }

        public AppState ContinueGame(string saveId)
        {
            return AppState.Game;
        }

        public AppState OpenEditor(string levelId)
        {
            return AppState.Editor;
        }

        public AppState ExitApp()
        {
            Application.Current.Shutdown();
            return AppState.Exit;
        }

        private LevelData LoadLevelFromFile(string path)
        {
            try
            {
                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<LevelData>(json); // ← ИЗМЕНИЛ ЗДЕСЬ
            }
            catch
            {
                return null;
            }
        }

        private void SaveLevel(LevelData level)
        {
            var path = Path.Combine(levelsPath, $"{level.Id}.json");
            var json = JsonConvert.SerializeObject(level, Formatting.Indented); // ← ИЗМЕНИЛ ЗДЕСЬ
            File.WriteAllText(path, json);
        }
    }

    public class GameSession : IGameSession
    {
        private LevelData currentLevel;
        private GameStateDTO gameState;
        private TileDTO playerTile;
        private Random random = new Random();
        private string currentLevelId;

        // Конструктор уже существует, просто правим его
        public GameSession(string id, bool isNewGame)
        {
            if (isNewGame)
            {
                // Новая игра - id это levelId
                currentLevelId = id;
                currentLevel = LoadLevel(id);

                gameState = new GameStateDTO
                {
                    Status = GameStatus.Playing,
                    Health = 100,
                    MaxHealth = 100,
                    Score = 0,
                    CrystalsCollected = 0,
                    TotalCrystals = currentLevel.Tiles.Count(t => t.EntityType == EntityVisualType.Crystal),
                    Map = new List<TileDTO>(currentLevel.Tiles)
                };
                playerTile = gameState.Map.FirstOrDefault(t => t.EntityType == EntityVisualType.Player);
            }
            else
            {
                // Загрузка сохранения - id это saveId
                var save = LoadSave(id);

                if (save == null)
                {
                    // Если сохранение не найдено, создаем новую игру по умолчанию
                    currentLevelId = "default";
                    currentLevel = LoadLevel("default");

                    gameState = new GameStateDTO
                    {
                        Status = GameStatus.Playing,
                        Health = 100,
                        MaxHealth = 100,
                        Score = 0,
                        CrystalsCollected = 0,
                        TotalCrystals = currentLevel.Tiles.Count(t => t.EntityType == EntityVisualType.Crystal),
                        Map = new List<TileDTO>(currentLevel.Tiles)
                    };
                    playerTile = gameState.Map.FirstOrDefault(t => t.EntityType == EntityVisualType.Player);
                }
                else
                {
                    // Загружаем сохранение
                    currentLevelId = save.LevelId;
                    currentLevel = LoadLevel(save.LevelId);
                    gameState = save.GameState;
                    playerTile = gameState.Map.FirstOrDefault(t => t.EntityType == EntityVisualType.Player);
                }
            }
        }

        // Остальные методы остаются без изменений
        private LevelData LoadLevel(string levelId)
        {
            var path = Path.Combine("Levels", $"{levelId}.json");

            if (!File.Exists(path))
            {
                // Если файл уровня не найден, используем дефолтный
                path = Path.Combine("Levels", "default.json");

                if (!File.Exists(path))
                {
                    throw new FileNotFoundException($"Не найден уровень: {levelId} и дефолтный уровень");
                }
            }

            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<LevelData>(json);
        }

        private SaveData LoadSave(string saveId)
        {
            var path = Path.Combine("Saves", $"{saveId}.json");

            if (!File.Exists(path))
            {
                // Пробуем найти любой файл сохранения
                var saves = Directory.GetFiles("Saves", "*.json");
                if (saves.Length > 0)
                {
                    path = saves[0]; // Берем первый найденный
                }
                else
                {
                    return null; // Нет сохранений
                }
            }

            try
            {
                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<SaveData>(json);
            }
            catch
            {
                return null; // Ошибка чтения файла
            }
        }

        // Остальные методы (MovePlayer, PlayerAttack и т.д.) остаются без изменений
        public GameStateDTO MovePlayer(FacingDirection direction)
        {
            // ... существующий код без изменений
            if (gameState.Status != GameStatus.Playing || playerTile == null)
                return gameState;

            int newX = playerTile.X;
            int newY = playerTile.Y;

            switch (direction)
            {
                case FacingDirection.Up: newY--; break;
                case FacingDirection.Down: newY++; break;
                case FacingDirection.Left: newX--; break;
                case FacingDirection.Right: newX++; break;
            }

            playerTile.FacingDirection = direction;

            var targetTile = gameState.Map.FirstOrDefault(t => t.X == newX && t.Y == newY);

            if (targetTile == null)
            {
                playerTile.X = newX;
                playerTile.Y = newY;
            }
            else if (targetTile.EntityType == EntityVisualType.Empty)
            {
                playerTile.X = newX;
                playerTile.Y = newY;
            }
            else if (targetTile.EntityType == EntityVisualType.Crystal)
            {
                playerTile.X = newX;
                playerTile.Y = newY;
                gameState.Map.Remove(targetTile);
                gameState.Score += 50;
                gameState.CrystalsCollected++;

                if (gameState.CrystalsCollected >= gameState.TotalCrystals)
                {
                    gameState.Status = GameStatus.Victory;
                }
            }
            else if (targetTile.EntityType == EntityVisualType.Trap)
            {
                playerTile.X = newX;
                playerTile.Y = newY;
                gameState.Map.Remove(targetTile);
                gameState.Health -= 20;
                gameState.Score -= 10;

                if (gameState.Health <= 0)
                {
                    gameState.Status = GameStatus.Defeat;
                }
            }
            else if (targetTile.EntityType == EntityVisualType.Exit)
            {
                gameState.Status = GameStatus.Victory;
            }
            else if (targetTile.EntityType == EntityVisualType.Wall)
            {
                return gameState;
            }
            else if (targetTile.EntityType == EntityVisualType.Enemy)
            {
                gameState.Health -= 10;
                gameState.Score -= 5;

                if (gameState.Health <= 0)
                {
                    gameState.Status = GameStatus.Defeat;
                }
            }

            MoveEnemies();
            return gameState;
        }

        private void MoveEnemies()
        {
            var enemies = gameState.Map.Where(t => t.EntityType == EntityVisualType.Enemy).ToList();

            foreach (var enemy in enemies)
            {
                var possibleMoves = new List<(int dx, int dy)>
            {
                (0, -1), (0, 1), (-1, 0), (1, 0)
            }.OrderBy(x => random.Next()).ToList();

                foreach (var move in possibleMoves)
                {
                    int newX = enemy.X + move.dx;
                    int newY = enemy.Y + move.dy;

                    var existingTile = gameState.Map.FirstOrDefault(t => t.X == newX && t.Y == newY);

                    if (existingTile == null || existingTile.EntityType == EntityVisualType.Empty)
                    {
                        enemy.X = newX;
                        enemy.Y = newY;
                        break;
                    }
                    else if (existingTile.EntityType == EntityVisualType.Player)
                    {
                        gameState.Health -= 10;
                        gameState.Score -= 5;

                        if (gameState.Health <= 0)
                        {
                            gameState.Status = GameStatus.Defeat;
                        }
                        break;
                    }
                }
            }
        }

        public GameStateDTO PlayerAttack()
        {
            if (gameState.Status != GameStatus.Playing || playerTile == null)
                return gameState;

            int attackX = playerTile.X;
            int attackY = playerTile.Y;

            switch (playerTile.FacingDirection)
            {
                case FacingDirection.Up: attackY--; break;
                case FacingDirection.Down: attackY++; break;
                case FacingDirection.Left: attackX--; break;
                case FacingDirection.Right: attackX++; break;
            }

            var target = gameState.Map.FirstOrDefault(t => t.X == attackX && t.Y == attackY);

            if (target != null && target.EntityType == EntityVisualType.Enemy)
            {
                gameState.Map.Remove(target);
                gameState.Score += 100;
            }

            return gameState;
        }

        public GameStateDTO PauseGame()
        {
            if (gameState.Status == GameStatus.Playing)
            {
                gameState.Status = GameStatus.Paused;
            }
            return gameState;
        }

        public GameStateDTO ResumeGame()
        {
            if (gameState.Status == GameStatus.Paused)
            {
                gameState.Status = GameStatus.Playing;
            }
            return gameState;
        }

        public AppState SaveAndExit()
        {
            var save = new SaveData
            {
                Id = Guid.NewGuid().ToString(),
                LevelId = currentLevelId,
                SaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                GameState = gameState
            };

            var path = Path.Combine("Saves", $"{save.Id}.json");
            var json = JsonConvert.SerializeObject(save, Formatting.Indented);
            File.WriteAllText(path, json);

            return AppState.MainMenu;
        }

        public GameStateDTO GetGameState()
        {
            return gameState;
        }
    }

    public class LevelEditorService : ILevelEditorService
    {
        private EditorStateDTO editorState;
        private EntityVisualType selectedEntity = EntityVisualType.Wall;

        public LevelEditorService()
        {
            editorState = new EditorStateDTO
            {
                LevelName = "Новый уровень",
                Width = 10,
                Height = 10,
                AvailableEntities = GetAvailableEntities(),
                Map = new List<TileDTO>()
            };
            InitializeEmptyMap();
        }

        private void InitializeEmptyMap()
        {
            for (int x = 0; x < editorState.Width; x++)
            {
                for (int y = 0; y < editorState.Height; y++)
                {
                    editorState.Map.Add(new TileDTO
                    {
                        X = x,
                        Y = y,
                        EntityType = EntityVisualType.Empty
                    });
                }
            }
        }

        public EditorStateDTO CreateNewLevel()
        {
            editorState = new EditorStateDTO
            {
                LevelName = "Новый уровень",
                Width = 10,
                Height = 10,
                AvailableEntities = GetAvailableEntities(),
                Map = new List<TileDTO>()
            };
            InitializeEmptyMap();
            return editorState;
        }

        public EditorStateDTO LoadLevel(string levelId)
        {
            var path = Path.Combine("Levels", $"{levelId}.json");
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var levelData = JsonConvert.DeserializeObject<LevelData>(json); // ← ИЗМЕНИЛ ЗДЕСЬ

                editorState = new EditorStateDTO
                {
                    LevelName = levelData.Name,
                    Width = levelData.Width,
                    Height = levelData.Height,
                    AvailableEntities = GetAvailableEntities(),
                    Map = levelData.Tiles
                };
            }
            return editorState;
        }

        public List<AvailableEntityDTO> GetAvailableEntities()
        {
            return new List<AvailableEntityDTO>
            {
                new AvailableEntityDTO { Type = EntityVisualType.Player, Name = "Игрок" },
                new AvailableEntityDTO { Type = EntityVisualType.Enemy, Name = "Враг" },
                new AvailableEntityDTO { Type = EntityVisualType.Wall, Name = "Стена" },
                new AvailableEntityDTO { Type = EntityVisualType.Trap, Name = "Ловушка" },
                new AvailableEntityDTO { Type = EntityVisualType.Crystal, Name = "Кристалл" },
                new AvailableEntityDTO { Type = EntityVisualType.Exit, Name = "Выход" },
                new AvailableEntityDTO { Type = EntityVisualType.Empty, Name = "Пусто" }
            };
        }

        public EditorStateDTO PlaceEntity(int x, int y, EntityVisualType entityType)
        {
            var tile = editorState.Map.FirstOrDefault(t => t.X == x && t.Y == y);
            if (tile != null)
            {
                // Если ставим игрока, удаляем старого
                if (entityType == EntityVisualType.Player)
                {
                    var oldPlayer = editorState.Map.FirstOrDefault(t => t.EntityType == EntityVisualType.Player);
                    if (oldPlayer != null)
                    {
                        oldPlayer.EntityType = EntityVisualType.Empty;
                    }
                }

                tile.EntityType = entityType;
                selectedEntity = entityType;
            }
            return editorState;
        }

        public EditorStateDTO RemoveEntity(int x, int y)
        {
            var tile = editorState.Map.FirstOrDefault(t => t.X == x && t.Y == y);
            if (tile != null)
            {
                tile.EntityType = EntityVisualType.Empty;
            }
            return editorState;
        }

        public AppState SaveLevelAs(string name)
        {
            var levelData = new LevelData
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Width = editorState.Width,
                Height = editorState.Height,
                Tiles = editorState.Map.Where(t => t.EntityType != EntityVisualType.Empty).ToList()
            };

            var path = Path.Combine("Levels", $"{levelData.Id}.json");
            var json = JsonConvert.SerializeObject(levelData, Formatting.Indented); // ← ИЗМЕНИЛ ЗДЕСЬ
            File.WriteAllText(path, json);

            return AppState.MainMenu;
        }

        public EditorStateDTO GetEditorState()
        {
            return editorState;
        }
    }
}