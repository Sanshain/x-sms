using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace XxmsApp
{

    public class SearchPanel<T>
    {

        public ToolbarItem SearchButton { get; private set; } = null;
        public StackLayout SearchLayout { get; private set; } = null;

        private ListView listView = null;
        private View bottomView = null;

        private IList<T> itemSource = null;

        public SearchPanel(ContentPage page, View subView = null, ListView lstView = null)
        {
            AbsoluteLayout rootLayout = page.Content as AbsoluteLayout;
            
            bottomView = subView ?? rootLayout.Children.Last();
            listView = lstView ?? (ListView)rootLayout.Children.First() as ListView;

            page.ToolbarItems.Add(SearchButton = new ToolbarItem
            {
                Order = ToolbarItemOrder.Primary,
                Icon = new FileImageSource { File = "i_search.png" },
                Priority = 0
            });

            SearchButton.Clicked += (object sender, EventArgs e) =>
            {

                if (SearchLayout != null)
                {
                    var searchEntry = SearchLayout.Children.FirstOrDefault() as Entry;

                    if (SearchLayout.IsVisible = !SearchLayout.IsVisible)
                    {
                        searchEntry?.Focus();
                    }

                }
                else SearchLayout = SearchLayoutCreation(rootLayout, page.Width);

            };

        }


        private StackLayout SearchLayoutCreation(AbsoluteLayout rootLayout, double pageWidth)
        {
            SearchLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
            };
            Entry searchEntry = new Entry
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Placeholder = "Введите текст для поиска",                                       //  "Enter text for search"
                BackgroundColor = Color.LightGray,
                Opacity = 0.9
            };

            searchEntry.Completed += (object s, EventArgs ev) =>
            {               
                SearchLayout.IsVisible = false;
            };

            searchEntry.TextChanged += (object s, TextChangedEventArgs ev) =>
            {
                this.WSearchText(searchEntry.Text);
            };

            searchEntry.Focused += SearchEntry_Focused;
            searchEntry.Unfocused += SearchEntry_Focused;
            
            var searchFrame = new Frame
            {
                Content = searchEntry,
                Padding = new Thickness (0),
                Margin = new Thickness(3),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                /*HasShadow = true,
                OutlineColor = Color.Red,             // material design //*/
                CornerRadius = 10,
                IsClippedToBounds = true             // border-radius //*/
            };

            SearchLayout.Children.Add(searchFrame);
            rootLayout.Children.Add(SearchLayout, new Rectangle(0, 0, pageWidth, 50), AbsoluteLayoutFlags.None);

            searchEntry.Focus();

            return SearchLayout;
        }


        private void WSearchText(string searchedText)
        {
            if (itemSource == null) itemSource = listView.ItemsSource as IList<T>;

            listView.ItemsSource = itemSource.Where(item => {

                try
                {
                    var r = item.ToString().ToLower().Contains(searchedText.ToLower());
                    return r;
                }
                catch(Exception ex)
                {
                    Console.Write(ex.Message);
                }
                return false;
                
            });
        }


        public static SearchPanel<T> Initialize(ContentPage page, View subBtn = null)
        {
            return new SearchPanel<T>(page, subBtn);
        }



        private void SearchEntry_Focused(object sender, FocusEventArgs ev)
        {
            if (ev.IsFocused)
            {
                if (!(itemSource is null || listView.ItemsSource == itemSource))
                {
                    listView.ItemsSource = itemSource;
                }


                Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (!ev.IsFocused) return;

                        if (listView.Parent is AbsoluteLayout) AbsoluteLayout.SetLayoutBounds(listView, new Rectangle(0, 50, 1, 0.9));
                    });

                    return false;
                });

                

                (sender as Entry).Text = "";
                bottomView.IsVisible = false;
                
            }
            else
            {
                SearchLayout.IsVisible = false;


                if (listView.Parent is AbsoluteLayout) AbsoluteLayout.SetLayoutBounds(listView, new Rectangle(0, 0, 1, 0.9));

                Device.StartTimer(TimeSpan.FromMilliseconds(150), () =>
                {
                    Device.BeginInvokeOnMainThread(() => bottomView.IsVisible = true); return false;

                });
            }

            var filename = (SearchLayout.IsVisible ? "d" : "i") + "_search.png";
            SearchButton.Icon = ImageSource.FromFile(filename) as FileImageSource;
        }

    }

}
