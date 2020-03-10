using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Xamarin.Forms;
using Android.Content.Res;
using Xamarin.Forms.Platform.Android;

[assembly: Dependency(typeof(XxmsApp.Piece.MeasureString))]
namespace XxmsApp.Piece
{ 

    public class MeasureString : IMeasureString
    {
        public double StringSize(string text)
        {
            var bounds = new Android.Graphics.Rect();
            TextView view = new TextView(Forms.Context);
            view.Paint.GetTextBounds(text, 0, text.Length, bounds);
            var length = bounds.Width();
            return length / Resources.System.DisplayMetrics.ScaledDensity;
        }
    }
}

namespace XxmsApp.Droid
{
    [Activity(Label = "XxmsApp", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);

            var application = new App();

            LoadApplication(application);
        }
    }

}




