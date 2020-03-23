using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using XxmsApp.Model;


using SQLiteNetExtensions.Attributes;
using System.Linq;

namespace XxmsApp.Model
{

    // [Obsolete("пока не уверен, что стоит его использовать")]    

    public class Dialog
    {
        public string Address { get; set; }

        public DateTime Time => Messages?.FirstOrDefault()?.Time ?? DateTime.Now;
        public string Label => Messages?.FirstOrDefault()?.Label ?? "Nothing";
            
        public IEnumerable<Message> Messages { get; set; }
    }
    



    [Table("Messages")]
    public class Message : IModel
    {
        public bool IsActual => true;

        [PrimaryKey, AutoIncrement, Column("_Number")]
        public int Id { get; set; }


        public DateTime Time { get; set; }
        public string Address { get; set; }                                                                 // long
        public string Value { get; set; }

        // [SQLite.Ignore]
        public string Label
        {
            get => this.Value.Substring(0, Math.Min(this.Value.Length, 30)) + "...";
        }
        // [ForeignKey(typeof(Contacts))] // therror on deploy
        [ManyToOne] public Contacts Contact { get; set; }

        public IModel CreateAs(object obj)
        {
            return obj as Message;
        }

        // public Boolean Outbound { get; set; } = false;

    }


}

