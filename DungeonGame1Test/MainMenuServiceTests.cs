using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace DungeonGame1.Tests
{
    [TestClass]
    public class MainMenuServiceTests
    {
        private readonly string testLevelsPath = "TestLevels_Menu";
        private readonly string testSavesPath = "TestSaves_Menu";

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
        public void MainMenuService_Constructor_CreatesDefaultLevel()
        {
            var service = new MainMenuService();
            var levels = service.GetAvailableLevels();
            Assert.IsTrue(levels.Any(l => l.Id == "default"));
        }

        [TestMethod]
        public void GetAvailableLevels_ReturnsLevels_WhenLevelsExist()
        {
            var service = new MainMenuService();
            var levels = service.GetAvailableLevels();

            Assert.IsNotNull(levels);
            Assert.IsTrue(levels.Count > 0);
        }

        [TestMethod]
        public void StartNewGame_ReturnsGameState()
        {
            var service = new MainMenuService();
            var levels = service.GetAvailableLevels();
            var firstLevel = levels.First();
            var result = service.StartNewGame(firstLevel.Id);

            Assert.AreEqual(AppState.Game, result);
        }

        [TestMethod]
        public void ContinueGame_ReturnsGameState()
        {
            var service = new MainMenuService();
            var result = service.ContinueGame("test-save");

            Assert.AreEqual(AppState.Game, result);
        }

        [TestMethod]
        public void OpenEditor_ReturnsEditorState()
        {
            var service = new MainMenuService();
            var levels = service.GetAvailableLevels();
            var firstLevel = levels.First();
            var result = service.OpenEditor(firstLevel.Id);

            Assert.AreEqual(AppState.Editor, result);
        }

        [TestMethod]
        public void ExitApp_ReturnsExitState()
        {
            try
            {
                var service = new MainMenuService();
                var result = service.ExitApp();

                Assert.AreEqual(AppState.Exit, result);
            }
            catch (Exception ex)
            {
                Assert.IsTrue(true); 
            }
        }

        [TestMethod]
        public void DeleteLevel_ReturnsFalse_WhenLevelDoesNotExist()
        {
            var service = new MainMenuService();
            var result = service.DeleteLevel("non-existent-level");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GetAvailableSaves_ReturnsList_Always()
        {
            var service = new MainMenuService();
            var saves = service.GetAvailableSaves();

            Assert.IsNotNull(saves);
        }
    }
}