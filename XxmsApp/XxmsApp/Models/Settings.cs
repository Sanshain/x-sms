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
using System.Diagnostics;
using System.Runtime.CompilerServices;

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

    [Table("Settings")]
    public class Setting : INotifyPropertyChanged
    {

        [PrimaryKey]
        public string Name { get; set; }
        public string Description { get; set; }
        public string FullDescription { get; set; }
        public bool Content
        {
            get => content;
            set 
            {
                content = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs((content).ToString()));
            }
        }
        bool content;

        public event PropertyChangedEventHandler PropertyChanged;

        public static implicit operator Setting((string Name, bool Value, string Desc, string FullDesc) setting) 
        {
            return new Setting {
                Name = setting.Name,
                Content = setting.Value,
                Description = setting.Desc,
                FullDescription = setting.FullDesc
            };
        }

        public static implicit operator bool(Setting stg) => stg.Content;

    }


    public abstract class AbstractSettings : List<Setting>
    {

        public AbstractSettings(IEnumerable<Setting> settings) : base(settings) { }

        protected static Func<bool> Get = () =>
        {
            var name = new StackTrace(false).GetFrame(1).GetMethod().Name.Substring(4);

            if (App.Current.Properties.ContainsKey(name))
            {
                return (bool)App.Current.Properties[name];
            }
            else return (bool)(App.Current.Properties[name] = false);

        };

        protected static Action<bool> Set = (value) =>
        {
            var name = System.Reflection.MethodBase.GetCurrentMethod().Name.Substring(4);
            if (App.Current.Properties.ContainsKey(name)) App.Current.Properties[name] = value;
            else App.Current.Properties.Add(name, value);
        };


        public static List<Setting> ToList()
        {
            PropertyInfo[] props = typeof(Settings).GetProperties(BindingFlags.Static | BindingFlags.Public);            

            List<Setting> stgs = props.Select(p =>
            {
                var attr = p.GetCustomAttributes(false).Single(a => a.GetType() == typeof(FullDescriptionAttribute)) as FullDescriptionAttribute;               

                var value = (bool)p.GetValue(null);

                return new Setting
                {
                    Name = p.Name,
                    Description = attr.Description,
                    FullDescription = attr.FullDescription,
                    Content = value
                };
            }).ToList();

            return stgs;
        }



        internal void Setting_Changed(object sender, PropertyChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, new CollectionChangedEventArgs<Setting>(sender as Setting, this.IndexOf(sender as Setting)));

            App.Current.Properties[(sender as Setting).Name] = (sender as Setting).Content;
        }        
        
        public virtual event CollectionChangedEventHandler CollectionChanged;
    }

    


    public class ObSettings : AbstractSettings
    {

        public ObSettings(IEnumerable<Setting> settings) : base(settings) { }


        private static string Serialize(Setting msg)
        {
            return $"{ msg.Name }%{ msg.Content.ToString() }%{msg.Description }%{ msg.FullDescription }";
        }
        private static Setting Unserialize(string usmsg)
        {
            var attrs = usmsg.Split('%');
            return new Setting
            {
                Name = attrs[0],
                Content = Convert.ToBoolean(attrs[1]),
                Description = attrs[2],
                FullDescription = attrs[3]
            };
        }

        protected new static Func<bool> Get = () =>
        {
            var method = new StackTrace(false).GetFrame(1).GetMethod();
            var name = method.Name.Substring(4);

            if (App.Current.Properties.ContainsKey(name)) return Unserialize(App.Current.Properties[name].ToString()).Content;
            else
            {                              
                var desc = method.DeclaringType.GetProperty(name).GetCustomAttribute(typeof(FullDescriptionAttribute)) as FullDescriptionAttribute;

                App.Current.Properties[name] = Serialize(new Setting
                {
                    Content = false,
                    Name = name,
                    Description = desc.Description,
                    FullDescription = desc.FullDescription
                });

                return App.Current.Properties[name] as Setting;
            }

        };

        protected new static Action<bool> Set = (value) =>
        {
            var method = System.Reflection.MethodBase.GetCurrentMethod();
            var name = method.Name.Substring(4);
            if (App.Current.Properties.ContainsKey(name))
            {
                var setting = Unserialize(App.Current.Properties[name] as string);
                setting.Content = value;
                App.Current.Properties[name] = Serialize(setting);
            }
            else
            {
                var desc = method.DeclaringType.GetProperty(name).GetCustomAttribute(typeof(FullDescriptionAttribute)) as FullDescriptionAttribute;

                // throw new Exception("unsuppoerted setting");

                App.Current.Properties.Add(name, Serialize(new Setting
                {
                    Name = name,
                    Content = value,
                    Description = desc.Description,
                    FullDescription = desc.FullDescription
                }));

            }                
        };


        int initialized = 0;
        public static ObSettings Initialize()
        {
            Stopwatch sw = new Stopwatch(); sw.Start();

            var stgs = App.Current.Properties;

            var ss = Reset().ToDictionary(k => k.Key, v => Unserialize(v.Value as string));

            var settings = new ObSettings(ss.Values.ToList());

            settings.ForEach(s => s.PropertyChanged += settings.Setting_Changed);

            sw.Stop(); var l = sw.ElapsedMilliseconds;


            settings.initialized = settings.Count(s => s.Content);


            return settings;
        }

        public override event CollectionChangedEventHandler CollectionChanged;



        internal new void Setting_Changed(object sender, PropertyChangedEventArgs e)
        {
            if (initialized == 0)
            {
                CollectionChanged?.Invoke(this, new CollectionChangedEventArgs<Setting>(sender as Setting, this.IndexOf(sender as Setting)));

                App.Current.Properties[(sender as Setting).Name] = Serialize(sender as Setting);
            }

            else initialized--;
            
        }


        internal static IDictionary<string, object> Reset()
        {
            var stgs = App.Current.Properties;

            if (App.Current.Properties.Count == 0) FillCurrentAppProps(Settings.ToList().ToDictionary(s => s.Name, s => Serialize(s)));

            else if (App.Current.Properties.Any(kv =>
                {
                    bool v = kv.Value.GetType() != typeof(string);
                    return v;
                }))
            {
                RemoveAllCurrentProps();

                FillCurrentAppProps(Settings.ToList().ToDictionary(s => s.Name, s => Serialize(s)));
            }

            return App.Current.Properties;
        }

        public static IDictionary<string, object> RemoveAllCurrentProps()
        {
            // remove unnecessary
            var UnKeys = new List<string>();
            foreach (var kv in App.Current.Properties) UnKeys.Add(kv.Key);          // if (!list.Any(el => el.Name == kv.Key)) 
            foreach (var key in UnKeys) App.Current.Properties.Remove(key);

            return App.Current.Properties;
        }

        internal static void FillCurrentAppProps(Dictionary<string, string> dict)
        {
            foreach (var kv in dict)
            {
                if (App.Current.Properties.ContainsKey(kv.Key))
                {
                    App.Current.Properties[kv.Key] = kv.Value;
                }
                else App.Current.Properties.Add(kv.Key, kv.Value);
            }
        }
    }


    public class Settings : AbstractSettings
    {

        public Settings(IEnumerable<Setting> settings) : base(settings) { }

        public override event CollectionChangedEventHandler CollectionChanged;


        [FullDescription("Краткое описание", "Полное описание")]
        public static bool AutoFocus { get => Get(); set => Set(value); }
        [FullDescription("Краткое описание", "Полное описание")]
        public static bool AutoFocus1 { get => Get(); set => Set(value); }
        [FullDescription("Краткое описание", "Полное описание")]
        public static bool AutoFocus2 { get => Get(); set => Set(value); }


        public static Settings Initialize()
        {
            Stopwatch sw = new Stopwatch(); sw.Start();

            var lst = Reset();

            var settings = new Settings(lst);

            settings.ForEach(s => s.PropertyChanged += settings.Setting_Changed);

            sw.Stop(); var l = sw.ElapsedMilliseconds;

            return settings;
        }

        internal static List<Setting> Reset()
        {

            if (App.Current.Properties.Any(kv =>
            {
                bool v = kv.Value.GetType() != typeof(bool);
                return v;
            }))
            {
                ObSettings.RemoveAllCurrentProps();
            }

            return Settings.ToList();
        }

        internal static void FillCurrentAppProps(Dictionary<string, bool> dict)
        {
            foreach (var kv in dict)
            {
                if (App.Current.Properties.ContainsKey(kv.Key))
                {
                    App.Current.Properties[kv.Key] = kv.Value;
                }
                else App.Current.Properties.Add(kv.Key, kv.Value);
            }
        }

    }

}






