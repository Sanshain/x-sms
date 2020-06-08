using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace XxmsApp
{
    public static class ModelUtils
    {
        public static bool ToBoolean(this string value)
        {
            if (bool.TryParse(value, out bool result))
            {
                return result;
            }
            else return false;
        }
    }


    public class UniversalConverter : Xamarin.Forms.IValueConverter
    {
        Func<object, object> func = null;

        public UniversalConverter(Func<object, object> act) => func = act;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => func?.Invoke(value);        

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(this.GetType().Name + " is for just OneWay");
        }
    }
}
