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


[assembly: Dependency(typeof(XxmsApp.Api.ILowLevelApi))]
namespace XxmsApp.Api.Utilites
{
    public class LowLevelApi : ILowLevelApi
    {
        private static Context context = Android.App.Application.Context;     
        

        public void Vibrate(int ms)
        {
            var context = Android.App.Application.Context;
            Vibrator vibrator = (Vibrator)context.GetSystemService(Context.VibratorService);
            vibrator.Vibrate(150);
        }

        public void Play()
        {                     
            var uri = Android.Media.RingtoneManager.GetDefaultUri(Android.Media.RingtoneType.Notification);
            var r = Android.Media.RingtoneManager.GetRingtone(context, uri);
            r.Play();
        }

    }

}