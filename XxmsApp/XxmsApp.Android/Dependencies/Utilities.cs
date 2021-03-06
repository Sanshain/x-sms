﻿using System;
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
using XxmsApp.Api.Droid;
using Android.Support.V7.App;
using Android.Provider;
using Xamarin.Essentials;
using Android.Support.V4.App;

[assembly: Dependency(typeof(XxmsApp.Api.LowLevelApi))]
namespace XxmsApp.Api
{
    public class LowLevelApi : ILowLevelApi
    {
        private static Context context = Android.App.Application.Context;
        static LowLevelApi instance;
        public static LowLevelApi Instance => instance ?? new LowLevelApi();

        public LowLevelApi() => instance = instance ?? this;
        

        /// <summary>
        /// Vibrate 150 ms by default
        /// </summary>
        /// <param name="ms"></param>
        public void Vibrate(int ms)
        {
            var context = Android.App.Application.Context;
            Vibrator vibrator = (Vibrator)context.GetSystemService(Context.VibratorService);
            vibrator.Vibrate(150); // ms
        }




        /// <summary>
        /// Exit App
        /// </summary>
        public void AppExit()
        {                                
            XxmsApp.Droid.MainActivity.Instance.FinishAndRemoveTask();                        // завершает приложение полностью
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());                       // подобен GetCurrentProcess().CloseMainWindow() // GetCurrentProcess().Close()

            // (Xamarin.Forms.Forms.Context as Activity).Finish();                             // устаревший метод
            // XxmsApp.Droid.MainActivity.Instance.FinishAffinity();                           // по факту как будто сворачивает просто
        }



        // [Obsolete("for crossplatform, but not not finished")]
        public void ShowNotification(string title, string content)
        {
            var context = Android.App.Application.Context;

            Intent notificationIntent = new Intent(Android.App.Application.Context, typeof(XxmsApp.Droid.MainActivity));

            NotificationCompat.Builder builder = new NotificationCompat.Builder(context);
            builder.SetSmallIcon(Android.Resource.Drawable.IcDialogInfo)                   // icon
            .SetContentTitle(title)
            .SetContentText(content)                                               // content

            .SetContentIntent(PendingIntent.GetActivity(context, 0, notificationIntent, 0)) // where to pass view by clicked                                
            .SetAutoCancel(true)                                                    // автоотключение уведомления при переходе на активити

            .SetPriority(NotificationCompat.PriorityMax);
            // .SetDefaults((int)NotificationPriority.High);


            Notification notification = builder.Build();
            notification.Defaults = NotificationDefaults.All;
            ((NotificationManager)context.GetSystemService(Context.NotificationService)).Notify("Новое сообщение", 2, notification);



            if (Options.ModelSettings.Vibration == true)
            {
                Vibrator vibrator = (Vibrator)context.GetSystemService(Context.VibratorService);
                vibrator.Vibrate(400);            // vibrator.Vibrate(new long[] { 0, 1000, 1000, 1000 }, 0);   
            }

            if (Options.ModelSettings.Sound == true)
            {
                new XxmsApp.Droid.Dependencies.Player().SoundPlay(Options.ModelSettings.Rington, null, null);
            }

        }



        public bool IsDefault => Telephony.Sms.GetDefaultSmsPackage(context).Equals(context.PackageName);

        static bool isEssentialInit = false;
        public bool IsEsentialInit
        {
            get
            {
                if (isEssentialInit) return true;
                else
                {
                    // add this line to your code, it may also be called: bundle
                    Xamarin.Essentials.Platform.Init(
                        XxmsApp.Droid.MainActivity.Instance, 
                        XxmsApp.Droid.MainActivity.Instance.initBundle);

                    isEssentialInit = true;

                    return true;
                }

            }
        }

        public string Model => Android.OS.Build.Model + $" ({Android.OS.Build.Manufacturer.ToUpper()})";


        public void ChangeDefault()
        {
            
            Intent intent = new Intent(Telephony.Sms.Intents.ActionChangeDefault);
            intent.PutExtra(Telephony.Sms.Intents.ExtraPackageName, context.PackageName);

            XxmsApp.Droid.MainActivity.Instance.StartActivityForResult(intent, (int)OnResult.SetDefaultApp);

        }








        [Obsolete]
        public void Play()
        {
            var uri = Android.Media.RingtoneManager.GetDefaultUri(Android.Media.RingtoneType.Notification);
            var r = Android.Media.RingtoneManager.GetRingtone(context, uri);
            r.Play();
        }//*/


        [Obsolete("just to test, not release")]
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


        public void ShowToast(string text)
        {
            var toast = Toast.MakeText(context, text, ToastLength.Long);
            toast.SetGravity(GravityFlags.Center, 0, 100);
            toast.Show();
        }

        public void Copy(string text)
        {
            ClipboardManager clipboard = (ClipboardManager)context.GetSystemService(Context.ClipboardService);
            // clipboard.Text = text;

            ClipData clip = ClipData.NewPlainText("Copied Text", text);            
            clipboard.PrimaryClip = clip;
            
        }
    }

}