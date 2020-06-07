using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Telephony;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using XxmsApp.Model;
using XxmsApp.Api;
using Android.Support.V4.App;
using static Android.Manifest;
using Android.Database;
using System.IO;

namespace XxmsApp.Api
{
    // [Obsolete("using nameof(Debud) instead")]
    public static class CompileConstant
    {
        public const string Debug = "DEBUG";
    }

    public enum Brand
    {
        Unknown,
        Default,
        Xiaomi
    }

    // [BroadcastReceiver(Enabled = true, Exported = true)]
    //               // "android.provider.Telephony.SMS_RECEIVED"
    // [IntentFilter(new string[] { Telephony.Sms.Intents.SmsDeliverAction })]

    [BroadcastReceiver(Enabled = true, Label = "SMS Receiver", Permission = Permission.BroadcastSms)]
    [IntentFilter(new string [] { Telephony.Sms.Intents.SmsReceivedAction, Telephony.Sms.Intents.SmsDeliverAction }, Priority = 1000)] 
    public class MessageReceiver : BroadcastReceiver
    {

        public static Brand deviceFirmware = Brand.Unknown;
        public static Brand DeviceFirmware
        {
            get
            {
                if (deviceFirmware != Brand.Unknown) return deviceFirmware;
                else if ( Api.LowLevelApi.Instance.Model.ToLower().Contains("xiaomi"))
                {
                    return deviceFirmware = Brand.Xiaomi;
                }
                else return deviceFirmware = Brand.Default;
            }
        }
        
        static Context primaryContext = null;

        public override void OnReceive(Context context, Intent intent)
        {

            bool onStart = false;
            try
            {
                Device.BeginInvokeOnMainThread(() => Toast.MakeText(context, "New one message", ToastLength.Long).Show());
            }
            catch (Exception ex)
            {
                var mess = ex.Message;

                Cache.database.Insert(new XxmsApp.Model.Errors
                {
                    Name = mess,
                    Method = nameof(OnReceive),
                    Params = ex.StackTrace
                });

                if (intent.Action != Telephony.Sms.Intents.SmsReceivedAction)                
                    Toast.MakeText(context, "New one message", ToastLength.Long).Show();                                

                onStart = true;
                primaryContext = context;
            }


            if (intent.Action != Telephony.Sms.Intents.SmsReceivedAction) return;           // and Telephony.Sms.Intents.SmsDeliverAction ?? for delevered?            

            var bundle = intent.Extras;
            // var slot = bundle.GetInt("slot", -1);
            var sub = bundle.GetInt("subscription", -1);
            var columns = bundle.KeySet().ToArray();

            SmsMessage[] messages = Telephony.Sms.Intents.GetMessagesFromIntent(intent);

            try
            {
                OnMessagesReiceved(messages, sub, onStart);
            }
            catch(Exception ex)
            {
                Cache.database.Insert(new XxmsApp.Model.Errors
                {
                    Name = ex.Message,
                    Method = nameof(OnMessagesReiceved),
                    Params = ex.StackTrace
                });
            }


        }

        private static void StartMainActivity(Context context, List<XxmsApp.Model.Message> messages)
        {
            var appIntent = new Intent(primaryContext, typeof(XxmsApp.Droid.SplashActivity));
            
            appIntent.PutExtra("messages", Serialize(messages));
            context.StartActivity(appIntent);                           // typeof(XxmsApp.Droid.SplashActivity)
        }

        private static string Serialize(List<Model.Message> messages)
        {
            System.Xml.Serialization.XmlSerializer xml = new System.Xml.Serialization.XmlSerializer(messages.GetType());
            

            using (StringWriter textWriter = new StringWriter())
            {
                xml.Serialize(textWriter, messages);
                var objs = textWriter.ToString();
                return objs;
            }
        }

        public static List<Model.Message> Deserialize(string messages)
        {
            
            System.Xml.Serialization.XmlSerializer xml = new System.Xml.Serialization.XmlSerializer(typeof(List<Model.Message>));

            using (StringReader sr = new StringReader(messages))
            {
                var r = xml.Deserialize(sr) as List<Model.Message>;
                return r;
            }
        }



