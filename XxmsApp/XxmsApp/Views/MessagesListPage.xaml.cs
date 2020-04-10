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
	public partial class MessagesPage : ContentPage
	{


        int bottomHeight = 50;
        int scrollHeight;
        ListView messagesList;
        Button sender_button;
        Dialog dialog;

        public MessagesPage(object source)
        {
            var root = new RelativeLayout(){ };
            Content = root;

            root.Children.AddAsRelative(messagesList = new ListView
            {
                ItemTemplate = new DataTemplate(this.CellInitialize),
                HasUnevenRows = true,
                Margin = new Thickness(0, 0, 0, bottomHeight),
                SeparatorVisibility = SeparatorVisibility.None,
                VerticalOptions = LayoutOptions.FillAndExpand
                // BackgroundColor = Color.LightSkyBlue

            }, 0, 0, p => p.Width, p => p.Height);
            root.Children.AddAsRelative(BottomCreation(),
                p => 0,
                p => p.Height - bottomHeight,
                p => p.Width,
                p => bottomHeight);


            if (source.GetType() == typeof(Dialog))
            {
                dialog = source as Dialog;


                this.Title = "Сообщения c " + dialog.Address;

                /*
                var bind = new Binding()
                {
                    Mode = BindingMode.Default,
                    Source = dialog,
                    Path = "Messages"
                };

                messagesList.SetBinding(ListView.ItemsSourceProperty, bind); // "Messages" */

                messagesList.SetValue(ListView.ItemsSourceProperty, dialog.Messages);
                dialog.Messages.CollectionChanged += Messages_CollectionChanged;

                // messagesList.ItemsSource = dialog.Messages;
            }

        }

        async private void Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {                

                var msg = e.NewItems[0] as Message;



            }
        }

        private ViewCell CellInitialize()
        {

            var view = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Padding = new Thickness(15, 0, 0, 0)                
            };
            view.SizeChanged += View_SizeChanged;

            Label time = new Label { HorizontalOptions = LayoutOptions.Start };
            Label content = new Label
            {
                HorizontalOptions = LayoutOptions.StartAndExpand,
            };

            time.SetBinding(Label.TextProperty, "Time");
            content.SetBinding(Label.TextProperty, "Value");

            view.Children.Extend(time, content);

            var messageView = new Frame
            {
                Content = view,
                HasShadow = true,                
                OutlineColor = Color.Red,             // material design
                CornerRadius = 10,
                IsClippedToBounds = true             // border-radius
            };

            
            messageView.SetBinding(Frame.MarginProperty, "Incoming", BindingMode.OneWay, new BoolToMarginConverter());

            /* System.Linq.Expressions.Expression<Func<Message, object>> colored =
                m => m.IsValid.Value ? Color.Transparent : Color.LightBlue;
            messageView.SetBinding(Frame.BackgroundColorProperty, colored);//*/

            /* 
            System.Linq.Expressions.Expression<Func<Message, Thickness>> alignment = 
                m => (m as Message).Incoming ? new Thickness(10, 10, 45, 10) : new Thickness(10, 10, 45, 10);
            messageView.SetBinding(Frame.MarginProperty, alignment, BindingMode.OneWay);

            BackgroundColor = Color.LightCyan,
             */


            var viewCell = new ViewCell { View = messageView };

            return viewCell;
        }



        int count = 0;
        bool inited = false;
        double totalHeihght = 0;
        /// <summary>
        /// for press less dialogs to bottom
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void View_SizeChanged(object sender, EventArgs e)
        {
            if (inited) return;

            var item = (sender as StackLayout);
            
            var cellHeight = item.Height + item.Spacing * 2;
            totalHeihght += cellHeight;

            var cnt = (messagesList.ItemsSource as IEnumerable<Message>).Count();


            if (++count == cnt)
            {                
                inited = true;

                totalHeihght = totalHeihght + messagesList.Margin.Bottom * (count > 1 ? 2 : 1);

                var containerHeight = (messagesList.Parent as Layout).Height;
                if (totalHeihght + messagesList.Margin.Bottom *2 < containerHeight)
                {
                    messagesList.VerticalOptions = LayoutOptions.End;
                }

                messagesList.HeightRequest = totalHeihght;
                count = 0;
                totalHeihght = 0;
            }

            // sender_button.Text = count.ToString();
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();

            var scroll = messagesList;
            // ((this.Content as RelativeLayout).Children.First() as StackLayout).Children.First() as ListView;

            var msgs = scroll.ItemsSource as IEnumerable<Message>;

            if (msgs != null) scroll.ScrollTo(msgs.Last(), ScrollToPosition.End, false);            

            scrollHeight = (int)scroll.Height; // for kb height calculate

        }

        private StackLayout BottomCreation()
        {
            StackLayout bottom = new StackLayout
            {
                BackgroundColor = Color.LightGray,
                Orientation = StackOrientation.Horizontal
            };

            var mess_editor = new Entry
            {
                Placeholder = "Введите сообщение",
                Margin = new Thickness(10, 0, 0, 0),
                HorizontalOptions = LayoutOptions.FillAndExpand
            };


            var listView = messagesList;
            //  ((this.Content as RelativeLayout).Children.First() as StackLayout).Children.First() as ListView;

            
            listView.SizeChanged += (object sender, EventArgs e) =>
            {

                if (scrollHeight == 0 || (mess_editor.IsFocused == false && listView.Margin.Bottom == 0)) return;
                if (scrollHeight - (int)listView.Height > 0) { var kbHeight = scrollHeight - (int)listView.Height; }

                Device.StartTimer(TimeSpan.FromMilliseconds(200), () => {

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        var last = (listView.ItemsSource as IEnumerable<Message>).Last();
                        listView.ScrollTo(last, ScrollToPosition.End, false);
                    });

                    return false;
                });

            };


            sender_button = new Button
            {
                Text = "Send",
                HorizontalOptions = LayoutOptions.End
            };
            sender_button.Clicked += Sender_button_Clicked;

            bottom.Children.AddRange(new View[] { mess_editor,
                new Frame(){
                    Content = sender_button,
                    Margin = new Thickness(10),
                    Padding = new Thickness(-10),
                    CornerRadius = 10, IsClippedToBounds = true             // border-radius                    
                }
            });

            return bottom;
        }

        private void Sender_button_Clicked(object sender, EventArgs e)
        {
            var bottom = ((sender as Button).Parent.Parent as StackLayout);
            var editor = bottom.Children.OfType<Entry>().SingleOrDefault();

            if (!string.IsNullOrWhiteSpace(editor.Text))
            {
                // messagesList.VerticalOptions = LayoutOptions.FillAndExpand;

                if (messagesList.HeightRequest > 0)
                {
                    inited = false;
                    count = dialog.Messages.Count;
                    totalHeihght = messagesList.HeightRequest - messagesList.Margin.Bottom;
                }

                dialog.CreateMessage(dialog.Address, editor.Text);

                // messagesList.ItemsSource = null;
                // messagesList.ItemsSource = dialog.Messages;

                // messagesList.SetValue(ListView.ItemsSourceProperty, dialog.Messages);

                // DisplayAlert(dialog.Messages.GetType().Name, "type", "ok");

                editor.Text = "";
            }


        }
    }
}