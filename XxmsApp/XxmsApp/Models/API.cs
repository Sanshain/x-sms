using System;
using System.Collections.Generic;
using System.Text;

namespace XxmsApp.Api
{

    public enum MessageState : byte
    {
        Incoming = 1,
        Outgoing = 2,
        Unsent = 3,
        Unread = 4
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
        
        void ShowNotification(string title, string content);
    }

}
