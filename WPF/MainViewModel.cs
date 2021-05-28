using Microsoft.Win32;
using ProjectLife_v_0_3.ProjectLife;
using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ProjectLife_v_0_3.WPF
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
            ;
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

                    if (needResume) Paused = true;
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
    }
}