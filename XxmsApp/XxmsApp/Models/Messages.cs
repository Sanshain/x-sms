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

    public class IncomingConverter : IValueConverter
    {


        public static IncomingConverter Single
        {
            get
            {
                if (instance == null) instance = new IncomingConverter();

                // return instance ?? (instance = new IncomingConverter());

                return instance;
            }
        }

        static IncomingConverter instance = null;
        static Dictionary<Type, Func<bool, object>> values = new Dictionary<Type, Func<bool, object>>()
        {
            {
                typeof(Thickness), v => v ? new Thickness(10, 10, 45, 10) : new Thickness(45, 10, 10, 10) },
            {
                typeof(Color), v => v ? Color.Default : Color.LightGreen }

        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">Message is incoming (bool)</param>
        /// <param name="targetType">(Backcolor) Color or Margin (Thinkness)</param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (values.ContainsKey(targetType)) return values[targetType]((bool)value);
            else
                throw new Exception("Unsuppoerted type in " + System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name);

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(this.GetType().Name + " just for OneWay binding ");
        }
    }

    [Table("Messages")]
    [Serializable]    
    public class Message : IModel, INotifyPropertyChanged
    {
        

        public Message() { }
        /// <summary>
        /// Constructor for outgoing sms
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="value"></param>
        /// <param name="incoming"></param>
        public Message(string receiver, string value, bool incoming = false)
        {
            Time = DateTime.Now;
            Address = receiver;
            Value = value;
            Incoming = incoming;
            IsValid = false;
        }




        [PrimaryKey, AutoIncrement, Column("_Number")]
        public int Id { get; set; }

        public DateTime Time { get; set; }
        public string Address { get; set; }                                                                 // long
        public string Value { get; set; }
        public bool Incoming { get; set; } = true;
        /// <summary>
        /// For incomming it means SPAM, for outgoing - unsented (неотправленные)
        /// </summary>
        bool? IsValid
        {
            get => valid;
            set
            {
                if (!this.Incoming)
                {
                    valid = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsValid"));
                }
            }
        }
        [ManyToOne] public Contacts Contact { get; set; }




        bool? valid = null;
        public event PropertyChangedEventHandler PropertyChanged;

        
        public string Label                                        // [SQLite.Ignore]
        {
            get => this.Value.Substring(0, Math.Min(this.Value.Length, 30)) + "...";
        }


        public bool IsActual => true;
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
        string count = string.Empty;

        ObservableCollection<Message> messages = new ObservableCollection<Message>();
        public ObservableCollection<Message> Messages
        {
            get
            {
                return messages;
            }
            set
            {
                messages = value;
                count = $"({messages?.Count.ToString()})";
            }
        }

        public DateTime Time => Messages?.LastOrDefault()?.Time ?? DateTime.Now;
        public string Label => Messages?.LastOrDefault()?.Label ?? "Nothing";
        // public string Count => $"({Messages?.Count.ToString()})";
        public string Count => count;

        public IEnumerable<Message> CreateMessage(string receiver, string value)
        {
            Message message = null;

            (Messages as IList<Message>).Add(message = new Message(receiver, value));                  

            return Messages;
        }

        public string Address { get; set; }

        public override string ToString()
        {
            return this.Address;
        }

    }
}

