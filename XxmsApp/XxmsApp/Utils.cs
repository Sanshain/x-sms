using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using XxmsApp.Piece;

namespace XxmsApp
{


    public static class Utils
    {

        public static void CallAfter(int ms, Action act)
        {
            Device.StartTimer(TimeSpan.FromMilliseconds(ms), () =>
            {
                Device.BeginInvokeOnMainThread(act);

                return false;
            });
        }


        public static Animation Apply(
            this Animation animation, 
            IAnimatable owner, 
            string name,
            uint rate = 16, uint length = 250, 
            Easing easing = null, 
            Action<double, bool> onFinish = null,
            Func<bool> repeat = null)
        {
            animation.Commit(owner, name, rate, length, easing, onFinish, repeat);

            return animation;
        }

        //public static Constraint Let(this double arg) => Constraint.Constant(arg);
        public static Constraint Let(this double? arg) => arg != null ? Constraint.Constant(arg.Value) : null;
        public static Constraint Let(this Func<RelativeLayout, double> func)// => Constraint.RelativeToParent(func);
        {
            return func != null ? Constraint.RelativeToParent(func) : null;
        }
        


        /// <summary>
        /// Append view element to RelativeLayout (more convenient to use). Its recommended
        /// </summary>
        /// <param name="self">RelativeLayout.IRelativeList<View> Children</param>
        /// <param name="view">view for additional to RelativeLayout</param>
        /// <param name="x">x constant</param>
        /// <param name="y">y constant</param>
        /// <param name="W">w - func</param>
        /// <param name="H">h - func</param>
        public static View AddAsRelative(this RelativeLayout.IRelativeList<View> self,
            View view,
            double x = 0,
            double y = 0,
            Func<RelativeLayout, double> W = null, 
            Func<RelativeLayout, double> H = null)
        {
            self.Add(view,
                Constraint.Constant(x),
                Constraint.Constant(x),
                W != null ? Constraint.RelativeToParent(W) : null,
                H != null ? Constraint.RelativeToParent(H) : null);

            return view;
        }



        public static View AddAsRelative(this RelativeLayout.IRelativeList<View> self,
            View view,
            Func<RelativeLayout, double> X = null,
            Func<RelativeLayout, double> Y = null,
            Func<RelativeLayout, double> W = null,
            Func<RelativeLayout, double> H = null)
        {
            self.Add(view, X.Let(), Y.Let(), W.Let(), H.Let());

            return view;
        }



        /// <summary>
        /// Add range for StackLayout
        /// </summary>
        /// <param name="self"></param>
        /// <param name="views"></param>
        public static void AddRange(this IList<View> self, View[] views)
        {
            foreach (var view in views)
            {
                self.Add(view);
            }
        }


        public static void Extend(this IList<View> self, params View[] views)
        {
            foreach (var view in views)
            {
                self.Add(view);
            }
        }

        public static StackLayout AddChilds(this StackLayout self, params View[] views)
        {
            foreach (var view in views)
            {
                self.Children.Add(view);
            }

            return self;
        }

        /// <summary>
        /// Calc string width
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double CalcString(string value = null)
        {
            value = value ?? DateTime.Now.ToString();
            var service = DependencyService.Get<IMeasureString>();
            var timeWidth = service.StringSize(value);

            return timeWidth;
        }

        public static double GetWidth(this string value)
        {
            var service = DependencyService.Get<IMeasureString>();
            var timeWidth = service.StringSize(value);

            return timeWidth;
        }


    }



}

namespace Utilites
{
    public class InvertConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(bool)) return !(bool)value;
            else
                throw new Exception("Unexpect convertation in " + nameof(InvertConverter));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("Unexpect back convertation in " + this.GetType().Name);
        }
    }


    public static class USettings
    {
        public static XxmsApp.Options.IAbstractOption ToOption(this string data)
        {
            return null;
        }
    }
    
    namespace Views
    {
        public static class Views
        {
            public static View SetBindings(this View view, 
                BindableProperty prop, 
                string path, 
                BindingMode mode = BindingMode.Default, 
                IValueConverter converter = null)
            {
                view.SetBinding(prop, path, mode, converter);
                return view;
            }
        }
    }

}
