using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

using XxmsApp.Piece;
using XxmsApp.Piece;

namespace XxmsApp
{

    [Flags]
    public enum SearchPanelState : byte
    {
        Hidden,
        Visible,
        InSearch
    }

    public class SearchToolbarButton : ToolbarItem
    {

        public static Dictionary<SearchPanelState, string> Icons = new Dictionary<SearchPanelState, string>
        {
            { SearchPanelState.Hidden, "i_search.png" },
            { SearchPanelState.Visible, "c_search.png" },
            { SearchPanelState.Hidden | SearchPanelState.InSearch, "r_search.png" },                                   // { SearchPanelState.InSearch, "i_search.png" }            
        };

        public SearchToolbarButton() : base()
        {
            Order = ToolbarItemOrder.Primary;
            Icon = new FileImageSource { File = Icons[State] };
            Priority = 0;
        }


        public Action ItemClicked = delegate { };
        public AbsoluteLayout ContentLayout { get; set; }
        public StackLayout SearchLayout { get; set; } = null;
        public SearchPanelState State { get; set; } = SearchPanelState.Hidden;



        internal void StateUpdate(bool onHide = false)
        {
            var text = ((SearchLayout.Children.First() as Frame).Content as Entry).Text;

            if (SearchLayout.IsVisible && !onHide) this.State = SearchPanelState.Visible;
            else
            {                
                this.State = SearchPanelState.Hidden;
            }
                
            if (!string.IsNullOrEmpty(text)) this.State |= SearchPanelState.InSearch;
        }

    }
    


    public class SearchPanel<T>
    {
        const string SHOW_ANIMATION = "searchFrameAppearAnimation";
        const string HIDE_ANIMATION = "searchFrameHideAnimation";
        const string VIEW_OUT_ANIMATION = "SearchLayoutAppearAnimation";
        const string VIEW_IN_ANIMATION = "SearchLayoutHideAnimation";

        public SearchToolbarButton SearchButton { get; private set; } = null;
        public StackLayout SearchLayout { get; private set; } = null;
        Entry searchEntry = null;

        public bool IsVisible { get; set; }
        private ListView listView = null;
        private View bottomView = null;
        private bool animeted = true;
        private double PageWidth;

        private const uint animateLong = 250;

        private IList<T> itemSource = null;

        public SearchPanel(ContentPage page, View subView = null, ListView lstView = null)
        {
            
            AbsoluteLayout rootLayout = page.Content as AbsoluteLayout;
            
            bottomView = subView ?? rootLayout.Children.Last();
            listView = lstView ?? (ListView)rootLayout.Children.First() as ListView;
            
            page.ToolbarItems.Add(SearchButton = new SearchToolbarButton
            {
                ContentLayout = rootLayout,
                ItemClicked = () => SearchButton_Clicked(rootLayout)
            });  

            PageWidth = page.Width;

            // SearchButton.Clicked += (object sender, EventArgs e) => SearchButton_Clicked(rootLayout);

        }


        private void SearchButton_Clicked(AbsoluteLayout rootLayout)
        {

            ContentPage page = rootLayout.Parent as ContentPage;

            if (SearchLayout != null)
            {

                var searchFrame = SearchLayout.Children.FirstOrDefault() as Frame;

                if (SearchButton.State == (SearchPanelState.Hidden | SearchPanelState.InSearch))
                {

                    (rootLayout.Parent as ContentPage).Title = "Диалоги";

                    // анимация scale для listView
                    var animate_in = new Animation(v => listView.Scale = v, 1, 0.9);
                    listView.Animate("list_in", animate_in, finished: (v, b) =>
                    {
                        var animate_out = new Animation(_v => listView.Scale = _v, 0.9, 1);
                        listView.Animate("list_out", animate_out, finished: (_v, _b) => { });
                        (searchFrame.Content as Entry).Text = "";
                    });

                }
                else if (SearchButton.State == SearchPanelState.Hidden)
                {
                    this.PreventAnimation();

                    if (SearchLayout.IsVisible = !SearchLayout.IsVisible) // SearchLayout.IsVisible == true
                    {
                        if (animeted)
                        {
                            void FinishAction() => searchFrame?.Content?.Focus();       // Action FinishAction = () => searchFrame?.Content?.Focus();
                            SmoothAppearance(page.Width, FinishAction);
                        }
                        else searchFrame?.Content?.Focus();
                    }
                }

            }
            else SearchButton.SearchLayout = SearchLayout = SearchLayoutCreation(rootLayout, page.Width);

            SearchButton.StateUpdate();

        }



        private void PreventAnimation()
        {
            // if (SearchLayout.AnimationIsRunning(SHOW_ANIMATION) || SearchLayout.AnimationIsRunning(HIDE_ANIMATION)) return;

            if (SearchLayout.AnimationIsRunning(SHOW_ANIMATION))
            {
                SearchLayout.AbortAnimation(SHOW_ANIMATION);
                SearchLayout.AbortAnimation(VIEW_OUT_ANIMATION);
            }

            if (SearchLayout.AnimationIsRunning(HIDE_ANIMATION))
            {
                SearchLayout.AbortAnimation(HIDE_ANIMATION);
                SearchLayout.AbortAnimation(VIEW_IN_ANIMATION);
            }
        }


