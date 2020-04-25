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

            SearchPanel<Dialog>.Initialize(this);

            // SearchPanelInitialize(rootLayout);

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

            Device.StartTimer(TimeSpan.FromSeconds(2), () =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    var lst = (dialogs.ItemsSource as List<Dialog>).SelectMany(m => m.Messages).ToList();

                    /*
                    foreach (var item in lst.Where(m => m.Status == Api.MessageState.Unsent))
                    {
                        DisplayAlert($"{item.Address}({item.Time.ToString()})", item.Value.ToString(), "ok");
                    }//*/

                    // DisplayAlert(lst.Count.ToString(), lst.Count(m => m.Status == Api.MessageState.Unsent).ToString(), "ok");
                });

                return false;
            });


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
