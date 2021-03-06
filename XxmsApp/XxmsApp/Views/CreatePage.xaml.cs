﻿using System;
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
        Editor messageEditor;
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
                TextColor = Color.DarkGray
            };
            // adresseeEntry.SetBinding(Entry.TextProperty, "View");
            adresseeEntry.TextChanged += AdresseeEntry_TextChanged;
            adresseeEntry.Completed += AdresseeEntry_Completed;
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

            messageEditor = new Editor { };
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

            var send = new Button { Text = "Send", IsEnabled = false };
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

        private void AdresseeEntry_Completed(object sender, EventArgs e)
        {
            var list = drdnList.ItemsSource as List<Model.Contacts>;
            if (list.Count > 0)
            {
                // (sender as Entry).BindingContext = (drdnList.ItemsSource as List<Model.Contacts>).First();
                // (sender as Entry).SetBinding(Entry.TextProperty, "View");

                DrdnList_ItemTapped(drdnList, new ItemTappedEventArgs(list, drdnList.SelectedItem = list[0]));
            }
        }

        private void AdresseeEntry_TextChanged(object sender, TextChangedEventArgs e)
        {
            
            drdnList.ItemsSource = (Application.Current as App)._contacts.Where(c =>
            {
                return 
                    (c.Phone?.ToLower().Contains(e.NewTextValue.ToLower()) ?? false) || 
                    (c.Name?.ToLower().Contains(e.NewTextValue.ToLower()) ?? false);

            }).ToList();

            
            if ((drdnList.ItemsSource as List<Model.Contacts>).Count >= 1)
            {
                if ((drdnList.ItemsSource as List<Model.Contacts>).Count == 1)
                {
                    drdnList.SelectedItem = (drdnList.ItemsSource as List<Model.Contacts>).First();
                }                

                if (msgFields.Children[1] != drdnList) msgFields.Children[1] = drdnList;
            }
            else if ((drdnList.ItemsSource as List<Model.Contacts>).Count == 0)
            {
                // drdnList.IsVisible = false;

                if (adresseeEntry.IsFocused && msgFields.Children[1] != messageFrame)
                {
                    msgFields.Children[1] = messageFrame;
                }
                    
            }
            //*/

            if (!string.IsNullOrWhiteSpace((sender as Entry).Text)) (frameSend.Content as Button).IsEnabled = true;
            

        }

        private void DrdnList_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e?.Item != null)
            {
                var contact = ((sender as ListView).SelectedItem as Model.Contacts);

                adresseeEntry.Text = contact.Phone + " (" + contact.Name + ")";
                // adresseeEntry.BindingContext = contact;

                msgFields.Children[1] = messageFrame;
                if (Convert.ToBoolean(Properties.Resources.AutoFocus))
                {
                    (messageFrame.Content as Editor).Focus();
                }

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
            
            if (sender == adresseeEntry && focused)
            {
                // var stopwatch = new System.Diagnostics.Stopwatch();  stopwatch.Start();



                // msgFields.Children[1].IsVisible = false;
                drdnList.IsVisible = true;                            // for case if it bu hidden on AdresseeEntry_TextChanged

                msgFields.Children[1] = drdnList;
                
                focused = false;

                Action ReFocus = () =>
                {

                    adresseeEntry.Unfocus();

                    Task.Factory.StartNew<bool>(() => true).ContinueWith(r =>
                    {

                        adresseeEntry.Text = "";

                        adresseeEntry.Focus();

                    }, TaskScheduler.FromCurrentSynchronizationContext());


                };
                ReFocus();

                // stopwatch.Stop(); ((bottom.Children[0] as Frame).Content as Button).Text = stopwatch.Elapsed.ToString();

            }//*/

            
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
        async private void Send_Clicked(object sender, EventArgs e)
        {
            string text = messageEditor.Text, receiver = adresseeEntry.Text;

            if (Model.Message.Sims.Length > 1)
                Cards.SimChoice(bottom, async (index) => await SendMessage(text, receiver, Model.Message.Sims[index].SubId));
                
                // SimChoice(async (index) => await SendMessage(text, receiver, Model.Message.Sims[index].SubId));
            else            
                await SendMessage(text, receiver);
            
        }

        [Obsolete]
        private void SimChoice(Action<int> onChoice = null)
        {            
           
            var simPicker = new Picker {
                Title = "Sim",
                IsVisible = false,
                ItemsSource = Model.Message.Sims,                                   // ItemDisplayBinding = new Binding("Name")
            };

            simPicker.SelectedIndexChanged += (object s, EventArgs ev) => onChoice?.Invoke(simPicker.SelectedIndex);
            bottom.Children.Add(simPicker);
            simPicker.Focus();
            
        }

        private async Task SendMessage(string text, string receiver, int? sim = null)
        {
            var b = await DisplayAlert("Сообщение", "Вы уверены, что хотите отправить сообщение?", "Да", "Нет");

            if (b == false)
            {
                DisplayAlert("Info", "Отменен пользователем", "Ok");
                return;
            }
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(receiver))
                DisplayAlert("Info", "Не заполнены данные", "Ok");

            else
            {

                // var msgApi = DependencyService.Get<Api.IMessages>();
                // msgApi.Send(receiver, text, sim);

                // необходимо обновить данные в основном списке

                var dialogs = (((this.Parent as NavPage).RootPage as MainPage).Dialogs.ItemsSource as IList<Dialog>);
                var dialog = dialogs.FirstOrDefault(d => d.Address == receiver);
                if (dialog != null)
                {                    
                    // dialog.Messages.Add(new Model.Message(receiver, text, sim));
                    dialog.CreateMessage(receiver, text, sim);
                }
                else
                {
                    dialog = new Dialog(receiver);
                    dialog.CreateMessage(receiver, text, sim);
                    dialogs.Add(dialog);
                }

                /*.source.Add(
                    new Model.Message(receiver, text, sim)
                );//*/

                await DisplayAlert("Info", "Сообщение отправлено", "Ok");

                messageEditor.Text = "";

                Navigation.PopAsync(true);

                // Navigation.RemovePage(this);
                // Navigation.PushAsync(new MessagesPage(Dialog.GetOrCreate(receiver, new Model.Message(receiver, text, sim))));
            }
        }

    }
}