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

            var container = new StackLayout();            
            msgFields = new StackLayout(){                                              // Initialize message Forms
                VerticalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.LightGray};

            var adresseeEntry = new Entry {
                BackgroundColor = Color.LightCoral,
                VerticalOptions = LayoutOptions.Start };
            msgFields.Children.Add(adresseeEntry);

            var messageEditor = new Editor {
                BackgroundColor = Color.Blue,
                Margin = new Thickness(5),
                VerticalOptions = LayoutOptions.FillAndExpand };
            msgFields.Children.Add(messageEditor);

            container.Children.Add(msgFields);

            
            bottom = new StackLayout { VerticalOptions = LayoutOptions.Center };        // Initialize Button for send
            var send = new Button { Text = "Send" };
            bottom.Children.Add(send);
            send.Clicked += Send_Clicked;
            container.Children.Add(bottom);

            
            Content = container;                                                        // set container


            /// if its not Android, set `Focused`
            if (Device.RuntimePlatform == Device.iOS)  
            {
                messageEditor.Focused += MessageEditor_Focused;
                messageEditor.Unfocused += MessageEditor_Unfocused;
                adresseeEntry.Focused += MessageEditor_Focused;
                adresseeEntry.Unfocused += MessageEditor_Unfocused;
            }

        }


        private void MessageEditor_Unfocused(object sender, FocusEventArgs e)
        {
            bottom.HeightRequest = -1;
        }

        private void MessageEditor_Focused(object sender, FocusEventArgs e)
        {

            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {

                var pageHeight = ((Application.Current.MainPage as MasterDetailPage).Detail as NavigationPage).RootPage.Height;
                bottom.HeightRequest = pageHeight * 0.55; // nearly

                return false;
            });                

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }




        [Obsolete("This yet just gag. Need to realize")]
        private void Send_Clicked(object sender, EventArgs e)
        {
            var entryHeight = Device.GetNamedSize(NamedSize.Default, typeof(Entry));

            var pageHeight = ((Application.Current.MainPage as MasterDetailPage).Detail as NavigationPage).RootPage.Height;

            DisplayAlert(
                msgFields.Height.ToString(),
                 ((sender as Button).Parent.Parent as StackLayout).Height.ToString(),
                Application.Current.MainPage.Height.ToString() + " : " + pageHeight.ToString(),
                entryHeight.ToString() + ":" + msgFields.Children[0].Height + ":" + (sender as Button).Height);
        }

    }
}