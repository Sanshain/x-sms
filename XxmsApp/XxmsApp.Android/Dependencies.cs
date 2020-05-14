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




// [assembly: Dependency(typeof(XxmsApp.Api.IncomingSms))]
namespace XxmsApp.Api
{

    // [BroadcastReceiver(Enabled = true, Exported = true)]

    [BroadcastReceiver(Enabled = true, Label = "SMS Receiver")]
    [IntentFilter(new string [] { Telephony.Sms.Intents.SmsReceivedAction })]               // "android.provider.Telephony.SMS_RECEIVED"
    public class IncomingSms : BroadcastReceiver
    {
        
        public override void OnReceive(Context context, Intent intent)
        {
            
            Device.BeginInvokeOnMainThread(() => Toast.MakeText(context, "New one message", ToastLength.Long).Show());

            if (intent.Action != Telephony.Sms.Intents.SmsReceivedAction) return;           // and Telephony.Sms.Intents.SmsDeliverAction ?? for delevered?            

            SmsMessage[] messages = Telephony.Sms.Intents.GetMessagesFromIntent(intent);

            OnMessagesReiceved(messages);

        }

        private void OnMessagesReiceved(SmsMessage[] messages)
        {
            
            var smsMesages = new List<(string Address, string Message)>();
            var XMessages = new List<XxmsApp.Model.Message>();

            for (var i = 0; i < messages.Length; i++)
            {
                smsMesages.Add((messages[i].OriginatingAddress, messages[i].MessageBody));


                XMessages.Add(new XxmsApp.Model.Message
                {
                    Address = messages[i].OriginatingAddress,
                    Value = messages[i].MessageBody,
                    Time = new DateTime(1970, 1, 1).AddMilliseconds(messages[i].TimestampMillis)
                });

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
            ((NotificationManager)context.GetSystemService(Context.NotificationService)).Notify(0, notification);


        }




    }




}