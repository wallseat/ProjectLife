using System.Windows;

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
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}