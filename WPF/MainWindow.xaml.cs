using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using ProjectLife_v_0_3.ProjectLife;

namespace ProjectLife_v_0_3.WPF
{
    public static class WindowLang
    {
        public static readonly string CellMode = "Клетка";
        public static readonly string EnergyMode = "Энергия";
        public static readonly string MineralMode = "Минералы";
        public static readonly string PauseState = "Пауза";
        public static readonly string SimulatingState = "Идёт симуляция";
        public static readonly string ClearConfirmationInfo = "Вы уверены что хотите начать с начала?";
    }


    public partial class MainWindow : Window
    {
        public bool Paused { get; private set; }

        private SolidColorBrush RedBrush = new(Colors.Red);
        private SolidColorBrush GreenBrush = new(Colors.Green);
        private SolidColorBrush BlueBrush = new(Colors.DarkBlue);
        private Drawer Drawer;
        private World World;

        public MainWindow(Drawer drawer, World world)
        {
            InitializeComponent();
            Paused = true;

            Drawer = drawer;
            img.Source = drawer.Bitmap;

            World = world;
            
            InitLabels();
        }

        private void InitLabels()
        {
            if (Paused) SetPauseLabel();
            else SetResumeLabel();
            SetDrawingModeLabel(Drawer.Mode);
        }

        private void SetDrawingModeLabel(DrawMode mode)
        {
            switch (Drawer.Mode)
            {
                case DrawMode.DEFAULT:
                    SetCellModeLabel();
                    break;

                case DrawMode.ENERGY:
                    SetEnergyModeLabel();
                    break;

                case DrawMode.MINERALS:
                    SetMineralModeLabel();
                    break;
            }
        }

        private void Pause()
        {
            Paused = true;
            SetPauseLabel();
        }

        private void Resume()
        {
            Paused = false;
            SetResumeLabel();
        }

        private void SetPauseLabel()
        {
            State.Text = WindowLang.PauseState;
            State.Foreground = RedBrush;
        }

        private void SetResumeLabel()
        {
            State.Text = WindowLang.SimulatingState;
            State.Foreground = GreenBrush;
        }

        private void SetCellModeLabel()
        {
            DrawingMode.Text = WindowLang.CellMode;
            DrawingMode.Foreground = GreenBrush;
        }

        private void SetEnergyModeLabel()
        {
            DrawingMode.Text = WindowLang.EnergyMode;
            DrawingMode.Foreground = RedBrush;
        }

        private void SetMineralModeLabel()
        {
            DrawingMode.Text = WindowLang.MineralMode;
            DrawingMode.Foreground = BlueBrush;
        }

        private void NextDrawMode()
        {
            Drawer.ChangeDrawingMode();
            SetDrawingModeLabel(Drawer.Mode);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.P:
                    if (!Paused) Pause();
                    else Resume();
                    break;

                case Key.M:
                    NextDrawMode();
                    break;

                case Key.S:
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        bool NeedResume = !Paused;
                        Pause();

                        SaveFileDialog saveFileDialog = new SaveFileDialog
                        {
                            Filter = "JSON File (*.json)|*.json|All files (*.*)|*.*"
                        };

                        if (saveFileDialog.ShowDialog() == true)
                        {
                            string path = saveFileDialog.FileName;
                            World.SaveToJSON(path);
                        }

                        if (NeedResume) Resume();
                    }

                    break;

                case Key.L:
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        bool NeedResume = !Paused;

                        Paused = true;
                        SetPauseLabel();

                        OpenFileDialog openFileDialog = new OpenFileDialog
                        {
                            Filter = "JSON File (*.json)|*.json|All files (*.*)|*.*"
                        };

                        if (openFileDialog.ShowDialog() == true)
                        {
                            string path = openFileDialog.FileName;
                            World.LoadFromJSON(path);
                        }
                        else if (NeedResume) Resume();
                    }

                    break;

                case Key.X:
                    if (Keyboard.Modifiers == ModifierKeys.Control)
                    {
                        bool NeedResume = !Paused;
                        Pause();

                        MessageBoxResult result = MessageBox.Show(WindowLang.ClearConfirmationInfo,
                            "Подтверждение очистки мира", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            World.Renew();
                        }
                        
                        else if (NeedResume) Resume();
                    }

                    break;
            }   
        }

        // protected override void OnMouseDown(MouseButtonEventArgs e)
        // {
        //     MessageBoxResult result = MessageBox.Show(Mouse.GetPosition(img).ToString(), img.RenderSize.ToString());
        //     
        // }
    }
}