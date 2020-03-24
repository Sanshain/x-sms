using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XxmsApp.Model;

namespace XxmsApp.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Messages : ContentPage
	{
		public Messages (object obj)
		{
			// InitializeComponent ();            


            var count = 10;

            var messageViews = new StackLayout();
            
            if (obj.GetType() == typeof(Dialog))
            {
                Title = "Сообщения";

                var dialog = obj as Dialog;
                //for (int i = 0; i < dialog.Messages.Count(); i++)
                foreach(var msg in dialog.Messages)
                {
                    Piece.MessageView msgView = new Piece.MessageView(msg);

                    messageViews.Children.Add(msgView);
                }
            }




            RelativeLayout root = new RelativeLayout();
            root.Children.Add(new ScrollView
            {
                Content = messageViews
            }, 
                Constraint.Constant(0), 
                Constraint.Constant(0), 
                Constraint.RelativeToParent((par) => 
                    {
                        return par.Width;
                    }),
                Constraint.RelativeToParent((par) => 
                    {
                        return par.Height * 0.9;      
                    })
             );

             Content = root;

        }
	}
}