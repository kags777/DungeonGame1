using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace DungeonGame1.Tests
{
    [TestClass]
    public class EnumTests
    {
        [TestMethod]
        public void AppState_Enum_HasAllValues()
        {
            // Arrange & Act
            var values = Enum.GetValues(typeof(AppState)).Cast<AppState>().ToList();

            // Assert
            Assert.IsTrue(values.Contains(AppState.MainMenu));
            Assert.IsTrue(values.Contains(AppState.Game));
            Assert.IsTrue(values.Contains(AppState.Editor));
            Assert.IsTrue(values.Contains(AppState.Exit));
            Assert.AreEqual(4, values.Count);
        }

        [TestMethod]
        public void GameStatus_Enum_HasAllValues()
        {
            // Arrange & Act
            var values = Enum.GetValues(typeof(GameStatus)).Cast<GameStatus>().ToList();

            // Assert
            Assert.IsTrue(values.Contains(GameStatus.Playing));
            Assert.IsTrue(values.Contains(GameStatus.Paused));
            Assert.IsTrue(values.Contains(GameStatus.Victory));
            Assert.IsTrue(values.Contains(GameStatus.Defeat));
            Assert.AreEqual(4, values.Count);
        }

        [TestMethod]
        public void FacingDirection_Enum_HasAllValues()
        {
            // Arrange & Act
            var values = Enum.GetValues(typeof(FacingDirection)).Cast<FacingDirection>().ToList();

            // Assert
            Assert.IsTrue(values.Contains(FacingDirection.None));
            Assert.IsTrue(values.Contains(FacingDirection.Up));
            Assert.IsTrue(values.Contains(FacingDirection.Down));
            Assert.IsTrue(values.Contains(FacingDirection.Left));
            Assert.IsTrue(values.Contains(FacingDirection.Right));
            Assert.AreEqual(5, values.Count);
        }

        [TestMethod]
        public void EntityVisualType_Enum_HasAllValues()
        {
            // Arrange & Act
            var values = Enum.GetValues(typeof(EntityVisualType)).Cast<EntityVisualType>().ToList();

            // Assert
            Assert.IsTrue(values.Contains(EntityVisualType.Player));
            Assert.IsTrue(values.Contains(EntityVisualType.Enemy));
            Assert.IsTrue(values.Contains(EntityVisualType.Wall));
            Assert.IsTrue(values.Contains(EntityVisualType.Trap));
            Assert.IsTrue(values.Contains(EntityVisualType.Crystal));
            Assert.IsTrue(values.Contains(EntityVisualType.Exit));
            Assert.IsTrue(values.Contains(EntityVisualType.Empty));
            Assert.AreEqual(7, values.Count);
        }
    }
}