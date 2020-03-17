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
        ListView drdnList;

        public CreatePage ()
        {
            // InitializeComponent ();

            InitializeComponents();

        }

        private void InitializeComponents()
        {
            
            msgFields = new StackLayout()
            {                                                                           // Initialize message Forms
                BackgroundColor = Color.LightGray,
                VerticalOptions = LayoutOptions.FillAndExpand
            };

            adresseeEntry = new Entry
            {
                //BackgroundColor = Color.LightCoral,
                VerticalOptions = LayoutOptions.Start,
                Placeholder = "Введите номер телефона",
                FontSize = Device.GetNamedSize(NamedSize.Default, typeof(Entry)),
                TextColor = Color.Green
            };
            adresseeEntry.TextChanged += AdresseeEntry_TextChanged;
            Frame adresseeFrame = new Frame
            {
                Margin = new Thickness(5),
                CornerRadius = 5,
                IsClippedToBounds = true,
                Padding = new Thickness(0, 0, 0, 0)
            };
            adresseeFrame.Content = adresseeEntry;
            msgFields.Children.Add(adresseeFrame);

            var main_contacts = (Application.Current as App)._contacts;
            // .GetRange(0, Math.Min((Application.Current as App)._contacts.Count, 15));
            drdnList = new ListView()
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                ItemsSource = main_contacts
            };
            drdnList.ItemTapped += DrdnList_ItemTapped;


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

            


            bottom = new StackLayout { VerticalOptions = LayoutOptions.Center };// Initialize Button for send

            var send = new Button { Text = "Send" };
            frameSend = new Frame
            {
                Content = send,
                CornerRadius = 10,
                Margin = new Thickness(5, 5, 5, 15),
                Padding = new Thickness(-10),
                IsClippedToBounds = true,
                BackgroundColor = Color.Red
            };
            bottom.Children.Add(frameSend);
            send.Clicked += Send_Clicked;



            messageEditor.Focused += MessageEditor_Focused;
            messageEditor.Unfocused += MessageEditor_Unfocused;
            adresseeEntry.Focused += MessageEditor_Focused;
            adresseeEntry.Unfocused += MessageEditor_Unfocused;

            // var contactsGetter = (Application.Current as App).contactsAwaiter;

            // contacts = contactsGetter.GetAwaiter().GetResult().ToList();

            var container = new StackLayout();
            container.Children.Add(msgFields);
            container.Children.Add(bottom);

            Content = container;                                                        // set container
        }

        [Obsolete("need remade to filter")]
        private void AdresseeEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((frameSend.Content as Button).Text.IndexOf('*') < 0)
                (frameSend.Content as Button).Text = "*";
            else
                (frameSend.Content as Button).Text += "*";
        }

        private void DrdnList_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e?.Item != null)
            {
                var contact = ((sender as ListView).SelectedItem as Model.Contacts);

                adresseeEntry.Text = contact.Phone + " (" + contact.Name + ")";

                msgFields.Children[1] = messageFrame;
                (messageFrame.Content as Editor).Focus();

                focused = true;

            }
        }

        private void MessageEditor_Unfocused(object sender, FocusEventArgs e)
        {
            /// ((sender as View).Parent as Frame).HasShadow = false;

           
            if (sender == adresseeEntry)
            {

                // DrdnList_ItemSelected(drdnList, null);
                // DisplayAlert(drdnList.IsFocused.ToString(), ".", ".").GetAwaiter();

                // msgFields.Children[1] = messageFrame;
                // (messageFrame.Content as Editor).Focus();

                // focused = true;
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
                // var stopwatch = new System.Diagnostics.Stopwatch();
                // stopwatch.Start();

                // msgFields.Children[1].IsVisible = false;

                msgFields.Children[1] = drdnList;
                focused = false;

                Action ReFocus = () =>
                {

                    adresseeEntry.Unfocus();

                    Task.Factory.StartNew<bool>(() => true).ContinueWith(r =>
                    {
                        adresseeEntry.Focus();

                    }, TaskScheduler.FromCurrentSynchronizationContext());//*/


                };
                ReFocus();


                // stopwatch.Stop();

                // ((bottom.Children[0] as Frame).Content as Button).Text = stopwatch.Elapsed.ToString();

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



        /// <summary>
        /// Send sms
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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