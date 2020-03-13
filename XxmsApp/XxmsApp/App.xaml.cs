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
        internal Task<IList<Contact>> contactsAwaiter;        
        internal List<Contact> contacts;

        internal Task<List<Model.Contacts>> contactsWaiter;
        internal List<Model.Contacts> _contacts;


        public App()
        {
            InitializeComponent();

            // contactsAwaiter = Plugin.ContactService.CrossContactService.Current.GetContactListAsync();

            _contacts = Cache.Read<Model.Contacts>();
            if (_contacts.Count == 0)
            {
                contactsWaiter = Cache.UpdateAsync(_contacts);
            }

            MainPage = (new MasterDetailPage()
            {
                Master = new MenuPage { Title = "Title" },
                Detail = new NavigationPage(new XxmsApp.MainPage())
            });

        }

        protected override void OnStart()
        {
            // Handle when your app starts
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