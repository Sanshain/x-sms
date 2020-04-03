using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        IEnumerable<Model.Message> messages = null;
        int limit = 
            (Cache.Read<Setting>().FirstOrDefault(s => s.Prop == "LazyLoad")?.Value ?? false) ? 3 : 30;                                    //! for faster dcrolling on big gialogs
        int msgsCount = 0; // log var. No production

        int scrollHeight;
        int bottomHeight = 50;

        Stopwatch t = new Stopwatch();

        protected override void OnAppearing()
        {
            base.OnAppearing();

            t.Stop();

            msgsCount = messages.Count();

            // DisplayAlert($"Time of loading {msgsCount} msgs", t.ElapsedMilliseconds.ToString(), "ok");

            var scroll = ((Content as RelativeLayout).Children.First() as ScrollView);

            scroll.ScrollToAsync(scroll.Children.Last(), ScrollToPosition.End, false);

            scrollHeight = (int)scroll.Height;

        }

        public Messages (object obj)
        {
            // InitializeComponent ();            

            RelativeLayout root = new RelativeLayout();
            
            var scroll = root.Children.AddAsRelative(new ScrollView
                {
                    Content = FillData(obj, new StackLayout
                    {
                        Padding = new Thickness(0, 0, 0, bottomHeight)
                    })
                },
                0, 0, par => par.Width, par => par.Height
            ) as ScrollView;            


            root.Children.AddAsRelative(BottomCreation(scroll),
                par => 0,
                par => par.Height - bottomHeight,
                par => par.Width,
                par => bottomHeight
             );

            Content = root;

        }



        private StackLayout BottomCreation(ScrollView scroll)
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
                // BackgroundColor = Color.Gray,
                // Opacity = 1
            };



            scroll.SizeChanged += (object sender, EventArgs e) =>
            {
                if (scrollHeight == 0 || (mess_editor.IsFocused == false && scroll.Margin.Bottom == 0)) return;

                var kbHeight = scrollHeight - (int)scroll.Height;

                if (scroll.Margin.Bottom == 0 && mess_editor.IsFocused)
                {
                    scroll.Margin = new Thickness(0, 0, 0, bottomHeight);                // kbHeight                    
                }
                else if (mess_editor.IsFocused == false) scroll.Margin = new Thickness(0);

                scroll.ScrollToAsync(scroll.Children.Last(), ScrollToPosition.End, false);

            };


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

        private StackLayout FillData(object obj, StackLayout messageViews = null)
        {

            messageViews = messageViews ?? new StackLayout
            {
                Padding = new Thickness(0, 0, 0, bottomHeight)
            };

            if (obj.GetType() == typeof(Dialog))
            {

                t.Start();


                var dialog = obj as Dialog;
                messages = dialog.Messages;

                Title = "Сообщения c " + dialog.Address;

                //for (int i = 0; i < dialog.Messages.Count(); i++)
                // var limit = 30 < messages.Count() ? 30 : messages.Count() - 1;
                // .ToList().GetRange(0, limit)


                var counter = 1;

                foreach (var msg in dialog.Messages)
                {
                    Piece.MessageView msgView = new Piece.MessageView(msg)
                    {
                        VerticalOptions = LayoutOptions.End
                    };

                    messageViews.Children.Add(msgView);

                    if (++counter > limit) break;
                }

                if (limit == 3)
                {
                    
                    var waitingLabel = new Label
                    {
                        VerticalOptions = LayoutOptions.End,
                        HorizontalOptions = LayoutOptions.Center,
                        Margin = new Thickness(0, 10),
                        FontSize = 20,
                        TextColor = Color.LightGray,
                        Text = "Подгрузка..."
                    };

                    messageViews.Children.Insert(0, waitingLabel);

                    var msgs = messages.ToArray();

                    Device.StartTimer(TimeSpan.FromMilliseconds(250), () =>
                    {
                       

                            messageViews.Children.Remove(waitingLabel);             // remove label
                        
                            counter = Math.Min(counter + msgs.Length, 100);

                            for (int i = limit-1; i < counter; i++)
                            {
                                Piece.MessageView msgView = new Piece.MessageView(msgs[i])
                                {
                                    VerticalOptions = LayoutOptions.End
                                };

                                messageViews.Children.Insert(0, msgView);
                            }

                            messageViews.Children.Insert(0, new Button
                            {
                                Text = "Еще"
                            });


                            (messageViews.Parent as ScrollView).ScrollToAsync(
                                (messageViews.Parent as ScrollView).Children.Last(), 
                                ScrollToPosition.End, false);

                            // here event: if scroll reached to end, append new 100




                        return false;
                    });

                }

            }

            return messageViews;
        }

        private void MessageViews_SizeChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}