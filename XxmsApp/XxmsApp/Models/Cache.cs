﻿using Plugin.ContactService.Shared;
using SQLite;
using SQLiteNetExtensions.Attributes;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;


namespace XxmsApp.Model
{

    public class Contacts : IModel
    {

        public bool IsActual => !string.IsNullOrWhiteSpace(this.Phone);

        [PrimaryKey, Column("_Number")]
        public string Phone { get; set; }
        public string Name { get; set; }
        public string Photo { get; set; }
        public string OptionalPhones { get; set; }

        /// <summary>
        /// view for present on binding Views
        /// </summary>
        public string View
        {
            get => this.Phone + " (" + this.Name + ")";
        }

        /// <summary>
        /// For coping properties after creation 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public IModel CreateAs(object obj)
        {

            var contact = obj as Contacts; // as Contact;

            this.Name = contact.Name;
            this.Phone = contact.Phone;
            this.OptionalPhones = contact.OptionalPhones;
            this.Photo = contact.Photo ?? string.Empty;

            return this;
        }


        [OneToMany]
        public List<Message> Messages { get; set; }//*/        

        public static explicit operator Contacts (Plugin.ContactService.Shared.Contact contact)
        {
            var cn = new Contacts();

            cn.Name = contact.Name;
            cn.Phone = contact.Number;
            cn.OptionalPhones = contact.Numbers != null ? string.Join(";", contact.Numbers) : string.Empty;
            cn.Photo = contact.PhotoUriThumbnail;

            return cn;

        }

        public override string ToString()
        {
            return $"{this.Name} ({this.Phone})";
        }

    }
}



namespace XxmsApp
{

    public interface IModel
    {
        /// <summary>
        /// For coping model properties after creation 
        /// </summary>
        /// <param name="obj">base object from API</param>
        /// <returns>model instance</returns>
        IModel CreateAs(object obj);
        /// <summary>
        /// object validate inside Model-class
        /// </summary>
        Boolean IsActual { get; }        
    }


    public static class Cache
    {

