using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Database;
using Android.OS;
using Android.Runtime;

using Android.Telephony;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using XxmsApp.Model;

using Android.Support.V7.App;

[assembly: Dependency(typeof(XxmsApp.Api.Droid.XMessages))]
namespace XxmsApp.Api.Droid
{

    class XMessages : IMessages
    {

        /// <summary>
        /// Для получения информации о том, что смс ушла до получателя
        /// </summary>
        internal static PendingIntent PendInSent { get; set; }
        /// <summary>
        /// Для получения информации о том, что смс дошла до получателя
        /// </summary>
        internal static PendingIntent PendInDelivered { get; set; }


        ContentResolver contentResolver;

        public List<XxmsApp.Model.Message> Read()
        {

            contentResolver = XxmsApp.Droid.MainActivity.InstanceResolver;

            // List<string> xms = new List<string>();

            ICursor qs = contentResolver.Query(Android.Net.Uri.Parse("content://sms"), null, null, null, null);
            // ICursor qs = contentResolver.Query(Android.Net.Uri.Parse("content://sms/inbox"), null, null, null, null); //on "ORDER BY _id DESC" - timeout exception error

            List<XxmsApp.Model.Message> messages = new List<Model.Message>();

            while (qs.MoveToNext())
            {
                var income = qs.GetShort(qs.GetColumnIndex("type"));

                XxmsApp.Model.Message msg = new Model.Message
                {
                    Id = qs.GetInt(qs.GetColumnIndex("_id")),
                    // Time = DateTime.FromBinary(qs.GetLong(qs.GetColumnIndex("date"))),
                    Address = qs.GetString(qs.GetColumnIndex("address")),
                    Value = qs.GetString(qs.GetColumnIndex("body")),
                    Incoming = income == 1 ? true : false
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





        [Obsolete]
        public void ShowNotification(string title, string content)
        {
            var context = Android.App.Application.Context;

            Intent notificationIntent = new Intent(Android.App.Application.Context, typeof(XxmsApp.Droid.MainActivity));

            var sUri = Android.Media.RingtoneManager.GetDefaultUri(Android.Media.RingtoneType.Notification);

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


            Vibrator vibrator = (Vibrator)context.GetSystemService(Context.VibratorService);
            vibrator.Vibrate(400);
            // vibrator.Vibrate(new long[] { 0, 1000, 1000, 1000 }, 0);            

        }



    }

}