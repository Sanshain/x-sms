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


    public enum MessageState : byte
    {
        IncomeAndRead,
        Unread,
        Unsent,
        Sent,
        Delivered
    }


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

    public class MessageStateConverter : IValueConverter
    {
        static Dictionary<MessageState, Color> values = new Dictionary<MessageState, Color>()
        {
            {MessageState.Unread, Color.DeepSkyBlue },            
            {MessageState.Unsent, Color.OrangeRed },
            {MessageState.Sent, Color.LimeGreen },                           // Gold | DarkSeaGreen | LimeGreen | DarkOliveGreen
            {MessageState.Delivered, Color.Silver },                         // DarkGoldenrod | LimeGreen
            {MessageState.IncomeAndRead, Color.Transparent }
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(Color))
            {
                return values[(MessageState)value];
            }
            else if (targetType == typeof(ImageSource))
            {
                switch ((MessageState)value)
                {
                    case MessageState.Delivered: return ImageSource.FromFile("ok.png") as FileImageSource;
                    case MessageState.Unsent: return ImageSource.FromFile("cancel.png") as FileImageSource;
                }

                return null;
            }

            return null;
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

        const string UnknownSim = "?";
        bool? valid = null;

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

        public event PropertyChangedEventHandler PropertyChanged;        


        [PrimaryKey, AutoIncrement, Column("_Number")]
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public string Address { get; set; }                                                                 // long
        public string Value { get; set; }
        public bool Incoming { get; set; } = true;

        public string SimOsId { get; set; } = UnknownSim;
        public string SimIccID { get; set; } = UnknownSim;

        [ManyToOne] public Contacts Contact { get; set; }
        
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


        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">SubscriptionId</param>
        public void SetSim(string value)
        {

            if (int.TryParse(value, out int simId))                                         
            {
                SimOsId = simId.ToString();

                var sim = Message.Sims.SingleOrDefault(s => s.SubId == simId);

                if (sim != null)
                {
                    SimIccID = sim.IccId;
                }
           
            }
            
        }
        public Color SimColor =>  Message.Sims.SingleOrDefault(s => s.IccId == SimIccID)?.BackColor ?? Color.Default;
        public string SlotSimId => Message.Sims.SingleOrDefault(s => s.IccId == SimIccID)?.Slot.ToString() ?? UnknownSim;
        public string SimName
        {
            get
            {
                if (int.TryParse(this.SimOsId, out int subId))
                {
                    var sim = Message.Sims.SingleOrDefault(s => s.SubId == subId);                   // или IccId

                    return sim?.Name.ToString() ?? UnknownSim;
                }

                return UnknownSim;
            }
        }

        public string Label => this.Value.Substring(0, Math.Min(this.Value.Length, 30)) + "...";



        public bool? Delivered { get; set; } = null;                            // 0 - получено, -1 - нет уведомления о получении, null - входящее
        public int ErrorCode { get; set; } = 0;                                 // 0 - отправлено
        public bool IsRead { get; set; }                                        // 1 - прочитано, 0 - не прочитано, null - исходящее

        public MessageState State
        {
            get
            {
                if (this.Incoming && !this.IsRead) return MessageState.Unread;
                else if (!this.Incoming)
                {
                    if (this.Delivered.HasValue && this.Delivered.Value) return MessageState.Delivered;
                    else if (this.ErrorCode == 0) return MessageState.Sent;
                    else return MessageState.Unsent;
                }

                return MessageState.IncomeAndRead;
            }
        }




        public string States
        {
            get
            {
                return
                    "Sim: " + this.SlotSimId + ", " +
                    "Sim: " + this.SimName + ", " +
                    "State: " + this.State;
                    /* "Delivered:" + this.Delivered.ToString() + ", " +
                    "ErrorCode:" + this.ErrorCode.ToString() + ", " +
                    "IsRead:" + this.IsRead.ToString() + ", ";//*/
            }
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
        static Dictionary<Dialog, string> contacts = new Dictionary<Dialog, string>();

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

        public bool LastIsOutGoing => !(Messages?.LastOrDefault()?.Incoming ?? true);      
        public MessageState LastMsgState => Messages?.LastOrDefault()?.State ?? MessageState.IncomeAndRead;      

        public string Sim => Messages?.LastOrDefault()?.SlotSimId ?? string.Empty;
        public Color SimBackColor => Messages?.LastOrDefault()?.SimColor ?? Color.Default;

        // public string Count => $"({Messages?.Count.ToString()})";
        public string Count => count;

        public IEnumerable<Message> CreateMessage(string receiver, string value)
        {
            Message message = null;

            (Messages as IList<Message>).Add(message = new Message(receiver, value));                  

            return Messages;
        }

        public string Address { get; set; }
        public string Contact {
            get
            {
                if (contacts.TryGetValue(this, out string contact)) return contact;
                else
                {
                    var name = Cache.Read<Contacts>().FirstOrDefault(c => c.Phone == this.Address)?.Name ?? this.Address;
                    contacts.Add(this, name);
                    return name;
                }
            }
        }

        public override string ToString()
        {
            // var contact = this.Contact;            
            // return string.IsNullOrEmpty(contact) ? this.Address : contact;
            return this.Contact;
        }

    }
}

