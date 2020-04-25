using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XxmsApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingPage : ContentPage
    {

        Options.AbstractSettings settings;

        

        public SettingPage(Options.AbstractSettings settingList = null)
        {


            var itemTemplate = new DataTemplate(() => CellGenerate());


            Button reset;
            var SettingList = new ListView()
            {
                ItemTemplate = itemTemplate,
                // BindingContext = settings = new ObservableCollection<Model.Setting>(Settings.Initialize())
                Footer = reset = new Button { Text = "Сброс настроек" },
                // ItemsSource = settings = Options.Settings.Initialize()                
                // ItemsSource = settings = Options.ObSettings.Initialize()
                ItemsSource = settings = settingList ?? Options.ModelSettings.Initialize()
                // ItemsSource = new List<Options.Setting> { new Options.Setting { Name = "1", Content = true, Description = "desc" }}

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

        private static object CellGenerate()
        {
            /*
            var view = new StackLayout {
                Orientation = StackOrientation.Horizontal,
                Padding = new Thickness(15, 0, 0, 0) };

            Label setting = new Label {
                HorizontalOptions = LayoutOptions.StartAndExpand, VerticalOptions = LayoutOptions.Center };
            Switch swtch = new Switch {
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Center
            };

            setting.SetBinding(Label.TextProperty, "Prop");
            swtch.SetBinding(Switch.IsEnabledProperty, "Enabled");
            swtch.Toggled += Swtch_Toggled;

            view.Children.Add(setting);
            view.Children.Add(swtch);

            var viewCell = new ViewCell { View = view };

            return viewCell;
            //*/

            var sc = new SwitchCell { };
            sc.SetBinding(SwitchCell.TextProperty, "Description");
            sc.SetBinding(SwitchCell.OnProperty, "Content");//*/

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
            if (e.Item == null)
                return;

            var setting = (((ListView)sender).SelectedItem as Options.Setting);

            await DisplayAlert("Описание", setting.FullDescription, "OK", "Отмена");

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }
    }
}
