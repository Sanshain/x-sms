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

        public MessagesPage(object source)
        {
            // InitializeComponent ();


            var root = new RelativeLayout()
            {
                
            };
            Content = root;

            var messagesList = new ListView
            {
                ItemTemplate = new DataTemplate(this.CellInitialize),
                HasUnevenRows = true,
                Margin = new Thickness(0, 0, 0, bottomHeight)
            };



            if (source.GetType() == typeof(Dialog))
            {

                var dialog = source as Dialog;

                this.Title = "Сообщения c " + dialog.Address;

                messagesList.ItemsSource = dialog.Messages.ToArray();

            }

            root.Children.AddAsRelative(messagesList, 0, 0, p => p.Width, p => p.Height);
            root.Children.AddAsRelative(BottomCreation(),
                p => 0,
                p => p.Height - bottomHeight,
                p => p.Width,
                p => bottomHeight);
            

        }

        private ViewCell CellInitialize()
        {
            var view = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Padding = new Thickness(15, 0, 0, 0)
            };


            Label time = new Label { HorizontalOptions = LayoutOptions.Start };
            Label content = new Label
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
            };

            time.SetBinding(Label.TextProperty, "Time");
            content.SetBinding(Label.TextProperty, "Value");

            view.Children.Extend(time, content);

            var messageView = new Frame
            {
                Content = view,
                Margin = new Thickness(10),
                HasShadow = true,
                OutlineColor = Color.Red,             // material design
                CornerRadius = 10,
                IsClippedToBounds = true             // border-radius
            };

            var viewCell = new ViewCell { View = messageView };

            return viewCell;
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();

            var scroll = (this.Content as RelativeLayout).Children.First() as ListView;

            var msgs = (Message[])scroll.ItemsSource;

            scroll.ScrollTo(msgs.Last(), ScrollToPosition.End, false);            

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
                HorizontalOptions = LayoutOptions.FillAndExpand
            };


            var scroll = (this.Content as RelativeLayout).Children.First() as ListView;

            /*
            scroll.SizeChanged += (object sender, EventArgs e) =>
            {
                if (scrollHeight == 0 || (mess_editor.IsFocused == false && scroll.Margin.Bottom == 0)) return;

                var kbHeight = scrollHeight - (int)scroll.Height;

                if (scroll.Margin.Bottom == 0 && mess_editor.IsFocused)
                {
                    scroll.Margin = new Thickness(0, 0, 0, bottomHeight);                // kbHeight                    
                }
                else if (mess_editor.IsFocused == false) scroll.Margin = new Thickness(0);

                scroll.ScrollTo((scroll.ItemsSource as IEnumerable<Message>).Last(), ScrollToPosition.End, false);
            };//*/


            var sender_button = new Button
            {
                Text = ">",
                HorizontalOptions = LayoutOptions.End
            };

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

    }
}