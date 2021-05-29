using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Color = System.Drawing.Color;

namespace ProjectLife_v_0_3.ProjectLife
{
    public enum DrawMode
    {
        DEFAULT,
        ENERGY,
        MINERALS
    }

    public class Drawer
    {
        // Params
        private const byte maxEnergyGreen = 180;

        private readonly World World;
        private readonly int CellSize;
        private byte[] WorldBackground;
        public DrawMode Mode { get; private set; }

        private Int32Rect CellRect;
        private byte[] CellColorBuffer;


        public WriteableBitmap Bitmap { get; private set; }

        public Drawer(World world, int cellSize)
        {
            World = world;
            CellSize = cellSize;


            InitCellRect();
            InitCellColorBuffer();

            InitBitmap();
            CreateWorldBackground();

            Mode = DrawMode.DEFAULT;
        }

        public void ChangeDrawingMode()
        {
            switch (Mode)
            {
                case DrawMode.DEFAULT:
                    Mode = DrawMode.ENERGY;
                    break;
                case DrawMode.ENERGY:
                    Mode = DrawMode.DEFAULT;
                    break;
                case DrawMode.MINERALS:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private void InitCellRect()
        {
            CellRect = new Int32Rect(0, 0, CellSize, CellSize);
        }

        private void InitCellColorBuffer()
        {
            CellColorBuffer = new byte[3 * CellSize * CellSize];
        }

        private void ColorBufferSetColor(Cell cell)
        {
            if (cell.CellType == CellType.LIFE)
            {
                LifeCell lifeCell = (LifeCell) cell;
                switch (Mode)
                {
                    case DrawMode.DEFAULT:
                        FillColorBuffer(lifeCell.Color);
                        break;
                    case DrawMode.ENERGY:
                        byte Parts = 20;

                        byte BaseGreen = 140;
                        byte BaseRed = 155;
                        byte BaseBlue = 20;
                        byte MaxBlue = 80;

                        float EnergyPerPart = Settings.LifeCell.MaxEnergy / Parts;

                        byte RedPerPart = (byte) ((255 - BaseRed) / Parts);
                        byte GreenPerPart = (byte) (BaseGreen / Parts);
                        byte BluePerPart = (byte) ((MaxBlue - BaseBlue) / Parts);

                        byte CurrentPartsNum = (byte) (lifeCell.Energy / EnergyPerPart);
                        byte ReversPartsNum = (byte) (Parts - CurrentPartsNum);

                        FillColorBuffer(
                            Color.FromArgb(
                                (byte) (BaseRed + RedPerPart * CurrentPartsNum),
                                (byte) (BaseGreen - GreenPerPart * CurrentPartsNum),
                                (byte) (BaseBlue + BluePerPart * ReversPartsNum))
                        );

                        break;
                    case DrawMode.MINERALS:
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                FillColorBuffer(cell.Color);
            }
        }

        private void FillColorBuffer(Color color)
        {
            for (var i = 0; i < CellSize * CellSize * 3; i += 3)
            {
                CellColorBuffer[i] = color.R;
                CellColorBuffer[i + 1] = color.G;
                CellColorBuffer[i + 2] = color.B;
            }
        }

        private void InitBitmap()
        {
            Bitmap = new WriteableBitmap(World.Width * CellSize,
                World.Height * CellSize, 96, 96, PixelFormats.Rgb24, null);
        }

        private void CreateWorldBackground()
        {
            WorldBackground = new byte[World.Width * World.Height * CellSize * CellSize * 3];

            int pixelsPerLevel = World.Height * CellSize / Settings.World.WorldLevelsCount;
            int greenReducePerLevel = maxEnergyGreen / Settings.World.WorldLevelsCount;

            byte Red = 15;
            byte Green = maxEnergyGreen;
            byte Blue = 50;

            for (int i = 0; i < World.Height * CellSize; i++)
            {
                if (i % pixelsPerLevel == 0)
                {
                    Green -= (byte) greenReducePerLevel;
                }

                for (int j = 0; j < World.Width * CellSize; j++)
                {
                    int pos = (World.Width * CellSize * i + j) * 3;
                    WorldBackground[pos] = Red;
                    WorldBackground[pos + 1] = Green;
                    WorldBackground[pos + 2] = Blue;
                }
            }
        }

        private void DrawBackground()
        {
            int pixelsInHeight = World.Height * CellSize;
            int pixelsInWidth = World.Width * CellSize;

            Bitmap.WritePixels(new Int32Rect(0, 0, pixelsInWidth, pixelsInHeight), WorldBackground,
                3 * World.Width * CellSize, 0);
        }

        public void DrawWorld()
        {
            DrawBackground();

            foreach (Cell cell in World.Cells)
            {
                if (cell.CellType != CellType.EMPTY)
                {
                    CellRect.X = cell.X * CellSize;
                    CellRect.Y = cell.Y * CellSize;

                    ColorBufferSetColor(cell);

                    Bitmap.WritePixels(CellRect, CellColorBuffer, 3 * CellSize, 0);
                }
            }
        }
    }
}