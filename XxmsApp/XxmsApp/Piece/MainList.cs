﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using Xamarin.Forms;
using XxmsApp.Model;



namespace XxmsApp.Piece
{

    public interface IMeasureString { double StringSize(string text); }


    public class DialogCell : ViewCell
    {
        public int TimeSize { get; set; } = 14;

        static NavPage navPage = ((App.Current.MainPage as MasterDetailPage).Detail as NavPage);
        Label PhoneLabel = null;
        Label TimeLabel = null;
        Label ValueLabel = null;
        Label CapacityLabel = null;
        Label SimLabel = null;
        BoxView SimView = null;
        Image StateImage = null;
        Frame StateFrame = null;
        public static bool hideSpam = Options.ModelSettings.HideSpam;

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

                // BackgroundColor = Color.Gray,
                // BorderColor = Color.Green
                // OutlineColor = Color.Red

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

            SetBindings();
        }


        Xamarin.Forms.MenuItem spamBtn, rmBtn;
        protected override void OnBindingContextChanged()
        {
            var bc = this.BindingContext;
            try
            {
                base.OnBindingContextChanged();
            }
            catch (Exception ex)
            {

            }

            if ((this.BindingContext as Dialog).IsSpam)
            {
                spamBtn.Text = "Восстановить";
            }
            spamBtn.CommandParameter = this.BindingContext;
            rmBtn.CommandParameter = this.BindingContext;

        }

        
        private void SetBindings()
        {
            PhoneLabel.SetBinding(Label.TextProperty, "Contact");       // Address
            TimeLabel.SetBinding(Label.TextProperty, "Time");
            ValueLabel.SetBinding(Label.TextProperty, "Label");
            CapacityLabel.SetBinding(Label.TextProperty, "Count");//*/
            SimLabel.SetBinding(Label.TextProperty, "Sim");//*/


            if (hideSpam == false)
            {
                var opacityConverter = new UniversalConverter(b =>
                {
                    if (b is bool isSpam && isSpam)
                    {
                        return 0.1f;
                    }
                    return 1;
                });

                
                PhoneLabel.SetBinding(Label.OpacityProperty, "IsSpam", BindingMode.OneWay, opacityConverter);
                ValueLabel.SetBinding(Label.OpacityProperty, "IsSpam", BindingMode.OneWay, opacityConverter);
                CapacityLabel.SetBinding(Label.OpacityProperty, "IsSpam", BindingMode.OneWay, opacityConverter);
                TimeLabel.SetBinding(Label.OpacityProperty, nameof(Dialog.IsSpam), BindingMode.OneWay, opacityConverter);
            }
            //*/

            // SimLabel.SetBinding(Label.TextColorProperty, "SimBackColor");//*/

            StateFrame.SetBinding(Frame.BackgroundColorProperty, "LastMsgState", BindingMode.OneWay, new MessageStateConverter());
            StateImage.SetBinding(Image.IsVisibleProperty, "LastIsOutGoing");
            // StateImage.SetBinding(Image.SourceProperty, "LastMsgState");



            spamBtn = new Xamarin.Forms.MenuItem { Text = "В спам", Command = new DialogCommander(d =>
            {
                d.IsSpam = !d.IsSpam;
                if (d.IsSpam)
                {
                    Api.Funcs.Toast("Вы можете скрыть сообщения, помеченные как спам, в настройках приложения, передернув рычаг");
                    // if (Options.ModelSettings.HideSpam) Options.ModelSettings.HideSpam = false;
                }

            })};

            rmBtn = new Xamarin.Forms.MenuItem()
            {
                Text = "Удалить",
                Command = new DialogCommander(async d =>
                {
                    if (await navPage.DisplayAlert( "Подтверждение", "Вы уверены, что хотите удалить весь диалог?","Да", "Нет"))
                    {
                        ((navPage.RootPage as MainPage).Dialogs.ItemsSource as IList<Dialog>).Remove(d);

                        // Cache.database.Execute("DELETE FROM Messages WHERE Address = ?", new string[] { d.Address });
                    }
                })
            };
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
        public MainList () : base(ListViewCachingStrategy.RecycleElement)        // RecycleElementAndDataTemplate
        {
            DataInitialize();

            HasUnevenRows = true;
            ItemTemplate = new DataTemplate(typeof(DialogCell));            // this.DataView();

            source.CollectionChanged += Source_CollectionChanged;
            this.ItemSelected += CustomList_ItemSelected;
            this.Scrolled += MainList_Scrolled;

        }        
        
        private void MainList_Scrolled(object sender, ScrolledEventArgs e)
        {                        
            if (e.ScrollY < 0)
            {

            }
        }


        internal void DataInitialize()
        {
            if (this.DialogViewType)                                        // by default
            {
                ItemsSource = ItemsUpdate();
            }
            else ItemsSource = this.DataLoad(30);                         // else   

            
            Options.ModelSettings.Instance.CollectionChanged += (s, e) =>
            {
                var setting = (s as Options.ModelSettings)[e.Id];
                if (setting.Name == nameof(Options.ModelSettings.HideSpam))
                {

                    // var source =  ItemsSource as ObservableCollection<Dialog>;


                    var r = this.DataLoad().GroupBy(m => m.Address).Select(g => new Dialog(g.Key)
                    {
                        Messages = new ObservableCollection<Message>(g.Reverse())
                    });

                    if (Options.ModelSettings.HideSpam)
                    {
                        r = r.Where(d => d.IsSpam == false).ToList();
                    }
                    
                    DialogCell.hideSpam = Options.ModelSettings.HideSpam;

                    ItemsSource = r;//*/                    
                }               
            };//*/
        }

        public ObservableCollection<Dialog> ItemsUpdate(bool no_cache = false)
        {
            
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            
            var msg = Cache.database.FindWithQuery<Model.Message>(
                "SELECT * FROM Messages WHERE _Number=(SELECT MAX(_Number) FROM Messages)"
            );// var l1 = sw.ElapsedMilliseconds;

            List<Message> msgs;

            if (msg is null) msgs = DependencyService.Get<Api.IMessages>().ReadAll();
            else
                msgs = DependencyService.Get<Api.IMessages>().ReadFrom(msg.Id);  // var l2 = sw.ElapsedMilliseconds;

            if (msgs.Count > 0)
            {
                Cache.database.InsertAll(msgs);                   // var l3 = sw.ElapsedMilliseconds;
                Cache.InsertAll(msgs);
            }
            
            var l = sw.ElapsedMilliseconds;

            var r = this.DataLoad(0, no_cache).GroupBy(m => m.Address).Select(g => new Dialog(g.Key)
            {                
                Messages = new ObservableCollection<Message>(g.Reverse())
            });

            if (Options.ModelSettings.HideSpam) r = r.Where(d => d.IsSpam == false);

            var f = sw.ElapsedMilliseconds;
            sw.Stop();

            
            return new ObservableCollection<Dialog>(r);
        }

        // static Dictionary<object, Views.MessagesPage> dialogCache = new Dictionary<object, Views.MessagesPage>();

        private async void CustomList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null) return;

            // var msgView = Views.MessagesPage.Create(e.SelectedItem);    // new Views.MessagesPage(e.SelectedItem);

            if (e.SelectedItem is Dialog dialog && dialog.Messages.Count == 0)
            {
                var navPage = ((App.Current.MainPage as MasterDetailPage).Detail as NavPage);
                navPage.DisplayAlert(
                    "Уведомление",
                    "В выбранном диалоге нет сообщений, удовлетворяющих вашему фильтру",
                    "Ok");

                    (sender as ListView).SelectedItem = null;
                return;
            }

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

        // [Obsolete("Срабатывает на изменение сообщений, а не диалогов. В принципе, работает. Но не востребовано")]
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