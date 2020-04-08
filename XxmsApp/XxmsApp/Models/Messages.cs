using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using XxmsApp.Model;


using SQLiteNetExtensions.Attributes;
using System.Linq;
using Xamarin.Forms;
using System.Globalization;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace XxmsApp.Model
{

    public class BoolToMarginConverter : IValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">Message (bool)</param>
        /// <param name="targetType">bool or Margin (Thinkness)</param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if ((bool)value)
            {
                if (targetType == typeof(Thickness)) return new Thickness(10, 10, 45, 10);
                else if (targetType == typeof(Color)) return null;
            }
            else
            {
                if (targetType == typeof(Thickness)) return new Thickness(45, 10, 10, 10);
                else if (targetType == typeof(Color)) return null;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("just for OneWay binding");
        }
    }

    [Table("Messages")]
    public class Message : IModel
    {
        public bool IsActual => true;

        public Message() { }
        public Message(string receiver, string value, bool incoming = false)
        {
            Time = DateTime.Now;
            Address = receiver;
            Value = value;
            Incoming = incoming;
        }

        [PrimaryKey, AutoIncrement, Column("_Number")]
        public int Id { get; set; }


        public DateTime Time { get; set; }
        public string Address { get; set; }                                                                 // long
        public string Value { get; set; }
        public bool Incoming { get; set; } = true;

        // [SQLite.Ignore]
        public string Label
        {
            get => this.Value.Substring(0, Math.Min(this.Value.Length, 30)) + "...";
        }
        // [ForeignKey(typeof(Contacts))] // therror on deploy
        [ManyToOne] public Contacts Contact { get; set; }

        public IModel CreateAs(object obj)
        {
            return obj as Message;            
        }

        // public Boolean Outbound { get; set; } = false;

    }


}



namespace XxmsApp
{
    // [Obsolete("пока не уверен, что стоит его использовать")]    

    public class Dialog 
    {


        public IEnumerable<Message> CreateMessage(string receiver, string value)
        {
            Message message = null;

            (Messages as IList<Message>).Add(message = new Message(receiver, value));                  

            return Messages;
        }

        public string Address { get; set; }

        public DateTime Time => Messages?.FirstOrDefault()?.Time ?? DateTime.Now;
        public string Label => Messages?.FirstOrDefault()?.Label ?? "Nothing";

        public ObservableCollection<Message> Messages { get; set; }
    }
}

