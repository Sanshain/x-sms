using Plugin.ContactService.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XxmsApp
{

    public partial class App : Application
    {
        public const string DATABASE_FILENAME = "messages.db";

        internal Task<List<Model.Contacts>> contactsWaiter;
        internal List<Model.Contacts> _contacts;
        Api.IReceived xMessages;

        public App()
        {
            
            InitializeComponent();

            DBUpdates();

            MainPage = (new MasterDetailPage()
            {
                Master = new MenuPage { Title = "Title" },
                Detail = new NavigationPage(new XxmsApp.MainPage()) { BarBackgroundColor = Color.Black }
            });

            xMessages = DependencyService.Get<XxmsApp.Api.IReceived>();
            xMessages.Received += XMessages_Received;//*/
            
            MessagingCenter.Subscribe<App, List<XxmsApp.Model.Message>>(
                this,                                                       // кто подписывается на сообщения
                "MessageReceived",                                              // название сообщения
                async (sender, messages) => 
                {
                    var StartPage = (((MainPage as MasterDetailPage).Detail as NavigationPage).RootPage as XxmsApp.MainPage);

                    var i = 0;                    
                    
                    while (i < messages.Count)
                    {
                        var b = await StartPage.DisplayAlert(
                            messages[i]?.Address ?? "No address",
                            messages[i].Value,
                            "ok",
                            $"Next ({++i}) message of {messages.Count.ToString()}" 
                        );

                        if (b) break;
                    }

                });    


        }

        private void XMessages_Received(IEnumerable<Model.Message> message)
        {
            var StartPage = (((MainPage as MasterDetailPage).Detail as NavigationPage).RootPage as XxmsApp.MainPage);

            StartPage.DisplayAlert(
                message.Count().ToString(),
                message.FirstOrDefault()?.Value ?? "void",
                "ok");
            
        }
        

        private void DBUpdates()
        {
            if ((_contacts = Cache.Read<Model.Contacts>()).Count == 0) contactsWaiter = Cache.UpdateAsync(_contacts);
            else    // сделать удаленное обвновление с задержкой
            {
                Device.StartTimer(TimeSpan.FromSeconds(20), () =>
                {
                    Cache.UpdateAsync(_contacts).ContinueWith(tsk => _contacts = tsk.Result);

                    return false;
                });
            }
        }

        protected override void OnStart()
        {
            // IncomingSms

            // how can I here get sms
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}