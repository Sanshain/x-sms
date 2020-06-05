﻿using System;
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
using System.Diagnostics;
using Android.Provider;

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
        

        ContentResolver contentResolver = null;
        const string SMS_CONTENT_URI = "content://sms";

        public int SetStateRead(int messId)
        {
            var context = Android.App.Application.Context;

            contentResolver = contentResolver ?? XxmsApp.Droid.MainActivity.InstanceResolver ?? context.ContentResolver;            

            ContentValues values = new ContentValues();

            values.Put("read", 1);
            var where = $"_id = {messId}";

            return contentResolver.Update(Android.Net.Uri.Parse(SMS_CONTENT_URI), values, where, null);
        }

        public int SetStateRead(int[] messIds)
        {
            var context = Android.App.Application.Context;

            contentResolver = contentResolver ?? XxmsApp.Droid.MainActivity.InstanceResolver ?? context.ContentResolver;

            ContentValues values = new ContentValues();

            values.Put("read", true);
            var where = $"_id={messIds.Last()}";

            // String selection = $"_id = ? AND read = 0";
            // String[] selectionArgs = { messIds.Last(), "1" };


            var uri = Android.Net.Uri.Parse(SMS_CONTENT_URI + "/inbox");
            var idUri = ContentUris.WithAppendedId(uri, messIds.Last());

            // var msgs = contentResolver.Query(idUri, null, null, null, null);
            var msgs = contentResolver.Query(uri, null, where, null, null);

            var mss = new List<XxmsApp.Model.Message>();
            while (msgs.MoveToNext())
            {
                var ms = this.ParseMessage(msgs);
                mss.Add(ms);
            }

            return contentResolver.Update(uri, values, where, null);
        }


            

        public List<XxmsApp.Model.Message> ReadFrom(int start)
        {
            contentResolver = contentResolver ?? XxmsApp.Droid.MainActivity.InstanceResolver;
            var query = $"_id > {start}";

            ICursor qs = contentResolver.Query(Android.Net.Uri.Parse(SMS_CONTENT_URI), null, query, null, null);

            var messages = parseMessages(qs);

            return messages;
        }

        /// <summary>
        /// назначает сим для вызова 
        /// </summary>
        /// <param name="messId"></param>
        /// <param name="simId"></param>
        public int SetSimSender(int messId, int simId)
        {
            var context = Android.App.Application.Context;

            contentResolver = contentResolver ?? XxmsApp.Droid.MainActivity.InstanceResolver ?? context.ContentResolver;

            ContentValues values = new ContentValues();

            values.Put("sim_id", simId);            
            var uri = Android.Net.Uri.Parse(SMS_CONTENT_URI);
            var idUri = ContentUris.WithAppendedId(uri, messId);
            // var where = $"_id = {messId}";            
            
            int r = contentResolver.Update(idUri, values, null, null);

            return r;
        }

        public List<XxmsApp.Model.Message> ReadAll()
        {

            contentResolver = XxmsApp.Droid.MainActivity.InstanceResolver;

            ICursor qs = contentResolver.Query(Android.Net.Uri.Parse("content://sms"), null, null, null, null);
           
            var messages = parseMessages(qs);

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

        private List<Model.Message> parseMessages(ICursor qs)
        {
            List<XxmsApp.Model.Message> messages = new List<Model.Message>();

            while (qs.MoveToNext())
            {
                
                Model.Message msg = ParseMessage(qs);

                msg.SetSim(qs.GetString(qs.GetColumnIndex("sim_id")));                      // SubscriptionId

                var time = qs.GetLong(qs.GetColumnIndex("date"));
                msg.Time = new DateTime(1970, 1, 1).AddMilliseconds(time);

                messages.Add(msg);
            }

            return messages;
        }

        private Model.Message ParseMessage(ICursor qs)
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
            return msg;
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

            try
            {
                sim?.SendTextMessage(adressee, null, content, PendInSent, PendInDelivered);
            }
            catch(Exception ex) // if no address to send
            {
                var m = ex.Message;
                return false;
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();

            var c =Android.App.Application.Context.ContentResolver.Query(Android.Net.Uri.Parse(SMS_CONTENT_URI), null, null, null, null);
            c.MoveToFirst();
            var message = ParseMessage(c);

            this.SetSimSender(message.Id, sim.SubscriptionId);

            sw.Stop();
            var lg = sw.ElapsedMilliseconds;

            if (LowLevelApi.Instance.IsDefault)
            {
                var values = FillValuesOut(adressee, content, simId != null ? simId.Value : 0);
                contentResolver.Insert(Telephony.Sms.Outbox.ContentUri, values);
            }

            return sim != null;
        }

        public void Send(XxmsApp.Model.Message msg)
        {
            var sim = int.TryParse(msg.SimOsId, out int subId) ? SmsManager.GetSmsManagerForSubscriptionId(subId) : SmsManager.Default;

            sim.SendTextMessage(msg.Address, null, msg.Value, PendInSent, PendInDelivered);

        }


        private ContentValues FillValuesOut(string number, string body, int simId)
        {

            ContentValues values = new ContentValues();


            // values.Put(Telephony.Sms.InterfaceConsts.Id, "");

            var stamp = new TimeSpan(-1970, 1, 1);
            var now = DateTime.Now.Add(stamp).Millisecond;


            values.Put(Telephony.Sms.InterfaceConsts.ThreadId, MessageReceiver.GetThreadId(number));      // chat id
            values.Put(Telephony.Sms.InterfaceConsts.Address, number);                                    // no read
            values.Put(Telephony.Sms.InterfaceConsts.Person, "");                                         // Person
            values.Put(Telephony.Sms.InterfaceConsts.Date, now.ToString());                               // now
            values.Put(Telephony.Sms.InterfaceConsts.DateSent, now.ToString());                           // sent
            values.Put(Telephony.Sms.InterfaceConsts.Protocol, 1);                                        // in/out
            // values.Put(Telephony.Sms.InterfaceConsts.Status, -1);                                         // always -1 for inbox
            values.Put(Telephony.Sms.InterfaceConsts.Type, 2);                                            // > 1 dor outgoing

            values.Put(Telephony.Sms.InterfaceConsts.ReplyPathPresent, 0);                                // I still don't know 
            values.Put(Telephony.Sms.InterfaceConsts.Subject, "");                                        // I still don't know 

            values.Put(Telephony.Sms.InterfaceConsts.Body, body);                                         // 
            values.Put(Telephony.Sms.InterfaceConsts.ServiceCenter, "");     // 

            values.Put(Telephony.Sms.InterfaceConsts.Locked, 0);                                          // spam/no spam?
            values.Put(Telephony.Sms.InterfaceConsts.ErrorCode, 0);
            values.Put(Telephony.Sms.InterfaceConsts.Seen, 1);

            if (MessageReceiver.DeviceFirmware == Brand.Xiaomi)
                values.Put("sim_id", simId);                                                                  // № sim
            else
            {
                values.Put(Telephony.Sms.InterfaceConsts.SubscriptionId, simId);                              // № sim
            }

            return values;
        }

        Sim[] sims = null;
        public Sim[] Sims => sims ?? (sims = GetSimsInfo().ToArray());

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




    }


}