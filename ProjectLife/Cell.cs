using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Windows.Media;
using Color = System.Drawing.Color;

namespace ProjectLife_v_0_3.ProjectLife
{
    public enum CellType
    {
        EMPTY = 0,
        ORGANIC,
        MINERAL,
        LIFE
    }

    public abstract class Cell
    {
        public CellType CellType { get; }

        public int X
        {
            get => _X;
            set
            {
                _X = value;
                IsMoved = true;
            }
        }

        public int Y
        {
            get => _Y;
            set
            {
                _Y = value;
                IsMoved = true;
            }
        }

        private int _X;
        private int _Y;
        public bool IsMoved { get; protected set; }
        public Color Color { get; set; }

        protected Cell(int x, int y, CellType cellType)
        {
            _X = x;
            _Y = y;
            CellType = cellType;
        }

        public abstract void Update(World world);
    }

    public class EmptyCell : Cell
    {
        public EmptyCell(int x, int y) : base(x, y, CellType.EMPTY)
        {
        }

        public override void Update(World world)
        {
            IsMoved = false;
        }
    }

    public class OrganicCell : Cell
    {
        public int Age;
        public bool Denaturate { get; private set; }
        public bool Eaten { get; private set; }

        public OrganicCell(int x, int y) : base(x, y, CellType.ORGANIC)
        {
            Color = Settings.OrganicCell.Color;
            Age = 0;

            Denaturate = false;
            Eaten = false;
        }

        public float Eat()
        {
            Eaten = true;
            return Settings.OrganicCell.Energy;
        }

        public override void Update(World world)
        {
            IsMoved = false;

            if (Denaturate || Eaten) return;

            Age++;

            if (Age > Settings.OrganicCell.MaxAge)
            {
                Denaturate = true;
            }
        }
    }

    public class MineralCell : Cell
    {
        public int Age;
        public bool Eaten { get; private set; }
        public bool Denaturate { get; private set; }

        public MineralCell(int x, int y) : base(x, y, CellType.MINERAL)
        {
            Age = 0;
            Color = Settings.MineralCell.Color;

            Denaturate = false;
            Eaten = false;
        }

        public int Eat()
        {
            Eaten = true;
            return 1;
        }

        public override void Update(World world)
        {
            IsMoved = false;

            if (Denaturate || Eaten) return;

            Age++;
            // Console.WriteLine($"Mineral at ({X} {Y}), under {cell.CellType} ({cell.X} {cell.Y})");

            if (world.getCellByDirection(this, Direction.DOWN)?.CellType == CellType.EMPTY)
            {
                Y++;
            }

            if (Age > Settings.MineralCell.MaxAge)
            {
                Denaturate = true;
            }
        }
    }

    public class LifeCell : Cell
    {
        private const int GenomeLen = Settings.LifeCell.GenomeLen;
        private const int ChromosomeLen = Settings.LifeCell.ChromosomeLen;

        public Direction Direction { get; set; }
        public float Energy { get; set; }
        public int Age { get; set; }
        public bool Alive { get; set; }
        public int[,] Genome { get; set; }
        public int CurrentCommand { get; set; }
        public int Minerals { get; set; }

        public LifeCell(
            int x,
            int y,
            Direction direction = Direction.RIGHT,
            float energy = 20f,
            int[,] genome = null
        ) : base(x, y, CellType.LIFE)
        {
            Energy = energy;
            Age = 0;
            Direction = direction;
            Color = Settings.LifeCell.BaseColor;
            Alive = true;

            Minerals = 0;
            CurrentCommand = 0;

            if (genome == null)
                Genome = new[,]
                {
                    {16, 16, 16, 16, 16, 16, 16, 16},
                    {16, 16, 16, 16, 16, 16, 16, 16},
                    {16, 16, 16, 16, 16, 16, 16, 16},
                    {16, 16, 16, 16, 16, 16, 16, 16},
                    {16, 16, 16, 16, 16, 16, 16, 16},
                    {16, 16, 16, 16, 16, 16, 16, 16},
                    {16, 16, 16, 16, 16, 16, 16, 16},
                    {16, 16, 16, 16, 16, 16, 16, 16},
                };

            else
                Genome = genome;
        }

