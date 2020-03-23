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
    

    public class CustomList : ListView
	{
        public ObservableCollection<Message> source { get; set; } = new ObservableCollection<Message>();
        public int timeSize { get; set; } = 14;
        protected bool DialogViewType => Properties.Resources.DialogsView == "True";

        public CustomList ()
		{
            // this.DataLoad(30);

            if (this.DialogViewType)
            {

                ItemsSource = this.DataLoad().GroupBy(m => m.Address).Select(g => new Dialog {
                    Address = g.Key,
                    Messages = g.ToArray()
                } );

            } else ItemsSource = this.DataLoad(30);

            ItemTemplate = this.DataView();

            this.ItemSelected += CustomList_ItemSelected;
            source.CollectionChanged += Source_CollectionChanged;

        }

        private async void CustomList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null) return;

            if (this.DialogViewType)
            {
                await Navigation.PushAsync(new Views.Messages(e.SelectedItem), true);
            }
            else
            {
                await Navigation.PushAsync(new Views.Messages(e.SelectedItem), true);
            }
           
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

        protected DataTemplate DataView()
        {
            
            HasUnevenRows = true;                                       // Включает multiline для ItemTemplate

            double timeWidth = calculateWidth();

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

        protected double calculateWidth()
        {

            var service = DependencyService.Get<IMeasureString>();
            var timeWidth = service.StringSize(DateTime.Now.ToString());

            return timeWidth;
        }

    }
}