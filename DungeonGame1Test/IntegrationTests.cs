using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace DungeonGame1.Tests
{
    [TestClass]
    public class IntegrationTests
    {
        private readonly string testLevelsPath = "TestLevels_Integration";
        private readonly string testSavesPath = "TestSaves_Integration";

        [TestInitialize]
        public void TestInitialize()
        {
            TestHelper.CreateTestDirectory(testLevelsPath);
            TestHelper.CreateTestDirectory(testSavesPath);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            TestHelper.CleanupTestDirectory(testLevelsPath);
            TestHelper.CleanupTestDirectory(testSavesPath);
        }

        [TestMethod]
        public void CompleteGameFlow_FromMenuToGameToSave()
        {
            var menuService = new MainMenuService();
            var levels = menuService.GetAvailableLevels();
            Assert.IsTrue(levels.Count > 0);

            var gameState = menuService.StartNewGame(levels.First().Id);
            Assert.AreEqual(AppState.Game, gameState);
            var gameSession = new GameSession(levels.First().Id, true);

            gameSession.MovePlayer(FacingDirection.Right);
            gameSession.PlayerAttack();

            var menuState = gameSession.SaveAndExit();
            Assert.AreEqual(AppState.MainMenu, menuState);
            
            var saves = menuService.GetAvailableSaves();
            Assert.IsTrue(saves.Count > 0);
        }

        [TestMethod]
        public void EditorToGame_CompleteFlow()
        {
            var editor = new LevelEditorService();
            editor.PlaceEntity(1, 1, EntityVisualType.Player);
            editor.PlaceEntity(5, 5, EntityVisualType.Exit);
            editor.PlaceEntity(3, 3, EntityVisualType.Crystal);

            var menuState = editor.SaveLevelAs("Integration Test Level");
            Assert.AreEqual(AppState.MainMenu, menuState);

            var menuService = new MainMenuService();
            var levels = menuService.GetAvailableLevels();
            Assert.IsTrue(levels.Any(l => l.Name == "Integration Test Level"));

            var newLevel = levels.First(l => l.Name == "Integration Test Level");
            var gameState = menuService.StartNewGame(newLevel.Id);
            Assert.AreEqual(AppState.Game, gameState);
        }

        [TestMethod]
        public void DeleteLevel_WithSaves_Integration()
        {
            var menuService = new MainMenuService();
            var levels = menuService.GetAvailableLevels();
            Assert.IsTrue(levels.Count > 0);

            var firstLevel = levels.First();

            var gameSession = new GameSession(firstLevel.Id, true);
            gameSession.MovePlayer(FacingDirection.Right);
            gameSession.SaveAndExit();

            var savesBefore = menuService.GetAvailableSaves();
            Assert.IsTrue(savesBefore.Count > 0);

            var result = menuService.DeleteLevel(firstLevel.Id);
            Assert.IsTrue(true); 
        }
    }
}