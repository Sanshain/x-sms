using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Telephony;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using XxmsApp.Model;

[assembly: Dependency(typeof(XxmsApp.Api.Droid.XMessages))]
namespace XxmsApp.Api.Droid
{
    
    class XMessages : IMessages
    {

        internal static PendingIntent PendInSent { get; set; }
        internal static PendingIntent PendInDelivered { get; set; }

        ContentResolver contentResolver;

        public List<XxmsApp.Model.Message> Read()
        {

            contentResolver = XxmsApp.Droid.MainActivity.InstanceResolver;

            // List<string> xms = new List<string>();

            ICursor qs = contentResolver.Query(
                Android.Net.Uri.Parse("content://sms/inbox"), null, null, null, null);  
            //on "ORDER BY _id DESC" - timeout exception error

            List<XxmsApp.Model.Message> messages = new List<Model.Message>();

            while (qs.MoveToNext())
            {
                XxmsApp.Model.Message msg = new Model.Message
                {
                    Id = qs.GetInt(qs.GetColumnIndex("_id")),
                    // Time = DateTime.FromBinary(qs.GetLong(qs.GetColumnIndex("date"))),
                    Address = qs.GetString(qs.GetColumnIndex("address")),
                    Value = qs.GetString(qs.GetColumnIndex("body"))
                };

                var time = qs.GetLong(qs.GetColumnIndex("date"));
                msg.Time = new DateTime(1970, 1, 1).AddMilliseconds(time);

                messages.Add(msg);
            }

            /*
            if (qs.MoveToFirst())
            {
                for (int idx = 0; idx < qs.ColumnCount; idx++)
                {
                    // xms.Add(qs.GetColumnName(idx) + ":" + qs.GetString(idx));
                    s += "&" + qs.GetColumnName(idx) + ":" + qs.GetString(idx);
                }
            }
            else
            {
                // empty box (no SMS)
            }//*/

            return messages;
        }

        public bool Send(string adressee, string content)
        {

            SmsManager.Default.SendTextMessage(adressee, null, content, PendInSent, PendInDelivered);

            return true;
        }





        public void Send(XxmsApp.Model.Message msg)
        {
            SmsManager.Default.SendTextMessage(msg.Address, null, msg.Value, PendInSent, PendInDelivered);
        }



        public void ShowNotification()
        {
            var context = Android.App.Application.Context;

            Intent notificationIntent = new Intent(Android.App.Application.Context, typeof(XxmsApp.Droid.MainActivity));

            const string CATEGORY_MESSAGE = "msg"; // developer.android.com/reference/android/app/Notification?hl=ru#CATEGORY_MESSAGE
            
            
            NotificationCompat.Builder builder = new NotificationCompat.Builder(context)
                .SetSmallIcon(Android.Resource.Drawable.IcDialogInfo)                   // icon
               
                .SetColor(0x93C54B)                                                     // text color
                .SetLights(Android.Graphics.Color.ParseColor("#ccffff"), 5000, 5000)    // установка освещения // 0xff0000ff

                .SetVibrate(new long[200])                                              // vibration
                .SetContentInfo("Info")                                                 // 
                .SetOngoing(true)                                                       // first place and not clear? == 50/50
                
                .SetCategory(CATEGORY_MESSAGE)                                          // category of notify
                .SetAutoCancel(false)                                                   // ?
                .SetTicker("notification_ticker_text")                                  // ? my by this can be used for top view
                .SetContentText("Content")                                              // content
                .SetContentIntent(PendingIntent.GetActivity(context, 0, notificationIntent, 0)) // where to pass view by clicked
                
                .SetFullScreenIntent(PendingIntent.GetActivity(context, 0, notificationIntent, 0), true)
                .SetSubText("subtext")
                .SetSound(Android.Net.Uri.Parse(""))
                
                .SetGroupSummary(true)
                .SetGroup("messages")
                // .SetVisibility()
                
                .SetUsesChronometer(true)
                .SetContentTitle("Title")
                .SetVisibility((int)ViewAttr.IsVisible)
                .SetDefaults((int)NotificationPriority.Max);                            // priority


                // .SetExtras()                                                         // as I understand for saving data for next notification
                // .SetDeleteIntent()                                                   // prohibition for clearing
                // .SetSound(Android.Net.Uri.Parse(""))                                 // sound
                // .SetCustomHeadsUpContentView()                                       // probably for ovverding head of notify as sms by Xiaomi
                // .SetCustomContentView()                                              // too difficult to use
                // .SetCustomBigContentView()                                           // too difficult to use
                // .SetSmallIcon(XxmsApp.Droid.Resource.Drawable.icon)
                // .SetWhen(DateTime.Now.Millisecond);                                  // set time for appearance
                // .setOnlyAlertOnce                                                    // or see or vibro/sound
                // .SetProgress()                                                       // в виде прогрессбара
                // .SetPublicVersion()                                                  // поверх экрана блокировки
                // .setVisibility                                                       // ??
                // .limitCharSequenceLength                                             // ограничение на количество символов

                // examples:

                // .SetCustomContentView(context.PackageName, XxmsApp.Droid.Resource.Layout.notification_action)
                // .SetLargeIcon(Android.Graphics.BitmapFactory.DecodeResource(XxmsApp.Droid.MainActivity.Instance.Resources, Android.Resource.Drawable.IcDialogInfo))


            Notification notification = builder.Build();
            ((NotificationManager)context.GetSystemService(Context.NotificationService)).Notify(0, notification);
        }

    }

    internal enum ViewAttr : int
    {
        IsVisible = 0x00000000,
        Invisible = 0x00000004,
        Gone = 0x00000008,
        VisibilityMask = 0x0000000C
    }

}