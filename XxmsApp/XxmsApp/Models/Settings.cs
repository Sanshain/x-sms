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

    public class FullDescriptionAttribute : DescriptionAttribute
    {

        public string FullDescription { get; private set; }
        public FullDescriptionAttribute(string desc, string fulldesc = null) : base(desc)
        {
            FullDescription = fulldesc;
        }

    }

    public class Setting : INotifyPropertyChanged
    {
        
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

        protected int _initialized = 0;

        public delegate void CollectionChangedEventHandler(object sender, CollectionChangedEventArgs<Setting> e);
        public virtual event CollectionChangedEventHandler CollectionChanged;
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
            var settings = new Settings(Settings.ToList());            

            settings.ForEach(s => s.PropertyChanged += settings.Setting_Changed);

            return settings;
        }

        private void Setting_Changed(object sender, PropertyChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, new CollectionChangedEventArgs<Setting>(sender as Setting, this.IndexOf(sender as Setting)));

            App.Current.Properties[(sender as Setting).Name] = (sender as Setting).Content;
        }
        
        

    }

}






namespace XxmsApp.Options.Database
{
    /// <summary>
    /// Alternative solution for storing settings in the database
    /// </summary>
    public abstract class ModelSettings : AbstractSettings
    {
        public ModelSettings(IEnumerable<Setting> settings) : base(settings) { }

        static Dictionary<string, bool> settings = new Dictionary<string, bool>();



        protected new static Func<bool> Get = () =>
        {
            var name = new StackTrace(false).GetFrame(1).GetMethod().Name.Substring(4);

            if (settings.ContainsKey(name)) return settings[name];
            else return settings[name] = false;

        };



        protected new static Action<bool> Set = (value) =>
        {
            var name = System.Reflection.MethodBase.GetCurrentMethod().Name.Substring(4);
            if (settings.ContainsKey(name)) settings[name] = value;
            else settings.Add(name, value);
        };

        

        private static void ReadToDictionary()
        {
            settings = Cache.Read<Setting>().ToDictionary(s => s.Name, s => s.Content);
        }

        private void Setting_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (++_initialized >= this.Count && CollectionChanged != null)
            {
                CollectionChanged(this, new CollectionChangedEventArgs<Setting>(sender as Setting, this.IndexOf(sender as Setting)));

                Cache.Update(sender as Setting, this.IndexOf(sender as Setting));
            }
        }

        public override event CollectionChangedEventHandler CollectionChanged;

    }
}