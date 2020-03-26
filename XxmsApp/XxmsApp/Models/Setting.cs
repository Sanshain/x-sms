using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SQLite;
using XxmsApp.Model;
using System.Linq;

namespace XxmsApp.Model
{
    [Table("Settings")]
    public class Setting 
    {
        [PrimaryKey]
        public string Prop { get; set; }
        public string Value { get; set; }

        public bool Enabled => Convert.ToBoolean(Value);

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
    public static class Settings
    {
        public static List<Setting> Initialize()
        {
            var settings = Read().ToList();

            if (settings.Count() > 0) return settings;
            else settings = new List<Setting>
            {
                (Name : "Автофокус", Value : true),
                (Name : "Вид диалога", Value : true)
            };

            Save(settings.ToArray());

            return settings;
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

    }


}
