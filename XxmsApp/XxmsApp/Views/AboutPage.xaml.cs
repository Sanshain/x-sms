using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XxmsApp.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class About : ContentPage
	{
        const string aboutUrl = "https://github.com/Sanshain/x-sms";

        public About ()
        {
            InitializeComponent();

            // var aboutPage = aboutPagуInit();

            TapGestureRecognizer tapWaiter = new TapGestureRecognizer();
            tapWaiter.Tapped += (s, e) =>
            {
                // Navigation.PushAsync(aboutPage);
                DependencyService.Get<Api.IEssential>().MoveTo(aboutUrl);
            };
            about.GestureRecognizers.Add(tapWaiter);

        }

        [Obsolete]
        private ContentPage aboutPagуInit()
        {
            var _about = new AbsoluteLayout { };
            var indicator = new ActivityIndicator
            {
                Color = Color.Orange,                
                // FlowDirection = FlowDirection.LeftToRight
            };
            WebView wui = new WebView
            {
                Source = new UrlWebViewSource { Url = aboutUrl },
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,                
            };            

            _about.Children.Add(wui, new Rectangle(0, 0, 1, 1), AbsoluteLayoutFlags.All);
            _about.Children.Add(indicator, new Rectangle(0.5, 0.5, 0.12, 0.12), AbsoluteLayoutFlags.All);

            
            var aboutPage = new ContentPage { Content = _about };
            aboutPage.ToolbarItems.Add(new ToolbarItem
            {
                Text = "Открыть в браузере",
                Command = new Command(() =>
                {
                    DependencyService.Get<Api.IEssential>().MoveTo((wui.Source as UrlWebViewSource).Url);
                }),
                Order = ToolbarItemOrder.Secondary,
                Priority = 0
            });

            wui.Navigating += (s, e) =>
            {
                indicator.IsRunning = true;
                aboutPage.Title = "Загрузка...";
                wui.Opacity = 0;
            };
            wui.Navigated += (s, e) =>
            {
                indicator.IsRunning = false;
                aboutPage.Title = (wui.Source as UrlWebViewSource).Url;
                wui.FadeTo(1);
            };

            return aboutPage;
        }
    }
}