        public override void Update(World world)
        {
            IsMoved = false;
            int currentCommandNum = 0;
            bool terminal = false;

            while (currentCommandNum++ < Settings.LifeCell.CommandsPerUpdate && !terminal && Alive)
            {
                int command = GetCurrentCommand();

                Direction direction;

                switch (command)
                {
                    case 10: // Move relative
                        direction = (Direction) ((GetRelativeCommand(1) % 8 + (int) Direction) % 8);
                        CommandShift(GetRelativeCommand(Move(world, direction) + 1));
                        break;

                    case 12: // Move absolute
                        CommandShift(GetRelativeCommand(Move(world, Direction)));
                        break;

                    case 14: // Attack cell relative (terminal)
                        direction = (Direction) ((GetRelativeCommand(1) % 8 + (int) Direction) % 8);
                        CommandShift(GetRelativeCommand(AttackCell(world, direction) + 1));
                        terminal = true;
                        break;

                    case 16: // Photosynthesis (terminal)
                        Photosynthesis(world);
                        CommandShift(GetRelativeCommand(1));
                        GoGreen(1);
                        terminal = true;
                        break;

                    case 18: // Attack cell absolute (terminal)
                        CommandShift(GetRelativeCommand(AttackCell(world, Direction)));
                        terminal = true;
                        break;

                    case 20: // Rotate
                        Rotate(GetRelativeCommand(1));
                        CommandShift(GetRelativeCommand(2));
                        break;

                    case 22: // Check energy
                        CommandShift(GetRelativeCommand(CompareEnergy(GetRelativeCommand(1)) + 1));
                        break;

                    case 24: // Check cell relative
                        direction = (Direction) ((GetRelativeCommand(1) % 8 + (int) Direction) % 8);
                        CommandShift(GetRelativeCommand(CheckCell(world, direction) + 1));
                        break;

                    case 26: // Check cell absolute
                        CommandShift(GetRelativeCommand(CheckCell(world, Direction)));
                        break;

                    case 28: // Mineral to Energy
                        MineralsToEnergy();
                        CommandShift(GetRelativeCommand(1));
                        GoBlue(5);
                        break;

                    case 30: // Donate energy relative
                        direction = (Direction) ((GetRelativeCommand(1) % 8 + (int) Direction) % 8);
                        CommandShift(GetRelativeCommand(DonateEnergy(world, direction) + 1));
                        break;

                    case 32: // Donate energy absolute
                        CommandShift(GetRelativeCommand(DonateEnergy(world, Direction)));
                        break;

                    case 34: // Donate minerals relative
                        direction = (Direction) ((GetRelativeCommand(1) % 8 + (int) Direction) % 8);
                        CommandShift(GetRelativeCommand(DonateMinerals(world, direction) + 1));
                        break;

                    case 36: // Donate minerals absolute
                        CommandShift(GetRelativeCommand(DonateMinerals(world, Direction)));
                        break;
                    
                    case 38: // Check energy
                        CommandShift(CheckEnergy(GetRelativeCommand(1) + 1));
                        break;

                    case 47: // Chemosynthesis (terminal)
                        ChemoSynthesis(world);
                        CommandShift(GetRelativeCommand(1));
                        terminal = true;
                        break;

                    default:
                        CommandShift(command);
                        break;
                }

                // Енергия больше максимальной ?
                if (Energy >= Settings.LifeCell.MaxEnergy)
                {
                    Energy = Settings.LifeCell.MaxEnergy;
                }

                // Хватает эенергии для деления ?
                if (Energy >= Settings.LifeCell.EnergyToDuplicate)
                {
                    Duplicate(world);
                    // Console.WriteLine($"Cell (X: {X}, Y: {Y}, Energy: {Energy}) was duplicated!");
                    terminal = true;
                }

                // Осталась ли энергия ?
                if (Energy <= 0)
                {
                    if (Minerals > 0)
                        MineralsToEnergy();
                    else
                        Kill();
                }
            }

            Age++;
            Energy -= Settings.LifeCell.EnergyReducePerUpdate + Age * Settings.LifeCell.AgeEnergyCoefficient;
        }

        int GetCurrentCommand()
        {
            return Genome[CurrentCommand / GenomeLen,
                CurrentCommand % ChromosomeLen];
        }

        int GetRelativeCommand(int shift)
        {
            return Genome[(CurrentCommand + shift) / ChromosomeLen % GenomeLen,
                (CurrentCommand + shift) % ChromosomeLen];
        }

