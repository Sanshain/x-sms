using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Xamarin.Forms;
using Android.Content.Res;
using Xamarin.Forms.Platform.Android;
using Android.Content;
using Android.Telephony;

using XxmsApp.Api.Droid;
using Android.Provider;


// [assembly: Permission(Name = "android.permission.BROADCAST_WAP_PUSH")] namespace Receiver {}

[assembly: Dependency(typeof(XxmsApp.Piece.MeasureString))]
namespace XxmsApp.Piece
{ 

    public class MeasureString : IMeasureString
    {
        public double StringSize(string text)
        {
            var bounds = new Android.Graphics.Rect();
            TextView view = new TextView(Forms.Context);
            view.Paint.GetTextBounds(text, 0, text.Length, bounds);
            var length = bounds.Width();
            return length / Resources.System.DisplayMetrics.ScaledDensity;
        }
    }
}



namespace XxmsApp.Droid
{
    public delegate void ActivityResultHandler(Android.Net.Uri uri, object subj);


    [Activity(
        Label = "XxmsApp", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait
        ), ]
    [IntentFilter(new string[] { Intent.ActionSend, Intent.ActionSendto/* , Intent.ActionMain*/ }, Categories = new string[]{
        // Intent.CategoryLauncher,
        Intent.CategoryDefault,
        Intent.CategoryBrowsable
    }, DataSchemes = new string[] { "sms", "smsto", "mms", "mmsto" })]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static Android.Content.ContentResolver InstanceResolver;
        internal static MainActivity Instance;

        protected override void OnCreate(Bundle bundle)
        {
            Instance = this;

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);

            var application = new App();

            try
            {
                LoadApplication(application);
            }
            catch(Exception ex)
            {
                var exc = ex;
            }
            

            CreateMessageStateListener();

            // set in App.xaml:
            // Xamarin.Forms.Application.Current.On<Xamarin.Forms.PlatformConfiguration.Android>()
            // .UseWindowSoftInputModeAdjust(WindowSoftInputModeAdjust.Resize);

