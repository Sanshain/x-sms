﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace XxmsApp
{
	public partial class App : Application
	{
        public App()
        {
            InitializeComponent();

            MainPage = new MasterDetailPage()
            {
                Master = new ContentPage
                {
                    Content = new Label { Text = "None", HorizontalTextAlignment = TextAlignment.Center },
                    Title = "Title"
                },
                Detail = new NavigationPage(new XxmsApp.MainPage()
                {
                    Title = "Detail",
                })
                {
                    Title = "Nav"
                }
            };
            //*/
        }

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}
