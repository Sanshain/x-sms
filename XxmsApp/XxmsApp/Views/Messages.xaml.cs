using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XxmsApp.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Messages : ContentPage
	{
		public Messages ()
		{
			InitializeComponent ();

            var count = 10;

            var messageViews = new StackLayout();
            
            for (int i = 0; i < count; i++)
            {
                Piece.MessageView msgView = new Piece.MessageView();

                messageViews.Children.Add(msgView);
            }


            RelativeLayout root = new RelativeLayout();
            root.Children.Add(new ScrollView
            {
                Content = messageViews
            }, Constraint.Constant(0), Constraint.Constant(0), Constraint.RelativeToParent((par) => 
            {
                return par.Width;
            }),
            Constraint.RelativeToParent((par) => 
            {
                return par.Height * 0.9;      
            }));

            Content = root;

        }
	}
}