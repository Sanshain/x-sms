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
        }//*/


        public static void RingtonPlay()
        {

            var context = Android.App.Application.Context;
            const int REQ_PICK_AUDIO = 0;

            // var action = Android.Media.RingtoneManager.ActionRingtonePicker;


            /*      
            Intent audio_picker_intent = new Intent(
                    Intent.ActionPick,
                    Android.Provider.MediaStore.Audio.Media.InternalContentUri
                );                   
            XxmsApp.Droid.MainActivity.Instance.StartActivityForResult(audio_picker_intent, REQ_PICK_AUDIO);//*/


            /*
            Intent audio_picker_intent = new Intent(Android.Provider.Settings.ActionSoundSettings);
            audio_picker_intent.SetFlags(ActivityFlags.NewTask);
            audio_picker_intent.SetFlags(ActivityFlags.NoHistory);
            audio_picker_intent.SetFlags(ActivityFlags.ExcludeFromRecents);
            audio_picker_intent.SetFlags(ActivityFlags.MultipleTask);
            XxmsApp.Droid.MainActivity.Instance.StartActivityForResult(
                Intent.CreateChooser(audio_picker_intent, "выбор есть" ),
                REQ_PICK_AUDIO);//*/



            try
            {
                var Uri = Android.Media.RingtoneManager.GetActualDefaultRingtoneUri(context, Android.Media.RingtoneType.Notification);
                var uri = Android.Media.RingtoneManager.GetActualDefaultRingtoneUri(context, Android.Media.RingtoneType.Ringtone);
                var auri = Android.Media.RingtoneManager.GetActualDefaultRingtoneUri(context, Android.Media.RingtoneType.Alarm);
                var r = Android.Media.RingtoneManager.GetRingtone(context, Uri ?? uri ?? auri);
                r.Play();
            }
            catch (Exception ex)
            {
                var er = ex.Message;
            }
            //*/
        }

    }

}