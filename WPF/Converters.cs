using ProjectLife_v_0_3.ProjectLife;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ProjectLife_v_0_3.WPF
{
    [ValueConversion(typeof(DrawMode), typeof(string))]
    public class DrawModeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((DrawMode)value)
            {
                case DrawMode.DEFAULT:
                    return WindowLang.CellMode;
                case DrawMode.ENERGY:
                    return WindowLang.EnergyMode;
                case DrawMode.MINERALS:
                    return WindowLang.MineralMode;
                default:
                    // добавить строку в WindowLang?
                    // другое значение этой строки?
                    return "Не устаовлено";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    [ValueConversion(typeof(DrawMode), typeof(SolidColorBrush))]
    public class DrawModeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((DrawMode)value)
            {
                case DrawMode.DEFAULT:
                    return new SolidColorBrush(Colors.Green);
                case DrawMode.ENERGY:
                    return new SolidColorBrush(Colors.Red);
                case DrawMode.MINERALS:
                    return new SolidColorBrush(Colors.Blue);
                default:
                    // другой цвет?
                    return new SolidColorBrush(Colors.Gray);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    [ValueConversion(typeof(DrawMode), typeof(string))]
    public class StateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return WindowLang.PauseState;
            else
                return WindowLang.SimulatingState;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    [ValueConversion(typeof(DrawMode), typeof(SolidColorBrush))]
    public class StateToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return new SolidColorBrush(Colors.Red);
            else
                return new SolidColorBrush(Colors.Green);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
