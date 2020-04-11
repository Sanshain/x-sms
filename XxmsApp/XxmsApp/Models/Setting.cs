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
    public class Setting : INotifyPropertyChanged // , IModel   
    {
        bool  _value;

        [PrimaryKey]
        public string Name { get; set; }
        public string Description { get; set; }
        public string FullDescription { get; set; }
        public bool Value { get => _value; set 
            {
                _value = value;

                if (propertyChanged != null)
                {
                    Delegate[] delegates = propertyChanged?.GetInvocationList();

                    // propertyChanged?.Invoke(this, new PropertyChangedEventArgs(_value.ToString()));
                    propertyChanged(this, new PropertyChangedEventArgs(_value.ToString()));
                }

            }
        }

        


        private event PropertyChangedEventHandler propertyChanged;
        public event PropertyChangedEventHandler PropertyChanged
        {
            add => propertyChanged += value;
            remove => propertyChanged -= value;
        }





        public static implicit operator Setting((string Name, string Value, string Desc) setting)
        {
            return new Setting {
                Name = setting.Name,
                Value = bool.Parse(setting.Value),
                Description = setting.Desc
            };
        }

        public static implicit operator Setting((string Name, bool Value, string Desc) setting) //
        {
            return new Setting { Name = setting.Name, Value = setting.Value, Description = setting.Desc }; // 
        }




        /*
        #region IModel realization

        public bool IsActual => true;
        public IModel CreateAs(object obj) => throw new NotImplementedException("It's compile time initialized class");

        #endregion

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

    // no Cachable
    public class Settings : List<Setting>//, INotifyCollectionChanged // , IEnumerable<Setting>
    {


        int _initialized = 0;

        public Settings(IEnumerable<Setting> _items) : base(_items) { }
        // public Settings() { }
        // public Settings(IEnumerable<Setting> _items) => Units = new ObservableCollection<Setting>(_items);        
        // public ObservableCollection<Model.Setting> Units { get; set; } = new ObservableCollection<Setting>();
        


        
        
        static Dictionary<string, bool> sttngs = new Dictionary<string, bool>();
        [Description("Краткое описание, полное описание")]
        public static bool AutoFocus
        {
            get
            {
                var name = System.Reflection.MethodBase.GetCurrentMethod().Name.Substring(4);
                if (sttngs.ContainsKey(name)) return sttngs[name];
                else
                {
                    return sttngs[name] = false;
                }
            }
            set
            {
                var name = System.Reflection.MethodBase.GetCurrentMethod().Name.Substring(4);
                if (sttngs.ContainsKey(name)) sttngs[name] = value;
                else
                {
                    sttngs.Add(name, value);
                }
            }
        }

        [Description("Краткое описание, полное описание")]
        public static bool AutoFocus2
        {
            get
            {
                var name = System.Reflection.MethodBase.GetCurrentMethod().Name.Substring(4);
                return Cache.Read<Setting>().FirstOrDefault(s => s.Name == name)?.Value ?? false;
            }
            set
            {
                var name = System.Reflection.MethodBase.GetCurrentMethod().Name.Substring(4);
                Cache.Read<Setting>().SingleOrDefault(s => s.Name == name).Value = value;
            }
        }

        private static List<Setting> ConvertToList()
        {
            var props = typeof(Settings).GetProperties(BindingFlags.Static | BindingFlags.Public);

            var stgs = props.Select(p =>
            {
                return new Setting
                {
                    Name = p.Name,
                    Description = p.GetCustomAttribute<DescriptionAttribute>().Description,
                    Value = (bool)p.GetValue(null)
                };
            }).ToList();

            foreach (var prop in props)
            {
                var desc = prop.GetCustomAttribute<DescriptionAttribute>().Description;
            }

            return stgs;
        }

        private static void ReadToDict()
        {
            sttngs = Cache.Read<Setting>().ToDictionary(s => s.Name, s => s.Value);
        }
        //*/

        public static Settings Initialize()
        {
            // PropertiesToList();


            // ReadToDict();


            // var settings = new Settings { Units = new ObservableCollection<Setting>(Read()) };
            var settings = new Settings(Cache.Read<Setting>());  // Read()                  // for save time

            if (settings.Count() == 0)
            {
                settings = new Settings(new List<Setting> // ConvertToList()
                {
                    (
                        Name : "Автофокус",
                        Value : true,
                        Desc : "Автофокус поля для ввода сообщения при выборе контакта"),
                    (
                        Name : "Вид диалога",
                        Value : true,
                        Desc : "Если выключен, то основной список будет показывать список сообщений"),
                    (
                        Name : "LazyLoad",
                        Value : true,
                        Desc : "Ленивая подгрузка сообщений при открытии диалога")
                });

                Save(settings.ToArray());
            }

            foreach (Setting setting in settings)
            {
                setting.PropertyChanged += new PropertyChangedEventHandler(settings.Settings_PropertyChanged);
            }

            return settings;
        }



        public delegate void CollectionChangedEventHandler(object sender, CollectionChangedEventArgs<Setting> e);

        public event CollectionChangedEventHandler CollectionChanged;
        // public event Action<Settings, CollectionChangedEventArgs<Setting>> CollectionChanged;        

        internal void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_initialized++ >= this.Count && CollectionChanged != null)
            {
                CollectionChanged(
                    this,
                    new CollectionChangedEventArgs<Setting>(sender as Setting, this.IndexOf(sender as Setting)));

                Cache.Update(sender as Setting, this.IndexOf(sender as Setting));

            }            

            /*
            var id = this.IndexOf(sender as Setting);
            this[id] = sender as Setting;///*/

        }


        [Obsolete("until replaced for Cache.Read<Setting>() for cache involved and saving time IO operation with DB")]
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
