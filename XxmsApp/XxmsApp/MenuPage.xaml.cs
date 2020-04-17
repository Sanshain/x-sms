using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XxmsApp.Piece;

namespace XxmsApp
{


	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MenuPage : ContentPage
	{
        // Dicionary<string, Action>

		public MenuPage ()
		{
            
            var menuContainer = new StackLayout { };
            var menu = new ListView
            {
                HorizontalOptions = LayoutOptions.Center,
                ItemsSource = new string[]
                {
                    "Настройки",
                    "Настройки 2",
                    "Настройки 3",
                    "Настройки 0",
                    "Read sms",
                    "О нас",
                },
                ItemTemplate = new DataTemplate(typeof(MenuPoint))
            };
            
            menuContainer.Children.Add(menu);
            menu.ItemSelected += Menu_ItemSelected;
            
            this.Appearing += (object _sender, EventArgs _e) =>
            {
                (this.Parent as MasterDetailPage).IsPresentedChanged += (object sender, EventArgs e) =>
                {
                    if (!(sender as MasterDetailPage).IsPresented)
                    {
                        Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                        {
                            menu.SelectedItem = null;

                            return false;
                        });

                        //menu.SelectedItem = null;
                    }
                };
            };//*/

            /*
            var method = typeof(MenuPage).GetMethod("GoToSettings", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var handle = method.MethodHandle;
            System.Runtime.CompilerServices.RuntimeHelpers.PrepareMethod(handle);//*/

            Content = menuContainer;

        }

        private async void Menu_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null) return;

            switch (e.SelectedItem.ToString())
            {
                case "О нас":

                    (this.Parent as MasterDetailPage).Detail.Navigation.PushAsync(new Views.About(), true);
                    (this.Parent as MasterDetailPage).IsPresented = false;                    

                    break;
                case "Read sms":

                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    /*
                    var x_messages = DependencyService.Get<XxmsApp.Api.IMessages>();
                    var messages = x_messages.Read();
                    //*/
                    // var c = messages.Count;

                    var messages = await Cache.UpdateAsync(new List<Model.Message>());

                    /*
                    var x_messages = DependencyService.Get<XxmsApp.Api.IMessages>();
                    var messages = x_messages.Read();
                    var objects = messages.Select(m => m as object).ToList();//*/

                    sw.Stop();

                    var di = DependencyService.Get <XxmsApp.Api.IMessages>();
                    di.ShowNotification("Test", "content");

                    DisplayAlert($"За {sw.ElapsedMilliseconds.ToString()} мс", messages.Count.ToString() + " sms", "Ok");

                    // sw.Elapsed.ToString()

                    ((ListView)sender).SelectedItem = null;

                    break;

                case "Настройки":

                    GoToSettings();

                    break;

                case "Настройки 2":

                    (this.Parent as MasterDetailPage).Detail.Navigation.PushAsync(
                        new Views.SettingPage(Options.Settings.Initialize()), true);
                    (this.Parent as MasterDetailPage).IsPresented = false;

                    break;

                case "Настройки 3":

                    (this.Parent as MasterDetailPage).Detail.Navigation.PushAsync(
                        new Views.SettingPage(Options.Database.ModelSettings.Initialize()), true);
                    (this.Parent as MasterDetailPage).IsPresented = false;

                    break;


                case "Настройки 0":

                    (this.Parent as MasterDetailPage).Detail.Navigation.PushAsync(
                        new Views.SettingPage(null), true);
                    (this.Parent as MasterDetailPage).IsPresented = false;

                    break;


                default:

                    bool r = await DisplayAlert("Start read?", e.SelectedItem.ToString(), "Ok", "No");

                    ((ListView)sender).SelectedItem = null; 

                    break;
                
            }



            /*
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                ((ListView)sender).SelectedItem = null;

                return false;
            });//*/

        }

        private void GoToSettings()
        {
            (this.Parent as MasterDetailPage).Detail.Navigation.PushAsync(
                new Views.SettingPage(Options.ObSettings.Initialize()), true);
            (this.Parent as MasterDetailPage).IsPresented = false;
        }
    }
}