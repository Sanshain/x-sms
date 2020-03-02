using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XxmsApp.Piece
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Note : ViewCell
	{
		public Note ()
		{
			InitializeComponent ();
		}
	}
}