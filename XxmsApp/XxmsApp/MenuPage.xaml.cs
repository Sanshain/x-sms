using System;
using System.Collections.Generic;
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
                    "О нас"
                },
                ItemTemplate = new DataTemplate(typeof(MenuPoint))
            };
            
            menuContainer.Children.Add(menu);
            menu.ItemSelected += Menu_ItemSelected;


            Content = menuContainer;

        }


        private void Menu_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null) return;

            switch (e.SelectedItem.ToString())
            {
                case "О нас":

                    (this.Parent as MasterDetailPage).Detail.Navigation.PushAsync(new Views.About(), true);
                    (this.Parent as MasterDetailPage).IsPresented = false;

                    break;
                default:

                    DisplayAlert("title", e.SelectedItem.ToString(), "Ok", "No");
                    break;
                
            }            
            
        }
    }
}