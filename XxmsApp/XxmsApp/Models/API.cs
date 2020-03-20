using System;
using System.Collections.Generic;
using System.Text;

namespace XxmsApp.Api
{

    public delegate void OnReceived(IEnumerable<XxmsApp.Model.Message> message);

    interface IReceived
    {
        event OnReceived Received;
    }
}
