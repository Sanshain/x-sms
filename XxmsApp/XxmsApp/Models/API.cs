using System;
using System.Collections.Generic;
using System.Text;

namespace XxmsApp.Api
{

    public delegate void OnReceived(IEnumerable<XxmsApp.Model.Message> message);


    /// <summary>
    /// for receiveng sms messages
    /// </summary>
    public interface IReceived
    {
        event OnReceived Received;
    }


    /// <summary>
    /// Read sms
    /// </summary>
    public interface IMessages
    {
        // List<string> Read();
        List<XxmsApp.Model.Message> Read();
    }

}
