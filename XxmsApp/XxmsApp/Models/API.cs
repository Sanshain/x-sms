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
    /// Deal with sms
    /// </summary>
    public interface IMessages
    {                
        List<XxmsApp.Model.Message> ReadAll();
        List<XxmsApp.Model.Message> ReadFrom(int start);

        bool Send(string adressee, string content, int? sim = null);
        void Send(XxmsApp.Model.Message msg);
        
        int SetStateRead(int messId);        
        int SetStateRead(int[] messIds);
        int SetSimSender(int messId, int simId);

        IEnumerable<Sim> GetSimsInfo();
    }



    public interface ILowLevelApi
    {                
        void Vibrate(int ms);        

        void ShowNotification(string title, string content);
        
        void AppExit();
        void ChangeDefault();

        void Play();                // Play(string sound, Action<string> onFinish);        
    }


    public interface IPlayer
    {
        (string, string, string) GetDefaultSound();
        List<(string Name, string Path, string Group)> GetStockSounds();
        void SelectExternalSound(Action<SoundMusic> sound);

        int SoundPlay(string nameOrUrl, string type = null, Action<string> onFinish = null, Action<Exception> onError = null);
        int SoundPlay(XxmsApp.Sound sound, Action<string> onFinish = null, Action<Exception> OnError = null);
        void StopSound();
    }











    public static class Funcs
    {
        static ILowLevelApi api = null;
        static Funcs() =>  api = DependencyService.Get<ILowLevelApi>();
        
        /// <summary>
        /// App exit
        /// </summary>
        public static void AppExit() => api.AppExit();

    }

}
