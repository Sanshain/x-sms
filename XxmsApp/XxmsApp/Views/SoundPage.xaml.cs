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
        public static Dictionary<string, SoundCell> Cells = new Dictionary<string, SoundCell>();       

        const string playIcon = "play2.png";
        const string pauseIcon = "pause2.png";

        static FileImageSource PlayIcon = new FileImageSource { File = playIcon };
    
        Label sound;    
        Frame frame;
        Image play;
        bool selected = false;
        /// <summary>
        /// исключительно для public использования
        /// </summary>
        public bool Selected 
        {
            get => selected;
            set
            {                

                if(value)
                {
                    if ( LastCell!=null)
                    {
                        LastCell.Selected = false;
                    }
                    LastCell = this;
                }
                
                OnSelectionChanged(selected = value);
            }
        }
        static SoundCell LastCell = null;

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

        protected override void OnTapped()
        {
            Selected = !Selected;

            base.OnTapped();            
        }

        async private void OnSelectionChanged(bool _selected)
        {
            await play.FadeTo(0);

            var player = DependencyService.Get<XxmsApp.Api.IMessages>();

            if (_selected)
            {

                var sound = this.BindingContext as Sound;
                player.SoundPlay(sound.Path ?? null, null, async s => // 
                {
                    await play.FadeTo(0);

                    play.Source = PlayIcon;
                    play.FadeTo(0.7);
                    selected = false;
                });
            }
            else player.StopSound();

            play.Source = new FileImageSource { File = selected ? pauseIcon : playIcon };
            play.FadeTo(0.7);
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            sound.SetBinding(Label.TextProperty, "Name");
            
            Cells.Add((this.BindingContext as Sound).Name, this);
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

            SoundCell.Cells = new Dictionary<string, SoundCell>();

            var lst = DependencyService.Get<XxmsApp.Api.IMessages>().GetStockSounds();

            var Items = lst
                .Select(s => new Sound(s.Name, s.Path, s.Group))
                .GroupBy(s => s.RingtoneType).ToList();

            var SoundList = new ListView
            {
                ItemTemplate = new DataTemplate(typeof(SoundCell)),
                ItemsSource = Items,

                IsGroupingEnabled = true,
                GroupDisplayBinding = new Binding("Key"),
                GroupHeaderTemplate = new DataTemplate(() =>
                {
                    new TextCell
                    {
                        Height = 55,
                        TextColor = Color.Orange
                    };

                    var lbl = new Label
                    {
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
                })
            };

            SoundList.ItemTapped += SoundList_ItemTapped;
            SoundList.ItemSelected += SoundList_ItemSelected;


            Content = SoundList;            
        }

        // 1? - if changed
        private void SoundList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            
        }

        // 2
        // 3
        async private void SoundList_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var s = e.Item;
            var s2 = (sender as ListView).SelectedItem;

            var b = await DisplayAlert("", "Выбрать текущую мелодию", "Ладно", "Нет");
            if (b)
            {
                
            }

            SoundCell.Cells[(e.Item as Sound).Name].Selected = false;
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