        private StackLayout SearchLayoutCreation(AbsoluteLayout rootLayout, double pageWidth)
        {
            SearchLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Opacity = 0.9
            };
            searchEntry = new Entry
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Placeholder = "Введите текст для поиска",                                       //  "Enter text for search"
                BackgroundColor = Color.LightGray,
            };

            searchEntry.Completed += (object s, EventArgs ev) => { };
            searchEntry.TextChanged += (object s, TextChangedEventArgs ev) => this.WSearchText(searchEntry.Text);
            searchEntry.Focused += SearchEntry_Focused;
            searchEntry.Unfocused += SearchEntry_Focused;

            var searchFrame = new Frame
            {
                Content = searchEntry,
                Padding = new Thickness(0),
                Margin = new Thickness(3),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                /*HasShadow = true,
                OutlineColor = Color.Red,             // material design //*/
                CornerRadius = 10,
                IsClippedToBounds = true             // border-radius //*/
            };

            SearchLayout.Children.Add(searchFrame);
            rootLayout.Children.Add(SearchLayout, new Rectangle(0, 0, pageWidth, 50), AbsoluteLayoutFlags.None);
            
            if (animeted) SmoothAppearance(pageWidth, () => searchEntry.Focus());
            else searchEntry.Focus();

            return SearchLayout;
        }

        private void SmoothAppearance(double pageWidth, Action onFinish = null)
        {

            uint overTime = animateLong;
            uint step = overTime / 25;

                       
            var searchFrameAppearAnimation = new Animation(v =>
                {
                    AbsoluteLayout.SetLayoutBounds(SearchLayout, new Rectangle(0, 0, pageWidth, v));

                }, 0, 50);            

            searchFrameAppearAnimation.Commit(SearchLayout, SHOW_ANIMATION, step, overTime, Easing.Linear, (v, c) => {

                    onFinish?.Invoke();

                }, () => false
            );


            var SearchLayoutAppearAnimation = new Animation
            (
                v => AbsoluteLayout.SetLayoutBounds(listView, new Rectangle(0, v, 1, 0.9)), 0, 55
            )
            .Apply(listView, VIEW_OUT_ANIMATION, step, overTime, Easing.Linear, null, () => false);
           
        }


        private void SmoothHide(uint overTime = 500, Action onFinish = null)
        {

            double pageWidth = SearchLayout.Width;
            uint step = overTime / 25;

            var searchFrameAppearAnimation = new Animation(v =>
            {
                AbsoluteLayout.SetLayoutBounds(SearchLayout, new Rectangle(0, 0, pageWidth, v));
            }, 50, 0);

            searchFrameAppearAnimation.Commit(
                SearchLayout, HIDE_ANIMATION, step, overTime, Easing.Linear, (v, c) => {

                    onFinish?.Invoke();

                }, () => false
            );

            var SearchLayoutAppearAnimation = new Animation(v =>
            {
                AbsoluteLayout.SetLayoutBounds(listView, new Rectangle(0, v, 1, 0.9));

            }, 55, 0);

            SearchLayoutAppearAnimation.Commit(listView, VIEW_IN_ANIMATION, step, overTime, Easing.Linear, null, () => false);//*/

        }


        private void WSearchText(string searchedText)
        {

            // if (searchedText.Length > 0) AbsoluteLayout.SetLayoutBounds(listView, new Rectangle(0, 55, 1, 0.9));


            if (itemSource == null) itemSource = listView.ItemsSource as IList<T>;

            listView.ItemsSource = itemSource.Where(item => {

                try
                {
                    var r = item.ToString().ToLower().Contains(searchedText.ToLower());
                    return r;
                }
                catch(Exception ex) { Console.Write(ex.Message); }

                return false;
                
            });
        }


        public static SearchPanel<T> Initialize(ContentPage page, View subBtn = null) => new SearchPanel<T>(page, subBtn);


        private void SearchEntry_Focused(object sender, FocusEventArgs e)
        {
            if (e.IsFocused)    
            {
                // для чего это условие?:                
                // if (itemSource != null && listView.ItemsSource != itemSource) listView.ItemsSource = itemSource;  

                bottomView.IsVisible = false;                
            }
            else // close all
            {
                var offTime = 150;

                Utils.CallAfter(offTime, () => {

                    if (animeted) SmoothHide(animateLong, () => SearchLayout.IsVisible = false);
                    else
                    {
                        SearchLayout.IsVisible = false;

                        if (listView.Parent is AbsoluteLayout) AbsoluteLayout.SetLayoutBounds(listView, new Rectangle(0, 0, 1, 0.9));
                    }

                });

                Utils.CallAfter(offTime + 150, () => bottomView.IsVisible = true);

            }

            SearchButton.StateUpdate();

        }

    }

    

}
