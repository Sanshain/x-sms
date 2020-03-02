using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XxmsApp
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();

            var views = new StackLayout();
            var scroller = new ScrollView { Content = views };

            var btn = new Button { Text = "Click" };
            btn.Clicked += Btn_Clicked;
            var rootLayout = new AbsoluteLayout();

            rootLayout.Children.Add(scroller, new Rectangle(0, 0, 1, 0.9));

            AbsoluteLayout.SetLayoutFlags(scroller, AbsoluteLayoutFlags.SizeProportional);            
            rootLayout.Children.Add(btn, new Rectangle(0, 1, 1, 0.1));
            AbsoluteLayout.SetLayoutFlags(btn, AbsoluteLayoutFlags.All);//*/

            Title = "Absolute Layout Exploration - Code";

            Content = rootLayout;

            for (int i = 0; i < 35; i++)
            {
                views.Children.Add(new Label
                {
                    Text = "Hello " + i,
                    FontSize = 16
                });
            }
        }

        private void Btn_Clicked(object sender, EventArgs e)
        {
            var target = sender as Button;
            var parent = (target.Parent as Layout).Parent as Page;
            ((parent.Parent as NavigationPage).Parent as MasterDetailPage).IsPresented = true;
        }
    }
}
