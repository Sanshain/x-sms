using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XxmsApp
{
	public partial class MainPage : ContentPage
	{

        Dictionary<Type, Action> onPop;
        Button subBtn = new Button { Text = "Создать", IsEnabled = false };

        public MainPage()
        {

            var rootLayout = Initialize() as AbsoluteLayout;


            ToolbarItem searchButton;
            StackLayout searchLayout = null;

            this.ToolbarItems.Add(searchButton = new ToolbarItem
            {
                Order = ToolbarItemOrder.Primary,
                Icon = new FileImageSource { File = "d_search.png" },
                Priority = 0
            });            

            searchButton.Clicked += (object sender, EventArgs e) =>
            {                 

                if (searchLayout != null)
                {
                    var searchEntry = searchLayout.Children.FirstOrDefault() as Entry;

                    if (searchLayout.IsVisible = !searchLayout.IsVisible)
                    {
                        searchEntry?.Focus();
                    }

                }
                else
                {
                    searchLayout = new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                    };
                    Entry searchEntry = new Entry
                    {
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        Placeholder = "Enter text for search",
                        BackgroundColor = Color.LightGray,
                        Opacity = 0.9
                    };

                    searchEntry.Completed += (object s, EventArgs ev) => searchLayout.IsVisible = false;
                    Action<object, FocusEventArgs> FocusChanged = (s, ev) =>
                    {
                        if (ev.IsFocused)
                        {
                            searchEntry.Text = "";
                            subBtn.IsVisible = false;
                        }
                        else
                        {
                            searchLayout.IsVisible = false;

                            Device.StartTimer(TimeSpan.FromMilliseconds(150), () =>
                            {
                                Device.BeginInvokeOnMainThread(() => subBtn.IsVisible = true); return false;

                            });
                        }

                        var filename = (searchLayout.IsVisible ? "i" : "d") + "_search.png";
                        searchButton.Icon = ImageSource.FromFile(filename) as FileImageSource;

                    };

                    searchEntry.Focused += new EventHandler<FocusEventArgs>(FocusChanged);
                    searchEntry.Unfocused += new EventHandler<FocusEventArgs>(FocusChanged);

                    searchLayout.Children.Add(searchEntry);
                    rootLayout.Children.Add(searchLayout, new Rectangle(0, 0, this.Width, 50), AbsoluteLayoutFlags.None);

                    searchEntry.Focus();

                }



            };


            if ((Application.Current as App)._contacts.Count == 0)       // если контактов нет, то запрашиваем их сразу же при загрузке
            {
                (Application.Current as App).contactsWaiter.ContinueWith((cn) =>
                {
                    (Application.Current as App)._contacts = cn.GetAwaiter().GetResult();

                    subBtn.IsEnabled = true;

                }, TaskScheduler.FromCurrentSynchronizationContext()).GetAwaiter();
            }
            else subBtn.IsEnabled = true;

        }


        private Layout Initialize()
        {
            var rootLayout = new AbsoluteLayout();
            var dialogs = new Piece.MainList();

            rootLayout.Children.Add(dialogs, new Rectangle(0, 0, 1, 0.9), AbsoluteLayoutFlags.SizeProportional);
            rootLayout.Children.Add(subBtn, new Rectangle(0, 1, 1, 0.1), AbsoluteLayoutFlags.All);

            Content = rootLayout;            

            subBtn.Clicked += Btn_Clicked;
            this.Appearing += MainPage_Appearing;

            onPop = new Dictionary<Type, Action>
            {
                { typeof(Views.Messages) , () => dialogs.SelectedItem = null }
            };

            Title = "Диалоги";

            return rootLayout;
        }

        private void MainPage_Appearing(object sender, EventArgs e)
        {
            var parent = (this.Parent as NavigationPage);
            parent.Popped += MainPage_PoppedToRoot;
        }

        private void Btn_Clicked(object sender, EventArgs e)
        {
            /*
            var target = sender as Button;
            var parent = (target.Parent as Layout).Parent as Page;
            ((parent.Parent as NavigationPage).Parent as MasterDetailPage).IsPresented = true;//*/

            Navigation.PushAsync(new Views.CreatePage { Title= "Новое сообщение" }, true);            

        }

        private void MainPage_PoppedToRoot(object sender, NavigationEventArgs e)
        {

            if (onPop.ContainsKey(e.Page.GetType()))
            {
                onPop[e.Page.GetType()]();
            }

        }
    }
}
