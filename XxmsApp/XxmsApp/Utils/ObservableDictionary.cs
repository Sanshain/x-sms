﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace XxmsApp
{
    public class ObservableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public ObservableDictionary() : base() { }
        public ObservableDictionary(int capacity) : base(capacity) { }
        public ObservableDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { }
        public ObservableDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }        
        public ObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer) { }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableDictionary<TKey, TValue> SetOnCollectionChangedEvent(NotifyCollectionChangedEventHandler action)
        {
            CollectionChanged = action;
            return this;
        }

        public new TValue this[TKey key]
        {
            get
            {                
                return base[key];
            }
            set
            {
                var newItem = new KeyValuePair<TKey, TValue>(key, value);
                var exists = base.TryGetValue(key, out TValue oldValue);
                base[key] = value;
                if (exists) this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Replace, newItem, oldValue, base.Keys.ToList().IndexOf(key)
                    ));                
                else
                {
                    
                    this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItem, base.Keys.ToList().IndexOf(key)));
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
                }
            }
        }

        public new void Add(TKey key, TValue value)
        {
            if (!base.ContainsKey(key))
            {
                var item = new KeyValuePair<TKey, TValue>(key, value);
                base.Add(key, value);
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, base.Keys.ToList().IndexOf(key)));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            }
        }

        public new bool Remove(TKey key)
        {
            TValue value;
            if (base.TryGetValue(key, out value))
            {
                var item = new KeyValuePair<TKey, TValue>(key, base[key]);
                bool result = base.Remove(key);
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, base.Keys.ToList().IndexOf(key)));
                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
                return result;
            }
            return false;
        }

        public new void Clear()
        {
            base.Clear();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}
