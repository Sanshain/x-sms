using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Xamarin.Forms;
using XxmsApp.Models;

namespace XxmsApp.Piece
{
    

    public class CustomList : ListView
	{
        public ObservableCollection<Message> source { get; set; } = new ObservableCollection<Message>();

        public CustomList ()
		{

            ItemsSource = DataLoad();

            ItemTemplate = new DataTemplate(() =>
            {                

                Label TimeLabel = new Label { FontSize = 18 };
                Label ValueLabel = new Label { FontSize = 16 };
                Label PhoneLabel = new Label { FontSize = 14 };

                TimeLabel.SetBinding(Label.TextProperty, "Time");
                ValueLabel.SetBinding(Label.TextProperty, "Value");
                PhoneLabel.SetBinding(Label.TextProperty, "Phone");

                
                return new ViewCell   // создаем объект ViewCell.
                {
                    View = new StackLayout
                    {
                        Padding = new Thickness(0, 5),
                        Orientation = StackOrientation.Vertical,
                        Children = { TimeLabel, ValueLabel, PhoneLabel }
                    }
                };
            });            

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
            
            // Включает multiline для ItemTemplate:
            HasUnevenRows = true;               

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
    }
}