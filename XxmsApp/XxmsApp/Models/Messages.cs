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
using XxmsApp.Api;

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

        public static Sim[] Sims { get; private set; }
        static Message()
        {
            var info = DependencyService.Get<Api.IMessages>(DependencyFetchTarget.GlobalInstance);
            Sims = info.GetSimsInfo().ToArray();
        }


        public Message() { }

        /// <summary>
        /// Constructor for outgoing sms [конструктор для исходящих смс]
        /// </summary>
        /// <param name="receiver">receiver [получатель]</param>
        /// <param name="value">value [значение]</param>
        /// <param name="incoming">type of message [тип сообщения]</param>
        public Message(string receiver, string value, bool incoming = true)
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


        const string Unknown = "Неизвестно";
        public string SimCard
        {
            get => string.IsNullOrEmpty(_sim) ? "1" : _sim;
            set
            {
                if (int.TryParse(value, out int subId))                                            // SubscriptionId
                {
                    var sim = Message.Sims.SingleOrDefault(s => s.SubId == subId);
                    if (sim != null)
                    {
                        // if (sim.Slot == 0) _sim = "1"; else

                        _sim = (sim.Slot + 1).ToString();
                        return;
                    }

                }
                else _sim = Unknown;

            }
        }
        public int Status { get; set; } = 0;                                    // 0 - получено, -1 - нет уведомления о получении
        public int ErrorCode { get; set; } = 0;                                 // 0 - отправлено
        public int? IsRead { get; set; } = null;                                // 1|[2|3]



        public string States
        {
            get
            {
                return
                    "Sim: " + this.SimCard + ", " +
                    "Status:" + this.Status.ToString() + ", " +
                    "ErrorCode:" + this.ErrorCode.ToString() + ", " +
                    "IsRead:" + this.IsRead.ToString() + ", ";
            }
        }


        /// <summary>
        /// For incomming it means SPAM, for outgoing - unsented (неотправленные)
        /// [Для входящих смс это значит спам, для исходящих - ошибка отправки]
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
        private string _sim = string.Empty;

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
        public bool LastIsIncoming => !(Messages?.LastOrDefault()?.Incoming ?? true);        
        public string Sim => Messages?.LastOrDefault()?.SimCard ?? string.Empty;


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

