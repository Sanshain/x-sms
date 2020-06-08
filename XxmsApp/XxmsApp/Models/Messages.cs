using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using XxmsApp.Model;
using System.Linq;

using SQLiteNetExtensions.Attributes;
using System.Linq;
using Xamarin.Forms;
using System.Globalization;
using System.Collections.ObjectModel;
using System.ComponentModel;
using XxmsApp.Api;
using System.Windows.Input;
using Actions = System.Collections.Specialized.NotifyCollectionChangedAction;

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

    public class TimeConverter : IValueConverter
    {
        string _format = null;
        static Dictionary<string, TimeConverter> cache = new Dictionary<string, TimeConverter>();

        static TimeConverter instance;
        public static TimeConverter Instance => instance ?? (instance = new TimeConverter());
        public static string DefaultFormat { get; set; } = "dd/MM/yyyy HH:mm:ss"; 
        
        public static TimeConverter GetInstance(string format = null)
        {
            if (format == null) return Instance;
            else if (cache.TryGetValue(format, out TimeConverter converter))
            {
                return converter;
            }
            else
            {
                var _converter = new TimeConverter { _format = format };
                cache.Add(format, _converter);
                return converter;
            }                       
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() == typeof(DateTime))
            {
                return ((DateTime)value).ToString(_format ?? DefaultFormat);
            }
            throw new NotImplementedException(this.GetType().Name + " get no datetime value");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(this.GetType().Name + " has no back converter");
        }
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

    // [Obsolete("to Props")]
    public class SpamDialog : IModel
    {
        [PrimaryKey]
        public string Address { get; set; }

        public bool IsActual => true;
        public IModel CreateAs(object obj) => obj as SpamDialog;

        public static implicit operator SpamDialog(string value) => new SpamDialog { Address = value };

        public override int GetHashCode() => Address.GetHashCode();
        /*
        public override bool Equals(object obj)
        {
            if (obj is SpamDialog sd) return sd.Address == this.Address;            
            else if (obj is string str) return str == this.Address;
            else return false;
        }//*/

        public override string ToString() => this.Address ?? string.Empty;
    }


    /// <summary>
    /// Для сообщений, которые были отправлены когда клиент не был назначен дефолтным 
    /// (чтобы запомнить номера симок, с кот были отправлены сообщения)
    /// </summary>
    [Table("SimStore")]
    public class SimStore
    {
        // [OneToOne]  - записывает null почему-то

        public int Message { get; set; }
        public int Sim { get; set; }
    }


    [Table("Messages")]
    [Serializable]
    public class Message : IModel, INotifyPropertyChanged
    {

        const string UnknownSim = "?";
        bool? valid = null;

        static Sim[] _sim; public static Sim[] Sims
        {
            get => _sim ?? (_sim = DependencyService.Get<Api.IMessages>(DependencyFetchTarget.GlobalInstance).GetSimsInfo().ToArray());
        }

        static Message()
        {            
            // var info = DependencyService.Get<Api.IMessages>(DependencyFetchTarget.GlobalInstance);
            // Sims = info.GetSimsInfo().ToArray();
        }



        public Message() { }

        /// <summary>
        /// Constructor for outgoing sms [конструктор для исходящих смс]
        /// </summary>
        /// <param name="receiver">receiver [получатель]</param>
        /// <param name="value">value [значение]</param>
        /// <param name="incoming">type of message [тип сообщения]</param>
        public Message(string receiver, string value, int? simId = null, bool incoming = false)
        {
            Time = DateTime.Now;            

            Address = receiver;
            Value = value;
            IsValid = Incoming = incoming;

            if (simId.HasValue)
            {
                SetSim(simId.Value.ToString());
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;        


        [PrimaryKey, AutoIncrement, Column("_Number")]
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public DateTime TimeOut { get; set; }                                                               // time of sending
        public string Service { get; set; }                                                                 // service_center

        public string Address { get; set; }                                                                 // long
        public string Value { get; set; }
        public bool Incoming { get; set; } = true;

        /// <summary>
        /// Simcard SubscribtionId
        /// </summary>
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
        /// Set sim IccID to sms by sim SubscriptionId
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

        public string Label => this.Value.Substring(0, Math.Min(this.Value.Length, 30)).Replace(Environment.NewLine, " ") + "...";



        public bool? Delivered { get; set; } = null;                            // 0 - получено, -1 - нет уведомления о получении, null - входящее
        public int ErrorCode { get; set; } = 0;                                 // 0 - отправлено
        bool isRead; public bool IsRead                                         // 1 - прочитано, 0 - не прочитано, null - исходящее
        {
            get => isRead;
            set
            {
                isRead = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRead)));
            }
        }                                        

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

    public class MessageCommander : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public Action<Message> Command = null;

        public MessageCommander(Action<Message> action)  
        {
            Command = action;
        }

        public bool CanExecute(object parameter) => parameter is Message;   
        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                //DependencyService.Get<>
                Command?.Invoke(parameter as Message);
            }
        }
    }


    public class DialogCommander : ICommand
    {
        public event EventHandler CanExecuteChanged;
        public Action<Dialog> Command = null;

        public DialogCommander(Action<Dialog> action = null) => Command = action;
        public bool CanExecute(object parameter) => parameter is Dialog;

        public void Execute(object parameter)
        {
            if (CanExecute(parameter)) Command?.Invoke(parameter as Dialog);
        }
    }

}



