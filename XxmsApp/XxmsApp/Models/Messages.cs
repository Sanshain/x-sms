using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using XxmsApp.Model;

using SQLiteNetExtensions.Attributes;

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

        // [ManyToOne]
        [ForeignKey(typeof(Contacts))]
        public Contacts Contact { get; set; }

        // public Boolean Outbound { get; set; } = false;

    }


}

