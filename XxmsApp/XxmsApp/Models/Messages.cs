using System;
using System.Collections.Generic;
using System.Text;
using XxmsApp.Model;


namespace XxmsApp.Model
{

    public class Message
    {
        public int Id { get; set; }


        public DateTime Time { get; set; }
        public long Phone { get; set; }
        public string Value { get; set; }

        // public Boolean Outbound { get; set; } = false;

    }


}

