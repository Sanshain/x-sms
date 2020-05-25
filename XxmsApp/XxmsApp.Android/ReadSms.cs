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
        public void SetSimSender(int messId, int simId)
        {
            var context = Android.App.Application.Context;

            contentResolver = contentResolver ?? XxmsApp.Droid.MainActivity.InstanceResolver ?? context.ContentResolver;

            ContentValues values = new ContentValues();

            values.Put("sim_id", simId);
            var where = $"_id = {messId}";

            int r = contentResolver.Update(Android.Net.Uri.Parse(SMS_CONTENT_URI), values, where, null);
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

        private static List<Model.Message> parseMessages(ICursor qs)
        {
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




    }


}