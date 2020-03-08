﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace XxmsApp
{

    public partial class App : Application
    {
        public const string DATA_FILE = "messages.db";

        public App()
        {
            InitializeComponent();
            
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