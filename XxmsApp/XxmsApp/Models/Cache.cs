using Plugin.ContactService.Shared;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XxmsApp.Model
{

    [Table("Contacts")]
    public class Contacts
    {
        
       
        [PrimaryKey, AutoIncrement, Column("_Number")]
        public string Phone { get; set; }
        public string Name { get; set; }
        public string Photo { get; set; }
        public string OptionalPhones { get; set; }        
        
        
        // [OneToMany]
        // public List<Model.Message> Duties { get; set; }


        public static implicit operator Contacts (Plugin.ContactService.Shared.Contact contact)
        {
            return new Contacts
            {
                Name = contact.Name,
                Phone = contact.Number,
                OptionalPhones = string.Join(";", contact.Numbers),
                Photo = contact.PhotoUriThumbnail
            };
        }

    }
}



namespace XxmsApp
{
    public static class Cache
    {
        static SQLiteConnection database = new SQLiteConnection(App.DATABASE_FILENAME);
        static Cache()
        {

            database.CreateTable<Model.Message>();
            database.Table<Model.Message>();      
            
        }

        static Dictionary<Type, IList<object>> cache = new Dictionary<Type, IList<object>>();

        static Dictionary<Type, Func<Task<List<object>>>> actions = new Dictionary<Type, Func<Task<List<object>>>>()
        {
            { typeof(Model.Contacts),  async () => {

                    return (await Plugin.ContactService.CrossContactService.Current.GetContactListAsync())
                        .Select(r => r as object).ToList();
                }
            }
        };

        // public static List<T> Read<T>() where T: new() => database.Table<T>().ToList();

        /// <summary>
        /// Читает из БД таблицу
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> Read<T>() where T : new()
        {
            if (cache.ContainsKey(typeof(T))) return cache[typeof(T)].Select(o => (T)o).ToList();
            else
            {
                var objects = database.Table<T>().ToList();
                cache[typeof(T)] = objects.Select(o => (object)o).ToList();
                return objects;
            }
        }

        public static async Task<List<T>> UpdateAsync<T>(List<T> model)
        {
            var rawList = await actions[typeof(T)]();

            var objectList = rawList.Select(o => (T)o).ToList();

            database.UpdateAll(objectList);                     // database.InsertAll(new List<Model.Contacts>());

            return objectList;
        }

        /// <summary>
        /// Sync model Update from native API
        /// </summary>
        /// <typeparam name="T">type of model</typeparam>
        /// <param name="model">model instance (not important)</param>
        /// <returns>List of the model instances from API</returns>
        [Obsolete("Sync method `Update` is not recommended, but allowed if really you need")]
        public static List<T> Update<T>(List<T> model)
        {
            var rawList = actions[typeof(T)]().GetAwaiter().GetResult();

            var objectList = rawList.Select(o => (T)o).ToList();

            database.UpdateAll(objectList);

            return objectList;
            
        }

    }


}