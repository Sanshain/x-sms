using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;


namespace XxmsApp.Droid
{

    interface IXmessages
    {
        string IXmessagesCall();
    }



    public class XmsServiceBinder : Binder
    {
        public XmsServiceBinder(XmsService service)
        {
            this.Service = service;
        }

        public XmsService Service { get; private set; }
    }



    // [Service(IsolatedProcess = true)]                       // [Service(Exported = true, Name = "com.xamarin.example.DemoService")]
    [Service(Name = "com.xamarin.ServicesDemo1")]
    public class XmsService : Service, IXmessages
    {

        static readonly string TAG = typeof(XmsService).FullName;


        public IBinder Binder { get; private set; }

        public string IXmessagesCall()
        {
            return "get from Service";
        }

        public override IBinder OnBind(Intent intent)
        {
            Console.WriteLine(TAG, "OnBind");
            this.Binder = new XmsServiceBinder(this);
            return this.Binder;
        }






        public override void OnCreate()
        {
            base.OnCreate();

            new XxmsApp.Api.Droid.XMessages().ShowNotification(
                this.GetType().Name, 
                System.Reflection.MethodBase.GetCurrentMethod().Name);
        }
        public override bool OnUnbind(Intent intent)
        {
            new XxmsApp.Api.Droid.XMessages().ShowNotification(
                this.GetType().Name,
                System.Reflection.MethodBase.GetCurrentMethod().Name);

            return base.OnUnbind(intent);
        }

        public override void OnDestroy()
        {
            new XxmsApp.Api.Droid.XMessages().ShowNotification(
                this.GetType().Name,
                System.Reflection.MethodBase.GetCurrentMethod().Name);

            base.OnDestroy();
        }





    }



    public class XmessagesServiceConnection : Java.Lang.Object, IServiceConnection, IXmessages
    {

        static readonly string TAG = typeof(XmessagesServiceConnection).FullName;
        
        public bool IsConnected { get; private set; }
        public XmsServiceBinder Binder { get; private set; }
        MainActivity mainActivity;

        public XmessagesServiceConnection(MainActivity activity)
        {
            IsConnected = false;
            Binder = null;
            this.mainActivity = activity;
            
        }


        public string IXmessagesCall()
        {
            return "get from ServiceConnection";
        }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            Binder = service as XmsServiceBinder;
            IsConnected = this.Binder != null;

            new XxmsApp.Api.Droid.XMessages().ShowNotification(
                this.GetType().Name,
                System.Reflection.MethodBase.GetCurrentMethod().Name);

            string message = "onServiceConnected - ";
            Console.WriteLine(TAG, $"OnServiceConnected {name.ClassName}");

            if (IsConnected)
            {
                message = message + " bound to service " + name.ClassName;
                mainActivity.UpdateUiForBoundService();
            }
            else
            {
                message = message + " not bound to service " + name.ClassName;
                mainActivity.UpdateUiForUnBoundService();
            }

            Console.WriteLine(TAG, message);

            ShowNotification(TAG, message, this.mainActivity);

            mainActivity.ServiceText = message;
        }

        /// <summary>
        /// вызывается только в том случае, если соединение между клиентом и службой неожиданно потеряно или разорвано. 
        /// Этот метод позволяет клиенту ответить на прерывание в службе
        /// </summary>
        /// <param name="name"></param>
        public void OnServiceDisconnected(ComponentName name)
        {
            Console.WriteLine(TAG, $"On__ServiceDisconnected {name.ClassName}");
            IsConnected = false;
            Binder = null;
            mainActivity.UpdateUiForUnBoundService("OnServiceDisconnected");
        }





        public void ShowNotification(string title, string content, Context context = null)
        {
            context = context ?? Android.App.Application.Context;

            Intent notificationIntent = new Intent(Android.App.Application.Context, typeof(XxmsApp.Droid.MainActivity));

            const string CATEGORY_MESSAGE = "msg"; // developer.android.com/reference/android/app/Notification?hl=ru#CATEGORY_MESSAGE


            NotificationCompat.Builder builder = new NotificationCompat.Builder(context)
                .SetSmallIcon(Android.Resource.Drawable.IcDialogInfo)                   // icon
                .SetContentTitle(title)
                .SetContentText(content)                                               // content

                .SetContentIntent(PendingIntent.GetActivity(context, 0, notificationIntent, 0)) // where to pass view by clicked                                
                .SetAutoCancel(true)                                                    // автоотключение уведомления при переходе на активити

                .SetLights(Android.Graphics.Color.ParseColor("#ccffff"), 5000, 5000)    // установка освещения // 0xff0000ff
                .SetVibrate(new long[200])                                              // vibration (need homonymous permission)
                                                                                        // .SetSound(Android.Net.Uri.Parse(""))                

                .SetCategory(CATEGORY_MESSAGE)                                          // category of notify
                .SetGroupSummary(true)
                .SetGroup("messages")

                .SetDefaults((int)NotificationPriority.High);                            // priority (high or max => sound as vk/watsapp)


            Notification notification = builder.Build();
            ((NotificationManager)context.GetSystemService(Context.NotificationService)).Notify(0, notification);
        }

    }

}