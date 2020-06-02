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

namespace XxmsApp.Api
{

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
    [IntentFilter(new string [] { Telephony.Sms.Intents.SmsReceivedAction, Telephony.Sms.Intents.SmsDeliverAction })] 
    public class MessageReceiver : BroadcastReceiver
    {
        public static Brand DeviceFirmware = Brand.Unknown;

        public override void OnReceive(Context context, Intent intent)
        {
            
            Device.BeginInvokeOnMainThread(() => Toast.MakeText(context, "New one message", ToastLength.Long).Show());

            if (intent.Action != Telephony.Sms.Intents.SmsReceivedAction) return;           // and Telephony.Sms.Intents.SmsDeliverAction ?? for delevered?            

            var bundle = intent.Extras;
            var slot = bundle.GetInt("slot", -1);
            var sub = bundle.GetInt("subscription", -1);

            SmsMessage[] messages = Telephony.Sms.Intents.GetMessagesFromIntent(intent);            

            OnMessagesReiceved(messages, sub);

        }

        private void OnMessagesReiceved(SmsMessage[] messages, int sim)
        {
            
            var smsMesages = new List<(string Address, string Message)>();
            var XMessages = new List<XxmsApp.Model.Message>();

            var isDefault = LowLevelApi.Instance.IsDefault;

            for (var i = 0; i < messages.Length; i++)
            {
                smsMesages.Add((messages[i].OriginatingAddress, messages[i].MessageBody));
                
                XMessages.Add(new XxmsApp.Model.Message
                {
                    SimOsId = sim.ToString(),
                    Address = messages[i].OriginatingAddress,
                    Value = messages[i].MessageBody,
                    Time = new DateTime(1970, 1, 1).AddMilliseconds(messages[i].TimestampMillis)                    
                });

                var icc = messages[i].IndexOnIcc;
                var sims = XxmsApp.Api.Droid.XMessages.Instance.GetSimsInfo().ToArray();
                var pdu = messages[i].GetPdu();

                if (isDefault) Save(messages[i], sim);

            }

            Device.BeginInvokeOnMainThread(() =>
            {
                MessagingCenter.Send<XxmsApp.App, List<XxmsApp.Model.Message>>(
                    Xamarin.Forms.Application.Current as XxmsApp.App, 
                    "MessageReceived",
                    XMessages);
            });

            ShowNotification(
                "Новое сообщение от " + XMessages.First().Address,
                XMessages.First().Value);


        }



        public void ShowNotification(string title, string content)
        {
            var context = Android.App.Application.Context;

            Intent notificationIntent = new Intent(Android.App.Application.Context, typeof(XxmsApp.Droid.MainActivity));            

            

            NotificationCompat.Builder builder = new NotificationCompat.Builder(context)
                .SetSmallIcon(Android.Resource.Drawable.IcDialogInfo)
                .SetContentTitle(title)
                .SetContentText(content)

                .SetContentIntent(PendingIntent.GetActivity(context, 0, notificationIntent, 0))
                .SetAutoCancel(true)
                                
                .SetDefaults((int)NotificationPriority.High);
                

            Notification notification = builder.Build();
            // notification.Sound = null;
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

            // if (Options.ModelSettings.Sound == true) XMessages.Instance.SoundPlay(Options.ModelSettings.Rington, null, null);

        }

        private void Save(SmsMessage smsMessage, int sim)
        {
            var uri = Telephony.Sms.ContentUri;
            var draftUri = Telephony.Sms.Draft.ContentUri;
            var inboxUri = Telephony.Sms.Inbox.ContentUri;
            var outboxUri = Telephony.Sms.Outbox.ContentUri;
            var sentUri = Telephony.Sms.Sent.ContentUri;

            var contentResolver = XxmsApp.Droid.MainActivity.InstanceResolver;            

            var values = FillValues(smsMessage, sim);

            var qs = contentResolver.Insert(Telephony.Sms.Inbox.ContentUri, values); // Android.Net.Uri.Parse("content://sms/inbox")
        }





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
            
            var contentResolver = XxmsApp.Droid.MainActivity.InstanceResolver;

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


        static void CheckFirm(ICursor cursor)
        {
            if (DeviceFirmware == Brand.Unknown)
            {
                if (cursor.GetColumnIndex(Telephony.Sms.InterfaceConsts.SubscriptionId) < 0)
                    DeviceFirmware = Brand.Xiaomi;
                else
                    DeviceFirmware = Brand.Default;
            }
        }

    }




}