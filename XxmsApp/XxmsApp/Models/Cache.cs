using Plugin.ContactService.Shared;
using SQLite;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;


namespace XxmsApp.Model
{
    
    public class Contacts : IModel
    {
        
       
        [PrimaryKey, Column("_Number")]        
        public string Phone { get; set; }
        public string Name { get; set; }
        public string Photo { get; set; }
        public string OptionalPhones { get; set; }

        /// <summary>
        /// For coping properties after creation 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public IModel Create(object obj)
        {
            var contact = obj as Contact;

            this.Name = contact.Name;
            this.Phone = contact.Number;
            this.OptionalPhones = contact.Numbers != null ? string.Join(";", contact.Numbers) : string.Empty;
            this.Photo = contact.PhotoUriThumbnail ?? string.Empty;

            return this;
        }

        
        [OneToMany]
        public List<Message> Messages { get; set; }//*/

        // [OneToMany]
        // public List<Model.Message> Duties { get; set; }


        public static explicit operator Contacts (Plugin.ContactService.Shared.Contact contact)
        {
            var cn = new Contacts();

            cn.Name = contact.Name;
            cn.Phone = contact.Number;
            cn.OptionalPhones = contact.Numbers != null ? string.Join(";", contact.Numbers) : string.Empty;
            cn.Photo = contact.PhotoUriThumbnail;

            return cn;

            /*
            return new Contacts
            {
                Name = contact.Name,
                Phone = contact.Number,
                OptionalPhones = string.Join(";", contact.Numbers),
                Photo = contact.PhotoUriThumbnail
            };//*/
        }


    }
}



namespace XxmsApp
{

    public interface IModel
    {
        /// <summary>
        /// For coping properties after creation 
        /// </summary>
        /// <param name="obj">base object from API</param>
        /// <returns>model instance</returns>
        IModel Create(object obj);
    }

    public static class Cache
    {
        static SQLiteConnection database = new SQLiteConnection(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                App.DATABASE_FILENAME));

        static Cache()
        {            

            database.CreateTable<Model.Message>();

            database.DropTable<Model.Contacts>();
            database.CreateTable<Model.Contacts>();
                
            
            //             database.Table<Model.Message>();      
            
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
        /// <typeparam name="T">model type</typeparam>
        /// <returns></returns>
        public static List<T> Read<T>() where T : IModel, new()
        {
            if (cache.ContainsKey(typeof(T))) return cache[typeof(T)].Select(o => (T)o).ToList();
            else
            {
                var objects = database.Table<T>().ToList();
                cache[typeof(T)] = objects.Select(o => (object)o).ToList();
                return objects;
            }
        }

        static List<T> iConvert<T>(List<object> raw) where T: IModel, new()
        {
            
            var res = new List<T>();
            for (int i=0;i< raw.Count; i++)
            {
                var r = new T().Create(raw[i]);
                res.Add((T)r);
            }
            return res;
        }

        public static async Task<List<T>> UpdateAsync<T>(List<T> model) 
            where T : IModel, new()
        {
            var rawList = await actions[typeof(T)]();

            // var objectList = rawList.Select(o => (T)o).ToList();
            var objectList = iConvert<T>(rawList);
            

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