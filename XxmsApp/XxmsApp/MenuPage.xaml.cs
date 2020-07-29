using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XxmsApp.Piece;

namespace XxmsApp
{


	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MenuPage : ContentPage
	{
        // Dicionary<string, Action>

        const string SIM_CARDS = "Сим-карты";
        const string MessagesUpdate = "Обновить сообщения";


        public MenuPage ()
		{
            
            var menuContainer = new StackLayout { };

            var menu = new ListView
            {
                HorizontalOptions = LayoutOptions.Center,
                ItemsSource = new string[]
                {
                    "Настройки",
                    MessagesUpdate,
                    SIM_CARDS,
                    "О нас",
                    
                    // "Inner read sms",                    
                    // "Сделать дефолтным",                    
                    // "Check default",
                    // "Play",                        

                },
                ItemTemplate = new DataTemplate(typeof(MenuPoint)),                
            };            
            menuContainer.Children.Add(menu);
            menu.ItemSelected += Menu_ItemSelected;


            var quit = new ListView
            {
                ItemsSource = new string[] {"","Выход"},                
                VerticalOptions = LayoutOptions.EndAndExpand,
                ItemTemplate = new DataTemplate(() => new MenuPoint { }),                
                HeightRequest = 90
            };
            quit.ItemSelected += (s, e) => 
            {
                if (e.SelectedItem.ToString().Length > 0) Api.Funcs.AppExit();
                else quit.SelectedItem = null;
            };
            // menuContainer.AddChilds(quit);

            
            
            this.Appearing += (object _sender, EventArgs _e) =>
            {
                (this.Parent as MasterDetailPage).IsPresentedChanged += (object sender, EventArgs e) =>
                {
                    if (!(sender as MasterDetailPage).IsPresented)
                    {
                        Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                        {
                            menu.SelectedItem = null;

                            return false;
                        });

                        //menu.SelectedItem = null;
                    }
                };
            };//*/

            /*
            var method = typeof(MenuPage).GetMethod("GoToSettings", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var handle = method.MethodHandle;
            System.Runtime.CompilerServices.RuntimeHelpers.PrepareMethod(handle);//*/

            Content = menuContainer;

        }



