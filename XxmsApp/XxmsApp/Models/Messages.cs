using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using XxmsApp.Model;


namespace XxmsApp.Model
{

    [Table("Messages")]
    public class Message
    {
        [PrimaryKey, AutoIncrement, Column("_Number")]
        public int Id { get; set; }


        public DateTime Time { get; set; }
        public long Phone { get; set; }
        public string Value { get; set; }

        // public Boolean Outbound { get; set; } = false;

    }


}

