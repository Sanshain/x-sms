using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using System.Linq;

namespace XxmsApp.Api
{

    public class Sim
    {

        static Dictionary<string, Color> providers = new Dictionary<string, Color>
        {
            { "megafon", Color.Green },
            { "mts", Color.Red },
            { "tele", Color.Black },
            { "beeline", Color.Yellow }
        };

        internal static string Empty = "Неизвестно";

        public Sim(int slot, int numId, string name, string iccId, Color backColor)
        {
            Slot = slot;
            SubId = numId;
            Name = name;
            IccId = iccId;
            BackColor = backColor;

            if (providers.TryGetValue(providers.Keys.SingleOrDefault(k => name.ToLower().Contains(k)) ?? string.Empty, out Color col))
            {
                BackColor = col;
            }

            // invert fo text =>  return Color.FromArgb(c.A, 0xFF - c.R, 0xFF - c.G, 0xFF - c.B);
        }
        public int Slot { get; private set; }
        /// <summary>
        /// SubscriptionId
        /// </summary>
        public int SubId { get; private set; }
        public string Name { get; private set; }
        public string IccId { get; private set; }
        public Color BackColor { get; private set; }

        public override string ToString()
        {
            return $" Слот № {this.Slot + 1} ({this.Name})";
        }
    }


    public delegate void OnReceived(IEnumerable<XxmsApp.Model.Message> message);



    /// <summary>
    /// Read sms
    /// </summary>
    public interface IMessages
    {
        // List<string> Read();
        List<XxmsApp.Model.Message> Read();
        bool Send(string adressee, string content, int? sim = null);
        void Send(XxmsApp.Model.Message msg);
        IEnumerable<Sim> GetSimsInfo();

        void ShowNotification(string title, string content);

        void Vibrate(int ms);
        void SoundPlay(string nameOrUrl, Action<string> onFinish, string type);
        List<(string Name, string Path, string Group)> GetStockSounds();
    }



    public interface ILowLevelApi
    {        
        void Play();                // Play(string sound, Action<string> onFinish);        
        void Vibrate(int ms);
    }

}
