using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace XxmsApp.Piece
{
    

    public class CustomList : ListView
	{
        public ObservableCollection<string> source { get; set; } = new ObservableCollection<string>();

        public CustomList ()
		{

            ItemsSource = DataLoad();

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

        protected ObservableCollection<string> DataLoad()
        {
            for (int i = 0; i < 20; i++)
            {
                source.Add("item " + i);
            }

            return source;
        }
    }
}