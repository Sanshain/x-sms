using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XxmsApp.Model;

namespace XxmsApp.Piece
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MessageView : ContentView
	{
		public MessageView (Message msg)
		{
			InitializeComponent ();

            var rltv = new StackLayout
            {
                Padding = new Thickness(5),
            };
            var msText = new Label { Text = msg.Value, Margin = new Thickness(5) };            

            var forgetPassword_tap = new TapGestureRecognizer();
            forgetPassword_tap.Tapped += ForgetPassword_tap_Tapped;
            msText.GestureRecognizers.Add(forgetPassword_tap);//*/

            // rltv.Children.Add(msText, 0, 20, p => p.Width);
            // rltv.Children.Add(new Label { Text = msg.Time.ToString(), TextColor = Color.Gray }, 0, 0);

            rltv.Children.Add(new Label { Text = msg.Time.ToString(), TextColor = Color.Gray });
            rltv.Children.Add(msText);


            Content = new Frame {
                Content = rltv, HasShadow = true, OutlineColor = Color.Red, Margin = new Thickness(10)
            };

        }

        private void ForgetPassword_tap_Tapped(object sender, EventArgs e)
        {
            (sender as Label).BackgroundColor = Color.Red;
        }
    }
}