using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SQLite;
using XxmsApp.Model;
using System.Linq;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;

namespace XxmsApp.Model
{
    [Table("Settings")]
    public class Setting : INotifyPropertyChanged
    {
        bool  _value;

        [PrimaryKey]
        public string Prop { get; set; }
        public bool Value { get => _value; set 
            {
                _value = value;

                if (propertyChanged != null)
                {
                    Delegate[] delegates = propertyChanged?.GetInvocationList();

                    // propertyChanged?.Invoke(this, new PropertyChangedEventArgs(_value.ToString()));
                    propertyChanged(this, new PropertyChangedEventArgs(_value.ToString()));
                }

                // OnPropertyChanged(_value.ToString());
            }
        }
        /// <summary>
        /// Binding bool property
        /// </summary>
        // public bool Enabled => Convert.ToBoolean(Value);



        private event PropertyChangedEventHandler propertyChanged;


        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                propertyChanged += value;
            }
            remove
            {
                propertyChanged -= value;
            }
        }


        public static implicit operator Setting((string Name, string Value) setting)
        {
            return new Setting { Prop = setting.Name, Value = bool.Parse(setting.Value) };
        }


        public static implicit operator Setting((string Name, bool Value) setting)
        {
            return new Setting { Prop = setting.Name, Value = setting.Value };
        }




        /*
        [Obsolete]
        public static implicit operator Setting(KeyValuePair<string, string> setting) => 
            new Setting { Prop = setting.Key, Value = setting.Value };    
        [Obsolete]
        public static implicit operator Setting(string[] setting) => 
            new Setting { Prop = setting[0], Value = setting[1] };
       //*/
    }

}



namespace XxmsApp
{
    // no Cachable
    public class Settings : ObservableCollection<Setting> // , IEnumerable<Setting>
    {
        

        public Settings(IEnumerable<Setting> _items) : base(_items)
        {

            foreach (Setting setting in this)
            {
                setting.PropertyChanged += new PropertyChangedEventHandler(this.Setting1_PropertyChanged);
            }
        }
        // public Settings() { }
        // public Settings(IEnumerable<Setting> _items) => Units = new ObservableCollection<Setting>(_items);        
        // public ObservableCollection<Model.Setting> Units { get; set; } = new ObservableCollection<Setting>();


        public static Settings Initialize()
        {
            // var settings = new Settings { Units = new ObservableCollection<Setting>(Read()) };
            var settings = new Settings(Read());

            if (settings.Count() == 0)
            {
                settings = new Settings(new List<Setting>
                {
                    (Name : "Автофокус", Value : true),
                    (Name : "Вид диалога", Value : true)
                });

                Save(settings.ToArray());
            }

            var setting_chanched =
                settings[0].GetType().GetField("propertyChanged", BindingFlags.Instance | BindingFlags.NonPublic);
            var handler = (PropertyChangedEventHandler)setting_chanched.GetValue(settings[0]);
            Delegate[] delegates = handler?.GetInvocationList();

            return settings;
        }

        public override event NotifyCollectionChangedEventHandler CollectionChanged;
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            // base.OnCollectionChanged(e);

            if (CollectionChanged != null)
            {
                using (BlockReentrancy())
                {
                    CollectionChanged(this, e);
                }
            }

            // e.Action == NotifyCollectionChangedAction.
        }






        protected override event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangedEventHandler ValueChanged
        {
            add => PropertyChanged += value;
            remove => PropertyChanged -= value;
        }
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        { 
            if (PropertyChanged != null)
            {
                Delegate[] delegates = PropertyChanged?.GetInvocationList();

                PropertyChanged(this, e);
            }
        }







        internal void Setting1_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // var id = this.IndexOf(sender as Setting);


            
            
            var id = this.IndexOf(sender as Setting);
            this[id] = sender as Setting;

            /*
            this.OnCollectionChanged(
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace,
                sender, sender,
                this.IndexOf(sender as Setting))
            );

            // Units.CollectionChanged*/
        }



        static Setting[] Read() 
        {
            var s = Cache.database.Table<Setting>();
            var a = s.ToArray();
            return a;
        }

        public static int Save(Setting[] settings) {
            int cnt = 0;
            Cache.database.RunInTransaction(() =>
            {

                foreach (var setting in settings)
                {
                     cnt += Cache.database.InsertOrReplace(setting); 
                }
            });
            return cnt;
        }



        // public IEnumerator GetEnumerator() => Units.GetEnumerator();
        // IEnumerator<Setting> IEnumerable<Setting>.GetEnumerator() { return Units.GetEnumerator(); }

    }


}
