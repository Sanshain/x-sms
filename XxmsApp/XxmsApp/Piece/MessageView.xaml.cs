using System;
using System.Collections.Generic;
using System.Linq;
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

            Content = new Label { Text = msg.Label };

        }
	}
}