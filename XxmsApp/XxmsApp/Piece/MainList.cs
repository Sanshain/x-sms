using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Xamarin.Forms;
using XxmsApp.Model;



namespace XxmsApp.Piece
{

    public interface IMeasureString { double StringSize(string text); }


    public class DialogCell : ViewCell
    {
        public int TimeSize { get; set; } = 14;

        Label PhoneLabel = null;
        Label TimeLabel = null;
        Label ValueLabel = null;
        Label CapacityLabel = null;
        Label SimLabel = null;
        BoxView SimView = null;
        Image StateImage = null;
        Frame StateFrame = null;

        public DialogCell()
        {
            

            double timeWidth = Utils.CalcString(DateTime.Now.ToString());

            var view = new RelativeLayout();
            var charLen = "a".GetWidth();

            PhoneLabel = new Label { FontSize = 18, FontAttributes = FontAttributes.Bold };
            TimeLabel = new Label { FontSize = TimeSize };  //, TextColor = Color.Gray
            ValueLabel = new Label { FontSize = 14, Margin = new Thickness(0, 10) };            
            CapacityLabel = new Label { FontSize = 12, TextColor = Color.LightSlateGray, Margin = new Thickness(0, 10) };
            SimLabel = new Label { FontSize = 12, TextColor = Color.Gray, Margin = new Thickness(2) };
            SimView = new BoxView { HeightRequest = 20, WidthRequest = 10};
            Frame simFrame = new Frame
            {
                CornerRadius = 3,
                IsClippedToBounds = true,
                Content = SimLabel,
                HeightRequest = 20,
                WidthRequest = 10,
                Padding = new Thickness(0),
                OutlineColor = Color.Red
                
            };
            StateImage = new Image
            {
                Source = ImageSource.FromFile("ok.png") as FileImageSource,
                HeightRequest = charLen * 2,
                WidthRequest = charLen * 2,
                Margin = new Thickness(0, 10),                
            };
            StateFrame = new Frame
            {
                HeightRequest = 10,
                WidthRequest = 10,
                CornerRadius = 5,
                HasShadow = false,
                Padding = new Thickness(0)
                // BackgroundColor = Color.Transparent
            };

            Func<RelativeLayout, double> StateFramePosition = delegate (RelativeLayout p)
            {
                var off = CapacityLabel.Text.Length * charLen;
                return p.Width - (off < 35 ? 50 : CapacityLabel.Text.Length * charLen + 15);
            };

            view.Children.Add(PhoneLabel, Constraint.Constant(20), Constraint.Constant(0));
            view.Children.Add(TimeLabel, Constraint.RelativeToParent((par) => par.Width - timeWidth - 10));
            view.Children.Add(ValueLabel, Constraint.Constant(25), Constraint.Constant(25),
                Constraint.RelativeToParent((par) => par.Width)
            );
            view.Children.AddAsRelative(CapacityLabel, p => p.Width - CapacityLabel.Text.Length  * charLen, p => 25);
            view.Children.AddAsRelative(simFrame, p => 5, p => 5);              // view.Children.AddAsRelative(SimLabel, p => p.Width - 60, p => 35);//*/
            // view.Children.AddAsRelative(StateImage, p => 5, p => 30);                   
            view.Children.AddAsRelative(StateFrame, StateFramePosition, p => 39);    

            View = view;       

        }


        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            
            PhoneLabel.SetBinding(Label.TextProperty, "Contact");       // Address
            TimeLabel.SetBinding(Label.TextProperty, "Time");
            ValueLabel.SetBinding(Label.TextProperty, "Label");
            CapacityLabel.SetBinding(Label.TextProperty, "Count");//*/
            SimLabel.SetBinding(Label.TextProperty, "Sim");//*/


            // SimLabel.SetBinding(Label.TextColorProperty, "SimBackColor");//*/

            StateFrame.SetBinding(Frame.BackgroundColorProperty, "LastMsgState", BindingMode.OneWay, new MessageStateConverter());
            StateImage.SetBinding(Image.IsVisibleProperty, "LastIsOutGoing");
            // StateImage.SetBinding(Image.SourceProperty, "LastMsgState");



            var spamBtn = new Xamarin.Forms.MenuItem()
            {
                Text = "В спам",
                CommandParameter = this.BindingContext,
                Command = new MessageCommander(async (d) => d.is
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
            var rmBtn = new Xamarin.Forms.MenuItem() { Text = "Удалить" };
            this.ContextActions.AddRange(spamBtn, rmBtn);

        }
    }

    public class MainList : ListView
	{
        public ObservableCollection<Message> source { get; set; } = new ObservableCollection<Message>();
        public int timeSize { get; set; } = 14;
        protected bool DialogViewType => Properties.Resources.DialogsView == "True";

