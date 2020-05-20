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
using Xamarin.Forms;
using System.Globalization;

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
        string DefaultValue { get; }
        string Value { get; }

        IAbstractOption FromString(string s);  
        IAbstractOption SetDefault();
    }


    [Table("Settings")]
    public class Setting : INotifyPropertyChanged
    {

        [PrimaryKey]
        public string Name { get; set; }
        public string Description { get; set; }
        public string FullDescription { get; set; }
        public string Content
        {
            get => content;
            set
            {
                content = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs((content).ToString()));
                if (IsBool == false)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Label)));
                }
                
            }
        }
        string content;

        public event PropertyChangedEventHandler PropertyChanged;
        public bool IsBool => bool.TryParse(Content, out bool result);

        public string Label => this.IsBool ? string.Empty : content.Split('|')[1]; 
        public string Type => content.Split('|').First();

        // public bool IsCustomOption => Content.Contains('|');

        public static implicit operator bool(Setting stg) => bool.Parse(stg.Content);

        public static implicit operator Setting((string Name, string Value, string Desc, string FullDesc) setting)
        {
            return new Setting {
                Name = setting.Name,
                Content = setting.Value,
                Description = setting.Desc,
                FullDescription = setting.FullDesc
            };
        }


        public class BoolConverter<T> : IValueConverter
        {

            Func<bool, Type, T> action;            
            public BoolConverter(Func<bool, Type, T> act) { action = act; }            

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is bool b)
                {
                     return action(b, targetType);                    
                }

                return null;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }


        public class ContentConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (targetType == typeof(bool))             // видимость
                {
                    // if (parameter is View view) { return !view.IsVisible; } // Error unexpected
                    return bool.TryParse(value.ToString(), out bool result) ? result : false;
                }
                else if(value is bool && targetType == typeof(double))       // размер шрифта
                {
                    if(System.Convert.ToBoolean(value) == false)
                    {
                        return Device.GetNamedSize(NamedSize.Medium, typeof(Label));
                    }
                    else return Device.GetNamedSize(NamedSize.Small, typeof(Label));
                }
                else
                    throw new Exception("Unexpect convertation in " + this.GetType().Name);
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (targetType == typeof(string)) return value.ToString();
                else
                    throw new Exception("Unexpect back convertation in " + this.GetType().Name);
            }
        }

    }


    public abstract class AbstractSettings : List<Setting>
    {

        static readonly Type Storage = typeof(ModelSettings);

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
            PropertyInfo[] props = Storage.GetProperties(BindingFlags.Static | BindingFlags.Public);

            List<Setting> stgs = props.Select(prop =>
            {
                

                var attr = prop.GetCustomAttributes(false).Single(a => a.GetType() == typeof(FullDescriptionAttribute)) as FullDescriptionAttribute;

                string value = prop.GetValue(null).ToString();

                if (prop.GetValue(null) is IAbstractOption option)
                {
                    // value = option.FromString(value).Value;
                }

                var setting = new Setting
                {
                    Name = prop.Name,
                    Description = attr.Description,
                    FullDescription = attr.FullDescription,
                    Content = value
                };

                return setting;

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



















}

namespace XxmsApp.Options.Obsolete
{ 


    public class Settings : AbstractSettings
    {

        public Settings(IEnumerable<Setting> settings) : base(settings) { }

        public override event CollectionChangedEventHandler CollectionChanged;


        [FullDescription("Мелодия сообщения")]
        public static string Ringtone { get; set; }
        [FullDescription("Включить вибрацию", "Вибрация при получении сообщения")]
        public static bool Vibration { get => Get(); set => Set(value); }
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
                Content = attrs[1], // Convert.ToBoolean(attrs[1]),
                Description = attrs[2],
                FullDescription = attrs[3]
            };
        }

        protected new static Func<bool> Get = () =>
        {
            var method = new StackTrace(false).GetFrame(1).GetMethod();
            var name = method.Name.Substring(4);

            if (App.Current.Properties.ContainsKey(name)) return bool.Parse(Unserialize(App.Current.Properties[name].ToString()).Content);
            else
            {                              
                var desc = method.DeclaringType.GetProperty(name).GetCustomAttribute(typeof(FullDescriptionAttribute)) as FullDescriptionAttribute;

                App.Current.Properties[name] = Serialize(new Setting
                {
                    Content = false.ToString(),
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
                setting.Content = value.ToString();
                App.Current.Properties[name] = Serialize(setting);
            }
            else
            {
                var desc = method.DeclaringType.GetProperty(name).GetCustomAttribute(typeof(FullDescriptionAttribute)) as FullDescriptionAttribute;

                // throw new Exception("unsuppoerted setting");

                App.Current.Properties.Add(name, Serialize(new Setting
                {
                    Name = name,
                    Content = value.ToString(),
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


            settings.initialized = settings.Count(s => bool.Parse(s.Content));


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
}





namespace XxmsApp.Options
{
    
    /// <summary>
    /// Alternative solution for storing settings in the database
    /// </summary>
    public class ModelSettings : AbstractSettings
    {

        public static Dictionary<string, Action<Setting>> Actions = new Dictionary<string, Action<Setting>>()
        {
            { typeof(Sound).Name, s =>
            {
                var navPage = (App.Current.MainPage as MasterDetailPage).Detail as NavigationPage;
                navPage.PushAsync(new Views.SoundPage(sound =>
                {
                     s.Content = sound.ToString();                    
                }));
            }}
        };


        [FullDescription("Выбрать мелодию", "1")]
        public static Sound Rington { get => GetFunc<Sound>(); set => SetFunc(value); }
        [FullDescription("Включить вибрацию", "Вибрация при получении сообщения")]
        public static bool Vibration { get => GetFunc(); set => Set(value); }
        [FullDescription("Краткое описание", "Полное описание")]
        public static bool AutoFocus1 { get => GetFunc(); set => Set(value); }
        [FullDescription("Краткое описание", "Полное описание")]
        public static bool AutoFocus2 { get => GetFunc(); set => Set(value); }

        internal static Dictionary<string, string> Items { get; private set; } = new Dictionary<string, string>();
        protected int _initialized = 0;
        public static ModelSettings Instance = null;


        [FullDescription("Мелодия сообщения")]
        public Sound Ringtone
        {
            get
            {
                if (Items.TryGetValue(nameof(Ringtone), out string value)) return value;
                else
                {
                    var ringtone = Cache.database.Find<Setting>(s => s.Name == nameof(Ringtone));
                    Items.Add(nameof(Ringtone), ringtone?.Content);
                    var data = ringtone?.Content ?? new Sound().SetDefault().ToString();
                    return data;
                }
            }
            set
            {
                if (Items.ContainsKey(nameof(Ringtone)))
                {
                    Items[nameof(Ringtone)] = value;
                    // just if Items dous not consists of this setting:
                    Cache.database.Update(new Setting
                    {
                        Content = value,
                        Name = nameof(Ringtone)
                    });
                }
                else
                {
                    Items.Add(nameof(Ringtone), value);
                    // just if Items dous not consists of this setting:
                    Cache.database.Insert(new Setting
                    {
                        Content = value,
                        Name = nameof(Ringtone)
                    });
                }

            }
        }





        public override event CollectionChangedEventHandler CollectionChanged;
        
        public ModelSettings(IEnumerable<Setting> settings) : base(settings)
        {
            this.CollectionChanged += ModelSettings_CollectionChanged;
        }



        // delegate bool GetSgn([CallerMemberName]string propertyName = null);



        static bool GetFunc([CallerMemberName]string propertyName = null)
        {
            var name = new StackTrace(false).GetFrame(1).GetMethod().Name.Substring(4);

            if (Items.ContainsKey(name)) return Items[name].ToBoolean();
            else return (Items[name] = false.ToString()).ToBoolean();
        }

        static T GetFunc<T>([CallerMemberName]string propertyName = null)  where T : class, IAbstractOption, new()
        {
            var name = new StackTrace(false).GetFrame(1).GetMethod().Name.Substring(4);

            if (Items.ContainsKey(name)) return new T().FromString(Items[name]) as T;
            else
                return new T().SetDefault() as T;
        }

        static void SetFunc(IAbstractOption value) 
        {
            var name = System.Reflection.MethodBase.GetCurrentMethod().Name.Substring(4);
            if (Items.ContainsKey(name)) Items[name] = value.ToString();
            else Items.Add(name, value.ToString());
        }





        [Obsolete]
        protected new static Func<bool> Get = () =>
        {            
            var name = new StackTrace(false).GetFrame(1).GetMethod().Name.Substring(4);

            if (Items.ContainsKey(name)) return Items[name].ToBoolean();
            else return (Items[name] = false.ToString()).ToBoolean();
        };

        protected new static Action<bool> Set = (value) =>
        {
            var name = System.Reflection.MethodBase.GetCurrentMethod().Name.Substring(4);
            if (Items.ContainsKey(name)) Items[name] = value.ToString();
            else Items.Add(name, value.ToString());
        };
        


        private static void ReadToDictionary()
        {

            var cache = Cache.Read<Setting>();
            try
            {
                Items = cache.ToDictionary(s => s.Name, s => s.Content);
            }
            catch(Exception ex)
            {
                throw new Exception(
                    "Cache from database is not synced with ModelSettings[] items. " +
                    "See Cache.Update to fix it or sync it (you need)");
            }
        }

        private void ModelSettings_CollectionChanged(object sender, CollectionChangedEventArgs<Setting> e)
        {
            Cache.database.Update((sender as Options.ModelSettings)[e.Id]);
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

            // var settings = Instance ?? (Instance = new ModelSettings(new List<Setting>())); // new ModelSettings(Cache.Read<Setting>());  // Read()                  
            var settings = Instance ?? (Instance = new ModelSettings(Cache.Read<Setting>().Where(p => p.Name != nameof(Ringtone))));                                                  

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
                
                settings = new ModelSettings(ModelSettings.ToList());

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