using System;
using System.Collections.Generic;
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
        const string playIcon = "play2.png";
        const string pauseIcon = "pause2.png";

        static FileImageSource PlayIcon = new FileImageSource { File = playIcon };
    
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

                var sound = this.BindingContext as Sound;
                r.SoundPlay(sound.Path ?? null, async s => // sound.RingtoneType ?? 
                {
                    await play.FadeTo(0);

                    play.Source = PlayIcon;
                    play.FadeTo(0.7);
                    selected = false;
                }, null);
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
        public Sound(string name, string path, string ringtoneType)
        {
            Name = name;
            Path = path;
            RingtoneType = ringtoneType;
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public string RingtoneType { get; set; }
    }

    // [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SoundPage : ContentPage
    {

        public static string CurrentMelody { get; set; } = string.Empty;        

        public SoundPage()
        {
            Title = "Выбор мелодии";

            // InitializeComponent();        

            var SoundList = new ListView
            {
                ItemTemplate = new DataTemplate(typeof(SoundCell))
            };

            var lst = DependencyService.Get<XxmsApp.Api.IMessages>().GetStockSounds();


            var Items = lst
                .Select(s => new Sound(s.Name, s.Path, s.Group))
                .GroupBy(s => s.RingtoneType).ToList();
			
			SoundList.ItemsSource = Items;
            SoundList.IsGroupingEnabled = true;
            SoundList.GroupDisplayBinding = new Binding("Key");
            SoundList.GroupHeaderTemplate = new DataTemplate(() =>
            {                
                new TextCell
                {
                    Height = 55,
                    TextColor = Color.Orange
                };

                var lbl = new Label {
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.Black,
                    HeightRequest = 55,
                    Margin = new Thickness(15),
                    HorizontalOptions = LayoutOptions.Center
                };
                lbl.SetBinding(Label.TextProperty, "Key");
                return new ViewCell
                {
                    View = new StackLayout().AddChilds(lbl)
                };
            });

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
