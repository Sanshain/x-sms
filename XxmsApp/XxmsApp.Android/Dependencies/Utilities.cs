using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using XxmsApp.Api;
using Xamarin.Forms;


[assembly: Dependency(typeof(XxmsApp.Api.Utilites.IUtilites))]
namespace XxmsApp.Api.Utilites
{
    public class Utilites : IUtilites
    {
        public Utilites() : base() { }

        public void Vibrate(int ms)
        {
            var context = Android.App.Application.Context;
            Vibrator vibrator = (Vibrator)context.GetSystemService(Context.VibratorService);
            vibrator.Vibrate(ms);
        }
    }

}