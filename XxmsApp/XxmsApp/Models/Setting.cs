using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SQLite;
using XxmsApp.Model;
using System.Linq;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace XxmsApp.Model
{
    [Table("Settings")]
    public class Setting : INotifyPropertyChanged
    {
        string  _value;

        [PrimaryKey]
        public string Prop { get; set; }
        public string Value { get => _value; set 
            {
                _value = value;
                OnPropertyChanged(_value);
            }
        }

        public bool Enabled => Convert.ToBoolean(Value);




        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }





        public static implicit operator Setting((string Name, string Value) setting)
        {
            return new Setting { Prop = setting.Name, Value = setting.Value };
        }


        public static implicit operator Setting((string Name, bool Value) setting)
        {
            return new Setting { Prop = setting.Name, Value = setting.Value.ToString() };
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
    public class Settings : IEnumerable<Setting>
    {
        

        public Settings() { }
        public Settings(IEnumerable<Setting> _items) => Units = new ObservableCollection<Setting>(_items);
        public ObservableCollection<Model.Setting> Units { get; set; } = new ObservableCollection<Setting>();


        public static Settings Initialize()
        {
            var settings = new Settings { Units = new ObservableCollection<Setting>(Read()) };

            if (settings.Count() == 0)
            {
                settings = new Settings(new List<Setting>
                {
                    (Name : "Автофокус", Value : true),
                    (Name : "Вид диалога", Value : true)
                });

                Save(settings.ToArray());
            }

            foreach(Setting setting in settings)
            {
                setting.PropertyChanged += Setting_PropertyChanged;
            }

            return settings;
        }



        private static void Setting_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Units.CollectionChanged

            // (sender as Setting)
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

        public IEnumerator GetEnumerator() => Units.GetEnumerator();
        IEnumerator<Setting> IEnumerable<Setting>.GetEnumerator() { return Units.GetEnumerator(); }

    }


}
