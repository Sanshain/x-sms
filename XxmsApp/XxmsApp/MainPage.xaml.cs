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
			InitializeComponent();
            

            
            var rootLayout = new AbsoluteLayout();
            var dialogs = new Piece.CustomList();
            rootLayout.Children.Add(dialogs, new Rectangle(0, 0, 1, 0.9), AbsoluteLayoutFlags.SizeProportional);            
            rootLayout.Children.Add(subBtn, new Rectangle(0, 1, 1, 0.1), AbsoluteLayoutFlags.All);
            Content = rootLayout;

            
            subBtn.Clicked += Btn_Clicked;
            dialogs.ItemSelected += Dialogs_ItemSelected;
            this.Appearing += MainPage_Appearing;
            onPop = new Dictionary<Type, Action>
            {
                { typeof(Views.Messages) , () => dialogs.SelectedItem = null }
            };            

            Title = "Диалоги";


            var contactsGetter = (Application.Current as App).contactsAwaiter;

            contactsGetter.ContinueWith((cnts) =>
            {
                subBtn.IsEnabled = true;

                (Application.Current as App).contacts = cnts.Result.ToList();
            }, TaskScheduler.FromCurrentSynchronizationContext());                               // 
            //.GetAwaiter().GetResult().ToList();


        }

        private void MainPage_Appearing(object sender, EventArgs e)
        {
            var parent = (this.Parent as NavigationPage);
            parent.Popped += MainPage_PoppedToRoot;
        }

        private async void Dialogs_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {            

            if (e.SelectedItem == null) return;

            await Navigation.PushAsync(new Views.Messages(), true);

            (sender as ListView).SelectedItem = null;
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
