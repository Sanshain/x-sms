using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace XxmsApp.Options
{
    public delegate void CollectionChangedEventHandler(object sender, CollectionChangedEventArgs<Setting> e);

    public class CollectionChangedEventArgs<T> : EventArgs
    {
        public T ChangedItem { get; }
        public int Id { get; }

        public CollectionChangedEventArgs(T changedItem, int id)
        {
            ChangedItem = changedItem;
            Id = id;
        }

    }

    public class FullDescriptionAttribute : DescriptionAttribute
    {

        public string FullDescription { get; private set; }
        public FullDescriptionAttribute(string desc, string fulldesc = null) : base(desc)
        {
            FullDescription = fulldesc;
        }

    }


    public interface IAbstractOption
    {
        String ToString();
        IAbstractOption FromString(string s);
        IAbstractOption SetDefault();
    }
}
