using System;
using System.Collections.Generic;
using System.Text;

namespace XxmsApp.Api
{

    public class Sim
    {

        internal static string Empty = "Неизвестно";

        public Sim(int slot, int numId, string name, string iccId)
        {
            Slot = slot;
            SubId = numId;
            Name = name;
            IccId = iccId;
        }
        public int Slot { get; private set; }
        public int SubId { get; private set; }
        public string Name { get; private set; }
        public string IccId { get; private set; }

    }


    public delegate void OnReceived(IEnumerable<XxmsApp.Model.Message> message);

    /// <summary>
    /// Read sms
    /// </summary>
    public interface IMessages
    {
        // List<string> Read();
        List<XxmsApp.Model.Message> Read();
        bool Send(string adressee, string content);
        void Send(XxmsApp.Model.Message msg);
        IEnumerable<Sim> GetSimsInfo();

        void ShowNotification(string title, string content);
    }

}
