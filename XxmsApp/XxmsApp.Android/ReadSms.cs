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
using Java.IO;
using Android.Net;

[assembly: Dependency(typeof(XxmsApp.Api.Droid.XMessages))]                       // , Dependency(typeof(XxmsApp.Api.IRington))
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

            ICursor qs = contentResolver.Query(Android.Net.Uri.Parse("content://sms"), null, null, null, null);            

            List<XxmsApp.Model.Message> messages = new List<Model.Message>();

            while (qs.MoveToNext())
            {
                var income = qs.GetShort(qs.GetColumnIndex("type"));                

                XxmsApp.Model.Message msg = new Model.Message
                {
                    Id = qs.GetInt(qs.GetColumnIndex("_id")),
                    Address = qs.GetString(qs.GetColumnIndex("address")),
                    Value = qs.GetString(qs.GetColumnIndex("body")),
                    Incoming = income == 1 ? true : false,
                    IsRead = Convert.ToBoolean(qs.GetShort(qs.GetColumnIndex("read"))),     // 1 - прочитано, 0 - не прочитано
                    Delivered = !Convert.ToBoolean(qs.GetShort(qs.GetColumnIndex("status"))),
                    ErrorCode = qs.GetInt(qs.GetColumnIndex("error_code"))

                    // Protocol = qs.GetString(qs.GetColumnIndex("protocol"))               // 0 - входящее, null - исходящее
                };

                msg.SetSim(qs.GetString(qs.GetColumnIndex("sim_id")));                      // SubscriptionId

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

        /// <summary>
        /// Send sms
        /// </summary>
        /// <param name="adressee"></param>
        /// <param name="content"></param>
        /// <param name="simId"></param>
        /// <returns></returns>
        public bool Send(string adressee, string content, int? simId = null)
        {

            SmsManager sim = null;

            if (simId.HasValue)
            {
                sim = SmsManager.GetSmsManagerForSubscriptionId(simId.Value);                
            }
            else sim = SmsManager.Default;

            sim?.SendTextMessage(adressee, null, content, PendInSent, PendInDelivered);            

            return sim != null;
        }

        public void Send(XxmsApp.Model.Message msg)
        {
            var sim = int.TryParse(msg.SimOsId, out int subId) ? SmsManager.GetSmsManagerForSubscriptionId(subId) : SmsManager.Default;

            sim.SendTextMessage(msg.Address, null, msg.Value, PendInSent, PendInDelivered);

        }

        public IEnumerable<Sim> GetSimsInfo()
        {
            var ls = new List<Sim>();

            var context = Android.App.Application.Context;

            SubscriptionManager localSubscriptionManager = SubscriptionManager.From(context);

            foreach (var simInfo in localSubscriptionManager.ActiveSubscriptionInfoList)
            {
                int slot = simInfo.SimSlotIndex;             // 1
                int id = simInfo.SubscriptionId;             // 6                
                string sim1 = simInfo.DisplayName;           // ~ MegaFon #1
                string IccId = simInfo.IccId;                // 897010287534043278ff
                Color backColor = Color.FromRgba(            // 
                    simInfo.IconTint.R, 
                    simInfo.IconTint.G, 
                    simInfo.IconTint.B, 
                    simInfo.IconTint.A);

                yield return new Sim(slot, id, sim1, IccId, backColor);
            }

        }


        public void Vibrate(int ms)
        {
            var context = Android.App.Application.Context;
            Vibrator vibrator = (Vibrator)context.GetSystemService(Context.VibratorService);
            vibrator.Vibrate(ms);            
        }

        public void Play()
        {

            var context = Android.App.Application.Context;

            /*
            var ringtoneMgr = new Android.Media.RingtoneManager(context);
            var connecttion = ringtoneMgr.Cursor;//*/

            const int REQ_PICK_AUDIO = 0;

            // var action = Android.Media.RingtoneManager.ActionRingtonePicker;


            /*      
            Intent audio_picker_intent = new Intent(
                    Intent.ActionPick,
                    Android.Provider.MediaStore.Audio.Media.InternalContentUri
                );                   
            XxmsApp.Droid.MainActivity.Instance.StartActivityForResult(audio_picker_intent, REQ_PICK_AUDIO);//*/

            
            var settingsContentObserver = new SettingsContentObserver(new Handler());

            context.ContentResolver.RegisterContentObserver(
                Android.Provider.Settings.System.ContentUri, 
                true,
                settingsContentObserver);


            
            Intent audio_picker_intent = new Intent(Android.Provider.Settings.ActionSoundSettings);
            audio_picker_intent.SetFlags(ActivityFlags.NewTask);
            audio_picker_intent.SetFlags(ActivityFlags.NoHistory);
            audio_picker_intent.SetFlags(ActivityFlags.ExcludeFromRecents);
            audio_picker_intent.SetFlags(ActivityFlags.MultipleTask);
            XxmsApp.Droid.MainActivity.Instance.StartActivityForResult(
                Intent.CreateChooser(audio_picker_intent, "выбор есть" ),
                REQ_PICK_AUDIO);//*/

            /*
            try
            {
                var Uri = Android.Media.RingtoneManager.GetActualDefaultRingtoneUri(context, Android.Media.RingtoneType.Notification);
                var uri = Android.Media.RingtoneManager.GetActualDefaultRingtoneUri(context, Android.Media.RingtoneType.Ringtone);
                var auri = Android.Media.RingtoneManager.GetActualDefaultRingtoneUri(context, Android.Media.RingtoneType.Alarm);
                var r = Android.Media.RingtoneManager.GetRingtone(context, auri ?? uri);
                r.Play();
            }
            catch(Exception ex)
            {
                var er = ex.Message;
            }
            //*/
        }




        [Obsolete("for crossplatform, but not not finished")]
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


            Vibrator vibrator = (Vibrator)context.GetSystemService(Context.VibratorService);
            vibrator.Vibrate(400);
            // vibrator.Vibrate(new long[] { 0, 1000, 1000, 1000 }, 0);        

        }

    }

    
    public class SettingsContentObserver : ContentObserver
    {

        public SettingsContentObserver(Handler handler) : base(handler) { }

        public override bool DeliverSelfNotifications()
        {
            return base.DeliverSelfNotifications();            
        }
    
        public override void OnChange(bool selfChange)
        {
            base.OnChange(selfChange); 
            
        }
    }//*/

}