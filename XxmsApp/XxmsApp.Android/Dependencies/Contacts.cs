using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using XxmsApp.Api;
using XxmsApp.Model;

[assembly: Dependency(typeof(XxmsApp.Droid.Dependencies.Specific))]
namespace XxmsApp.Droid.Dependencies
{
    public partial class Specific : IEssential
    {
        public IList<Model.Contacts> GetContacts()
        {
            var contacts = new List<Model.Contacts>();

            var context = Android.App.Application.Context;
            
            var cur = context.ContentResolver.Query(ContactsContract.Contacts.ContentUri,
                    null, null, null, null);

            cur?.L(c =>
            {
                var columns = cur.GetColumnNames();

                while (cur.MoveToNext())
                {                    

                    var rawName = cur.GetString(cur.GetColumnIndex(ContactsContract.ContactsColumns.NameRawContactId));
                    var name = cur.GetString(cur.GetColumnIndex(ContactsContract.ContactsColumns.DisplayName));
                    var hasPhone = cur.GetString(cur.GetColumnIndex(ContactsContract.ContactsColumns.HasPhoneNumber));
                    var photo = cur.GetString(cur.GetColumnIndex(ContactsContract.ContactsColumns.PhotoThumbnailUri));

                    var pResp = context.ContentResolver.Query(
                        ContactsContract.CommonDataKinds.Phone.ContentUri, null,
                        "contact_id" + " = ?",
                        new String[] { rawName }, null);

                    var _columns = pResp.GetColumnNames();

                    List<string> phones = new List<string>();

                    while (pResp?.MoveToNext() ?? false)
                    {
                        String phoneNo = pResp.GetString(
                            pResp.GetColumnIndex(ContactsContract.CommonDataKinds.Phone.Number));

                        phones.Add(phoneNo);
                    }
                  
                    var contact = new Model.Contacts
                    {
                        Name = name,
                        Phone = phones.FirstOrDefault()?.Replace(" ","").Replace("-","") ?? string.Empty,
                        OptionalPhones = string.Join(';', phones),
                        Photo = photo
                    };

                    contacts.Add(contact);

                }
            });

            return contacts;
        }
    }
}