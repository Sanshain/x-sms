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

[assembly: Dependency(typeof(XxmsApp.Api.Droid.XMessages))]
namespace XxmsApp.Api.Droid
{
    
    class XMessages : IMessages
    {
        ContentResolver contentResolver;

        public List<XxmsApp.Model.Message> Read()
        {

            contentResolver = XxmsApp.Droid.MainActivity.InstanceResolver;

            // List<string> xms = new List<string>();

            ICursor qs = contentResolver.Query(
                Android.Net.Uri.Parse("content://sms/inbox"), null, null, null, null);  
            //on "ORDER BY _id DESC" - timeout exception error

            List<XxmsApp.Model.Message> messages = new List<Model.Message>();

            while (qs.MoveToNext())
            {
                XxmsApp.Model.Message msg = new Model.Message
                {
                    Id = qs.GetInt(qs.GetColumnIndex("_id")),
                    // Time = DateTime.FromBinary(qs.GetLong(qs.GetColumnIndex("date"))),
                    Address = qs.GetString(qs.GetColumnIndex("address")),
                    Value = qs.GetString(qs.GetColumnIndex("body"))
                };

                var time = qs.GetLong(qs.GetColumnIndex("date"));
                msg.Time = new DateTime(1970, 1, 1).AddMilliseconds(time);

                messages.Add(msg);
            }

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

        public bool Send(string adressee, string content)
        {

            SmsManager.Default.SendTextMessage(adressee, null, content, null, null);

            return true;
        }










        public void Send(Model.Message msg)
        {
            throw new NotImplementedException();
        }
    }
}