        private void OnMessagesReiceved(SmsMessage[] messages, int sim, bool onStart = false)
        {
            var _sim = Api.Droid.XMessages.Instance.Sims.FirstOrDefault(s => s.SubId == sim);

            var smsMesages = new List<(string Address, string Message)>();
            var XMessages = new List<XxmsApp.Model.Message>();

            var isDefault = LowLevelApi.Instance.IsDefault;

            for (var i = 0; i < messages.Length; i++)
            {
                smsMesages.Add((messages[i].OriginatingAddress, messages[i].MessageBody));

                var message = XMessages.FirstOrDefault(m => m.Address == messages[i].OriginatingAddress);

                if (message != null) message.Value = message.Value + messages[i].MessageBody;                
                else
                    XMessages.Add(new XxmsApp.Model.Message
                    {                        
                        SimOsId = sim.ToString(),
                        SimIccID = _sim.IccId,
                        Address = messages[i].OriginatingAddress,
                        Value = messages[i].MessageBody,
                        TimeOut = new DateTime(1970, 1, 1).AddMilliseconds(messages[i].TimestampMillis),
                        Time = DateTime.Now,
                        Service = messages[i].ServiceCenterAddress,
                        Incoming = true,
                        IsRead = false,
                        ErrorCode = 0,                        

                    });

                /*
                var icc = messages[i].IndexOnIcc;
                var sims = XxmsApp.Api.Droid.XMessages.Instance.GetSimsInfo().ToArray();
                var pdu = messages[i].GetPdu();//*/

                // if (isDefault) Save(messages[i], sim);

            }
            
            if (isDefault) XMessages.ForEach(m => SaveMsg(m));

            Bundle bundle = new Bundle();
            bundle.PutString(typeof(Model.Message).Name, Serialize(XMessages));

            ShowNotification(
                "Новое сообщение от " + XMessages.First().Address,
                XMessages.First().Value,
                bundle);

            if (onStart)
            {
                // StartMainActivity(primaryContext, XMessages);
                return;
            }
            else Device.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Send<XxmsApp.App, List<XxmsApp.Model.Message>>(
                    Xamarin.Forms.Application.Current as XxmsApp.App, 
                    "MessageReceived",
                    XMessages);
            });


        }


        public void ShowNotification(string title, string content, Bundle bundle)
        {
            var context = Android.App.Application.Context;
            Intent notificationIntent = new Intent(Android.App.Application.Context, typeof(XxmsApp.Droid.MainActivity));

            if (bundle != null)
            {
                notificationIntent.PutExtras(bundle);                
            }

            NotificationCompat.Builder builder = new NotificationCompat.Builder(context)
                .SetSmallIcon(Android.Resource.Drawable.IcDialogInfo)
                .SetContentTitle(title)
                .SetContentText(content)

                .SetContentIntent(PendingIntent.GetActivity(context, 0, notificationIntent, PendingIntentFlags.UpdateCurrent)) // 0
                .SetAutoCancel(true)

                .SetDefaults((int)NotificationPriority.High);


            Notification notification = builder.Build();            
            if (Options.ModelSettings.Sound == true)
            {
                notification.Sound = Android.Net.Uri.Parse(Options.ModelSettings.Rington.Path);                
            }
            ((NotificationManager)context.GetSystemService(Context.NotificationService)).Notify(0, notification);


            if (Options.ModelSettings.Vibration == true)
            {
                Vibrator vibrator = (Vibrator)context.GetSystemService(Context.VibratorService);
                vibrator.Vibrate(400);            // vibrator.Vibrate(new long[] { 0, 1000, 1000, 1000 }, 0); 

            }
        }

        [Obsolete]
        private void Save(SmsMessage smsMessage, int sim)
        {
            var uri = Telephony.Sms.ContentUri;
            var draftUri = Telephony.Sms.Draft.ContentUri;
            var inboxUri = Telephony.Sms.Inbox.ContentUri;
            var outboxUri = Telephony.Sms.Outbox.ContentUri;
            var sentUri = Telephony.Sms.Sent.ContentUri;

            var contentResolver = XxmsApp.Droid.MainActivity.InstanceResolver ?? primaryContext.ContentResolver; 

            var values = FillValues(smsMessage, sim);

            var qs = contentResolver.Insert(Telephony.Sms.Inbox.ContentUri, values); // Android.Net.Uri.Parse("content://sms/inbox")
        }




        private void SaveMsg(Model.Message smsMessage)
        {
            var uri = Telephony.Sms.ContentUri;
            var draftUri = Telephony.Sms.Draft.ContentUri;
            var inboxUri = Telephony.Sms.Inbox.ContentUri;
            var outboxUri = Telephony.Sms.Outbox.ContentUri;
            var sentUri = Telephony.Sms.Sent.ContentUri;

            var contentResolver = XxmsApp.Droid.MainActivity.InstanceResolver ?? primaryContext.ContentResolver;            

            var values = FillValues(smsMessage);

            var qs = contentResolver.Insert(Telephony.Sms.Inbox.ContentUri, values); // Android.Net.Uri.Parse("content://sms/inbox")
        }

        private ContentValues FillValues(Model.Message smsMessage)
        {            

            ContentValues values = new ContentValues();

            var now = (DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;
            var outNow = (smsMessage.TimeOut - new DateTime(1970, 1, 1)).TotalMilliseconds;


            values.Put(Telephony.Sms.InterfaceConsts.ThreadId, GetThreadId(smsMessage.Address));         // chat id
            values.Put(Telephony.Sms.InterfaceConsts.Address, smsMessage.Address);                        // no read
            values.Put(Telephony.Sms.InterfaceConsts.Person, "");                                         // Person
            values.Put(Telephony.Sms.InterfaceConsts.Date, now.ToString());                               // now
            values.Put(Telephony.Sms.InterfaceConsts.DateSent, outNow.ToString());                        // sent
            values.Put(Telephony.Sms.InterfaceConsts.Protocol, 0);                                        // in/out
            values.Put(Telephony.Sms.InterfaceConsts.Status, -1);                                         // always -1 for inbox
            values.Put(Telephony.Sms.InterfaceConsts.Type, 1);                                            // always 1 for inbox

            values.Put(Telephony.Sms.InterfaceConsts.ReplyPathPresent, 0);                                // I still don't know 
            values.Put(Telephony.Sms.InterfaceConsts.Subject, "");                                        // I still don't know 

            values.Put(Telephony.Sms.InterfaceConsts.Body, smsMessage.Value);                              // 
            values.Put(Telephony.Sms.InterfaceConsts.ServiceCenter, smsMessage.Service);                   // 

            values.Put(Telephony.Sms.InterfaceConsts.Locked, 0);                                          // spam/no spam?
            values.Put(Telephony.Sms.InterfaceConsts.ErrorCode, 0);
            values.Put(Telephony.Sms.InterfaceConsts.Seen, 1);

            if (DeviceFirmware == Brand.Xiaomi)
                values.Put("sim_id", smsMessage.SimOsId);                                                  // № sim
            else            
                values.Put(Telephony.Sms.InterfaceConsts.SubscriptionId, smsMessage.SimOsId);              // № sim
            

            return values;
        }




        public void ShowNotification(string title, string content)
        {
            ShowNotification(title, content, null);
        }


        [Obsolete]
        private ContentValues FillValues(SmsMessage smsMessage, int sim)
        {
            var number = smsMessage.OriginatingAddress;

            ContentValues values = new ContentValues();

            // values.Put(Telephony.Sms.InterfaceConsts.Id, "");


            var now = (DateTime.Now - new DateTime(1970, 1, 1)).TotalMilliseconds;

            values.Put(Telephony.Sms.InterfaceConsts.ThreadId, GetThreadId(number));                      // chat id
            values.Put(Telephony.Sms.InterfaceConsts.Address, number);                                    // no read
            values.Put(Telephony.Sms.InterfaceConsts.Person, "");                                         // Person
            values.Put(Telephony.Sms.InterfaceConsts.Date, now.ToString());                               // now
            values.Put(Telephony.Sms.InterfaceConsts.DateSent, smsMessage.TimestampMillis);               // sent
            values.Put(Telephony.Sms.InterfaceConsts.Protocol, 0);                                        // in/out
            values.Put(Telephony.Sms.InterfaceConsts.Status, -1);                                         // always -1 for inbox
            values.Put(Telephony.Sms.InterfaceConsts.Type, 1);                                            // always 1 for inbox

            values.Put(Telephony.Sms.InterfaceConsts.ReplyPathPresent, 0);                                // I still don't know 
            values.Put(Telephony.Sms.InterfaceConsts.Subject, "");                                        // I still don't know 

            values.Put(Telephony.Sms.InterfaceConsts.Body, smsMessage.MessageBody);                       // 
            values.Put(Telephony.Sms.InterfaceConsts.ServiceCenter, smsMessage.ServiceCenterAddress);     // 

            values.Put(Telephony.Sms.InterfaceConsts.Locked, 0);                                          // spam/no spam?
            values.Put(Telephony.Sms.InterfaceConsts.ErrorCode, 0);
            values.Put(Telephony.Sms.InterfaceConsts.Seen, 1);
            
            if (DeviceFirmware == Brand.Xiaomi)
                values.Put("sim_id", sim);                                                                  // № sim
            else
            {
                values.Put(Telephony.Sms.InterfaceConsts.SubscriptionId, sim);                              // № sim
            }

            // values.Put(Telephony.Sms.InterfaceConsts.sim_id, 1);                                       // for Xiaomi


            // values.Put(Telephony.Sms.InterfaceConsts.timed, 1);
            // values.Put(Telephony.Sms.InterfaceConsts.deleted, 1);
            // values.Put(Telephony.Sms.InterfaceConsts.syncstate, 1);
            // values.Put(Telephony.Sms.InterfaceConsts.marker, 1);
            // values.Put(Telephony.Sms.InterfaceConsts.sourse, 1);
            // values.Put(Telephony.Sms.InterfaceConsts.bind_is, 1);
            // values.Put(Telephony.Sms.InterfaceConsts.mx_***, 1);
            // values.Put(Telephony.Sms.InterfaceConsts.outtime, 1);
            // values.Put(Telephony.Sms.InterfaceConsts.accaunt, 1);
            // values.Put(Telephony.Sms.InterfaceConsts.block_type, 1);
            // values.Put(Telephony.Sms.InterfaceConsts.b2c_, 1);
            // values.Put(Telephony.Sms.InterfaceConsts.fake_cell_type, 1);
            // values.Put(Telephony.Sms.InterfaceConsts.url_risky_type, 1);
            // values.Put(Telephony.Sms.InterfaceConsts.creator, 1);
            // values.Put(Telephony.Sms.InterfaceConsts.favorite_date, 1);

            return values;
        }




        /// <summary>
        /// get messages with this number from database (1) and get it thread. If not-> get max thread_id+1
        /// </summary>
        /// <param name="number">address</param>
        /// <returns>actual thread_id</returns>
        public static int GetThreadId(string number)
        {
            
            var contentResolver = XxmsApp.Droid.MainActivity.InstanceResolver ?? primaryContext.ContentResolver; 


            int threadId = 0;

            var cursor = contentResolver.Query(Telephony.Sms.ContentUri, null, $"address={number}", null, "_id LIMIT 1");
            if (cursor.MoveToFirst())
            {
                threadId = cursor.GetShort(cursor.GetColumnIndex("thread_id"));
            }
            else
            {
                cursor = contentResolver.Query(Telephony.Sms.ContentUri, null, $"address={number}", null, "thread_id DESC LIMIT 1");
                if (cursor.MoveToFirst())
                {
                    threadId = cursor.GetShort(cursor.GetColumnIndex("thread_id")) + 1;
                }
            }

            CheckFirm(cursor);

            return threadId;
        }


        static Brand CheckFirm(ICursor cursor)
        {
            if (DeviceFirmware == Brand.Unknown)
            {
                if (cursor.GetColumnIndex(Telephony.Sms.InterfaceConsts.SubscriptionId) < 0)
                    deviceFirmware = Brand.Xiaomi;
                else
                    deviceFirmware = Brand.Default;

                return deviceFirmware;
            }
            else return deviceFirmware;
        }

    }




}