namespace XxmsApp.Options
{
    /// <summary>
    /// Alternative solution for storing settings in the database
    /// </summary>
    public class ModelSettings : AbstractSettings
    {

        public static Dictionary<string, bool> Items { get; private set; } = new Dictionary<string, bool>();
        protected int _initialized = 0;

        public override event CollectionChangedEventHandler CollectionChanged;
        
        public ModelSettings(IEnumerable<Setting> settings) : base(settings)
        {
            this.CollectionChanged += ModelSettings_CollectionChanged;
        }

        private void ModelSettings_CollectionChanged(object sender, CollectionChangedEventArgs<Setting> e)
        {
            Cache.database.Update((sender as Options.ModelSettings)[e.Id]);
        }



        protected new static Func<bool> Get = () =>
        {
            var name = new StackTrace(false).GetFrame(1).GetMethod().Name.Substring(4);

            if (Items.ContainsKey(name)) return Items[name];
            else return Items[name] = false;

        };

        protected new static Action<bool> Set = (value) =>
        {
            var name = System.Reflection.MethodBase.GetCurrentMethod().Name.Substring(4);
            if (Items.ContainsKey(name)) Items[name] = value;
            else Items.Add(name, value);
        };

        

        private static void ReadToDictionary()
        {
            Items = Cache.Read<Setting>().ToDictionary(s => s.Name, s => s.Content);
        }

