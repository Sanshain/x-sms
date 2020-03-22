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
                    "Read sms",
                    "О нас"
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

                    var x_messages = DependencyService.Get<XxmsApp.Api.IMessages>();
                    var messages = x_messages.Read();
                    // var c = messages.Count;

                    sw.Stop();
                    DisplayAlert($"За {sw.ElapsedMilliseconds.ToString()} мс", messages.ToString(), "Ok");

                    // sw.Elapsed.ToString()

                    ((ListView)sender).SelectedItem = null;

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
    }
}