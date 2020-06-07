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
    public class MessageFrame : Frame
    {

    }


    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MessagesPage : ContentPage
	{

        double vCellPadding;
        int bottomHeight = 50;
        int scrollHeight;
        ListView messagesList;
        Button sender_button;
        internal Dialog RootDialog { private set; get; }

        static MessagesPage instance = null;
        [Obsolete]
        public static MessagesPage Create(object source)
        {
            if (instance == null) instance = new MessagesPage(source);
            else
            {
                instance.RootDialog = source as Dialog;

                instance.SourceInit(source);

                MsgSearchPanel.Initialize(instance);       
            }
            return instance;
        }


        public MessagesPage(object source)
        {
            var root = new RelativeLayout() { };
            Content = root;
            RootDialog = source as Dialog;

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

            SourceInit(source);

            MsgSearchPanel.Initialize(this);


        }

        private bool init = false;
        static HashSet<System.Collections.ObjectModel.ObservableCollection<Message>> cache = 
            new HashSet<System.Collections.ObjectModel.ObservableCollection<Message>>();

        private void SourceInit(object source)
        {
            if (source.GetType() == typeof(Dialog))
            {

                this.Title = "Сообщения c " + RootDialog.Contact;               // dialog.Address
                /* messagesList.SetBinding(ListView.ItemsSourceProperty, new Binding()
                {
                    Mode = BindingMode.Default,
                    Source = dialog,
                    Path = "Messages"
                }); // "Messages" */

                messagesList.SetValue(ListView.ItemsSourceProperty, RootDialog.Messages);

                
                // if (init == false)
                if (cache.Add(RootDialog.Messages))
                {
                    RootDialog.Messages.CollectionChanged += Messages_CollectionChanged;            // init = true;                    
                }
                // messagesList.ItemsSource = dialog.Messages;
                if (RootDialog.LastMsgState == MessageState.Unread) RootDialog.SetAsRead();
            }
        }

        private void Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                
            }
        }

        private View PreviousView;
        private ViewCell CellInitialize()
        {

            var view = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Padding = new Thickness(15, 0, 0, 0)
            };
            view.SizeChanged += View_SizeChanged;

            Label time = new Label { HorizontalOptions = LayoutOptions.Start, Opacity = 0.5 };
            Label content = new Label { HorizontalOptions = LayoutOptions.StartAndExpand };
            Label status = new Label() { HorizontalOptions = LayoutOptions.End, TextColor = Color.LightSlateGray };

            time.SetBinding(Label.TextProperty, "Time", BindingMode.OneWay, TimeConverter.Instance);
            content.SetBinding(Label.TextProperty, "Value");
            status.SetBinding(Label.TextProperty, "SimName");                                       // States    
            view.Children.Extend(time, content, status);

            var messageView = new Frame
            {
                Content = view,                
                HasShadow = true,                
                OutlineColor = Color.Red,             // material design
                CornerRadius = 10,
                IsClippedToBounds = true             // border-radius
            };

            vCellPadding = messageView.Padding.Bottom + messageView.Padding.Top;

            messageView.SetBinding(Frame.MarginProperty, "Incoming", BindingMode.OneWay, IncomingConverter.Single);
            messageView.SetBinding(Frame.BackgroundColorProperty, "Incoming", BindingMode.OneWay, IncomingConverter.Single);
            // messageView.SetBinding(Label.TextProperty, "Selected");

            var viewCell = new ViewCell { View = messageView };

            var copyContext = new MenuItem() { Text = "Копировать" };
            var removeContext = new MenuItem
            {
                Text = "Удалить",
                CommandParameter = content.BindingContext,
                // Command = dialog.RemoveCommand,                
                Command = new MessageCommander(async (mess) =>
                {
                    if (await DisplayAlert("Подтверждение", "Вы уверены, что хотите удалить сообщение?", "Да", "Нет"))
                    {
                        // Cache.database.Delete<Message>(mess.Id);
                        if (mess is Message)
                        {                            
                            RootDialog.RemoveCommand.Execute(mess);             // dialog.Messages.Remove(mess);
                        }

                    }
                })//*/
            };


            copyContext.Clicked += (object sender, EventArgs e) =>
            {
                if (string.IsNullOrWhiteSpace(content.Text) == false) DependencyService.Get<Api.ILowLevelApi>().Copy(content.Text);
            };                        
            viewCell.ContextActions.Add(copyContext);

            removeContext.SetBinding(MenuItem.CommandParameterProperty, ".");
            viewCell.ContextActions.Add(removeContext);

            

            
            viewCell.Tapped += (object sender, EventArgs e) =>
            {
                
                if (messagesList.SelectedItem == content.BindingContext)
                {
                    var message = content.BindingContext as Message;
                    DisplayAlert("Инфо", "", "Ok");
                    messagesList.SelectedItem = null;
                }
                
            };//*/

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


            var cellHeight = item.Height + vCellPadding + item.Spacing * 4;
            totalHeihght += cellHeight;

            var cnt = (messagesList.ItemsSource as IEnumerable<Message>).Count();

            if (++count == cnt)
            {                
                inited = true;

                if (totalHeihght < messagesList.Height)
                {
                    messagesList.VerticalOptions = LayoutOptions.End;
                    messagesList.HeightRequest = totalHeihght;
                }

                totalHeihght = count = 0;
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
            mess_editor.Focused += (s, e) =>
            {
                var appApi = DependencyService.Get<Api.ILowLevelApi>();
                if(appApi.IsDefault == false)
                {
                    appApi.ChangeDefault();
                }
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
                    count = RootDialog.Messages.Count;
                    totalHeihght = messagesList.HeightRequest - messagesList.Margin.Bottom;
                }

                string message = editor.Text;                

                
                Cards.SimChoice(bottom, (index) =>
                {                    
                    RootDialog.CreateMessage(RootDialog.Address, message, Message.Sims[index].SubId);
                });//*/
                

                // messagesList.ItemsSource = null;
                // messagesList.ItemsSource = dialog.Messages;

                // messagesList.SetValue(ListView.ItemsSourceProperty, dialog.Messages);

                // DisplayAlert(dialog.Messages.GetType().Name, "type", "ok");

                editor.Text = "";
            }


        }
    }
}