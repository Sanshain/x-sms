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
	public partial class MenuPoint : ViewCell
	{
		public MenuPoint ()
		{
			// InitializeComponent ();

            StackLayout mnuItem = new StackLayout()
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            Label content = new Label {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label))            
            };

            content.SetBinding(Label.TextProperty, ".");

            mnuItem.Children.Add(content);
            View = mnuItem;
        }
	}
}