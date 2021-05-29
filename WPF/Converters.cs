using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ProjectLife.Core;

namespace ProjectLife.WPF
{
    [ValueConversion(typeof(DrawMode), typeof(string))]
    public class DrawModeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
                switch ((DrawMode) value)
                {
                    case DrawMode.DEFAULT:
                        return WindowLang.CellMode;
                    case DrawMode.ENERGY:
                        return WindowLang.EnergyMode;
                    case DrawMode.MINERALS:
                        return WindowLang.MineralMode;
                    default:
                        throw new Exception($"undeclared draw mode! {value}");
                }

            throw new Exception("Value object cannot be null");
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
            if (value != null)
                switch ((DrawMode) value)
                {
                    case DrawMode.DEFAULT:
                        return new SolidColorBrush(Colors.Green);
                    case DrawMode.ENERGY:
                        return new SolidColorBrush(Colors.Red);
                    case DrawMode.MINERALS:
                        return new SolidColorBrush(Colors.Blue);
                    default:
                        return new SolidColorBrush(Colors.Gray);
                }
            throw new Exception("Value object cannot be null");
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
            if (value != null && (bool) value)
                return WindowLang.PauseState;
            
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
            if (value != null && (bool) value)
                return new SolidColorBrush(Colors.Red);
            
            return new SolidColorBrush(Colors.Green);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}