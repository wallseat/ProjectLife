using System.Windows;
using System.Windows.Controls;

namespace ProjectLife_v_0_3.WPF
{
    public static class WindowLang
    {
        public const string CellMode = "Клетка";
        public const string EnergyMode = "Энергия";
        public const string MineralMode = "Минералы";
        public const string PauseState = "Пауза";
        public const string SimulatingState = "Идёт симуляция";
        public const string ClearConfirmationInfo = "Вы уверены что хотите начать с начала?";
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}