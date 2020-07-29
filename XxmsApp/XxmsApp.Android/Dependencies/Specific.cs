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
using XxmsApp.Api;

using Xamarin.Forms;
using Java.IO;
using Android.Support.CustomTabs;
using Android.Graphics;

[assembly: Dependency(typeof(XxmsApp.Droid.Dependencies.Specific))]
namespace XxmsApp.Droid.Dependencies
{
    partial class Specific : IEssential
    {

        static Context context;

        public bool PhoneDialer(string number)
        {
            context = context ?? Android.App.Application.Context;

            if (context.PackageManager.HasSystemFeature(Android.Content.PM.PackageManager.FeatureTelephony))
            {                
                Intent intent = new Intent(Intent.ActionCall);
                intent.SetData(Android.Net.Uri.Parse("tel:" + number));
                if (intent.ResolveActivity(context.PackageManager) != null)
                {
                    Android.App.Application.Context.StartActivity(intent);
                    return true;
                }
                else
                {
                    // запросить разрешения
                }
            }
            return false;
        }

        public void SendEmail(string title, string content, string attach)
        {
            context = context ?? Android.App.Application.Context;
            Intent email = new Intent(Intent.ActionSend);

            var uriFile = Android.Net.Uri.Parse(CreateFile(attach));            

            if (uriFile != null) email.PutExtra(Intent.ExtraStream, uriFile);            

            email.PutExtra(Intent.ExtraEmail, new String[] { @"digital-mag@yandex.ru", @"anticafes@gmail.com" });      
            email.PutExtra(Intent.ExtraSubject, title);            
            email.PutExtra(Intent.ExtraText, content);            
            email.SetType("message/rfc822");

            MainActivity.Instance.StartActivityForResult(
                Intent.CreateChooser(email, "Выберите email клиент :"), (int)Api.Droid.OnResult.EmailSent);

            // context.StartActivity(Intent.CreateChooser(email, "Выберите email клиент :")); 
        }

        public string CreateFile(string content)
        {

            var filename = "logs.txt";

            File externalAppDir = new File(Android.OS.Environment.ExternalStorageDirectory + "/Android/data/" + context.PackageName);
            if (!externalAppDir.Exists()) externalAppDir.Mkdir();

            File file = new File(externalAppDir, filename);

            try
            {
                var r = file.CreateNewFile();

                using (FileOutputStream stream = new FileOutputStream(file))
                {
                    stream.Write(Encoding.ASCII.GetBytes(content));
                    stream.Close();
                }                

                return @"file://" + file.Path;
            }
            catch (IOException e)
            {
                throw new Exception("Ошибка создания файла такая - " + e.Message);                
            }         
            
        }

        [Obsolete]
        public void MoveTo(string host)
        {

            var context = Android.App.Application.Context;
            
            CustomTabsIntent builder = new CustomTabsIntent.Builder()
                //.AddDefaultShareMenuItem()        // dont work
                //.SetCloseButtonIcon()
                // .SetToolbarColor(Android.Graphics.Color.ParseColor("#43A047"))                
                .EnableUrlBarHiding()//*/
                .Build();

            // builder.Intent.AddFlags(ActivityFlags.NewTask);
            builder.LaunchUrl(MainActivity.Instance, Android.Net.Uri.Parse(host));

            /*
            Intent intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(host));
            Android.App.Application.Context.StartActivity(intent);
            // throw new Exception("Obsolete");//*/
        }
    }
}