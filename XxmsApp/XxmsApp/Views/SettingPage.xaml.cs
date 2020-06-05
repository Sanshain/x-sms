using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XxmsApp;
using Utilites.Views;
using XxmsApp.Options;

namespace XxmsApp.Views
{

    public class HeaderFrame : Frame
    {
        TapGestureRecognizer TapOnFrame = null;
        Label optionDesc = null;
        Label pickedSound = null;        

        public HeaderFrame(Action<HeaderFrame> onClick = null)
        {
            Content = new StackLayout().AddChilds(
                optionDesc = new Label {
                    Text = "Выбрать мелодию",
                    Margin = new Thickness(15, 20, 15, 0),
                    FontAttributes = FontAttributes.Bold,
                    FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label))
                },
                pickedSound = new Label
                {
                    // Text = "не выбрана",
                    Margin = new Thickness(15, -10, 15, 20),                    
                    FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label))
                }
            );

            var rt = Options.ModelSettings.Initialize();

            var pickedBinding = new Binding {
                Path = "Name",
                // Source = rt.Ringtone,
                Mode = BindingMode.TwoWay };

            // optionDesc.SetBinding(Label.TextProperty, new Binding { Path = "Ringtone", Source = Options.ModelSettings.Initialize() });
            pickedSound.SetBinding(Label.TextProperty, pickedBinding);
            pickedSound.BindingContextChanged += PickedSound_BindingContextChanged;

            TapOnFrame = new TapGestureRecognizer();
            TapOnFrame.Tapped += (object sender, EventArgs e) =>
            {
                var navPage = (App.Current.MainPage as MasterDetailPage).Detail as NavigationPage;

                navPage.PushAsync(new Views.SoundPage(sound =>
                {
                    // (this.Parent.Parent as ContentPage)?.DisplayAlert("Result", sound.Name, "ok");
                    pickedSound.Text = sound.Name;
                    // Options.ModelSettings.Initialize().Ringtone = sound;
                    // pickedSound.BindingContext = sound;
                }));
            };
            this.GestureRecognizers.Add(TapOnFrame);
        }

        private void PickedSound_BindingContextChanged(object sender, EventArgs e)
        {
            
        }
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingPage : ContentPage
    {

        Options.AbstractSettings settings;        

        public SettingPage(Options.AbstractSettings settingList = null)
        {


            // var itemTemplate = new DataTemplate(() => CellGenerate());
            var itemTemplate = new DataTemplate(MultiPurposeCellGenerate);


            Button send;
            Button reset;
            var SettingList = new ListView()
            {
                ItemTemplate = itemTemplate,                
                HasUnevenRows = true,
                // Header = new HeaderFrame { Padding = new Thickness(0), HeightRequest = 80 },
                Footer = new StackLayout
                {

                }.AddChilds(
                    reset = new Button { Text = "Сброс настроек" },
                    send = new Button { Text = "Отправить отчет с ошибками разработчикам" }
                ),
                ItemsSource = settings = settingList ?? Options.ModelSettings.Initialize()

                // BindingContext = settings = new ObservableCollection<Model.Setting>(Settings.Initialize())
                // ItemsSource = settings = Options.Settings.Initialize()                
                // ItemsSource = settings = Options.ObSettings.Initialize()
                // ItemsSource = new List<Options.Setting> { new Options.Setting { Name = "1", Content = true, Description = "desc" }}
            };

            send.Clicked += async (s, e) =>
             {
                 if (await DisplayAlert("Отправить?", "Вы уверены. что хотите отправить отчет разработчикам?", "ok", "Нет"))
                 {
                     DisplayAlert("Поздравляем!", "Отчет отправлен", "ok");


                 }
             };

            reset.Clicked += async (object sender, EventArgs e) =>
            {

                if (await DisplayAlert($"Сброс настроек", "Настройки Xxms будут сброшены на заводские", "Ладно", "Отмена"))
                {
                    // Cache.database.DeleteAll<Options.Setting>();

                    Cache.database.DropTable<Options.Setting>();

                    Cache.CacheClear<Options.Setting>();

                    // Options.ObSettings.RemoveAllCurrentProps();
                }
            };


            // lv.BindingContext = settings = Settings.Initialize();
            // lv.SetBinding(ListView.ItemsSourceProperty, "Units");         // if declare items inside settings list            

            SettingList.ItemTapped += SettingList_ItemTapped;
            Content = SettingList;
            Title = "Настройки";
            // Properties.Resources.Culture

        }

        /// <summary>
        /// for MultiPurposeCellGenerate
        /// </summary>
        private IValueConverter cellViewBinder = new BoolConverter<object>((b, type) =>
        {
            if (type == typeof(double))
                return Device.GetNamedSize(b ? NamedSize.Default : NamedSize.Medium, typeof(Label));
            else if (type == typeof(FontAttributes))
                return b ? FontAttributes.None : FontAttributes.Bold;
            else if (type == typeof(Thickness))
                return new Thickness(0, 0, 0, b ? 0 : 20);
            return null;
        });


        
        private object MultiPurposeCellGenerate()
        {
            var w = "ж".GetWidth();

            var view = new RelativeLayout { };
            var picker = new Picker { IsVisible = false, TextColor = Color.DarkGray };
            var descLabel = new Label { };            
            var valueLabel = new Label { FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)) };

            view.Children
                .AddAsRelative(descLabel, 15, 15)
                .SetBindings(Label.TextProperty, "Description")
                .SetBindings(Label.FontAttributesProperty, "IsBool", BindingMode.Default, cellViewBinder)
                .SetBindings(Label.FontSizeProperty, "IsBool", BindingMode.Default, cellViewBinder);
            view.Children                
                .AddAsRelative(valueLabel, p => 15, p=> 35)         // .AddAsRelative(valueLabel, p => p.Width - w * valueLabel.Text.Length, p=>15)
                .SetBindings(Label.IsVisibleProperty, "IsBool", BindingMode.Default, new Utilites.InvertConverter())
                // .SetBindings(Label.MarginProperty, "IsBool", BindingMode.Default, cellViewBinder)
                .SetBinding(Label.TextProperty, "Label");
            view.Children
                .AddAsRelative(new Switch { }, p => p.Width - 50, p => 15)
                .SetBindings(Switch.IsVisibleProperty, "IsBool")
                .SetBinding(Switch.IsToggledProperty, new Binding("Content", BindingMode.TwoWay, new Options.ContentConverter())); // , valueLabel
            view.Children.AddAsRelative(picker, 0)
                .SetBinding(Picker.TitleProperty, "Description");
            
            picker.SelectedIndexChanged += (object sender, EventArgs e) =>
            {
                if (picker.SelectedIndex < 0) return;
                var setting = (view.BindingContext as Setting);
                setting.Content = setting.Type + "|" + picker.Items[picker.SelectedIndex];
            };

            view.SetBinding(RelativeLayout.HeightRequestProperty, "IsBool", converter: new BoolConverter<double>((b, type) =>
            {
                return b ? 45 : 70;
            }));

            var viewCell = new ViewCell { View = view };

            viewCell.BindingContextChanged += (object sender, EventArgs e) =>
            {
                if (((sender as ViewCell).BindingContext as Setting).IsIterate)
                {
                    RelativeLayout.SetXConstraint(valueLabel, Constraint.RelativeToParent(p => p.Width - valueLabel.Text.GetWidth() - 15));
                    RelativeLayout.SetYConstraint(valueLabel, Constraint.Constant(15));
                    // descLabel.FontAttributes = FontAttributes.None;
                    descLabel.FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label));

                    view.HeightRequest = 50;
                    // view.BackgroundColor = Color.LightSkyBlue;
                }
            };

            viewCell.Tapped += (object sender, EventArgs e) =>
            {
                if (valueLabel.BindingContext is Options.Setting setting)
                {
                    var type = setting.Type;
                    if (Options.ModelSettings.Actions.TryGetValue(type, out Action<Options.Setting, Picker> action))
                    {
                        action(setting, picker);
                    }
                }
            };

            return viewCell;
        }

        [Obsolete("Simplified")]
        private static object CellGenerate()
        {

            var sc = new SwitchCell { };
            sc.SetBinding(SwitchCell.TextProperty, "Description");
            // sc.SetBinding(SwitchCell.OnProperty, "Content",  BindingMode.TwoWay, new Options.Setting.ContentConverter());       //*/            

            return sc;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (settings != null) settings.CollectionChanged += Settings_CollectionChanged;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // App.Current.SavePropertiesAsync();
        }


        private void Settings_CollectionChanged(object sender, Options.CollectionChangedEventArgs<Options.Setting> e)
        {
            var id = this.GetHashCode();
        }


        async void SettingList_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null) return;

            var setting = (((ListView)sender).SelectedItem as Options.Setting);

            if (setting.IsBool)
            {                
                var okBtn = bool.Parse(setting.Content) ? "Включено" : "Выключено";
                await DisplayAlert(okBtn, setting.FullDescription, "Ok");
            }

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }
    }
}
