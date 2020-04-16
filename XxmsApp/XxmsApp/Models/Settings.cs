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

namespace XxmsApp.Options
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

            if (App.Current.Properties.ContainsKey(name)) return (bool)App.Current.Properties[name];
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

        public static Settings Initialize()
        {
            Stopwatch sw = new Stopwatch(); sw.Start();

            var settings = new Settings(Settings.ToList());

            settings.ForEach(s => s.PropertyChanged += settings.Setting_Changed);

            sw.Stop(); var l = sw.ElapsedMilliseconds;

            return settings;
        }

        internal void Setting_Changed(object sender, PropertyChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, new CollectionChangedEventArgs<Setting>(sender as Setting, this.IndexOf(sender as Setting)));

            App.Current.Properties[(sender as Setting).Name] = (sender as Setting).Content;
        }        

        public delegate void CollectionChangedEventHandler(object sender, CollectionChangedEventArgs<Setting> e);
        public virtual event CollectionChangedEventHandler CollectionChanged;
    }

    


    public class ObSettings : AbstractSettings
    {

        public ObSettings(IEnumerable<Setting> settings) : base(settings) { }


        protected new static Func<bool> Get = () =>
        {
            var method = new StackTrace(false).GetFrame(1).GetMethod();
            var name = method.Name.Substring(4);

            if (App.Current.Properties.ContainsKey(name)) return (bool)App.Current.Properties[name];
            else
            {                              
                var desc = method.DeclaringType.GetProperty(name).GetCustomAttribute(typeof(FullDescriptionAttribute)) as FullDescriptionAttribute;

                App.Current.Properties[name] = new Setting
                {
                    Content = false,
                    Name = name,
                    Description = desc.Description,
                    FullDescription = desc.FullDescription
                };

                return App.Current.Properties[name] as Setting;
            }

        };

        protected new static Action<bool> Set = (value) =>
        {
            var method = System.Reflection.MethodBase.GetCurrentMethod();
            var name = method.Name.Substring(4);
            if (App.Current.Properties.ContainsKey(name)) (App.Current.Properties[name] as Setting).Content = value;
            else
            {
                var desc = method.DeclaringType.GetProperty(name).GetCustomAttribute(typeof(FullDescriptionAttribute)) as FullDescriptionAttribute;

                App.Current.Properties.Add(name, new Setting
                {
                    Name = name,
                    Content = value,
                    Description = desc.Description,
                    FullDescription = desc.FullDescription
                });
                throw new Exception("unsuppoerted setting");
            }                
        };


        public new static Settings Initialize()
        {
            Stopwatch sw = new Stopwatch(); sw.Start();

            if (App.Current.Properties.Count == 0)
            {
                var list = Settings.ToList();
                for (int i = 0; i < list.Count; i++)
                {
                    App.Current.Properties.Add(list[i].Name, list[i]);
                }
            }

            var ss = App.Current.Properties.ToDictionary(k => k.Key, v => v.Value as Setting);

            var settings = new Settings(ss.Values.ToList());

            settings.ForEach(s => s.PropertyChanged += settings.Setting_Changed);

            sw.Stop(); var l = sw.ElapsedMilliseconds;

            return settings;
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

  

    }

}






namespace XxmsApp.Options.Database
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
            Cache.database.Update((sender as Options.Database.ModelSettings)[e.Id]);
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
            if (_initialized++ >= this.Count && CollectionChanged != null)
            {
                CollectionChanged(this, new CollectionChangedEventArgs<Setting>(sender as Setting, this.IndexOf(sender as Setting)));

                Cache.Update(sender as Setting, this.IndexOf(sender as Setting));

                ReadToDictionary();
            }
        }


        public new static ModelSettings Initialize()
        {

            Stopwatch sw = new Stopwatch(); sw.Start();

            var settings = new ModelSettings(Cache.Read<Setting>());  // Read()                  

            if (settings.Count == 0)
            {
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
                });

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