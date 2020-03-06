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
		public MenuPage ()
		{
            
            StackLayout menu = new StackLayout { };
            menu.Children.Add(new ListView
            {
                HorizontalOptions = LayoutOptions.Center,
                ItemsSource = new string[]
                {
                    "Настройки",
                    "О нас"
                },
                ItemTemplate = new DataTemplate(typeof(MenuPoint))
            });

            Content = menu;

        }
	}
}