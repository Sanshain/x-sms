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
        Message[] msgs;
        int limit = 
            (Cache.Read<Options.Setting>().FirstOrDefault(s => s.Name == "LazyLoad")?.Content ?? false) ? 4 : 30;                                    //! for faster dcrolling on big gialogs
        int _msgsCount = 0; // log var. No production
        int counter = 1;

        int scrollHeight;
        int bottomHeight = 50;
        

        Stopwatch _time = new Stopwatch();

        protected override void OnAppearing()
        {
            base.OnAppearing();

            _time.Stop();

            _msgsCount = messages.Count();

            // DisplayAlert($"Time of loading {msgsCount} msgs", t.ElapsedMilliseconds.ToString(), "ok");

            var scroll = ((Content as RelativeLayout).Children.First() as ScrollView);

            scroll.ScrollToAsync(scroll.Children.Last(), ScrollToPosition.End, false);

            scrollHeight = (int)scroll.Height; // for kb height calculate

            var messageViews = scroll.Content as StackLayout;
            if (limit < 5) counter = UploadPanel(messageViews, counter);

        }

        public Messages (object obj)
        {
            // InitializeComponent ();            

            RelativeLayout root = new RelativeLayout();
            
            var scroll = root.Children.AddAsRelative(new ScrollView
                {
                    Content = FillData(obj, new StackLayout
                    {
                        Padding = new Thickness(0, 30, 0, bottomHeight)
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

                _time.Start();


                var dialog = obj as Dialog;
                messages = dialog.Messages;

                Title = "Сообщения c " + dialog.Address;

                //for (int i = 0; i < dialog.Messages.Count(); i++)
                // var limit = 30 < messages.Count() ? 30 : messages.Count() - 1;
                // .ToList().GetRange(0, limit)


                

                foreach (var msg in dialog.Messages)
                {
                    Piece.MessageView msgView = new Piece.MessageView(msg)
                    {
                        VerticalOptions = LayoutOptions.End
                    };

                    messageViews.Children.Add(msgView);

                    if (++counter > limit) break;
                }                

            }

            return messageViews;
        }

        private int UploadPanel(StackLayout messageViews, int counter)
        {
            {

                var uploadLabel = new Label
                {
                    VerticalOptions = LayoutOptions.End,
                    // HorizontalOptions = LayoutOptions.CenterAndExpand,
                    Margin = new Thickness(0, 10, 0, 0),
                    // MinimumHeightRequest = 30,
                    IsVisible = false,
                    Opacity = 0.7,
                    FontSize = 20,
                    TextColor = Color.LightGray,
                    Text = "Подгрузка..."
                };

                (messageViews.Parent.Parent as RelativeLayout).Children.AddAsRelative(
                    uploadLabel,
                    par => (par.Width - uploadLabel.Width) / 2,
                    par => 5);///*/

                messageViews.Children.Insert(0, new Label
                {
                    VerticalOptions = LayoutOptions.EndAndExpand
                });

                msgs = messages.ToArray();



                
                Action<double> upload_messages = (scrolly) =>
                {
                    /*
                    bool flagOpacity = true;                                    /// slow uploadLabel hidding
                    Device.StartTimer(TimeSpan.FromMilliseconds(25), () =>
                    {
                        if (uploadLabel.Opacity > 0.05) uploadLabel.Opacity -= 0.05;
                        else
                        {
                            flagOpacity = false;
                            uploadLabel.IsVisible = false;
                        }                        

                        return flagOpacity;
                    });//*/

                    Device.StartTimer(TimeSpan.FromMilliseconds(250), () =>
                    {

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            counter = UploadMessages(messageViews, counter);


                            var len = ((messageViews.Parent as ScrollView).Content as StackLayout).Children.Count;

                            (messageViews.Parent as ScrollView).ScrollToAsync(
                                ((messageViews.Parent as ScrollView).Content as StackLayout).Children[len - 2],
                                ScrollToPosition.MakeVisible, false);//*/
                        
                            // scrolly = ((messageViews.Parent as ScrollView).Content as StackLayout).Height - scrolly;
                            // (messageViews.Parent as ScrollView).ScrollToAsync(0, scrolly, false);

                            uploadLabel.IsVisible = false;
                            
                        });

                        return false;
                    });


                };

                (messageViews.Parent as ScrollView).Scrolled += (object sen, ScrolledEventArgs ev) =>
                {
                    // var scroll = sender as ScrollView;
                    // double scrollingSpace = scroll.ContentSize.Height - scroll.Height;


                    uploadLabel.IsVisible = ev.ScrollY <= 20;

                    if (ev.ScrollY <= 0)
                    {                        
                        upload_messages(ev.ScrollY);
                    }
                    
                };

                // uploadButton.Clicked += upload_messages;

            }

            return counter;
        }

        private int UploadMessages(StackLayout messageViews, int counter)
        {

            counter = Math.Min(msgs.Length, counter + 100);         // messages uploading

            for (int i = limit - 1; i < counter; i++)
            {
                Piece.MessageView msgView = new Piece.MessageView(msgs[i])
                {
                    VerticalOptions = LayoutOptions.End,
                    // IsVisible = false
                };

                messageViews.Children.Insert(1, msgView);
            }


            return counter;
        }


    }
}