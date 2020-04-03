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

            var messageViews = new StackLayout
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
                    Piece.MessageView msgView = new Piece.MessageView(msg);

                    messageViews.Children.Add(msgView);

                    if (++counter > limit) break;
                }                

            }

            RelativeLayout root = new RelativeLayout();
            var scroll = new ScrollView
            {
                Content = messageViews                
            };

            root.Children.Add(scroll, 
                Constraint.Constant(0), 
                Constraint.Constant(0), 
                Constraint.RelativeToParent((par) => 
                    {
                        return par.Width;
                    }),
                Constraint.RelativeToParent((par) => 
                    {
                        return par.Height;      
                    })
            );

            StackLayout bottom = new StackLayout
            {
                BackgroundColor = Color.LightGray,
                Orientation = StackOrientation.Horizontal
            };
            root.Children.AddAsRelative(bottom,
                par => 0,
                par => par.Height - 50,
                par => par.Width,
                par => 50
             );


            var mess_editor = new Entry
            {
                Placeholder = "Введите сообщение",
                HorizontalOptions = LayoutOptions.FillAndExpand
                // BackgroundColor = Color.Gray,
                // Opacity = 1
            };


            
            scroll.SizeChanged += (object sender, EventArgs e) =>
            {
                if (scrollHeight == 0) return;
                if (mess_editor.IsFocused == false && scroll.Margin.Bottom == 0) return;

                var kbHeight = scrollHeight - (int)scroll.Height;

                // (scroll.Content as StackLayout).Margin = new Thickness(0, 0, 0, kbHeight);
                // (scroll.Content as StackLayout).Children.Last().Margin = new Thickness(0, 0, 0, kbHeight);
                // (scroll.Parent as RelativeLayout).Padding = new Thickness(0, 0, 0, bottomHeight + kbHeight);

                if (scroll.Margin.Bottom == 0 && mess_editor.IsFocused)
                {
                    scroll.Margin = new Thickness(0, 0, 0, bottomHeight); // kbHeight                    
                }
                else if (mess_editor.IsFocused == false)
                {
                    scroll.Margin = new Thickness(0);
                }


                scroll.ScrollToAsync(scroll.Children.Last(), ScrollToPosition.End, false);
                // 
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

             Content = root;

        }

        private void MessageViews_SizeChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}