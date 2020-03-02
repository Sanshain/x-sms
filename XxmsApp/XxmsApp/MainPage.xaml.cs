using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            var subScroller = new ScrollView { Content = views };
            var subBtn = new Button { Text = "Click" };
            var rootLayout = new AbsoluteLayout();            
                        
            rootLayout.Children.Add(subScroller, new Rectangle(0, 0, 1, 0.9), AbsoluteLayoutFlags.SizeProportional);            
            rootLayout.Children.Add(subBtn, new Rectangle(0, 1, 1, 0.1), AbsoluteLayoutFlags.All);
            Content = rootLayout;

            subBtn.Clicked += Btn_Clicked;
            

            Title = "Detail";
            
            views.Children.Add(new Piece.CustomList());

            
        }





        private void Btn_Clicked(object sender, EventArgs e)
        {
            var target = sender as Button;
            var parent = (target.Parent as Layout).Parent as Page;
            ((parent.Parent as NavigationPage).Parent as MasterDetailPage).IsPresented = true;


        }
    }
}
