using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;
using ProjectLife.Core;

namespace ProjectLife.WPF
{
    public class MainViewModel : Notifier
    {
        private int updatesPerFrame = 1;

        public int UpdatesPerFrame
        {
            get { return updatesPerFrame; }
            set
            {
                updatesPerFrame = value;
                NotifyPropertyChanged(nameof(UpdatesPerFrame));
            }
        }

        private bool paused = true;

        public bool Paused
        {
            get { return paused; }
            set
            {
                paused = value;
                NotifyPropertyChanged(nameof(Paused));
            }
        }

        private DrawMode drawMode = DrawMode.DEFAULT;

        public DrawMode DrawMode
        {
            get { return drawMode; }
            private set
            {
                drawMode = value;
                NotifyPropertyChanged(nameof(DrawMode));
            }
        }

        private int generation;

        public int Generation
        {
            get { return generation; }
            set
            {
                generation = value;
                NotifyPropertyChanged(nameof(Generation));
            }
        }

        private int mineralsCount;

        public int MineralsCount
        {
            get { return mineralsCount; }
            set
            {
                mineralsCount = value;
                NotifyPropertyChanged(nameof(MineralsCount));
            }
        }

        private int organicCount;

        public int OrganicCount
        {
            get { return organicCount; }
            set
            {
                organicCount = value;
                NotifyPropertyChanged(nameof(OrganicCount));
            }
        }

        private int lifeCount;

        public int LifeCount
        {
            get { return lifeCount; }
            set
            {
                lifeCount = value;
                NotifyPropertyChanged(nameof(LifeCount));
            }
        }

        public WriteableBitmap Bitmap { get; private init; }

        private readonly World world;
        private readonly Drawer drawer;

        public MainViewModel()
        {
            world = new World(120, 80);
            drawer = new Drawer(world, 6);

            DrawMode = drawer.Mode;
            Bitmap = drawer.Bitmap;
            NotifyPropertyChanged(nameof(Bitmap));

            var timer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(1)};
            timer.Tick += (o, args) =>
            {
                if (!Paused)
                {
                    for (int i = 0; i < UpdatesPerFrame; i++)
                        world.Update();
                }

                Generation = world.Generation;
                MineralsCount = world.MineralsCount;
                OrganicCount = world.OrganicCount;
                LifeCount = world.LifeCount;

                drawer.DrawWorld();
            };
            timer.Start();
        }

        private RelayCommand togglePauseCommand;

        public RelayCommand TogglePauseCommand =>
            togglePauseCommand ??= new RelayCommand
            (
                _ => Paused = !Paused
            );

        private RelayCommand switchDrawModeCommand;

        public RelayCommand SwitchDrawModeCommand =>
            switchDrawModeCommand ??= new RelayCommand
            (
                _ =>
                {
                    drawer.ChangeDrawingMode();
                    DrawMode = drawer.Mode;
                }
            );

        private RelayCommand saveCommand;

        public RelayCommand SaveCommand =>
            saveCommand ??= new RelayCommand
            (
                _ =>
                {
                    bool needResume = !Paused;
                    Paused = true;

                    SaveFileDialog saveFileDialog = new SaveFileDialog
                    {
                        Filter = "JSON File (*.json)|*.json|All files (*.*)|*.*"
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        string path = saveFileDialog.FileName;
                        world.SaveToJSON(path);
                    }

                    if (needResume) Paused = false;
                }
            );

        private RelayCommand loadCommand;

        public RelayCommand LoadCommand =>
            loadCommand ??= new RelayCommand
            (
                _ =>
                {
                    bool needResume = !Paused;

                    Paused = true;

                    OpenFileDialog openFileDialog = new OpenFileDialog
                    {
                        Filter = "JSON File (*.json)|*.json|All files (*.*)|*.*"
                    };

                    if (openFileDialog.ShowDialog() == true)
                    {
                        string path = openFileDialog.FileName;
                        world.LoadFromJSON(path);
                    }
                    else if (needResume) Paused = false;
                }
            );

        private RelayCommand clearCommand;

        public RelayCommand ClearCommand =>
            clearCommand ??= new RelayCommand
            (
                _ =>
                {
                    bool needResume = !Paused;
                    Paused = true;

                    MessageBoxResult result = MessageBox.Show(WindowLang.ClearConfirmationInfo,
                        "Подтверждение очистки мира", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        world.Renew();
                    }

                    else if (needResume) Paused = true;
                }
            );

        private RelayCommand revisionCellCommand;

        public RelayCommand RevisionCellCommand =>
            revisionCellCommand ??= new RelayCommand
            (
                obj =>
                {
                    Size size;

                    if (obj is Image canvas) size = canvas.RenderSize;
                    else return;

                    bool needResume = !Paused;
                    Point pos = Mouse.GetPosition(canvas);

                    if (pos.X < 0 || pos.Y < 0 || pos.X > size.Width || pos.Y > size.Height) return;
                    Paused = true;

                    double RenderWidthPerCell = size.Width / world.Width;
                    double RenderHeightPerCell = size.Height / world.Height;

                    int X = (int) (pos.X / RenderWidthPerCell);
                    int Y = (int) (pos.Y / RenderHeightPerCell);

                    Cell cell = world.Cells[Y, X];
                    string ShowString = $"Cell position: {X}, {Y}";

                    if (cell.CellType == CellType.LIFE)
                    {
                        LifeCell lifeCell = (LifeCell) cell;
                        string genomeString = "";
                        for (int i = 0; i < Settings.LifeCell.GenomeLen; i++)
                        {
                            string chromosomeString = "";
                            for (int j = 0; j < Settings.LifeCell.ChromosomeLen; j++)
                            {
                                if (j != Settings.LifeCell.ChromosomeLen - 1)
                                    chromosomeString += lifeCell.Genome[i, j] + " ";
                                else
                                    chromosomeString += lifeCell.Genome[i, j];
                            }

                            genomeString += chromosomeString + '\n';
                        }

                        ShowString += '\n' + genomeString;
                    }
                    
                    else return;
                    
                        MessageBoxResult result = MessageBox.Show(ShowString, "inspect cell");
                    if (result == MessageBoxResult.OK)
                    {
                        if (needResume) Paused = false;
                    }
                }
            );
    }
}