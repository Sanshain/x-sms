﻿using System;
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

            view.Children.Add(PhoneLabel, Constraint.Constant(20), Constraint.Constant(0));
            view.Children.Add(TimeLabel,
                Constraint.RelativeToParent((par) => par.Width - timeWidth - 10));
            view.Children.Add(ValueLabel, Constraint.Constant(25), Constraint.Constant(25),
                Constraint.RelativeToParent((par) => par.Width)
            );

            view.Children.AddAsRelative(CapacityLabel, p => p.Width - CapacityLabel.Text.Length  * charLen - 5, p => 25);//*/            
            
            view.Children.AddAsRelative(simFrame, p => 5, p => 10);//*/
            // view.Children.AddAsRelative(SimLabel, p => p.Width - 60, p => 35);//*/
            view.Children.AddAsRelative(StateImage, p => 5, p => 25);                           //*/

            View = view;
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            
            PhoneLabel.SetBinding(Label.TextProperty, "Address");
            TimeLabel.SetBinding(Label.TextProperty, "Time");
            ValueLabel.SetBinding(Label.TextProperty, "Label");
            CapacityLabel.SetBinding(Label.TextProperty, "Count");//*/
            SimLabel.SetBinding(Label.TextProperty, "Sim");//*/
            // SimLabel.SetBinding(Label.TextColorProperty, "SimBackColor");//*/

            StateImage.SetBinding(Image.IsVisibleProperty, "LastIsIncoming");

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
            // this.DataLoad(30);            

            

            if (this.DialogViewType)                                        // by default
            {
                ItemsSource = this.DataLoad().GroupBy(m => m.Address).Select(g => new Dialog {
                    Address = g.Key,
                    Messages = new ObservableCollection<Message>(g.Reverse())
                } ).ToList();

            } else ItemsSource = this.DataLoad(30);                         // else    
                        

            HasUnevenRows = true;
            // ItemTemplate = this.DataView();                          // в DialogCell из-за вычисления отступа скорее всего тормоза
            ItemTemplate = new DataTemplate(typeof(DialogCell));            // this.DataView();

            this.ItemSelected += CustomList_ItemSelected;
            source.CollectionChanged += Source_CollectionChanged;

        }

        private async void CustomList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null) return;

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
            }
        }


        protected ObservableCollection<Message> DataLoad(int limit = 0)
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

            var msgs = Cache.Read<Message>().OrderByDescending(m => m.Id).ToList();

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