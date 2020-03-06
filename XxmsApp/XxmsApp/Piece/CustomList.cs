using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Xamarin.Forms;
using XxmsApp.Models;



namespace XxmsApp.Piece
{

    public interface IMeasureString { double StringSize(string text); }

    public class CustomList : ListView
	{
        public ObservableCollection<Message> source { get; set; } = new ObservableCollection<Message>();
        public int timeSize { get; set; } = 14;

        private ViewCell lastCell;

        public CustomList ()
		{

            ItemsSource = this.DataLoad();

            ItemTemplate = this.DataView();

            source.CollectionChanged += Source_CollectionChanged;


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

        protected ObservableCollection<Message> DataLoad()
        {   

            for (int i = 0; i < 20; i++)
            {
                source.Add(new Message()
                {
                    Value = "message " + i + 1,
                    Time = DateTime.Now,
                    Phone = 8918 + i^3
                });
            }

            return source;
        }

        protected DataTemplate DataView()
        {
            
            HasUnevenRows = true;                                       // Включает multiline для ItemTemplate

            double timeWidth = calculateWidth();

            return new DataTemplate(() =>
            {
                var view = new RelativeLayout();

                Label PhoneLabel = new Label { FontSize = 16 };
                Label TimeLabel = new Label { FontSize = timeSize };  //, TextColor = Color.Gray
                Label ValueLabel = new Label { FontSize = 14 , Margin = new Thickness(0, 10) };                

                PhoneLabel.SetBinding(Label.TextProperty, "Phone");
                TimeLabel.SetBinding(Label.TextProperty, "Time");
                ValueLabel.SetBinding(Label.TextProperty, "Value");
                                
                view.Children.Add(PhoneLabel, Constraint.Constant(10), Constraint.Constant(0));
                view.Children.Add(TimeLabel, 
                    Constraint.RelativeToParent((par) => par.Width - timeWidth - 10));
                view.Children.Add(ValueLabel, Constraint.Constant(10), Constraint.Constant(15),
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