        private async void Menu_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null) return;

            switch (e.SelectedItem.ToString())
            {
                case "О нас":

                    (this.Parent as MasterDetailPage).Detail.Navigation.PushAsync(new Views.About(), true);
                    (this.Parent as MasterDetailPage).IsPresented = false;                    

                    break;
                case MessagesUpdate:

                    {

                        (this.Parent as MasterDetailPage).IsPresented = false;

                        Stopwatch sw = new Stopwatch();
                        sw.Start();

                        /*
                        var x_messages = DependencyService.Get<XxmsApp.Api.IMessages>();
                        var messages = x_messages.Read();
                        //*/                        


                        var _layout = GetDetailLayout();
                        var indicator = new ActivityIndicator() { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };                        
                        
                        Task<bool> fade = null;     // (_layout.Parent as ContentPage).Content = indicator;
                        _layout.Children.ToList().ForEach(v => { v.IsEnabled = false; fade = v.FadeTo(0.3, 1000); });                        

                        _layout.Children.Add(indicator, new Rectangle(0.5, 0.5, 0.2, 0.2), AbsoluteLayoutFlags.All);                                                

                        indicator.IsRunning = true;

                            var messages = await Cache.UpdateAsync(new List<Model.Message>());
                        indicator.IsRunning = false;

                        fade?.Wait();
                        _layout.Children.ToList().ForEach(v => {v.IsEnabled = true; v.FadeTo(1, 1000); });       // (indicator.Parent as ContentPage).Content = _layout;


                        /*
                        var x_messages = DependencyService.Get<XxmsApp.Api.IMessages>();
                        var messages = x_messages.Read();
                        var objects = messages.Select(m => m as object).ToList();//*/

                        sw.Stop();

                        var di = DependencyService.Get<XxmsApp.Api.ILowLevelApi>();
                        // di.ShowNotification("Test", "content");

                        var layout = GetDetailLayout();
                        (layout.Children.First() as MainList).DataInitialize();                              // layout.Children[0] = new MainList();
                        AbsoluteLayout.SetLayoutBounds(layout.Children[0], new Rectangle(0, 0, 1, 0.9));
                        AbsoluteLayout.SetLayoutFlags(layout.Children[0], AbsoluteLayoutFlags.SizeProportional);

                        // DisplayAlert($"За {sw.ElapsedMilliseconds.ToString()} мс", messages.Count.ToString() + " sms", "Ok");                        

                    }
                    goto default;

                case "Inner read sms":

                    Task.Run(() =>
                    {
                        Stopwatch sw = new Stopwatch();
                        sw.Start();

                        /*
                        var msg = Cache.database.FindWithQuery<Model.Message>(
                            "SELECT * FROM Messages WHERE _Number=(SELECT MAX(_Number) FROM Messages)"
                        );
                        var id = msg.Id;//*/
                        
                        var id = ((((this.Parent as MasterDetailPage).Detail as NavPage).RootPage as MainPage).Dialogs.ItemsSource
                            as IEnumerable<Dialog>).SelectMany(d => d.Messages).Max(m => m.Id);//*/

                        var msgs = DependencyService.Get<Api.IMessages>().ReadFrom(id);

                        sw.Stop();

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            DisplayAlert($"За {sw.ElapsedMilliseconds.ToString()} мс", msgs.Count.ToString() + " sms", "Ok");
                        });
                        
                    });

                    goto default;

                case "Настройки":

                    GoToSettings();

                    break;

                case SIM_CARDS:

                    var info = DependencyService.Get<Api.IMessages>(DependencyFetchTarget.GlobalInstance);
                    // info = DependencyService.Get<Api.IMessages>();
                    foreach (var sim in info.GetSimsInfo())
                    {
                        DisplayAlert(sim.IccId, sim.Name + ": слот - " + sim.Slot + $" номер - {sim.SubId}", "ok");
                    }

                    goto default;

                case "Play":

                    var r = DependencyService.Get<XxmsApp.Api.IPlayer>();
                    var ringtone = Options.ModelSettings.Rington;
                    r.SoundPlay(ringtone);

                    goto default;

                case "Выход":
                    
                    // Process.GetCurrentProcess().CloseMainWindow();                      // App.Current.Quit();
                    // Process.GetCurrentProcess().Close();


                    Api.Funcs.AppExit();

                    goto default;

                case "Spam": DisplayAlert("spam", string.Join("|", Dialog.Spams), "ok");break;
                case "Сделать дефолтным": DependencyService.Get<Api.ILowLevelApi>().ChangeDefault(); goto default;
                case "Check default": DisplayAlert(DependencyService.Get<Api.ILowLevelApi>().IsDefault.ToString(), "", "ok");
                    goto default;

                default:

                    // bool r = await DisplayAlert("Start read?", e.SelectedItem.ToString(), "Ok", "No");

                    ((ListView)sender).SelectedItem = null;

                    (this.Parent as MasterDetailPage).IsPresented = false;

                    break;                
            }


            /*
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                ((ListView)sender).SelectedItem = null;

                return false;
            });//*/

        }

        private AbsoluteLayout GetDetailLayout()
        {
            var page = ((this.Parent as MasterDetailPage).Detail as NavigationPage).RootPage as ContentPage;
            return page.Content as AbsoluteLayout;
        }

        Views.SettingPage settingsPage = null;
        private void GoToSettings()
        {            
            settingsPage = settingsPage ?? new Views.SettingPage(Options.ModelSettings.Initialize());
            (this.Parent as MasterDetailPage).Detail.Navigation.PushAsync(settingsPage, true);
            (this.Parent as MasterDetailPage).IsPresented = false;
            
        }
    }
}