        private void Setting_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ( CollectionChanged != null) // _initialized++ >= this.Count &&
            {
                CollectionChanged(this, new CollectionChangedEventArgs<Setting>(sender as Setting, this.IndexOf(sender as Setting)));

                Cache.Update(sender as Setting, this.IndexOf(sender as Setting));

                ReadToDictionary();
            }
        }


        public static ModelSettings Initialize()
        {

            Stopwatch sw = new Stopwatch(); sw.Start();

            var settings = new ModelSettings(Cache.Read<Setting>());  // Read()                  

            if (settings.Count == 0)
            {

                /*
                settings = new ModelSettings(new List<Setting> // ConvertToList()
                {(
                        Name : "AutoFocus",
                        Value : true,
                        Desc : "Автофокус поля ввода сообщения",
                        FullDesc : "Автофокус поля для ввода сообщения при выборе контакта"), (

                        Name : "DialogType",
                        Value : true,
                        Desc : "Основной список в виде диалогов",
                        FullDesc : "Если выключен, то основной список будет показывать список сообщений"), (

                        Name : "LazyMsgLoad",
                        Value : true,
                        Desc : "Ленивая подгрузка сообщений",
                        FullDesc : "Ленивая подгрузка сообщений при открытии диалога")
                });//*/
                
                settings = new ModelSettings(Settings.ToList());

                Save(settings.ToArray());
            }

            settings.ForEach(s => s.PropertyChanged += new PropertyChangedEventHandler(settings.Setting_PropertyChanged));

            sw.Stop(); var l = sw.ElapsedMilliseconds;

            ReadToDictionary();

            return settings;
        }





        /*
        [Obsolete("until replaced for Cache.Read<Setting>() for cache involved and saving time IO operation with DB")]
        static Setting[] Read()
        {
            var s = Cache.database.Table<Setting>(); 
            var a = s.ToArray();
            return a;
        }//*/


        public static int Save(Setting[] settings)
        {
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

    }
}