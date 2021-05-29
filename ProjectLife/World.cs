using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProjectLife_v_0_3.ProjectLife
{
    public enum Direction
    {
        RIGHT = 0,
        UP_RIGHT,
        UP,
        UP_LEFT,
        LEFT,
        DOWN_LEFT,
        DOWN,
        DOWN_RIGHT
    }

    public static class Settings
    {
        public static readonly Random Random = new Random(DateTime.Now.Millisecond);

        public static class MineralCell
        {
            public const int MaxAge = 400;
            public const float Energy = 4f;
            public static readonly Color Color = Color.DimGray;
        }

        // OrganicCell settings
        public static class OrganicCell
        {
            public const int MaxAge = 300;
            public const float Energy = 15f;
            public static readonly Color Color = Color.LightGray;
        }

        // LifeCell settings
        public static class LifeCell
        {
            public const int GenomeLen = 8;
            public const int ChromosomeLen = 8;
            public const float MinSimilarity = 0.96f;
            public const int CommandsPerUpdate = 15;
            public const float MaxEnergy = 500f;
            public const float EnergyToDuplicate = 200f;
            public const float EnergyReducePerUpdate = 3f;
            public const float ChanceToMutate = 0.4f;
            public const float AgeEnergyCoefficient = 0.01f;
            public const float MaxEnergyDiffForAttack = 25f;
            public static readonly Color BaseColor = Color.YellowGreen;
        }

        // World settings
        public static class World
        {
            public const int UpdatesPerGeneration = 20;
            public const int WorldLevelsCount = 20;
            public const float EnergyReducePerLevel = 0.08f;
            public const float MaxEnergyLevel = 7f;
            public const int MaxMineralLevel = 7;
            public const int MineralsReducePerLevel = 1;
        }
    }

    public class World
    {
        // World params
        public int Width { get; }
        public int Height { get; }
        public float[] EnergyLevels;
        public int[] MineralLevels;

        public Cell[,] Cells { get; set; }
        private Cell[,] EmptyCells { get; set; }

        public int Generation;
        public int UpdateCount;
        public int MineralsCount;
        public int OrganicCount;
        public int LifeCount;

        public World(int width, int height)
        {
            Width = width;
            Height = height;

            BakeEnergyLevels();
            BakeMineralLevels();

            InitEmptyCells();
            Renew();
        }

        public void Update()
        {
            MineralsCount = 0;
            OrganicCount = 0;
            LifeCount = 0;

            for (int i = Height - 1; i >= 0; i--)
            {
                for (int j = Width - 1; j >= 0; j--)
                {
                    Cell cell = Cells[i, j];


                    int prevX = cell.X, prevY = cell.Y;
                    cell.Update(this);

                    if (cell.IsMoved)
                    {
                        NormalizeCoords(cell);
                        Cells[cell.Y, cell.X] = cell;
                        SetEmpty(prevX, prevY);
                    }

                    switch (cell.CellType)
                    {
                        case CellType.EMPTY:
                            break;

                        case CellType.ORGANIC:
                            OrganicCell organic = (OrganicCell) cell;

                            if (organic.Eaten)
                                SetEmpty(organic.X, organic.Y);

                            else if (organic.Denaturate)
                            {
                                MineralCell denaturedOrganic = new MineralCell(organic.X, organic.Y);
                                SetCell(denaturedOrganic, organic.X, organic.Y);
                            }
                            else OrganicCount++;

                            break;

                        case CellType.MINERAL:
                            MineralCell mineral = (MineralCell) cell;

                            if (mineral.Eaten || mineral.Denaturate)
                            {
                                SetEmpty(mineral.X, mineral.Y);
                            }
                            else MineralsCount++;

                            break;

                        case CellType.LIFE:
                            LifeCell life = (LifeCell) cell;

                            if (!life.Alive)
                            {
                                if (life.Energy >= Settings.OrganicCell.Energy)
                                {
                                    OrganicCell organicFromLife = new OrganicCell(life.X, life.Y);
                                    SetCell(organicFromLife, life.X, life.Y);
                                }

                                else
                                {
                                    SetEmpty(life.X, life.Y);
                                }
                            }
                            else LifeCount++;

                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            UpdateCount++;
            if (UpdateCount >= Settings.World.UpdatesPerGeneration)
            {
                Generation++;
                UpdateCount = 0;
            }
        }

        public void Renew()
        {
            InitCells();
            
            Cells[5, 50] = new LifeCell(50, 5, Direction.UP);
            
            Generation = 0;
            UpdateCount = 0;
            MineralsCount = 0;
            OrganicCount = 0;
            LifeCount = 1;
        }

        public void SetCell(Cell cell, int x, int y)
        {
            Cells[y, x] = cell;
        }

        public void SetEmpty(int x, int y)
        {
            Cells[y, x] = EmptyCells[y, x];
        }
        
        public float getEnergy(int y)
        {
            int EnergyLevel = y / (Height / Settings.World.WorldLevelsCount);
            return EnergyLevels[EnergyLevel];
        }

        public int getMinerals(int y)
        {
            int MineralLevel = y / (Height / Settings.World.WorldLevelsCount);
            return MineralLevels[MineralLevel];
        }

        public Cell getCellByDirection(Cell cell, Direction direction)
        {
            int otherCellX = cell.X;
            int otherCellY = cell.Y;

            switch (direction)
            {
                case Direction.RIGHT:
                    otherCellX++;
                    break;
                case Direction.UP_RIGHT:
                    otherCellX++;
                    otherCellY--;
                    break;
                case Direction.UP:
                    otherCellY--;
                    break;
                case Direction.UP_LEFT:
                    otherCellX--;
                    otherCellY--;
                    break;
                case Direction.LEFT:
                    otherCellX--;
                    break;
                case Direction.DOWN_LEFT:
                    otherCellX--;
                    otherCellY++;
                    break;
                case Direction.DOWN:
                    otherCellY++;
                    break;
                case Direction.DOWN_RIGHT:
                    otherCellX++;
                    otherCellY++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            if (otherCellX > Width - 1)
                otherCellX %= Width;

            else if (otherCellX < 0)
                otherCellX = Width + otherCellX;

            if (0 <= otherCellX && otherCellX < Width && 0 <= otherCellY && otherCellY < Height)
            {
                return Cells[otherCellY, otherCellX];
            }

            return null;
        }
        
        public void SaveToJSON(string path)
        {
            List<JsonSerializableCell> cells = new List<JsonSerializableCell>();

            foreach (Cell cell in Cells)
            {
                switch (cell.CellType)
                {
                    case CellType.EMPTY:
                        break;

                    case CellType.ORGANIC:
                        JsonSerializableCell jsonOrganicCell = JsonSerializableCell.FromOrganicCell((OrganicCell) cell);
                        cells.Add(jsonOrganicCell);
                        break;

                    case CellType.MINERAL:
                        JsonSerializableCell jsonMineralCell = JsonSerializableCell.FromMineralCell((MineralCell) cell);
                        cells.Add(jsonMineralCell);
                        break;

                    case CellType.LIFE:
                        JsonSerializableCell jsonLifeCell = JsonSerializableCell.FromLifeCell((LifeCell) cell);
                        cells.Add(jsonLifeCell);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            JsonSerializableWorld serializableWorld =
                new JsonSerializableWorld(Generation, UpdateCount, cells, Width, Height);

            string jsonString = JsonSerializer.Serialize(serializableWorld);
            File.WriteAllText(path, jsonString);
        }

        public void LoadFromJSON(string path)
        {
            JsonSerializableWorld serializableWorld;

            string jsonString = File.ReadAllText(path);
            try
            {
                serializableWorld = JsonSerializer.Deserialize<JsonSerializableWorld>(jsonString);
            }
            catch (Exception)
            {
                Console.WriteLine("[ERROR] Invalid json world file!");
                return;
            }

            if (serializableWorld != null && (serializableWorld.Width != Width || serializableWorld.Height != Height))
            {
                Console.WriteLine("[ERROR] Invalid world size!");
                return;
            }


            Generation = serializableWorld?.Generation ?? 0;
            UpdateCount = serializableWorld?.UpdateCount ?? 0;

            if (serializableWorld != null)
            {
                InitCells();
                foreach (var serializableCell in serializableWorld.Cells)
                {
                    Cells[serializableCell.Y, serializableCell.X] =
                        JsonSerializableCell.ToCell(serializableCell);
                }
            }

            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    Cells[i, j] ??= EmptyCells[i, j];
                }
            }
        }

        private void InitCells()
        {
            Cells = new Cell[Height, Width];

            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    Cells[i, j] = EmptyCells[i, j];
                }
            }
        }
        
        private void InitEmptyCells()
        {
            EmptyCells = new Cell[Height, Width];

            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                {
                    EmptyCells[i, j] = new EmptyCell(j, i);
                }
            }
        }

        private void BakeEnergyLevels()
        {
            EnergyLevels = new float[Settings.World.WorldLevelsCount];
            float curReduce = 1f;
            for (int i = 0; i < Settings.World.WorldLevelsCount; i++)
            {
                EnergyLevels[i] = Settings.World.MaxEnergyLevel * curReduce;
                curReduce *= 1f - Settings.World.EnergyReducePerLevel;
            }
        }

        private void BakeMineralLevels()
        {
            MineralLevels = new int[Settings.World.WorldLevelsCount];
            int curReduce = 0;
            for (int i = Settings.World.WorldLevelsCount - 1; i >= 0; i--)
            {
                if (Settings.World.MaxMineralLevel - curReduce <= 0)
                    MineralLevels[i] = 0;

                else
                    MineralLevels[i] = Settings.World.MaxMineralLevel - curReduce;

                curReduce += Settings.World.MineralsReducePerLevel;
            }
        }

        private void NormalizeCoords(Cell cell)
        {
            if (cell.X > Width - 1)
                cell.X %= Width;

            else if (cell.X < 0)
                cell.X = Width + cell.X;
        }
    }

    public class JsonSerializableWorld
    {
        [JsonInclude] public readonly int Generation;
        [JsonInclude] public readonly int UpdateCount;
        [JsonInclude] public readonly int Width;
        [JsonInclude] public readonly int Height;
        [JsonInclude] public readonly List<JsonSerializableCell> Cells;

        public JsonSerializableWorld(
            int generation, int updateCount,
            List<JsonSerializableCell> cells,
            int width, int height)
        {
            Generation = generation;
            UpdateCount = updateCount;
            Cells = cells;
            Width = width;
            Height = height;
        }
    }
}