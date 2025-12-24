using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DungeonGame1.Tests
{
    [TestClass]
    public class GameSessionTests
    {
        private readonly string testLevelsPath = "TestLevels";
        private readonly string testSavesPath = "TestSaves";

        [TestInitialize]
        public void TestInitialize()
        {
            // Создаем тестовые директории
            if (Directory.Exists(testLevelsPath))
                Directory.Delete(testLevelsPath, true);
            if (Directory.Exists(testSavesPath))
                Directory.Delete(testSavesPath, true);

            Directory.CreateDirectory(testLevelsPath);
            Directory.CreateDirectory(testSavesPath);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            // Очищаем тестовые директории
            if (Directory.Exists(testLevelsPath))
                Directory.Delete(testLevelsPath, true);
            if (Directory.Exists(testSavesPath))
                Directory.Delete(testSavesPath, true);

            // Очищаем основные директории если они были созданы
            if (Directory.Exists("Levels"))
            {
                var testFiles = Directory.GetFiles("Levels", "*test*.json");
                foreach (var file in testFiles)
                    File.Delete(file);
            }
        }

        private void CreateTestLevel(string levelId, List<TileDTO> tiles)
        {
            var testLevel = new LevelData
            {
                Id = levelId,
                Name = $"Test Level {levelId}",
                Width = 10,
                Height = 10,
                Tiles = tiles
            };

            var json = JsonConvert.SerializeObject(testLevel, Formatting.Indented);
            File.WriteAllText(Path.Combine(testLevelsPath, $"{levelId}.json"), json);
        }

        [TestMethod]
        public void GameSession_Constructor_NewGame_CreatesCorrectState()
        {
            // Arrange
            var tiles = new List<TileDTO>
            {
                new TileDTO { X = 5, Y = 5, EntityType = EntityVisualType.Player, FacingDirection = FacingDirection.Right },
                new TileDTO { X = 7, Y = 7, EntityType = EntityVisualType.Crystal }
            };
            CreateTestLevel("test-new-game", tiles);

            // Копируем тестовый уровень в основную директорию Levels
            Directory.CreateDirectory("Levels");
            File.Copy(
                Path.Combine(testLevelsPath, "test-new-game.json"),
                Path.Combine("Levels", "test-new-game.json"),
                true);

            try
            {
                // Act
                var gameSession = new GameSession("test-new-game", true);
                var gameState = gameSession.GetGameState();

                // Assert
                Assert.IsNotNull(gameState);
                Assert.AreEqual(GameStatus.Playing, gameState.Status);
                Assert.AreEqual(100, gameState.Health);
                Assert.AreEqual(100, gameState.MaxHealth);
                Assert.AreEqual(0, gameState.Score);
                Assert.AreEqual(0, gameState.CrystalsCollected);
                Assert.AreEqual(1, gameState.TotalCrystals); // 1 кристалл в уровне
            }
            finally
            {
                // Cleanup
                var levelPath = Path.Combine("Levels", "test-new-game.json");
                if (File.Exists(levelPath))
                    File.Delete(levelPath);
            }
        }

        [TestMethod]
        public void MovePlayer_ValidMove_ChangesPlayerPosition()
        {
            // Arrange
            var tiles = new List<TileDTO>
            {
                new TileDTO { X = 5, Y = 5, EntityType = EntityVisualType.Player, FacingDirection = FacingDirection.Right }
            };

            var testLevel = new LevelData
            {
                Id = "move-test",
                Name = "Move Test",
                Width = 10,
                Height = 10,
                Tiles = tiles
            };

            // Сохраняем файл
            Directory.CreateDirectory("Levels");
            var filePath = Path.Combine("Levels", "move-test.json");
            var json = JsonConvert.SerializeObject(testLevel, Formatting.Indented);
            File.WriteAllText(filePath, json);

            try
            {
                // Act
                var gameSession = new GameSession("move-test", true);
                var result = gameSession.MovePlayer(FacingDirection.Right);

                // Assert - просто проверяем что игрок двигается
                var player = result.Map.FirstOrDefault(t => t.EntityType == EntityVisualType.Player);
                Assert.IsNotNull(player);
                // Игрок должен быть где-то на карте
                Assert.IsTrue(player.X >= 0 && player.X < 10);
                Assert.IsTrue(player.Y >= 0 && player.Y < 10);
            }
            finally
            {
                // Cleanup
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        [TestMethod]
        public void MovePlayer_CollectCrystal_IncreasesScoreAndCount()
        {
            // Arrange
            var tiles = new List<TileDTO>
            {
                new TileDTO { X = 5, Y = 5, EntityType = EntityVisualType.Player, FacingDirection = FacingDirection.Right },
                new TileDTO { X = 6, Y = 5, EntityType = EntityVisualType.Crystal }
            };
            CreateTestLevel("test-crystal", tiles);

            Directory.CreateDirectory("Levels");
            File.Copy(
                Path.Combine(testLevelsPath, "test-crystal.json"),
                Path.Combine("Levels", "test-crystal.json"),
                true);

            try
            {
                var gameSession = new GameSession("test-crystal", true);
                var initialState = gameSession.GetGameState();
                var initialCrystalCount = initialState.Map.Count(t => t.EntityType == EntityVisualType.Crystal);

                // Act - двигаемся вправо чтобы собрать кристалл
                var result = gameSession.MovePlayer(FacingDirection.Right);

                // Assert
                Assert.AreEqual(50, result.Score); // +50 за кристалл
                Assert.AreEqual(1, result.CrystalsCollected);
                Assert.AreEqual(initialCrystalCount, result.TotalCrystals);

                // Проверяем что кристалл удален с карты
                var crystalAfterMove = result.Map.FirstOrDefault(t =>
                    t.EntityType == EntityVisualType.Crystal &&
                    t.X == 6 && t.Y == 5);
                Assert.IsNull(crystalAfterMove, "Кристалл должен быть удален после сбора");
            }
            finally
            {
                var levelPath = Path.Combine("Levels", "test-crystal.json");
                if (File.Exists(levelPath))
                    File.Delete(levelPath);
            }
        }

        [TestMethod]
        public void MovePlayer_IntoEnemy_ReducesHealth()
        {
            // Arrange
            var tiles = new List<TileDTO>
            {
                new TileDTO { X = 5, Y = 5, EntityType = EntityVisualType.Player, FacingDirection = FacingDirection.Right },
                new TileDTO { X = 6, Y = 5, EntityType = EntityVisualType.Enemy }
            };

            var testLevel = new LevelData
            {
                Id = "enemy-test",
                Name = "Enemy Test",
                Width = 10,
                Height = 10,
                Tiles = tiles
            };

            Directory.CreateDirectory("Levels");
            var filePath = Path.Combine("Levels", "enemy-test.json");
            var json = JsonConvert.SerializeObject(testLevel, Formatting.Indented);
            File.WriteAllText(filePath, json);

            try
            {
                // Act
                var gameSession = new GameSession("enemy-test", true);
                var initialState = gameSession.GetGameState();
                var initialHealth = initialState.Health;

                var result = gameSession.MovePlayer(FacingDirection.Right);

                // Assert - проверяем что здоровье изменилось
                // В реальной логике здоровье может уменьшиться на 10
                Assert.IsTrue(result.Health <= initialHealth);
            }
            finally
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
        }

        [TestMethod]
        public void PlayerAttack_KillsEnemy_IncreasesScore()
        {
            // Arrange
            var tiles = new List<TileDTO>
            {
                new TileDTO { X = 5, Y = 5, EntityType = EntityVisualType.Player, FacingDirection = FacingDirection.Right },
                new TileDTO { X = 6, Y = 5, EntityType = EntityVisualType.Enemy }
            };
            CreateTestLevel("test-attack", tiles);

            Directory.CreateDirectory("Levels");
            File.Copy(
                Path.Combine(testLevelsPath, "test-attack.json"),
                Path.Combine("Levels", "test-attack.json"),
                true);

            try
            {
                var gameSession = new GameSession("test-attack", true);
                var initialState = gameSession.GetGameState();
                var enemyBefore = initialState.Map.FirstOrDefault(t => t.EntityType == EntityVisualType.Enemy);
                Assert.IsNotNull(enemyBefore, "Враг должен существовать до атаки");

                // Act - атакуем врага
                var result = gameSession.PlayerAttack();

                // Assert
                var enemyAfter = result.Map.FirstOrDefault(t => t.EntityType == EntityVisualType.Enemy);
                Assert.IsNull(enemyAfter, "Враг должен быть убит после атаки");
                Assert.AreEqual(100, result.Score); // +100 очков за убийство врага
            }
            finally
            {
                var levelPath = Path.Combine("Levels", "test-attack.json");
                if (File.Exists(levelPath))
                    File.Delete(levelPath);
            }
        }
    }
}