            InstanceResolver = this.ContentResolver;
            
        }

        private void CreateMessageStateListener()
        {
            const string SENT = "SMS_SENT";
            const string DELIVERED = "SMS_DELIVERED";            

            XMessages.PendInSent = PendingIntent.GetBroadcast(
                global::Android.App.Application.Context, 0,
                new Intent(SENT), 0);

            XMessages.PendInDelivered = PendingIntent.GetBroadcast(
                Android.App.Application.Context,
                0, new Intent(DELIVERED), 0);

            RegisterReceiver(new XamBroadcastReceiver(XamBroadcastReceiver.SentListener), new IntentFilter(SENT));
            RegisterReceiver(new XamBroadcastReceiver(XamBroadcastReceiver.DeliveredListener), new IntentFilter(DELIVERED));

            serviceConnection = new XmessagesServiceConnection(this);
            Intent serviceToStart = new Intent(this, typeof(XmsService));
            var r = BindService(serviceToStart, this.serviceConnection, Bind.AutoCreate);
            
            if (r)
            {

            }

        }

        
        public Action<Android.Net.Uri, object> ReceiveActivityResult = null;

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            switch (requestCode)
            {
                case 0:

                    if (data == null)  return;
                    if (resultCode == Result.Ok)
                    {
                        Android.Net.Uri chosenImageUri = data.Data;

                        /*
                        XxmsApp.Api.Droid.XMessages.currentSound?.Stop();                        
                        XxmsApp.Api.Droid.XMessages.currentSound = Android.Media.RingtoneManager.GetRingtone(this, chosenImageUri);
                        XxmsApp.Api.Droid.XMessages.currentSound.Play();                       
                        ReceiveActivityResult?.Invoke(chosenImageUri, XxmsApp.Api.Droid.XMessages.currentSound);
                        //*/


                        Dependencies.Player.currentMelody?.Stop();
                        var player = Dependencies.Player.currentMelody = new Android.Media.MediaPlayer();
                        /*
                        player.Reset();
                        player.SetDataSource(chosenImageUri.Scheme + ":" + chosenImageUri.SchemeSpecificPart);
                        // player.SetDataSource(this, chosenImageUri);  // player.SetDataSource(context, Urim);
                        player.Prepare();
                        var duration = player.Duration;
                        player.Start();//*/

                        ReceiveActivityResult?.Invoke(chosenImageUri, player);                        
                        //*/

                    }
                    
                    // String name = data.GetStringExtra("name");

                    break;

                default:

                    break;
            }

        }



        #region Service

        XmessagesServiceConnection serviceConnection;

        public void UpdateUiForBoundService()
        {
            Toast.MakeText(Android.App.Application.Context, "UpdateUiForBoundService", ToastLength.Long).Show();

            // new AlertDialog.Builder()

        }

        public void UpdateUiForUnBoundService(string txt = "")
        {
            Toast.MakeText(Android.App.Application.Context, "Unbound service of " + txt, ToastLength.Long).Show();

            // new AlertDialog.Builder()

        }

        public string ServiceText { get; set; } 

        #endregion

    }


    /// <summary>
    /// Need move to Utils. flags for SMS state inside XamBroadcastReceiver (from SmsManager final constants)
    /// </summary>
    public enum MessageStateManager
    {
        RESULT_ERROR_GENERIC_FAILURE = 1,                           // Generic failure
        RESULT_ERROR_RADIO_OFF = 2,                                 // Radio off
        RESULT_ERROR_NULL_PDU = 3,                                  // Null PDU or no pdu provided
        RESULT_ERROR_NO_SERVICE = 4,                                // no service or service is currently unavailable
        
        RESULT_ERROR_LIMIT_EXCEEDED = 5,                            // Failed because we reached the sending queue limit.  {@hide}
        RESULT_ERROR_FDN_CHECK_FAILURE = 6                          // Failed because FDN is enabled. {@hide}
    }







    [BroadcastReceiver(Label = "MMS Receiver", Permission = Android.Manifest.Permission.BroadcastWapPush)]
    // [IntentFilter(new string[] { Telephony.Sms.Intents.SmsDeliverAction })] // ->

    [IntentFilter(new string[] { Telephony.Sms.Intents.WapPushDeliverAction }, DataMimeType = @"application/vnd.wap.mms-message")]   
    class XamBroadcastReceiver : BroadcastReceiver
    {
        public Action<Intent, int> Hook { get; set; } = null;

        public XamBroadcastReceiver() : base()
        {

        }

        public XamBroadcastReceiver(Action<Intent, int> action) : base()
        {
            Hook = action;
        }

        public override void OnReceive(Context context, Intent intent) 
        {
            
            var res = (int)this.ResultCode;

            Hook?.Invoke(intent, (int)this.ResultCode);

        }


        public static void SentListener(Intent intent, int res)
        {
            switch (res)
            {
                case (int)Result.Ok:

                    Toast.MakeText(Android.App.Application.Context, "SMS sent", ToastLength.Long).Show();
                    break;

                case (int)MessageStateManager.RESULT_ERROR_GENERIC_FAILURE:

                    Toast.MakeText(Android.App.Application.Context, "Generic failure", ToastLength.Long).Show();
                    break;

                case (int)MessageStateManager.RESULT_ERROR_NO_SERVICE:

                    Toast.MakeText(Android.App.Application.Context, "No service", ToastLength.Long).Show();
                    break;

                case (int)MessageStateManager.RESULT_ERROR_NULL_PDU:

                    Toast.MakeText(Android.App.Application.Context, "Null PDU", ToastLength.Long).Show();
                    break;

                case (int)MessageStateManager.RESULT_ERROR_RADIO_OFF:

                    Toast.MakeText(Android.App.Application.Context, "Radio off", ToastLength.Long).Show();
                    break;

            }
        }

        public static void DeliveredListener(Intent intent, int res)
        {
            switch ((Result)res)
            {
                case Result.Ok:

                    Toast.MakeText(Android.App.Application.Context, "SMS delivered", ToastLength.Long).Show();
                    break;

                case Result.Canceled:

                    Toast.MakeText(Android.App.Application.Context, "SMS not delivered", ToastLength.Long).Show();
                    break;

            }
        }

    }

}




