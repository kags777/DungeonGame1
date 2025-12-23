using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json; 
using Newtonsoft.Json.Converters; 

namespace DungeonGame1
{
    public enum AppState { MainMenu, Game, Editor, Exit }
    public enum GameStatus { Playing, Paused, Victory, Defeat }
    public enum FacingDirection { None, Up, Down, Left, Right }
    public enum EntityVisualType { Player, Enemy, Wall, Trap, Crystal, Exit, Empty }

    public class LevelInfoDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class SaveInfoDTO
    {
        public string Id { get; set; }
        public string LevelName { get; set; }
        public string SaveTime { get; set; }
        public int Score { get; set; }
    }

    public class GameStateDTO
    {
        public GameStatus Status { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Score { get; set; }
        public int CrystalsCollected { get; set; }
        public int TotalCrystals { get; set; }
        public List<TileDTO> Map { get; set; } = new List<TileDTO>();
    }

    public class EditorStateDTO
    {
        public string LevelName { get; set; }
        public int Width { get; set; } = 10;
        public int Height { get; set; } = 10;
        public List<TileDTO> Map { get; set; } = new List<TileDTO>();
        public List<AvailableEntityDTO> AvailableEntities { get; set; } = new List<AvailableEntityDTO>();
    }

    public class TileDTO
    {
        public int X { get; set; }
        public int Y { get; set; }
        public EntityVisualType EntityType { get; set; }
        public FacingDirection FacingDirection { get; set; }
    }

    public class AvailableEntityDTO
    {
        public EntityVisualType Type { get; set; }
        public string Name { get; set; }
    }

    public class LevelData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public List<TileDTO> Tiles { get; set; } = new List<TileDTO>();
    }

    public class SaveData
    {
        public string Id { get; set; }
        public string LevelId { get; set; }
        public string SaveTime { get; set; }
        public GameStateDTO GameState { get; set; }
    }
}