namespace XxmsApp
{



    // [Obsolete("пока не уверен, что стоит его использовать")]    
    public class Dialog : INotifyPropertyChanged
    {

        // [OneToMany] public Contacts Contact { get; set; }

        [PrimaryKey] public string Address { get; set; }
        string count = string.Empty;
        static Dictionary<Dialog, string> contacts = new Dictionary<Dialog, string>();

        ObservableCollection<Message> messages = new ObservableCollection<Message>(); // ObservableCollection<Message>

        public event PropertyChangedEventHandler PropertyChanged;

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

                Messages.CollectionChanged += (s, e) =>
                {
                    /*
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Label)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Time)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.LastMsgState)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.LastIsOutGoing)));//*/
                };
            }
        }

        public string Time => (Messages?.LastOrDefault()?.Time ?? DateTime.Now).ToString(TimeConverter.DefaultFormat);
        public string Label
        {
            get
            {
                if (Filter != null)
                {
                    return Messages?.LastOrDefault(Filter)?.Label ?? "Nothing";
                }
                else return Messages?.LastOrDefault()?.Label ?? "Nothing";
            }
        }

        public bool LastIsOutGoing => !(Messages?.LastOrDefault()?.Incoming ?? true);
        public MessageState LastMsgState => Messages?.LastOrDefault()?.State ?? MessageState.IncomeAndRead;

        public string Sim => Messages?.LastOrDefault()?.SlotSimId ?? string.Empty;
        public Color SimBackColor => Messages?.LastOrDefault()?.SimColor ?? Color.Default;

        // public string Count => $"({Messages?.Count.ToString()})";
        public string Count => count;


        public Dialog CreateMessage(string value, int? simId = null)
        {
            CreateMessage(this.Address, value, simId);
            return this;
        }

        public IEnumerable<Message> CreateMessage(string receiver, string value, int? simId = null)
        {
            Message message = null;

            (Messages as IList<Message>).Add(message = new Message(receiver, value, simId));

            // var msg = e.NewItems[0] as Message;
            var msgApi = DependencyService.Get<Api.IMessages>();
            msgApi.Send(message.Address, message.Value, int.Parse(message.SimOsId));


            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Label)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Time)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.LastMsgState)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.LastIsOutGoing)));

            return Messages;
        }

        public Dialog(string address)
        {
            Address = address;
            isSpam = Spams.Contains(address);

            RemoveCommand = new Command(m =>
            {
                if (m is Message message)
                {
                    Messages.Remove(message);
                }
            });
        }

        public string Contact {
            get
            {
                var limit = 18;
                string rez = string.Empty;
                if (contacts.TryGetValue(this, out string contact))
                {
                    rez = contact ?? string.Empty;
                }
                else
                {
                    var name = Cache.Read<Contacts>().FirstOrDefault(c => c.Phone == this.Address)?.Name ?? this.Address;
                    contacts.Add(this, name ?? string.Empty);
                    rez = name ?? string.Empty;
                }

                return rez.Length < limit ? rez : rez.Substring(0, limit) + "...";
            }
        }

        public override string ToString()
        {
            // var contact = this.Contact;            
            // return string.IsNullOrEmpty(contact) ? this.Address : contact;
            return this.Contact;
        }

        public string Query { get; set; } = string.Empty;
        public Func<Message, bool> Filter
        {
            get
            {
                if (!string.IsNullOrEmpty(Query))
                {
                    return m => m.Value.ToLower().Contains(this.Query.ToLower());
                }
                else return null;
            }
        }

        public bool SetAsRead()
        {
            var id_s = this.Messages.Select(m => {

                m.IsRead = true;

                // Cache.Update(m, m.Id);
                return m.Id;

            }).ToArray();
            var r = DependencyService.Get<Api.IMessages>().SetStateRead(id_s);

            Cache.database.UpdateAll(this.Messages);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastMsgState)));

            return Convert.ToBoolean(r);
        }

        public static Dialog GetOrCreate(string adressee, Message newMsg = null)
        {

            var msgs = Cache.Read<Message>().OrderBy(m => m.Id).ToList();

            var source = new ObservableCollection<Message>(msgs).GroupBy(m => m.Address).Select(g => new Dialog(g.Key)
            {                
                Messages = new ObservableCollection<Message>(g)
            });

            var dialog = source.SingleOrDefault(d => d.Address == adressee) ?? new Dialog(adressee)
            {                
                Messages = new ObservableCollection<Message>()
            };

            if (newMsg != null) dialog.Messages.Add(newMsg);
            return dialog;
        }

        public ICommand RemoveCommand { protected set; get; }

        public static ObservableHashSet<string> Spams;            

        static Dialog()
        {
            Spams = new ObservableHashSet<string>(Cache.database.Table<SpamDialog>().Select(s => s.ToString()));
            Spams.CollectionChanged += (s, e) =>
            {
                switch (e.Action)
                {
                    case Actions.Add: Cache.database.Insert(new SpamDialog { Address = e.NewItems[0] as string }); break;
                    case Actions.Remove: Cache.database.Delete(new SpamDialog { Address = e.OldItems[0] as string }); break;
                    case Actions.Reset: Cache.database.DeleteAll<SpamDialog>(); break;
                }
            };
        }

        bool isSpam;
        public bool IsSpam {
            get => isSpam;
            set
            {
                if (isSpam != value)
                {
                    isSpam = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSpam)));

                    if (IsSpam) Spams.Add(this.Address);
                    else
                    {
                        Spams.Remove(this.Address);
                    }                        
                }

            }
        }

        
        // static List<SpamDialog> spamCache = Cache.database.Table<SpamDialog>().ToList();

        /*
        bool? _isSpam = null;
        public bool IsSpam1
        {
            get
            {
                if (_isSpam.HasValue) return _isSpam.Value;
                
                if (spamCache == null)
                {
                    // App.Current.Properties.TryGetValue(nameof(IsSpam), out object spam)
                    if (App.Current.Properties.TryGetValue(nameof(IsSpam), out object spam))
                    {
                        if (spam is string _spam)
                        {
                            spamCache = _spam.Split('|').ToList();
                            _isSpam = spamCache.Contains(this.Address);
                        }

                        App.Current.Properties.Remove(nameof(IsSpam));
                        spamCache = new List<string>();
                    }
                    spamCache = new List<string>();
                }
                else _isSpam = spamCache.Contains(this.Address);

                
                return false;
            }
            set
            {
                var isSpam = IsSpam;
                if (value && !isSpam) spamCache.Add(this.Address);
                else if (!value && isSpam)
                {
                    spamCache.Remove(this.Address);
                }
                
                var str = string.Join("|", spamCache);
                App.Current.Properties[nameof(IsSpam)] = str;
            }
        }//*/

    }
}

