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
        public bool IsIterate
        {
            get
            {
                return Content.Count(c => c == '|') == 1;
            }             
        }

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

    }


    public abstract class AbstractSettings : List<Setting>
    {

        static readonly Type Storage = typeof(ModelSettings);

        public AbstractSettings(IEnumerable<Setting> settings) : base(settings) { }




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




        // delegate bool GetSgn([CallerMemberName]string propertyName = null);


        public static Dictionary<string, string> Items { get; protected set; } = new Dictionary<string, string>();

        protected static bool GetFunc([CallerMemberName]string propertyName = null)
        {
            var name = new StackTrace(false).GetFrame(1).GetMethod().Name.Substring(4);

            if (Items.ContainsKey(name)) return Items[name].ToBoolean();
            else return (Items[name] = false.ToString()).ToBoolean();
        }

        protected static T GetFunc<T>([CallerMemberName]string propertyName = null) where T : class, IAbstractOption, new()
        {
            var name = new StackTrace(false).GetFrame(1).GetMethod().Name.Substring(4);

            if (Items.ContainsKey(name)) return new T().FromString(Items[name]) as T;
            else
                return new T().SetDefault() as T;
        }

        protected static void SetFunc(IAbstractOption value)
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

    }



















}







namespace XxmsApp.Options
{
    
    public class Settings : AbstractSettings
    {
        public Settings(IEnumerable<Setting> settings) : base(settings) { }        

        [FullDescription("Выбрать мелодию", "")]
        public static Sound Rington { get => GetFunc<Sound>(); set => SetFunc(value); }
        [FullDescription("Звуковое уведомление", "Звуковое уведомление при получении смс")]
        public static bool Sound { get => GetFunc(); set => Set(value); }
        [FullDescription("Включить вибрацию", "Вибрация при получении сообщения")]
        public static bool Vibration { get => GetFunc(); set => Set(value); }
        [FullDescription("Выберите язык", "")]
        public static Piece.Languages Language { get => GetFunc<Piece.Languages>(); set => SetFunc(value); }
    }



    /// <summary>
    /// Alternative solution for storing settings in the database
    /// </summary>
    public class ModelSettings : AbstractSettings
    {

        public static Dictionary<string, Action<Setting, Picker>> Actions = new Dictionary<string, Action<Setting, Picker>>()
        {
            { typeof(Sound).Name, (s, v) =>
            {
                var navPage = (App.Current.MainPage as MasterDetailPage).Detail as NavigationPage;
                navPage.PushAsync(new Views.SoundPage(sound =>
                {
                     s.Content = sound.ToString();                    
                }));
            }},
            { typeof(Piece.Languages).Name, (s, v) => 
            {                
                v.ItemsSource = new Piece.Languages().FromString(s.Content) as Piece.Languages;
                v.Focus();
            }}
        };
        static ModelSettings()
        {
            Actions.Add(typeof(SoundMusic).Name, Actions[typeof(Sound).Name]);
        }

        [FullDescription("Выбрать мелодию", "")]
        public static Sound Rington { get => GetFunc<Sound>(); set => SetFunc(value); }
        [FullDescription("Звуковое уведомление", "Звуковое уведомление при получении смс")]
        public static bool Sound { get => GetFunc(); set => Set(value); }
        [FullDescription("Включить вибрацию", "Вибрация при получении сообщения")]
        public static bool Vibration { get => GetFunc(); set => Set(value); }
        [FullDescription("Выберите язык", "")]
        public static Piece.Languages Language { get => GetFunc<Piece.Languages>(); set => SetFunc(value); }

        
        protected int _initialized = 0;
        public static ModelSettings Instance = null;


        /*
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
        }//*/





        public override event CollectionChangedEventHandler CollectionChanged;
        
        public ModelSettings(IEnumerable<Setting> settings) : base(settings)
        {
            this.CollectionChanged += ModelSettings_CollectionChanged;
        }




        


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
            var settings = Instance ?? (Instance = new ModelSettings(Cache.Read<Setting>())); //.Where(p => p.Name != nameof(Ringtone))));                                                  

            if (settings.Count == 0)
            {
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