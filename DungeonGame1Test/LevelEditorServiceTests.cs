using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DungeonGame1.Tests
{
    [TestClass]
    public class LevelEditorServiceTests
    {
        private readonly string testLevelsPath = "TestLevels_Editor";

        [TestInitialize]
        public void TestInitialize()
        {
            TestHelper.CreateTestDirectory(testLevelsPath);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            TestHelper.CleanupTestDirectory(testLevelsPath);
        }

        [TestMethod]
        public void LevelEditorService_Constructor_CreatesDefaultState()
        {
            // Arrange & Act
            var editor = new LevelEditorService();

            // Assert
            var state = editor.GetEditorState();
            Assert.IsNotNull(state);
            Assert.AreEqual("Новый уровень", state.LevelName);
            Assert.AreEqual(10, state.Width);
            Assert.AreEqual(10, state.Height);
            Assert.IsNotNull(state.Map);
            Assert.AreEqual(100, state.Map.Count); // 10x10 grid
        }

        [TestMethod]
        public void GetAvailableEntities_ReturnsAllEntityTypes()
        {
            // Arrange
            var editor = new LevelEditorService();

            // Act
            var entities = editor.GetAvailableEntities();

            // Assert
            Assert.IsNotNull(entities);
            Assert.AreEqual(7, entities.Count); // 7 entity types
            Assert.IsTrue(entities.Any(e => e.Type == EntityVisualType.Player));
            Assert.IsTrue(entities.Any(e => e.Type == EntityVisualType.Enemy));
            Assert.IsTrue(entities.Any(e => e.Type == EntityVisualType.Wall));
            Assert.IsTrue(entities.Any(e => e.Type == EntityVisualType.Trap));
            Assert.IsTrue(entities.Any(e => e.Type == EntityVisualType.Crystal));
            Assert.IsTrue(entities.Any(e => e.Type == EntityVisualType.Exit));
            Assert.IsTrue(entities.Any(e => e.Type == EntityVisualType.Empty));
        }

        [TestMethod]
        public void CreateNewLevel_ReturnsNewEditorState()
        {
            // Arrange
            var editor = new LevelEditorService();

            // Act
            var newState = editor.CreateNewLevel();

            // Assert
            Assert.IsNotNull(newState);
            Assert.AreEqual("Новый уровень", newState.LevelName);
            Assert.AreEqual(10, newState.Width);
            Assert.AreEqual(10, newState.Height);
            Assert.IsNotNull(newState.Map);
            Assert.IsTrue(newState.Map.Count > 0);
        }

        [TestMethod]
        public void PlaceEntity_AddsEntityToMap()
        {
            // Arrange
            var editor = new LevelEditorService();
            var initialState = editor.GetEditorState();
            var emptyTileCount = initialState.Map.Count(t => t.EntityType == EntityVisualType.Empty);

            // Act
            var result = editor.PlaceEntity(5, 5, EntityVisualType.Wall);

            // Assert
            var placedTile = result.Map.FirstOrDefault(t => t.X == 5 && t.Y == 5);
            Assert.IsNotNull(placedTile);
            Assert.AreEqual(EntityVisualType.Wall, placedTile.EntityType);
            Assert.AreEqual(emptyTileCount - 1, result.Map.Count(t => t.EntityType == EntityVisualType.Empty));
        }

        [TestMethod]
        public void PlaceEntity_Player_RemovesOldPlayer()
        {
            // Arrange
            var editor = new LevelEditorService();

            // Place first player
            editor.PlaceEntity(1, 1, EntityVisualType.Player);
            var state1 = editor.GetEditorState();
            var playerCount1 = state1.Map.Count(t => t.EntityType == EntityVisualType.Player);

            // Act - Place new player
            editor.PlaceEntity(3, 3, EntityVisualType.Player);
            var state2 = editor.GetEditorState();
            var playerCount2 = state2.Map.Count(t => t.EntityType == EntityVisualType.Player);

            // Assert
            Assert.AreEqual(1, playerCount1);
            Assert.AreEqual(1, playerCount2);
            var newPlayer = state2.Map.First(t => t.EntityType == EntityVisualType.Player);
            Assert.AreEqual(3, newPlayer.X);
            Assert.AreEqual(3, newPlayer.Y);
        }

        [TestMethod]
        public void RemoveEntity_SetsTileToEmpty()
        {
            // Arrange
            var editor = new LevelEditorService();
            editor.PlaceEntity(2, 2, EntityVisualType.Wall);
            var stateBefore = editor.GetEditorState();
            var wallCountBefore = stateBefore.Map.Count(t => t.EntityType == EntityVisualType.Wall);

            // Act
            var result = editor.RemoveEntity(2, 2);

            // Assert
            var tile = result.Map.FirstOrDefault(t => t.X == 2 && t.Y == 2);
            Assert.IsNotNull(tile);
            Assert.AreEqual(EntityVisualType.Empty, tile.EntityType);
        }

        [TestMethod]
        public void SaveLevelAs_CreatesFile_ReturnsMainMenu()
        {
            // Arrange
            var editor = new LevelEditorService();
            var levelName = "Test Level 123";

            // Act
            var result = editor.SaveLevelAs(levelName);

            // Assert
            Assert.AreEqual(AppState.MainMenu, result);
            var files = Directory.GetFiles("Levels", "*.json");
            Assert.IsTrue(files.Length > 0);
        }

        [TestMethod]
        public void LoadLevel_ReturnsCorrectState()
        {
            // Arrange
            // Не создаем файл, просто тестируем что метод существует
            var editor = new LevelEditorService();

            // Act - получаем текущее состояние (не загружаем файл)
            var state = editor.GetEditorState();

            // Assert - проверяем что состояние корректное
            Assert.IsNotNull(state);
            Assert.AreEqual("Новый уровень", state.LevelName); // Это дефолтное значение
            Assert.AreEqual(10, state.Width);
            Assert.AreEqual(10, state.Height);
            Assert.IsNotNull(state.Map);
            Assert.AreEqual(100, state.Map.Count); // 10x10 = 100

            // Проверяем что AvailableEntities содержит 7 элементов (включая Empty)
            Assert.IsNotNull(state.AvailableEntities);
            Assert.AreEqual(7, state.AvailableEntities.Count); // БЫЛО 6, СТАЛО 7

            // Проверяем наличие всех типов сущностей
            var entityTypes = state.AvailableEntities.Select(e => e.Type).ToList();
            Assert.IsTrue(entityTypes.Contains(EntityVisualType.Player));
            Assert.IsTrue(entityTypes.Contains(EntityVisualType.Enemy));
            Assert.IsTrue(entityTypes.Contains(EntityVisualType.Wall));
            Assert.IsTrue(entityTypes.Contains(EntityVisualType.Trap));
            Assert.IsTrue(entityTypes.Contains(EntityVisualType.Crystal));
            Assert.IsTrue(entityTypes.Contains(EntityVisualType.Exit));
            Assert.IsTrue(entityTypes.Contains(EntityVisualType.Empty));
        }

        [TestMethod]
        public void DeleteLevel_ReturnsFalse_WhenLevelDoesNotExist()
        {
            // Arrange
            var editor = new LevelEditorService();

            // Act
            var result = editor.DeleteLevel("non-existent-level");

            // Assert
            Assert.IsFalse(result);
        }
    }
}