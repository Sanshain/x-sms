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
        
        Settings settings;

        public SettingPage()
        {
            /*
             
            InitializeComponent();

            this.SettingsList.ItemsSource = Items;

            this.SettingsList.ItemsSource = new Dictionary<string, bool>()
            {
                {"Автофокус", Convert.ToBoolean(Properties.Resources.AutoFocus)},
                {"В виде диалогов", Convert.ToBoolean(Properties.Resources.DialogsView)}
            };
            
            //*/


            var itemTemplate = new DataTemplate(() =>
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
                sc.SetBinding(SwitchCell.TextProperty, "Prop");
                sc.SetBinding(SwitchCell.OnProperty, "Value");//*/
                // sc.OnChanged += Sc_OnChanged;                            // realized inside binding object

                return sc;
                
            });

            Button reset;
            var SettingList = new ListView()
            {
                ItemTemplate = itemTemplate,
                // BindingContext = settings = new ObservableCollection<Model.Setting>(Settings.Initialize())
                // ItemsSource = settings = new ObservableCollection<Model.Setting>(Settings.Initialize())
                Footer = reset = new Button { Text = "Сброс настроек" },
                ItemsSource = settings = Settings.Initialize()

            };
            reset.Clicked += async (object sender, EventArgs e) =>
            {
                
                if (await DisplayAlert($"Сброс настроек", "Настройки Xxms будут сброшены на заводские", "Ладно", "Отмена"))
                {
                    Cache.database.DropTable<Model.Setting>();
                }
            };


            // lv.BindingContext = settings = Settings.Initialize();
            // lv.SetBinding(ListView.ItemsSourceProperty, "Units");         // if declare items inside settings list            

            SettingList.ItemTapped += SettingList_ItemTapped;
            Content = SettingList;
            Title = "Настройки";
            // Properties.Resources.Culture

        }


        protected override void OnAppearing()
        {
            base.OnAppearing();

            settings.CollectionChanged += Settings_CollectionChanged;
        }

        private void Settings_CollectionChanged(object sender, CollectionChangedEventArgs<Model.Setting> e)
        {
            // await DisplayAlert(e.ChangedItem.Prop, e.Id.ToString(), "Settings_CollectionChanged", "OK");

            Cache.database.Update((sender as Settings)[e.Id]);
        }


        async void SettingList_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;

            var setting = (((ListView)sender).SelectedItem as Model.Setting);

            await DisplayAlert("Описание", setting.Description, "OK");

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }
    }
}
