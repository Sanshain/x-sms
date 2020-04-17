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
            
            this.Binder = new XmsServiceBinder(this);
            return this.Binder;
        }






        public override void OnCreate()
        {
            base.OnCreate();

        }
        public override bool OnUnbind(Intent intent)
        {
            /*
            new XxmsApp.Api.Droid.XMessages().ShowNotification(this.GetType().Name,
                System.Reflection.MethodBase.GetCurrentMethod().Name);*/

            return base.OnUnbind(intent);
        }

        public override void OnDestroy()
        {
            /*
            new XxmsApp.Api.Droid.XMessages().ShowNotification(this.GetType().Name,
                System.Reflection.MethodBase.GetCurrentMethod().Name);//*/

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


            if (IsConnected)
            {                
                mainActivity.UpdateUiForBoundService();
            }
            else
            {
                mainActivity.UpdateUiForUnBoundService();
            }


            mainActivity.ServiceText = "is connected = " + IsConnected;
        }

        /// <summary>
        /// вызывается только в том случае, если соединение между клиентом и службой неожиданно потеряно или разорвано. 
        /// Этот метод позволяет клиенту ответить на прерывание в службе
        /// </summary>
        /// <param name="name"></param>
        public void OnServiceDisconnected(ComponentName name)
        {

            IsConnected = false;
            Binder = null;

            mainActivity.UpdateUiForUnBoundService("OnServiceDisconnected");
        }



    }

}