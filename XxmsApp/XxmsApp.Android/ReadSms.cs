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
using System.Text.RegularExpressions;

[assembly: Dependency(typeof(XxmsApp.Api.Droid.XMessages))]                       // , Dependency(typeof(XxmsApp.Api.Rington))
namespace XxmsApp.Api.Droid
{

    public static class Utils
    {
        public static int ToInt(this OnResult arg) => (int) arg;
    }

    public enum OnResult 
    {
        IsAudio = 0
    }

    class XMessages : IMessages
    {
        static XMessages instance = null;
        public static XMessages Instance
        {
            get => instance ?? (instance = new XMessages());
            set => instance = value;
        }

        public XMessages() : base()
        {
            Instance = this;
        }

        /// <summary>
        /// Для получения информации о том, что смс ушла до получателя
        /// </summary>
        internal static PendingIntent PendInSent { get; set; }
        /// <summary>
        /// Для получения информации о том, что смс дошла до получателя
        /// </summary>
        internal static PendingIntent PendInDelivered { get; set; }

        internal static Android.Media.Ringtone currentSound = null;
        internal static Android.Media.MediaPlayer currentMelody = null;

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

        /// <summary>
        /// Stop current ringtone playing if playing was started by RingtoneManager,
        /// Unless of you need pass OnFinish action for playing stop
        /// </summary>
        public void StopSound()
        {
            currentSound?.Stop();
            currentMelody?.Stop();
        }

        public int SoundPlay(XxmsApp.Sound sound, Action<string> onFinish, Action<Exception> OnError)
        {
            return SoundPlay(
                sound.RingtoneType == typeof(SoundMusic).Name 
                    ? sound.Path 
                    : sound.Name, 
                sound.RingtoneType, 
                onFinish, 
                OnError);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">url или имя русурса для воспроизведения</param>
        /// <param name="soundType">указывается, если в параметр path передается имя</param>
        /// <param name="onFinish">метод, кторый будет вызван по завершении воспроизведения</param>
        /// <returns>вернет продолжительность воспроизведения, если указан soundType, иначе - -1</returns>
        public int SoundPlay(string path, string soundType, Action<string> onFinish, Action<Exception> OnError)
        {
            var context = Android.App.Application.Context;

            if (string.IsNullOrEmpty(soundType) == false)
            {

                string sound = soundType == typeof(SoundMusic).Name 
                    ? path 
                    : $@"/system/media/audio/{soundType.ToLower()}s/{path}.ogg";
                
                StopSound();                                                   // currentMelody?.Stop();
                var player = currentMelody = new Android.Media.MediaPlayer();
                player.Reset();                

                try
                {
                    var soundUri = Android.Net.Uri.Parse(sound);                // player.SetDataSource(sound); - just for asci
                    player.SetDataSource(context, soundUri);                    // player.SetDataSource(context, Urim);
                    player.Prepare();
                    // var duration = player.Duration;

                    player.Start();                    
                    player.Completion += (object sender, EventArgs e) => onFinish?.Invoke(sound);

                    return 1; // duration;   
                }
                catch (Exception ex)
                {
                    var er = ex.Message;
                    OnError?.Invoke(ex);
                    return -2;
                }
                
            }

            if (currentSound != null) currentSound.Stop();
            var uri = Android.Net.Uri.Parse(path);
            currentSound = Android.Media.RingtoneManager.GetRingtone(context, uri);
            currentSound.Play();                

            return -1;

        }

        public void SelectExternalSound(Action<SoundMusic> onselect)
        {
            
            XxmsApp.Droid.MainActivity.Instance.ReceiveActivityResult = (Android.Net.Uri uri, object subj) =>
            {                
                onselect?.Invoke(new SoundMusic(uri.Path, string.Join(":", uri.Scheme, uri.EncodedSchemeSpecificPart)));
            };
            
            Intent audio_picker_intent = new Intent(Intent.ActionGetContent);
            audio_picker_intent.SetType("audio/*");
            XxmsApp.Droid.MainActivity.Instance.StartActivityForResult(audio_picker_intent, OnResult.IsAudio.ToInt());


            /*
            Intent audio_picker_intent = new Intent(
                    Intent.ActionPick,
                    Android.Provider.MediaStore.Audio.Media.ExternalContentUri
                );                   
            XxmsApp.Droid.MainActivity.Instance.StartActivityForResult(audio_picker_intent, REQ_PICK_AUDIO);//*/
        }
        
        public (string, string, string) GetDefaultSound()
        {
            var context = Android.App.Application.Context;
            Android.Media.Ringtone ringtone = null;

            var uri = Android.Media.RingtoneManager.GetDefaultUri(Android.Media.RingtoneType.Notification);
            var path = uri != null ? string.Join(":", uri?.Scheme, uri?.SchemeSpecificPart) : null;            
            if (uri is Android.Net.Uri)
            {
                ringtone = Android.Media.RingtoneManager.GetRingtone(context, uri);
            }

            var title = ringtone?.GetTitle(context);
            if (title != null)
            {
                var m = new Regex(@"\((\w+)\.").Match(title);
                if (m.Groups.Count > 1) title = m.Groups[1].Value;
            }

            return (title, path, "Notification");
        }

        public List<(string, string, string)> GetStockSounds()
        {
                   
            var sounds = new List<(string, string, string)>();

            Android.Media.RingtoneManager ringtoneMgr = null;
            
            Android.Media.RingtoneType[] types =
            {
                Android.Media.RingtoneType.Alarm,
                Android.Media.RingtoneType.Notification,
                Android.Media.RingtoneType.Ringtone
            };

            foreach (var ringtoneType in types)
            {
                // Android.Media.RingtoneType.All

                (ringtoneMgr = new Android.Media.RingtoneManager(XxmsApp.Droid.MainActivity.Instance)).SetType(ringtoneType);                                

                var cursor = ringtoneMgr.Cursor;//*/

                while (!cursor.IsAfterLast && cursor.MoveToNext())
                {
                    var title = cursor.GetString(cursor.GetColumnIndex("title"));                    
                    var _uri = ringtoneMgr.GetRingtoneUri(cursor.Position);
                    var path = string.Join(":", _uri.Scheme, _uri.SchemeSpecificPart);

                    sounds.Add((title, path, ringtoneType.ToString()));
                }

                // cursor.MoveToPosition(0);

            }


            /*
            var names = cursor.GetColumnNames();
            var cc = cursor.ColumnCount;
            var fields = cursor.Extras.KeySet().ToList();
            //*/

            return sounds;

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



            if (Options.ModelSettings.Vibration == true)
            {
                Vibrator vibrator = (Vibrator)context.GetSystemService(Context.VibratorService);
                vibrator.Vibrate(400);            // vibrator.Vibrate(new long[] { 0, 1000, 1000, 1000 }, 0);   
            }

            if (Options.ModelSettings.Sound == true) XMessages.Instance.SoundPlay(Options.ModelSettings.Rington, null, null);

        }

    }


}