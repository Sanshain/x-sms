using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XxmsApp;

namespace XxmsApp.Views
{

    public class SoundCell : ViewCell
    {
        Label sound;
        Frame frame;
        Image play;
        bool selected = false;
        public bool Selected
        {
            get => selected;
            set
            {
                selected = value;
                if (selected && LastCell != null)
                {
                    LastCell.Selected = false;
                }
            }
        }
        string playIcon = "play2.png";
        string pauseIcon = "pause2.png";
        SoundCell LastCell = null;

        public SoundCell()
        {            

            RelativeLayout root = new RelativeLayout();
            root.Children.AddAsRelative(sound = new Label(), 10, 10, p => p.Width - 70, p => 35);
            root.Children.AddAsRelative(frame = new Frame
            {
                CornerRadius = 20,
                HasShadow = false,
                Padding = 0,
                BackgroundColor = Color.Beige,
                Content = play = new Image
                {
                    Source = new FileImageSource { File = playIcon },
                    Margin = new Thickness(8),
                    Opacity = 0.7
                }
            }, p => p.Width - 50, p => 10, p => 35, p => 35);

            this.View = root;            

        }     

                

        async protected override void OnTapped()
        {
            selected = !selected;

            base.OnTapped();

            await play.FadeTo(0);
            if (selected)
            {
                var r = DependencyService.Get<XxmsApp.Api.IMessages>();
                r.Play("", async s =>
                {
                    await play.FadeTo(0);

                    play.Source = new FileImageSource { File = playIcon };
                    play.FadeTo(0.7);
                    selected = false;
                });
            }

            play.Source = new FileImageSource { File = selected ? pauseIcon : playIcon };
            play.FadeTo(0.7);
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            sound.SetBinding(Label.TextProperty, "Name");            
        }
    }

    public class Sound
    {
        public Sound(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }

    // [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SoundPage : ContentPage
    {

        public static string CurrentMelody { get; set; } = string.Empty;
        public ObservableCollection<Sound> Items { get; set; }

        public SoundPage()
        {
            Title = "Выбор мелодии";

            // InitializeComponent();        

            var SoundList = new ListView
            {
                ItemTemplate = new DataTemplate(typeof(SoundCell))
            };

            var lst = DependencyService.Get<XxmsApp.Api.IMessages>().GetStockSounds();

            Items = new ObservableCollection<Sound>(lst.Select(s => new Sound(s.Name)));
			
			SoundList.ItemsSource = Items;

            Content = SoundList;
        }

        private void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null) return;
            else
                ((ListView)sender).SelectedItem = null;

            DisplayAlert("Item Tapped", "An item was tapped.", "OK");
            
        }
    }
}
