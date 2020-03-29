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

            Items = new ObservableCollection<string>
            {
                "Item 1",
                "Item 2",
                "Item 3",
                "Item 4",
                "Item 5"
            };

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
                sc.OnChanged += Sc_OnChanged;

                return sc;
                
            });

            
            var lv = new ListView()
            {
                ItemTemplate = itemTemplate,
                // BindingContext = settings = new ObservableCollection<Model.Setting>(Settings.Initialize())
                // ItemsSource = settings = new ObservableCollection<Model.Setting>(Settings.Initialize())
                ItemsSource = settings = Settings.Initialize()
            };


            // lv.BindingContext = settings = Settings.Initialize();
            // lv.SetBinding(ListView.ItemsSourceProperty, "Units");         // if declare items inside settings list            

            
            Content = lv;
            Title = "Настройки";
            // Properties.Resources.Culture

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            settings.CollectionChanged += Settings_CollectionChanged;
        }

        async private void Settings_CollectionChanged(object sender, CollectionChangedEventArgs<Model.Setting> e)
        {

            await DisplayAlert(e.ChangedItem.Prop, e.Id.ToString(), "Settings_CollectionChanged", "OK");
        }


        private void Sc_OnChanged(object sender, ToggledEventArgs e)
        {
            var a = (sender as SwitchCell).BindingContext;
            var b = e.Value;

        }

        /// just for CellView. Not for SwitchView!
        private void Swtch_Toggled(object sender, ToggledEventArgs e)
        {
            var setting = (((sender as Switch).Parent as StackLayout).Children.First() as Label).BindingContext;
            (setting as Model.Setting).Value = e.Value;
            
        }

        void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;

            // await DisplayAlert("Item Tapped", "An item was tapped.", "OK");

            //Deselect Item
            ((ListView)sender).SelectedItem = null;
        }
    }
}
