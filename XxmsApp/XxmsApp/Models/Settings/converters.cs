using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace XxmsApp.Options
{
    public class BoolConverter<T> : IValueConverter
    {

        Func<bool, Type, T> action;
        public BoolConverter(Func<bool, Type, T> act) { action = act; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return action(b, targetType);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class ContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(bool))             // видимость
            {
                // if (parameter is View view) { return !view.IsVisible; } // Error unexpected
                return bool.TryParse(value.ToString(), out bool result) ? result : false;
            }
            else if (value is bool && targetType == typeof(double))       // размер шрифта
            {
                if (System.Convert.ToBoolean(value) == false)
                {
                    return Device.GetNamedSize(NamedSize.Medium, typeof(Label));
                }
                else return Device.GetNamedSize(NamedSize.Small, typeof(Label));
            }
            else
                throw new Exception("Unexpect convertation in " + this.GetType().Name);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(string)) return value.ToString();
            else
                throw new Exception("Unexpect back convertation in " + this.GetType().Name);
        }
    }

}
