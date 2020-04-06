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
            // InitializeComponent ();


            var root = new RelativeLayout()
            {
                
            };
            Content = root;

            messagesList = new ListView
            {
                ItemTemplate = new DataTemplate(this.CellInitialize),
                HasUnevenRows = true,
                Margin = new Thickness(0, 0, 0, bottomHeight),
                SeparatorVisibility = SeparatorVisibility.None,
                VerticalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.LightSkyBlue
            };

            

            if (source.GetType() == typeof(Dialog))
            {

                dialog = source as Dialog;

                this.Title = "Сообщения c " + dialog.Address;

                messagesList.ItemsSource = dialog.Messages.ToArray();

            }

            /*
            var msgsContainer = new Grid
            {
                BackgroundColor = Color.LightGreen,
                VerticalOptions = LayoutOptions.FillAndExpand
            };
            msgsContainer.RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition { Height = GridLength.Star },
                new RowDefinition { Height = GridLength.Auto }
            };
            msgsContainer.Children.Add(new BoxView
            {
                BackgroundColor = Color.Red,
                VerticalOptions = LayoutOptions.StartAndExpand
            }, 0, 0);
            msgsContainer.Children.Add(messagesList, 0, 1);//*/

            
            var messagesContainer = new StackLayout
            {
                BackgroundColor = Color.LightGreen,
                VerticalOptions = LayoutOptions.FillAndExpand

            }.AddChilds(messagesList, new BoxView {
                BackgroundColor = Color.Red,
                IsVisible = false,
                VerticalOptions = LayoutOptions.EndAndExpand
            });//*/
            

            root.Children.AddAsRelative(messagesList, 0, 0, p => p.Width, p => p.Height);
            root.Children.AddAsRelative(BottomCreation(),
                p => 0,
                p => p.Height - bottomHeight,
                p => p.Width,
                p => bottomHeight);
            

        }

        private ViewCell CellInitialize()
        {

            // var grid = new Grid

            var view = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Padding = new Thickness(15, 0, 0, 0),
                
            };
            view.SizeChanged += View_SizeChanged;

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

        int count = 0;
        bool inited = false;
        double totalHeihght = 0;
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

                /*
                var containerHeight = (messagesList.Parent as StackLayout).Height;
                messagesList.VerticalOptions = totalHeihght < containerHeight 
                    ? LayoutOptions.End 
                    : LayoutOptions.FillAndExpand;//*/

                messagesList.HeightRequest = totalHeihght;
                count = 0;
                totalHeihght = 0;
            }

            sender_button.Text = count.ToString();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            var scroll = messagesList;
            // ((this.Content as RelativeLayout).Children.First() as StackLayout).Children.First() as ListView;

            var msgs = (Message[])scroll.ItemsSource;

            scroll.ScrollTo(msgs.Last(), ScrollToPosition.End, false);            

            scrollHeight = (int)scroll.Height; // for kb height calculate

            /*
            scroll.Header = new BoxView
            {
                VerticalOptions = LayoutOptions.StartAndExpand,
                HeightRequest = scroll.Height
            };//*/

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


            var listView = messagesList;
            //  ((this.Content as RelativeLayout).Children.First() as StackLayout).Children.First() as ListView;

            
            listView.SizeChanged += (object sender, EventArgs e) =>
            {
                if (scrollHeight == 0)
                {
                    return;
                }

                if (mess_editor.IsFocused == false && listView.Margin.Bottom == 0)
                {
                    return;
                }

                var kbHeight = scrollHeight - (int)listView.Height;

                if (kbHeight > 0)
                {
                    

                    var el = dialog.Messages.First();
                    var el2 = dialog.Messages.Last();
                    listView.ScrollTo(el , ScrollToPosition.Start, false);
                }
                else
                {
                    
                }
               
                /*
                if (listView.Header != null && mess_editor.IsFocused)
                {
                    listView.Header = null;

                }
                else if (!mess_editor.IsFocused)
                {
                    listView.Header = new BoxView
                    {
                        VerticalOptions = LayoutOptions.StartAndExpand,
                        HeightRequest = listView.Height
                    };

                    listView.ScrollTo((listView.ItemsSource as IEnumerable<Message>).First(), ScrollToPosition.MakeVisible, false);
                }//*/

                




                /*
                if (listView.Margin.Bottom == bottomHeight && mess_editor.IsFocused)                          // scroll.Margin.Bottom == 0 && 
                {
                    listView.Margin = new Thickness(0, 0, 0, kbHeight);

                    listView.ScrollTo((listView.ItemsSource as IEnumerable<Message>).Last(), ScrollToPosition.End, false);
                }
                else if (mess_editor.IsFocused == false)
                {
                    // listView.Footer = null;

                    listView.Margin = new Thickness(0, 0, 0, bottomHeight);

                    listView.ScrollTo((listView.ItemsSource as IEnumerable<Message>).Last(), ScrollToPosition.End, false);
                }//*/


            };//*/


            sender_button = new Button
            {
                Text = "Send",
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