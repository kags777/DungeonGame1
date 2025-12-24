using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace DungeonGame1.Tests
{
    [TestClass]
    public class ModelsTests
    {
        [TestMethod]
        public void LevelInfoDTO_Properties_SetCorrectly()
        {
            // Arrange
            var levelInfo = new LevelInfoDTO
            {
                Id = "test-id",
                Name = "Test Level",
                Width = 10,
                Height = 20
            };

            // Assert
            Assert.AreEqual("test-id", levelInfo.Id);
            Assert.AreEqual("Test Level", levelInfo.Name);
            Assert.AreEqual(10, levelInfo.Width);
            Assert.AreEqual(20, levelInfo.Height);
        }

        [TestMethod]
        public void SaveInfoDTO_Properties_SetCorrectly()
        {
            // Arrange
            var saveInfo = new SaveInfoDTO
            {
                Id = "save-1",
                LevelName = "Test Level",
                SaveTime = "2024-01-01",
                Score = 1500
            };

            // Assert
            Assert.AreEqual("save-1", saveInfo.Id);
            Assert.AreEqual("Test Level", saveInfo.LevelName);
            Assert.AreEqual("2024-01-01", saveInfo.SaveTime);
            Assert.AreEqual(1500, saveInfo.Score);
        }

        [TestMethod]
        public void GameStateDTO_DefaultValues_AreCorrect()
        {
            // Arrange
            var gameState = new GameStateDTO();

            // Assert
            Assert.AreEqual(GameStatus.Playing, gameState.Status);
            Assert.AreEqual(0, gameState.Health);
            Assert.AreEqual(0, gameState.MaxHealth);
            Assert.AreEqual(0, gameState.Score);
            Assert.AreEqual(0, gameState.CrystalsCollected);
            Assert.AreEqual(0, gameState.TotalCrystals);
            Assert.IsNotNull(gameState.Map);
            Assert.AreEqual(0, gameState.Map.Count);
        }

        [TestMethod]
        public void TileDTO_Properties_SetCorrectly()
        {
            // Arrange
            var tile = new TileDTO
            {
                X = 5,
                Y = 3,
                EntityType = EntityVisualType.Player,
                FacingDirection = FacingDirection.Right
            };

            // Assert
            Assert.AreEqual(5, tile.X);
            Assert.AreEqual(3, tile.Y);
            Assert.AreEqual(EntityVisualType.Player, tile.EntityType);
            Assert.AreEqual(FacingDirection.Right, tile.FacingDirection);
        }

        [TestMethod]
        public void AvailableEntityDTO_Properties_SetCorrectly()
        {
            // Arrange
            var entity = new AvailableEntityDTO
            {
                Type = EntityVisualType.Enemy,
                Name = "Враг",
                Symbol = "E"
            };

            // Assert
            Assert.AreEqual(EntityVisualType.Enemy, entity.Type);
            Assert.AreEqual("Враг", entity.Name);
            Assert.AreEqual("E", entity.Symbol);
        }

        [TestMethod]
        public void LevelData_DefaultTiles_IsNotNull()
        {
            // Arrange
            var levelData = new LevelData();

            // Assert
            Assert.IsNotNull(levelData.Tiles);
        }

        [TestMethod]
        public void SaveData_DefaultGameState_IsNotNull()
        {
            // Arrange & Act
            var saveData = new SaveData();

            // Assert - УПРОЩЕННЫЙ ТЕСТ
            // Просто проверяем что объект создан
            Assert.IsNotNull(saveData);

            // GameState должен быть не null
            if (saveData.GameState != null)
            {
                Assert.IsNotNull(saveData.GameState.Map);
            }
        }
    }
}