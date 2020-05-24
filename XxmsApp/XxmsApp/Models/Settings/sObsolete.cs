using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using XxmsApp.Options;

namespace XxmsApp.Options.Obsolete
{


    public class AltSettings : AbstractSettings
    {

        public AltSettings(IEnumerable<Setting> settings) : base(settings) { }

        public override event CollectionChangedEventHandler CollectionChanged;


        [FullDescription("Мелодия сообщения")]
        public static string Ringtone { get; set; }
        [FullDescription("Включить вибрацию", "Вибрация при получении сообщения")]
        public static bool Vibration { get => Get(); set => Set(value); }
        [FullDescription("Краткое описание", "Полное описание")]
        public static bool AutoFocus1 { get => Get(); set => Set(value); }
        [FullDescription("Краткое описание", "Полное описание")]
        public static bool AutoFocus2 { get => Get(); set => Set(value); }


        public static AltSettings Initialize()
        {
            Stopwatch sw = new Stopwatch(); sw.Start();

            var lst = Reset();

            var settings = new AltSettings(lst);

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

            return AltSettings.ToList();
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

        
        protected static Func<bool> Get = () =>
        {
            var name = new StackTrace(false).GetFrame(1).GetMethod().Name.Substring(4);

            if (App.Current.Properties.ContainsKey(name))
            {
                return (bool)App.Current.Properties[name];
            }
            else return (bool)(App.Current.Properties[name] = false);

        };//*/


        protected static Action<bool> Set = (value) =>
        {
            var name = System.Reflection.MethodBase.GetCurrentMethod().Name.Substring(4);
            if (App.Current.Properties.ContainsKey(name)) App.Current.Properties[name] = value;
            else App.Current.Properties.Add(name, value);
        };


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

            if (App.Current.Properties.Count == 0) FillCurrentAppProps(ToList().ToDictionary(s => s.Name, s => Serialize(s)));

            else if (App.Current.Properties.Any(kv =>
            {
                bool v = kv.Value.GetType() != typeof(string);
                return v;
            }))
            {
                RemoveAllCurrentProps();

                FillCurrentAppProps(ToList().ToDictionary(s => s.Name, s => Serialize(s)));
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
