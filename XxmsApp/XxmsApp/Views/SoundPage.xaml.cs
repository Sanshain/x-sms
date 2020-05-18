using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XxmsApp;
using XxmsApp.Piece;


namespace XxmsApp
{
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

        public override string ToString()
        {
            return RingtoneType == typeof(SoundMusic).Name ? Path : Name;
        }
    }

    public class SoundMusic : Sound
    {
        public SoundMusic(string name, string path) : base(null, path, null)
        {
            Name = name.Split('/').LastOrDefault();
            if (Name.Length > 25)
            {
                Name = "..." + Name.Substring(Name.Length - 25);
            }            
            RingtoneType = this.GetType().Name;
        }        
    }
}


namespace XxmsApp.Views
{


    public class SoundCell : ViewCell
    {

        
        public static ObservableDictionary<string, SoundCell> Cells = new ObservableDictionary<string, SoundCell>();       

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
                    if (LastCell!=null && LastCell != this)
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

                player.SoundPlay(sound.ToString(), sound.RingtoneType, async s => // 
                {
                    await play.FadeTo(0);

                    play.Source = PlayIcon;
                    play.FadeTo(0.7);
                    selected = false;

                }, er =>
                {
                    player.SoundPlay(sound.Path);
                    // write log error to database
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
            
            if (this.BindingContext != null)
            {
                Cells.Add((this.BindingContext as Sound).Name, this);
            }
            
        }
    }

    public class SoundListView : ListView
    {
        ContentPage parentPage = null;

        public SoundListView(ContentPage page)
        {            
            this.parentPage = page;
            this.ItemTemplate = new DataTemplate(typeof(SoundCell));

            this.ItemSelected += SoundList_ItemSelected;
            this.ItemTapped += SoundList_ItemTapped;            
        }

        // 1? - item_selected - if selection changed
        private void SoundList_ItemSelected(object sender, SelectedItemChangedEventArgs e) { }

        // 2 - cell_tapped
        // 3 - item_tapped
        async internal void SoundList_ItemTapped(object sender, ItemTappedEventArgs e)
        {            
            //var s2 = (sender as ListView)?.SelectedItem;

            var b = await parentPage.DisplayAlert("", "Выбрать текущую мелодию", "Ладно", "Нет");
            if (b)
            {

            }
        
            
            var name = (e?.Item as Sound)?.Name ?? (sender as SoundMusic).Name;

            SoundCell.Cells[name].Selected = false;            

        }

    }

    // [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SoundPage : ContentPage
    {
        public bool inited = false;

        public static string CurrentMelody { get; set; } = string.Empty;        

        public SoundPage()
        {
            Title = "Выбор мелодии";

            // InitializeComponent();        

            SoundCell.Cells = new ObservableDictionary<string, SoundCell>();

            var lowApi = DependencyService.Get<XxmsApp.Api.IMessages>();
            var lst = lowApi.GetStockSounds();

            var Items = lst
                .Select(s => new Sound(s.Name, s.Path, s.Group))
                .GroupBy(s => s.RingtoneType).ToList();


            var SoundList = new SoundListView(this)
            {
                ItemsSource = Items,
                Header = new StackLayout().AddChilds(new RoundedButton("Выбрать файл", (s, e) =>
                {
                    
                    lowApi.SelectExternalSound(sound =>
                    {                        

                        var lv = ((s as RoundedButton).Parent as StackLayout).Children.OfType<SoundListView>().First();                        

                        if (lv != null)
                        {

                            if (lv.ItemsSource == null)
                            {
                                lv.ItemsSource = new Sound[] { sound };
                            }
                            else
                            {
                                // lv.ItemsSource = null;
                                lv.ItemsSource = new Sound[] { sound };                                
                            }

                            // if (inited) return;

                            lv.IsVisible = true;                            

                            SoundCell.Cells.SetOnCollectionChangedEvent((object sender, NotifyCollectionChangedEventArgs ev) =>
                            {                                
                                
                                if (ev.Action == NotifyCollectionChangedAction.Add || ev.Action == NotifyCollectionChangedAction.Replace)
                                {
                                    if (ev.NewItems[0] is KeyValuePair<string, SoundCell> soundItem)
                                    {
                                        if (soundItem.Key == sound.Name)
                                        {                                             

                                            SoundCell.Cells[sound.Name].Selected = true;
                                            lv.SoundList_ItemTapped(sound, null);
                                            SoundCell.Cells.SetOnCollectionChangedEvent(null);

                                            inited = true;
                                        }
                                    }
                                }
                            });
                            //*/

                        }

                    });
                }), new SoundListView(this) { IsVisible = false, HeightRequest = 55 }),

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
                        
            Content = SoundList;            
        }


    }
}
