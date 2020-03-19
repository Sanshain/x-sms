using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using XxmsApp.Model;

using SQLiteNetExtensions.Attributes;

namespace XxmsApp.Model
{

    [Table("Messages")]
    public class Message : IModel
    {
        public bool IsActual => true;

        [PrimaryKey, AutoIncrement, Column("_Number")]
        public int Id { get; set; }


        public DateTime Time { get; set; }
        public long Phone { get; set; }
        public string Value { get; set; }

        // [ForeignKey(typeof(Contacts))] // therror on deploy
        [ManyToOne]
        public Contacts Contact { get; set; }

        public IModel CreateAs(object obj)
        {
            throw new NotImplementedException();
        }

        // public Boolean Outbound { get; set; } = false;

    }


}

