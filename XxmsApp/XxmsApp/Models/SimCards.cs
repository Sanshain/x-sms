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

            var defaultSim = Options.ModelSettings.Sim;

            if ((defaultSim.Value == Piece.SimDefault.DefaultChoice) == false)
            {
                // var _sim = Model.Message.Sims.FirstOrDefault(sim => sim.Name == Piece.SimDefault.DefaultChoice);
                // Array.IndexOf(Model.Message.Sims, _sim);

                var simId = Array.FindIndex(Model.Message.Sims, sim => sim.Name == defaultSim.Value);
                if (simId < 0) DependencyService.Get<Api.ILowLevelApi>().ShowToast("Sim карта, указанная в настройках, не найдена");
                else
                {
                    onChoice?.Invoke(simId);
                    return;
                }                                
            }

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