        /// <summary>
        /// Для списка сообщений
        /// </summary>
        public MainList () : base(ListViewCachingStrategy.RecycleElementAndDataTemplate)
        {        

            DataInitialize();

            HasUnevenRows = true;
            ItemTemplate = new DataTemplate(typeof(DialogCell));            // this.DataView();

            this.ItemSelected += CustomList_ItemSelected;
            source.CollectionChanged += Source_CollectionChanged;

        }

        internal void DataInitialize()
        {
            if (this.DialogViewType)                                        // by default
            {
                ItemsSource = ItemsUpdate();
            }
            else ItemsSource = this.DataLoad(30);                         // else    
        }

        public List<Dialog> ItemsUpdate(bool no_cache = false)
        {

            var msg = Cache.database.FindWithQuery<Model.Message>(
                "SELECT * FROM Messages WHERE _Number=(SELECT MAX(_Number) FROM Messages)"
            );

            var msgs = DependencyService.Get<Api.IMessages>().ReadFrom(msg.Id);

            Cache.database.InsertAll(msgs);
            Cache.InsertAll(msgs);

            var r = this.DataLoad(0, no_cache).GroupBy(m => m.Address).Select(g => new Dialog
            {
                Address = g.Key,
                Messages = new ObservableCollection<Message>(g.Reverse())
            }).ToList();



            r.ForEach(d => d.PropertyChanged += (s, e) =>
            {
                Source_CollectionChanged(r,
                    new System.Collections.Specialized.NotifyCollectionChangedEventArgs(
                        System.Collections.Specialized.NotifyCollectionChangedAction.Replace,
                        d,
                        null,
                        d.Messages.Last().Id));
            });
        

            return r;
        }

        // static Dictionary<object, Views.MessagesPage> dialogCache = new Dictionary<object, Views.MessagesPage>();

        private async void CustomList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null) return;

            // var msgView = Views.MessagesPage.Create(e.SelectedItem);    // new Views.MessagesPage(e.SelectedItem);
            var msgView = new Views.MessagesPage(e.SelectedItem);

            await Navigation.PushAsync(msgView, false);

            /*
            if (this.DialogViewType)
            {
                await Navigation.PushAsync(new Views.Messages(e.SelectedItem), true);
            }
            else
            {
                await Navigation.PushAsync(new Views.Messages(e.SelectedItem), true);
            }//*/
           
            (sender as ListView).SelectedItem = null;
        }


        private void Source_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:

                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:

                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:

                    break;
            }
        }


        protected ObservableCollection<Message> DataLoad(int limit = 0, bool no_cache = false)
        {
            /*
            for (int i = 0; i < 20; i++)
            {
                source.Add(new Message()
                {
                    Value = "message " + i + 1,
                    Time = DateTime.Now,
                    Address = (8918 + i^3).ToString()
                });
            }//*/

            List<Message> msgs;

            if (no_cache)
            {
                msgs = Cache.UpdateAsync(new List<Model.Message>()).GetAwaiter().GetResult();
            }
            else msgs = Cache.Read<Message>().OrderByDescending(m => m.Id).ToList();

            limit = limit > 0 ? Math.Min(limit, msgs.Count) : msgs.Count;
            source = new ObservableCollection<Message>(msgs.GetRange(0, Math.Min(limit, msgs.Count)));

            return source;

        }


        [Obsolete("not cached")]
        protected DataTemplate DataView()
        {
            
            HasUnevenRows = true;                                       // Включает multiline для ItemTemplate

            double timeWidth = Utils.CalcString(DateTime.Now.ToString());

            return new DataTemplate(() =>
            {
                var view = new RelativeLayout();

                Label PhoneLabel = new Label { FontSize = 18, FontAttributes = FontAttributes.Bold };
                Label TimeLabel = new Label { FontSize = timeSize };  //, TextColor = Color.Gray
                Label ValueLabel = new Label { FontSize = 14, Margin = new Thickness(0, 10) };                

                PhoneLabel.SetBinding(Label.TextProperty, "Address");
                TimeLabel.SetBinding(Label.TextProperty, "Time");
                ValueLabel.SetBinding(Label.TextProperty, "Label");

                view.Children.Add(PhoneLabel, Constraint.Constant(10), Constraint.Constant(0));
                view.Children.Add(TimeLabel, 
                    Constraint.RelativeToParent((par) => par.Width - timeWidth - 10));
                view.Children.Add(ValueLabel, Constraint.Constant(10), Constraint.Constant(25),
                    Constraint.RelativeToParent((par) => par.Width)
                );

                var viewCell = new ViewCell { View = view };
                // viewCell.Tapped += ViewCell_Tapped;                

                return viewCell;
            });

        }

        /*
        private void ViewCell_Tapped(object sender, EventArgs e)
        {
            if (lastCell != null) lastCell.View.BackgroundColor = Color.Transparent;

            var viewCell = (ViewCell)sender;
            if (viewCell.View != null)
            {
                viewCell.View.BackgroundColor = Color.LightSteelBlue;
                lastCell = viewCell;
            }
        }//*/



    }
}