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

[assembly: Dependency(typeof(XxmsApp.Api.IncomingSms))]
namespace XxmsApp.Api
{

   
    // [BroadcastReceiver(Enabled = true, Exported = true)]
    
    [BroadcastReceiver(Enabled = true, Label = "SMS Receiver")]
    [IntentFilter(new string [] { Telephony.Sms.Intents.SmsReceivedAction })]           // "android.provider.Telephony.SMS_RECEIVED"
    public class IncomingSms : BroadcastReceiver, IReceived
    {
        
        public event OnReceived Received;

        private const string TAG = "AA:SmsReceiver";
        public override void OnReceive(Context context, Intent intent)
        {

            Toast.MakeText(context, intent.GetStringExtra("key"), ToastLength.Long).Show();

            if (intent.Action != Telephony.Sms.Intents.SmsReceivedAction) return;

            SmsMessage[] messages = Telephony.Sms.Intents.GetMessagesFromIntent(intent);

            OnMessagesReiceved(messages);


            /*
            var _messages = new List<string>();

            for (var i = 0; i < messages.Length; i++)
            {
                _messages.Add(messages[i].OriginatingAddress + ":" +  messages[i].MessageBody);
            }
            //*/
        }

        private void OnMessagesReiceved(SmsMessage[] messages)
        {
            // var sb = new StringBuilder();
            var smsMesages = new List<(string Address, string Message)>();
            var XMessages = new List<XxmsApp.Model.Message>();

            for (var i = 0; i < messages.Length; i++)
            {
                smsMesages.Add((messages[i].OriginatingAddress, messages[i].MessageBody));


                XMessages.Add(new XxmsApp.Model.Message
                {
                    Phone = messages[i].OriginatingAddress,
                    Value = messages[i].MessageBody
                });


                //*/

                /*
                sb.Append(string.Format(
                    "SMS From: {0}{1}Body: {2}{1}", 
                    messages[i].OriginatingAddress,
                    System.Environment.NewLine, 
                    messages[i].MessageBody));//*/
            }

            Received(XMessages);

            /*
            if (intent.Action.Equals(Telephony.Sms.Intents.SmsReceivedAction))
            {
                var msgs = Telephony.Sms.Intents.GetMessagesFromIntent(intent);
                foreach (var msg in msgs)
                {
                    Log.Debug(TAG, $" MessageBody {msg.MessageBody}");
                    Log.Debug(TAG, $"DisplayOriginatingAddress {msg.DisplayOriginatingAddress}");
                    Log.Debug(TAG, $"OriginatingAddress {msg.OriginatingAddress}");
                }
            }//*/

        }
    }
}