        void CommandShift(int shift)
        {
            CurrentCommand = (CurrentCommand + shift) % (GenomeLen * ChromosomeLen);
        }

        bool CheckSurround(World world)
        {
            for (int i = 0; i < 8; i++)
            {
                if (world.getCellByDirection(this, (Direction) i)?.CellType == CellType.EMPTY)
                {
                    return false;
                }
            }

            return true;
        }

        private int CompareEnergy(int numParts)
        {
            float energyPart = Settings.LifeCell.MaxEnergy / 64;

            if (Energy >= energyPart * numParts)
            {
                return 1;
            }

            return 2;
        }

        private int CheckCell(World world, Direction direction)
        {
            Cell cell = world.getCellByDirection(this, direction);
            if (cell == null)
                return 6;

            switch (cell.CellType)
            {
                case CellType.EMPTY:
                    return 1;
                case CellType.ORGANIC:
                    return 2;
                case CellType.MINERAL:
                    return 3;
                case CellType.LIFE:

                    // Сongener cell
                    if (CompareGenome((LifeCell) cell))
                        return 4;

                    // Other cell
                    else
                        return 5;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private int CheckEnergy(int param)
        {
            float EnergyPart = Settings.LifeCell.MaxEnergy / 64;

            if (param * EnergyPart >= Energy)
            {
                return 1;
            }

            return 2;
        }

        private bool CompareGenome(LifeCell otherCell)
        {
            int genomeDiff = 0;
            for (int i = 0; i < GenomeLen; i++)
            {
                for (int j = 0; j < ChromosomeLen; j++)
                {
                    if (Genome[i, j] != otherCell.Genome[i, j])
                        genomeDiff++;
                }
            }

            if (genomeDiff / GenomeLen * ChromosomeLen >= Settings.LifeCell.MinSimilarity)
                return true;

            return false;
        }

        private void Rotate(int angle)
        {
            Direction = (Direction) (((int) Direction + angle) % 8);
        }

        private void MineralsToEnergy()
        {
            Energy += Minerals * Settings.MineralCell.Energy;
            Minerals = 0;
        }

        private int AttackCell(World world, Direction direction)
        {
            Cell cell = world.getCellByDirection(this, direction);

            if (cell == null)
                return 6;

            switch (cell.CellType)
            {
                case CellType.EMPTY:
                    return 1;

                case CellType.ORGANIC:
                    Energy += ((OrganicCell) cell).Eat();
                    GoGreen(5);
                    return 2;

                case CellType.MINERAL:
                    Minerals += ((MineralCell) cell).Eat();
                    GoBlue(10);
                    return 3;

                case CellType.LIFE:
                    LifeCell otherLifeCell = (LifeCell) cell;
                    if (otherLifeCell.Energy - Settings.LifeCell.MaxEnergyDiffForAttack < Energy &&
                        otherLifeCell.Minerals <= Minerals)
                    {
                        Minerals -= otherLifeCell.Minerals;
                        Energy += otherLifeCell.Energy / 2;

                        otherLifeCell.Energy = 0;
                        otherLifeCell.Kill();
                        GoRed(100);
                    }
                    else if (otherLifeCell.Energy - Settings.LifeCell.MaxEnergyDiffForAttack < Energy &&
                             otherLifeCell.Minerals > Minerals)
                    {
                        otherLifeCell.Minerals -= Minerals;
                        Energy -= otherLifeCell.Minerals * Settings.MineralCell.Energy;

                        Minerals = 0;
                        otherLifeCell.Minerals = 0;

                        if (Energy > 0)
                        {
                            Energy += otherLifeCell.Energy;

                            otherLifeCell.Energy = 0;
                            otherLifeCell.Kill();
                            GoRed(100);
                        }
                        else
                        {
                            Kill();
                        }
                    }
                    else
                    {
                        otherLifeCell.Energy -= Energy;
                        Kill();
                    }

                    return 4;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private int Duplicate(World world)
        {
            if (CheckSurround(world))
            {
                Kill();
                return 2;
            }

            for (int i = 0; i < 8; i++)
            {
                Cell cell = world.getCellByDirection(this, (Direction) i);

                if (cell == null)
                    continue;

                if (cell.CellType == CellType.EMPTY)
                {
                    int[,] childGenome = (int[,]) Genome.Clone();

                    if (Settings.Random.NextDouble() > 1f - Settings.LifeCell.ChanceToMutate)
                    {
                        MutateGenome(childGenome);
                    }

                    LifeCell child = new LifeCell(cell.X, cell.Y, (Direction) i, Energy / 2, childGenome);

                    Energy /= 2;

                    world.SetCell(child, cell.X, cell.Y);

                    return 1;
                }
            }

            return 2;
        }

        private void MutateGenome(int[,] genome)
        {
            int MutationsCount = Settings.Random.Next(1, 2);

            for (int i = 0; i < MutationsCount; i++)
            {
                int mutatedGenomeNum = Settings.Random.Next(0, GenomeLen);
                int mutatedChromosomeNum = Settings.Random.Next(0, ChromosomeLen);

                genome[mutatedGenomeNum, mutatedChromosomeNum] = Settings.Random.Next(0, 64);
            }
        }

        private int Move(World world, Direction direction)
        {
            int ret = CheckCell(world, direction);

            // if empty cell
            if (ret == 1)
            {
                switch (direction)
                {
                    case Direction.RIGHT:
                        X++;
                        break;
                    case Direction.UP_RIGHT:
                        Y--;
                        X++;
                        break;
                    case Direction.UP:
                        Y--;
                        break;
                    case Direction.UP_LEFT:
                        Y--;
                        X--;
                        break;
                    case Direction.LEFT:
                        X--;
                        break;
                    case Direction.DOWN_LEFT:
                        Y++;
                        X--;
                        break;
                    case Direction.DOWN:
                        Y++;
                        break;
                    case Direction.DOWN_RIGHT:
                        Y++;
                        X++;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
                }

                // if (X >= 100 || X < 0 || Y >= 100 || Y < 0)
                // {
                //     Console.WriteLine($"Cell (X: {X}, Y: {Y}, Energy: {Energy}) out of bounds!");
                // }
            }

            return ret;
        }

        private void Photosynthesis(World world)
        {
            float energy = world.getEnergy(Y);
            if (Energy >= 4f)
            {
                GoGreen(1);
                Energy += energy;
            }
        }

        private void ChemoSynthesis(World world)
        {
            int minerals = world.getMinerals(Y);
            if (minerals > 0)
            {
                GoBlue(5);
                Minerals += minerals;
            }
        }

        private int DonateEnergy(World world, Direction direction)
        {
            Cell cell = world.getCellByDirection(this, direction);

            if (cell == null)
                return 6;

            switch (cell.CellType)
            {
                case CellType.EMPTY:
                    return 1;

                case CellType.ORGANIC:
                    return 2;

                case CellType.MINERAL:
                    return 3;

                case CellType.LIFE:
                    LifeCell otherCell = (LifeCell) cell;
                    otherCell.Energy += Energy / 2;
                    Energy /= 2;
                    return 5;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private int DonateMinerals(World world, Direction direction)
        {
            Cell cell = world.getCellByDirection(this, direction);

            if (cell == null)
                return 6;

            switch (cell.CellType)
            {
                case CellType.EMPTY:
                    return 1;

                case CellType.ORGANIC:
                    return 2;

                case CellType.MINERAL:
                    return 3;

                case CellType.LIFE:
                    LifeCell otherCell = (LifeCell) cell;
                    otherCell.Minerals += Minerals / 2;
                    Minerals /= 2;
                    return 5;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Kill()
        {
            Alive = false;
        }

        private void GoRed(int v)
        {
            var Green = Color.G;
            var Red = Color.R;
            var Blue = Color.B;

            if (v > 255) v = 255;
            byte bv = (byte) v;

            if (Green - 5 >= 0) Green -= bv;
            else Green = 0;

            if (Red + 5 <= 255) Red += bv;
            else Red = 255;

            if (Blue - 3 >= 0) Blue -= bv;
            else Blue = 0;

            Color = Color.FromArgb(Red, Green, Blue);
        }

        private void GoGreen(int v)
        {
            var Green = Color.G;
            var Red = Color.R;
            var Blue = Color.B;

            if (v > 255) v = 255;
            byte bv = (byte) v;

            if (Green + 5 <= 255) Green += bv;
            else Green = 255;

            if (Red - 5 >= 0) Red -= bv;
            else Red = 0;

            if (Blue - 3 >= 0) Blue -= bv;
            else Blue = 0;

            Color = Color.FromArgb(Red, Green, Blue);
        }

        private void GoBlue(int v)
        {
            var Green = Color.G;
            var Red = Color.R;
            var Blue = Color.B;

            if (v > 255) v = 255;
            byte bv = (byte) v;

            if (Green - 3 >= 0) Green -= bv;
            else Green = 0;

            if (Red - 5 >= 0) Red -= bv;
            else Red = 0;

            if (Blue + 3 <= 255) Blue += bv;
            else Blue = 255;

            Color = Color.FromArgb(Red, Green, Blue);
        }
    }

    public class JsonSerializableCell
    {
        [JsonInclude] public int X;
        [JsonInclude] public int Y;
        [JsonInclude] public CellType CellType;
        [JsonInclude] public List<int> Color;
        [JsonInclude] public int Age;
        [JsonInclude] public List<List<int>> Genome;
        [JsonInclude] public int Minerals;
        [JsonInclude] public float Energy;
        [JsonInclude] public int CurrentCommand;
        [JsonInclude] public Direction Direction;

        public JsonSerializableCell(int x, int y, CellType cellType)
        {
            CellType = cellType;
            X = x;
            Y = y;
        }

        public static JsonSerializableCell FromLifeCell(LifeCell cell)
        {
            List<List<int>> convertedGenome = new List<List<int>>();
            for (int i = 0; i < Settings.LifeCell.GenomeLen; i++)
            {
                convertedGenome.Add(new List<int>());
                for (int j = 0; j < Settings.LifeCell.ChromosomeLen; j++)
                {
                    convertedGenome[i].Add(cell.Genome[i, j]);
                }
            }

            JsonSerializableCell serializableLifeCell = new JsonSerializableCell(cell.X, cell.Y, CellType.LIFE)
            {
                Age = cell.Age,
                Minerals = cell.Minerals,
                Color = new List<int> {cell.Color.R, cell.Color.G, cell.Color.B, cell.Color.A},
                CurrentCommand = cell.CurrentCommand,
                Direction = cell.Direction,
                Energy = cell.Energy,
                Genome = convertedGenome
            };

            return serializableLifeCell;
        }

        public static JsonSerializableCell FromMineralCell(MineralCell cell)
        {
            JsonSerializableCell serializableCell = new JsonSerializableCell(cell.X, cell.Y, CellType.MINERAL)
            {
                Age = cell.Age
            };

            return serializableCell;
        }

        public static JsonSerializableCell FromOrganicCell(OrganicCell cell)
        {
            JsonSerializableCell serializableCell = new JsonSerializableCell(cell.X, cell.Y, CellType.ORGANIC)
            {
                Age = cell.Age
            };

            return serializableCell;
        }

        public static Cell ToCell(JsonSerializableCell cell)
        {
            switch (cell.CellType)
            {
                case CellType.ORGANIC:
                    OrganicCell organicCell = new OrganicCell(cell.X, cell.Y)
                    {
                        Age = cell.Age
                    };
                    return organicCell;

                case CellType.MINERAL:
                    MineralCell mineralCell = new MineralCell(cell.X, cell.Y)
                    {
                        Age = cell.Age
                    };

                    return mineralCell;

                case CellType.LIFE:
                    int[,] genome = new int[Settings.LifeCell.GenomeLen, Settings.LifeCell.ChromosomeLen];
                    for (int i = 0; i < Settings.LifeCell.GenomeLen; i++)
                    {
                        for (int j = 0; j < Settings.LifeCell.ChromosomeLen; j++)
                        {
                            genome[i, j] = cell.Genome[i][j];
                        }
                    }

                    LifeCell lifeCell = new LifeCell(cell.X, cell.Y, cell.Direction, cell.Energy, genome)
                    {
                        Color = System.Drawing.Color.FromArgb(
                            cell.Color[3], cell.Color[0],
                            cell.Color[1], cell.Color[2]),
                        Energy = cell.Energy,
                        Minerals = cell.Minerals,
                        Age = cell.Age,
                        CurrentCommand = cell.CurrentCommand
                    };

                    return lifeCell;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}