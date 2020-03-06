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
	public partial class CreateNote : ContentPage
	{
		public CreateNote ()
		{
            var h = Device.GetNamedSize(NamedSize.Large, typeof(Entry))*1.5;

			InitializeComponent ();

            var container = new RelativeLayout()
            {

            };

            container.Children.Add(new Editor {
                
            },Constraint.Constant(0), Constraint.Constant(h), 
                Constraint.RelativeToParent(par => par.Width),
                Constraint.RelativeToParent(par => par.Height - h*3));

            container.Children.Add(new Button
            {
                Text = "Send"  // Отправить

            }, Constraint.Constant(0), 
                Constraint.RelativeToParent((par) => par.Height - h*2),
                Constraint.RelativeToParent(par => par.Width)
            );

            Content = container;

        }
	}
}