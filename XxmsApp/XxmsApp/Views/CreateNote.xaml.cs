using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace XxmsApp.Views
{
    public class CustomEditor : Entry
    {

    }



	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CreateNote : ContentPage
	{
        StackLayout msgFields;
        StackLayout bottom;
        

        public CreateNote ()
		{            

			InitializeComponent ();

            
            // var h = Device.GetNamedSize(NamedSize.Large, typeof(Entry)) * 1.5;



            var container = new StackLayout();


            msgFields = new StackLayout()
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.LightGray
            };

            var pageHeight = ((Application.Current.MainPage as MasterDetailPage).Detail as NavigationPage).RootPage.Height;
            var entryHeight = Device.GetNamedSize(NamedSize.Default, typeof(Entry));

            var adresseeEntry = new Entry { BackgroundColor = Color.LightCoral, VerticalOptions = LayoutOptions.Start };
            msgFields.Children.Add(adresseeEntry);
            /*Constraint.Constant(0),
            Constraint.Constant(0),
            Constraint.RelativeToParent(p => p.Width),
            Constraint.Constant(16));
            // new Rectangle(0, 0, 1, 0.1), AbsoluteLayoutFlags.All);//*/

            var messageEditor = new Editor {
                BackgroundColor = Color.Blue,
                Margin = new Thickness(5),
                VerticalOptions = LayoutOptions.FillAndExpand };
            msgFields.Children.Add(messageEditor);
            /*
            Constraint.Constant(0),
            Constraint.Constant(0),
            Constraint.RelativeToParent(p => p.Width),
            Constraint.RelativeToView(container, (v, p) => v.Height * 0.8));
            //new Rectangle(0, 0.1, 1, 0.9), AbsoluteLayoutFlags.All);//*/

            container.Children.Add(msgFields);

            messageEditor.Focused += MessageEditor_Focused;
            messageEditor.Unfocused += MessageEditor_Focused;
            adresseeEntry.Focused += MessageEditor_Focused; 
            adresseeEntry.Unfocused += MessageEditor_Focused;


            // container.Children.Add(new StackLayout { VerticalOptions = LayoutOptions.CenterAndExpand });
            var send = new Button { Text = "Send" };
            bottom = new StackLayout { VerticalOptions = LayoutOptions.Center };
            bottom.Children.Add(send);
            send.Clicked += (object sender, EventArgs e) =>
            {
                
                DisplayAlert(
                    msgFields.Height.ToString(), 
                    container.Height.ToString(), 
                    Application.Current.MainPage.Height.ToString() + " : " + pageHeight.ToString(),
                    entryHeight.ToString() + ":" + msgFields.Children[0].Height + ":" + send.Height);
                //*/
            };

            container.Children.Add(bottom);
            
            Content = container;//*/

        }

        private void MessageEditor_Unfocused(object sender, FocusEventArgs e)
        {
            
        }

        private void MessageEditor_Focused(object sender, FocusEventArgs e)
        {
            /*
            if (e.IsFocused) {
                var pageHeight = ((Application.Current.MainPage as MasterDetailPage).Detail as NavigationPage).RootPage.Height;
                bottom.HeightRequest = pageHeight * 0.55;
            }            
            else
                bottom.HeightRequest = -1;//*/

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            /*
            var entryHeight = msgFields.Children.First().Height;
            var pageHeight = ((Application.Current.MainPage as MasterDetailPage).Detail as NavigationPage).RootPage.Height;
            var messageEditor = new Editor { BackgroundColor = Color.Blue, HeightRequest = pageHeight - entryHeight * 3 };
            // messageEditor.Focused += MessageEditor_Focused;
            msgFields.Children.Add(messageEditor);//*/
        }


    }
}