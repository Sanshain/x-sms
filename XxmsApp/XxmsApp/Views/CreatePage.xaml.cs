using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Plugin.ContactService.Shared;

namespace XxmsApp.Views
{
    public class CustomEditor : Entry
    {

    }



	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CreatePage : ContentPage
	{
        StackLayout msgFields;
        StackLayout bottom;
        Entry adresseeEntry;
        Frame messageFrame;
        Frame frameSend;

        public CreatePage ()
		{
            
			InitializeComponent ();

            var container = new StackLayout();            
            msgFields = new StackLayout()
            {                                                                           // Initialize message Forms
                BackgroundColor = Color.LightGray,
                VerticalOptions = LayoutOptions.FillAndExpand                
            };

            adresseeEntry = new Entry {
                //BackgroundColor = Color.LightCoral,
                VerticalOptions = LayoutOptions.Start,
                Placeholder = "Введите номер телефона"
            };
            Frame adresseeFrame = new Frame {
                Margin = new Thickness(5),
                CornerRadius =5,
                IsClippedToBounds = true,
                Padding = new Thickness(0,0,0,0)
            };
            adresseeFrame.Content = adresseeEntry;
            msgFields.Children.Add(adresseeFrame);


            var messageEditor = new Editor { };
            messageFrame = new Frame
            {
                Margin = new Thickness(5),
                CornerRadius = 5,
                IsClippedToBounds = true,
                Padding = new Thickness(0, 0, 0, -10),
                VerticalOptions = LayoutOptions.FillAndExpand
            };
            messageFrame.Content = messageEditor;
            msgFields.Children.Add(messageFrame);

            container.Children.Add(msgFields);

            
            bottom = new StackLayout { VerticalOptions = LayoutOptions.Center };// Initialize Button for send
                        
            var send = new Button { Text = "Send" };
            frameSend = new Frame {
                Content = send,
                CornerRadius = 10,
                Margin = new Thickness(5,5,5,15),
                Padding = new Thickness(-10),                
                IsClippedToBounds = true,
                BackgroundColor = Color.Red
            };
            bottom.Children.Add(frameSend);
            send.Clicked += Send_Clicked;
            container.Children.Add(bottom);

            
            Content = container;                                                        // set container

            
            messageEditor.Focused += MessageEditor_Focused;
            messageEditor.Unfocused += MessageEditor_Unfocused;
            adresseeEntry.Focused += MessageEditor_Focused;
            adresseeEntry.Unfocused += MessageEditor_Unfocused;

            // var contactsGetter = (Application.Current as App).contactsAwaiter;

            // contacts = contactsGetter.GetAwaiter().GetResult().ToList();

        }


        private void MessageEditor_Unfocused(object sender, FocusEventArgs e)
        {
            /// ((sender as View).Parent as Frame).HasShadow = false;

            

            if (sender == adresseeEntry)
            {
                msgFields.Children[1] = messageFrame;
                (messageFrame.Content as Editor).Focus();

                focused = true;
            }

            if (Device.RuntimePlatform == Device.iOS)  bottom.HeightRequest = -1;
        }

        bool focused = true;
        private void MessageEditor_Focused(object sender, FocusEventArgs e)
        {
            /*
            ((sender as View).Parent as Frame).HasShadow = true;            
            ((sender as View).Parent as Frame).OutlineColor = Color.DarkRed;//*/
            if (sender == adresseeEntry && focused)
            {
                var stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();

                var main_contacts = (Application.Current as App).contacts.GetRange(0, 5);

                // msgFields.Children[1].IsVisible = false;
                msgFields.Children[1] = (new ListView()
                {
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    ItemsSource = main_contacts
                });
                focused = false;
                adresseeEntry.Focus();

                stopwatch.Stop();

                ((bottom.Children[0] as Frame).Content as Button).Text = stopwatch.Elapsed.ToString();

            }

            if (Device.RuntimePlatform == Device.iOS)                               /// if its not Android, set `Focused`
            {
                Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                {

                    var pageHeight = ((Application.Current.MainPage as MasterDetailPage).Detail as NavigationPage)
                        .RootPage.Height;
                    bottom.HeightRequest = pageHeight * 0.55; // nearly

                    return false;
                });    
            }
            

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            adresseeEntry.Focus();

            (frameSend.Content as Button).HeightRequest = (frameSend.Content as View).Height + 15;
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