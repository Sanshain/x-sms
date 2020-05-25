using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using System.Linq;

namespace XxmsApp
{
    public class Cards
    {
        public static void SimChoice(StackLayout bottom, Action<int> onChoice = null)
        {
            const string title = "Sim";

            var simPicker = bottom.Children.OfType<Picker>().FirstOrDefault(p => p.Title == title);

            if (simPicker == null)
            {
                simPicker = new Picker
                {
                    Title = title,
                    IsVisible = false,
                    ItemsSource = Model.Message.Sims,                                   // ItemDisplayBinding = new Binding("Name")
                };

                simPicker.SelectedIndexChanged += (object s, EventArgs ev) => onChoice?.Invoke(simPicker.SelectedIndex);
                bottom.Children.Add(simPicker);
            }

            simPicker.Focus();            
        }
    }
}