        readonly static string db_filename = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),                          // .Resources  for iOS and Android the better
                App.DATABASE_FILENAME);

        public static SQLiteConnection database = new SQLiteConnection(db_filename);

        static Cache()
        {
            // database.DropTable<Model.Message>();
            database.CreateTable<Model.Message>();

            // database.DropTable<Model.Contacts>();
            database.CreateTable<Model.Contacts>();

            database.CreateTable<Model.SimStore>();

            // database.DropTable<Model.SpamDialog>();
            database.CreateTable<Model.SpamDialog>();
            database.CreateTable<Model.Errors>();

            // database.DropTable<Options.Setting>();
            database.CreateTable<Options.Setting>();
            
        }

        [Conditional("DEBUG")]
        public static void Test()
        {
            var sw = Stopwatch.StartNew();
            var r1 = actions[typeof(Model.Contacts)]().GetAwaiter().GetResult();
            sw.Stop();
            var log = sw.ElapsedMilliseconds;            
        }

        internal static Dictionary<Type, IList<object>> cache = new Dictionary<Type, IList<object>>();

        static Dictionary<Type, Func<Task<List<object>>>> actions = new Dictionary<Type, Func<Task<List<object>>>>()
        {

            { typeof(Model.Contacts),  async () =>
                {
                    /*
                    return (await 
                        Plugin.ContactService.CrossContactService.Current.GetContactListAsync())
                        .Select(r => r as object).ToList();//*/
                    return DependencyService.Get<Api.IEssential>()
                            .GetContacts().Select(r => r as object).ToList();
                }
            },
            { typeof(Model.Message), async () =>

                {
                    var msgs = await Task.Factory.StartNew<List<object>>(() =>
                    {
                        var x_messages = DependencyService.Get<XxmsApp.Api.IMessages>();
                        var messages = x_messages.ReadAll();
                        var objects = messages.Select(m => m as object).ToList();

                        return objects;
                    });

                    return msgs;
                }
            },
            { typeof(Options.Setting), () => {

                    throw new NotImplementedException(
                        "This type has no low-level api for long execution, "+
                        "therefore Update methods by Cache class for the type don't intended");
                }
            }

        };



        /// <summary>
        /// Читает из БД таблицу моделей, если ее нет в кэше
        /// </summary>
        /// <typeparam name="T">model type</typeparam>
        /// <returns></returns>
        // public static List<T> Read<T>() where T: new() => database.Table<T>().ToList();
        public static List<T> Read<T>(ObservableCollection<T> observable = null) where T : new() // IModel
        {

            if (cache.ContainsKey(typeof(T))) return cache[typeof(T)].Select(o => (T)o).ToList();
            else
            {
                var objects = database.Table<T>().ToList();

                if (objects.Count > 0) cache[typeof(T)] = objects.Select(o => (object)o).ToList();

                else
                {
                    // подписываемся на событие On UpdateAsync()
                    void ListenLoading(IList<IModel> obj)
                    {

                        Cache.OnUpdate -= ListenLoading;
                    }

                    Cache.OnUpdate += ListenLoading;
                }

                return objects;
            }
        }


        public static bool CacheClear<T>()
        {
            cache.Remove(typeof(T));

            return true;
        }

        /// <summary>
        /// Обновляет запись в кэше с определенным индексом. Если ее нет, то добавляет
        /// </summary>
        /// <typeparam name="T">тип модели</typeparam>
        /// <param name="subject">объект модели для добавления</param>
        /// <param name="id">ид</param>
        public static void Update<T>(T subject, int id) where T : new() 
        {
            var type = typeof(T);
            if (cache.ContainsKey(typeof(T)))
            {
                var cc = cache[typeof(T)];
                cc[id] = subject;                                                           // 
            }
            else
            {
                cache.Add(typeof(T), database.Table<T>().Select(t => (Object)t).ToList());
                // throw new KeyNotFoundException("The type yet was not added to cache");
            }
        }

        public static bool Contains<T>(T subject) => cache.ContainsKey(typeof(T)) && cache[typeof(T)].Contains(subject);
        /// <summary>
        /// Добавляет запись в кэш
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subject"></param>
        /// <returns></returns>
        public static bool Insert<T>(T subject) where T : IModel, new()
        {
            var type = typeof(T);
            if (cache.ContainsKey(typeof(T)))
            {
                var cc = cache[typeof(T)];
                cc.Add(subject); 
            }
            else
            {
                cache.Add(typeof(T), database.Table<T>().Select(t => (Object)t).ToList());
                // throw new KeyNotFoundException("The type yet was not added to cache");
            }
            return true;
        }

        public static void InsertAll<T>(IEnumerable<T> lstSubj) where T : IModel, new()
        {
            var type = typeof(T);
            if (cache.ContainsKey(typeof(T)))
            {
                var cc = cache[typeof(T)];
                foreach (var subj in lstSubj)
                {
                    cc.Add(subj);
                }                                                                
            }
            else
            {

                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();

                cache.Add(typeof(T), database.Table<T>().Select(t => (Object)t).ToList());
                // throw new KeyNotFoundException("The type yet was not added to cache");

                sw.Stop();
                var l = sw.ElapsedMilliseconds;
            }
        }

        static List<T> iConvert<T>(List<object> raw) where T: IModel, new()
        {
            
            var res = new List<T>();
            for (int i=0;i< raw.Count; i++)
            {
                var r = new T().CreateAs(raw[i]);
                res.Add((T)r);
            }
            return res;
        }

        public static event Action<IList<IModel>> OnUpdate;

        /// <summary>
        /// Асинхронно считывает данные из АПИ и сохраняет их в БД. Возвращает обновленные данные
        /// </summary>
        /// <typeparam name="T">тип, реализующий интерфейс, который будет сохранен в БД</typeparam>
        /// <param name="model">список объектов, реализующих тип T</param>
        /// <returns>возвращает обновленные данные</returns>
        public static async Task<List<T>> UpdateAsync<T>(List<T> model) 
            where T : IModel, new()
        {            

            var rawList = await actions[typeof(T)]();

            // var objectList = rawList.Select(o => (T)o).ToList();
            var objectList = iConvert<T>(rawList);

            int cnt = 0;
            database.RunInTransaction(() =>
            {
                
                foreach (var obj in objectList)
                {
                    if (obj.IsActual) cnt += database.InsertOrReplace(obj);
                }
            });

            var objects = database.Table<T>().ToList();

            cache[typeof(T)] = objects.Select(o => (object)o).ToList();



            OnUpdate?.Invoke(objects.Select(o => o as IModel).ToList());

            return objectList;
        }

        /// <summary>
        /// Sync model Update from native API
        /// </summary>
        /// <typeparam name="T">type of model</typeparam>
        /// <param name="model">model instance (not important)</param>
        /// <returns>List of the model instances from API</returns>
        // [Obsolete("Sync method `UpdateSync` is not recommended, but allowed if really you need")]
        public static List<T> UpdateSync<T>(List<T> model)
        {
            var rawList = actions[typeof(T)]().GetAwaiter().GetResult();

            var objectList = rawList.Select(o => (T)o).ToList();

            database.UpdateAll(objectList, true);

            cache[typeof(T)] = objectList.Select(o => (object)o).ToList();

            return objectList;
            
        }

    }


}