using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using XxmsApp.Options;

namespace XxmsApp.Piece
{
    public abstract class AbstractOption : List<string>, Options.IAbstractOption
    {
        public AbstractOption(IEnumerable<string> values) : base(values) { }

        public int Id => this.IndexOf(Value);
        public string Value { private set; get; }

        public IAbstractOption FromString(string s)
        {
            var arr = s.Split('|');
            if (arr.Length > 1)
            {
                Value = arr[1];
                return this;
            }
            else return SetDefault();

        }

        public IAbstractOption SetDefault()
        {
            this.Value = this.First();
            return this;
        }

        public override string ToString()
        {
            return this.GetType().Name + "|" + this.Value;
        }
    }

    public class SimDefault : AbstractOption
    {
        public const string DefaultChoice = "На выбор (по умолчанию)";
        static List<string> values = new List<string> {  };

        static SimDefault()
        {
            var info = DependencyService.Get<Api.IMessages>(DependencyFetchTarget.GlobalInstance);
            values.AddRange(info.GetSimsInfo().ToArray().Select(s => s.Name));
            values.Add(DefaultChoice);
        }

        public SimDefault() : base(values) {}
    }


    public class Languages : List<string>, Options.IAbstractOption
    {
        public const string Russian = "Русский";
        public const string English = "English";


        static string[] values = new string[]
        {
            Russian,
            English
        };
      
        public Languages() : base(values) { }




        public int Id => values.ToString().IndexOf(Value);
        public string Value { private set; get; }

        public IAbstractOption FromString(string s)
        {
            var arr = s.Split('|');
            if (arr.Length > 1)
            {
                Value = arr[1];
                return this;
            }
            else return SetDefault();
            
        }

        public IAbstractOption SetDefault()
        {
            this.Value = values.First();
            return this;
        }

        public override string ToString()
        {
            return this.GetType().Name + "|" + this.Value;
        }
    }
}
