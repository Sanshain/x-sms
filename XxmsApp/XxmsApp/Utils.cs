using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace XxmsApp
{
    public static class Utils
    {



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

        /*
        public static void Add(this RelativeLayout.IRelativeList<View> self,
            View view,
            double x,
            double y,
            (Func<RelativeLayout, double> W, Func<RelativeLayout, double> H) size)
        {
            self.Add(view,
                Constraint.Constant(x),
                Constraint.Constant(x),
                Constraint.RelativeToParent(size.W),
                Constraint.RelativeToParent(size.H));
        }

        public static void Add(this RelativeLayout.IRelativeList<View> self, 
            View view, 
            Point loc, 
            (Func<RelativeLayout, double> W, Func<RelativeLayout, double> H) size)
        {
            self.Add(view, loc.X, loc.Y, size);
        }//*/

    }
}
