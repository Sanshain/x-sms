using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;

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



    [Service(IsolatedProcess = true)]                       // [Service(Exported = true, Name = "com.xamarin.example.DemoService")]
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
            Android.Util.Log.Debug(TAG, "OnBind");
            this.Binder = new XmsServiceBinder(this);
            return this.Binder;
        }






        public override void OnCreate()
        {
            base.OnCreate();
            Android.Util.Log.Debug(TAG, "OnCreate");
        }
        public override bool OnUnbind(Intent intent)
        {
            Android.Util.Log.Debug(TAG, "OnUnbind");
            return base.OnUnbind(intent);
        }

        public override void OnDestroy()
        {
            Android.Util.Log.Debug(TAG, "OnDestroy");
            base.OnDestroy();
        }


    }



    public class XmessagesServiceConnection : Java.Lang.Object, IServiceConnection, IXmessages
    {

        static readonly string TAG = typeof(XmessagesServiceConnection).FullName;

        MainActivity mainActivity;

        public XmessagesServiceConnection(MainActivity activity)
        {
            IsConnected = false;
            Binder = null;
            this.mainActivity = activity;
        }

        public bool IsConnected { get; private set; }
        public XmsServiceBinder Binder { get; private set; }



        public string IXmessagesCall()
        {
            return "get from ServiceConnection";
        }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            Binder = service as XmsServiceBinder;
            IsConnected = this.Binder != null;


            string message = "onServiceConnected - ";
            Android.Util.Log.Debug(TAG, $"OnServiceConnected {name.ClassName}");

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

            Android.Util.Log.Info(TAG, message);

            mainActivity.ServiceText = message;
        }

        /// <summary>
        /// вызывается только в том случае, если соединение между клиентом и службой неожиданно потеряно или разорвано. 
        /// Этот метод позволяет клиенту ответить на прерывание в службе
        /// </summary>
        /// <param name="name"></param>
        public void OnServiceDisconnected(ComponentName name)
        {
            Android.Util.Log.Debug(TAG, $"On__ServiceDisconnected {name.ClassName}");
            IsConnected = false;
            Binder = null;
            mainActivity.UpdateUiForUnBoundService("OnServiceDisconnected");
        }
    }

}