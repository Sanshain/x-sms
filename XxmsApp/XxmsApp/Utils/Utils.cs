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
        Hidden = 1,
        Visible = 2,
        InSearch = 4
    }

    public class SearchToolbarButton : ToolbarItem
    {

        public static Dictionary<SearchPanelState, string> Icons = new Dictionary<SearchPanelState, string>
        {
            { SearchPanelState.Hidden, "i_search.png" },
            { SearchPanelState.Visible, "c_search.png" },
            { SearchPanelState.Hidden | SearchPanelState.InSearch, "r_search.png" },                                   // { SearchPanelState.InSearch, "i_search.png" }            
            { SearchPanelState.Visible | SearchPanelState.InSearch, "d_search.png" }, 
        };

        public bool StateByFocus = false;

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
            var text = (((SearchLayout.Children.First() as Frame).Content as Frame).Content as Entry).Text;

            if (SearchLayout.IsVisible && !onHide) this.State = SearchPanelState.Visible;
            else
            {                
                this.State = SearchPanelState.Hidden;
            }

            if (!string.IsNullOrEmpty(text))
            {
                // this.State = this.State | SearchPanelState.InSearch;
            }
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
        public Image SearchInMessages;
        private int SearchImageTop = 5;
        Entry searchEntry = null;

        public bool IsVisible { get; set; }
        private ListView listView = null;
        private View bottomView = null;
        private bool animeted = true;
        private double PageWidth;

        private const uint animateLong = 250;

        private IList<T> itemSource = null;
        Color OnMessageSearchColor = Color.DarkGray;

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

                if (SearchLayout.IsVisible) return;                                                     // Also impoertant as State attribute                
                if (SearchButton.State == (SearchPanelState.Hidden | SearchPanelState.InSearch))        // Clean
                {
                    
                    (rootLayout.Parent as ContentPage).Title = "Диалоги";
                    searchEntry.Text = "";
                    
                    var animate_in = new Animation(v => listView.Scale = v, 1, 0.9);                   // анимация scale для listView
                    listView.Animate("list_in", animate_in, finished: (v, b) =>
                    {
                        var animate_out = new Animation(_v => listView.Scale = _v, 0.9, 1);
                        listView.Animate("list_out", animate_out, finished: (_v, _b) => { });                        
                    });
                    
                }
                else if (SearchButton.State == SearchPanelState.Hidden)                                 // Show cleaned
                {
                    this.PreventAnimation();

                    if (SearchLayout.IsVisible = !SearchLayout.IsVisible)  //  true
                    {
                        var searchFrame = SearchLayout.Children.FirstOrDefault() as Frame;

                        if (animeted)
                        {
                            void FinishAction() => searchEntry?.Focus();       // Action FinishAction = () => searchFrame?.Content?.Focus();
                            SmoothAppearance(page.Width, FinishAction);
                        }
                        else searchFrame?.Content?.Focus();

                        // (rootLayout.Parent as ContentPage).Title = "Диалоги";
                        // searchEntry.Text = "";
                    }                    

                }

                SearchButton.StateUpdate();

            }
            else
            {
                SearchButton.SearchLayout = SearchLayout = SearchLayoutCreation(rootLayout, page.Width);

                SearchButton.StateUpdate();
            }

            

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
            (SearchButton.ContentLayout.Parent as MainPage).OnBackPressed = () =>
            {
                if (string.IsNullOrEmpty(searchEntry.Text)) return false;
                else
                {
                    searchEntry.Text = "";
                    
                    // var di = DependencyService.Get<Api.Utilites.IUtilites>(DependencyFetchTarget.NewInstance);
                    // di.Vibrate(400);

                    return true;
                }
            };

            var searchFrame = new Frame
            {
                Content = searchEntry,
                Padding = new Thickness(0),
                Margin = new Thickness(2),
                HorizontalOptions = LayoutOptions.FillAndExpand,
                /*HasShadow = true,
                OutlineColor = Color.Red,             // material design //*/
                CornerRadius = 10,
                IsClippedToBounds = true             // border-radius //*/
            };

            var FrameContainer = new Frame
            {
                BackgroundColor = Color.Blue,
                Padding = new Thickness(0),
                Content = searchFrame,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                CornerRadius = 12,
                IsClippedToBounds = true 
            };




            SearchLayout.Children.Add(FrameContainer);
            rootLayout.Children.Add(SearchLayout, new Rectangle(0, 0, pageWidth, 50), AbsoluteLayoutFlags.None);


            SearchInMessages = new Image
            {
                BackgroundColor = Color.Transparent,
                Source = new FileImageSource { File = "deep_search.png" },                
                // Opacity = 0
            };
            var InsideMessagesFinder = new TapGestureRecognizer();
            InsideMessagesFinder.Tapped += (s,e) => InsideMessagesFinder_Tapped(s,e);            
            SearchInMessages.GestureRecognizers.Add(InsideMessagesFinder);            
            rootLayout.Children.Add(SearchInMessages, new Rectangle(rootLayout.Width - 65, 5, 35, 35));


            itemSource = listView.ItemsSource as IList<T>;

            if (animeted) SmoothAppearance(pageWidth, () => { EmptyLabelCreation(); searchEntry.Focus();});
            else searchEntry.Focus();

            return SearchLayout;
        }


        private void EmptyLabelCreation()
        {
            lblFailed = new Label
            {
                Text = "Ничего не найдено :(",
                Opacity = 0.7,
                Scale = 1,
                IsVisible = false
            };

            SearchButton.ContentLayout.Children.Add(
                lblFailed,
                new Rectangle(0.5, 200, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize),
                AbsoluteLayoutFlags.XProportional
            );//*/


        }

        private int InsideMessagesFinder_Tapped(object sender, EventArgs e)
        {
            // (rootLayout.Parent as ContentPage).DisplayAlert("Focused", true.ToString(), "ok");

            if (string.IsNullOrWhiteSpace(searchEntry.Text)) return -1;

            List<T> dialogs = new List<T>();

            if (SearchInMessages.Scale == 1)
            {

                dialogs = itemSource.Where(dial =>
                {
                    return (dial as Dialog).Messages.Any(m => m.Value.ToLower().Contains(searchEntry.Text.ToLower()));
                }).ToList();

                (searchEntry.Parent.Parent as Frame).BackgroundColor = OnMessageSearchColor;
                SearchInMessages.ScaleTo(0.7f);
            }
            else
            {
                dialogs = itemSource.Where(dial =>
                {
                    return (dial as Dialog).Address?.ToLower().Contains(searchEntry.Text.ToLower()) ?? false;
                }).ToList();

                (searchEntry.Parent.Parent as Frame).BackgroundColor = Color.Blue;
                SearchInMessages.ScaleTo(1);
            }

            listView.ItemsSource = dialogs;            

            return dialogs.Count;

            /*
            SearchInMessages.Animate("down", new Animation(v => SearchInMessages.Scale = v, 1, 0.7f), finished: (v, b) =>
            {
                SearchInMessages.ScaleTo(1);
            });//*/

        }

        private void SmoothAppearance(double pageWidth, Action onFinish = null)
        {

            uint overTime = animateLong;
            uint step = overTime / 25;

            SearchInMessages.Animate("3", new Animation(o =>
            {
                AbsoluteLayout.SetLayoutBounds(SearchInMessages, new Rectangle(SearchButton.ContentLayout.Width - 65, o, 35, 35));
            }, -45, 5), length: overTime / 2);

            // onFinish = null;//*/

            SearchLayout.Scale = 0.9;


            var searchFrameAppearAnimation =
                new Animation(v =>
                {
                    AbsoluteLayout.SetLayoutBounds(SearchLayout, new Rectangle(0, 0, pageWidth, v));
                    // SearchLayout.Scale = 0.2 * v / 55 + 0.8;

                }, 0, 50
            )
            .Apply(SearchLayout, SHOW_ANIMATION, step, overTime / 2, Easing.Linear, (v, c) =>
            {

                // SearchInMessages.Animate("1", new Animation(o => SearchInMessages.Opacity = o, 0, 1));

                onFinish?.Invoke();

            }, () => false);



            var SearchLayoutAppearAnimation = new Animation
            (
                v => {

                    // AbsoluteLayout.SetLayoutBounds(listView, new Rectangle(0, v, 1, 0.9));

                }, 0, 55
            )
            .Apply(listView, VIEW_OUT_ANIMATION, step, overTime, Easing.Linear, null, () => false);
                       
        }


        private void SmoothHide(uint overTime = 500, Action onFinish = null)
        {

            

            double pageWidth = SearchLayout.Width;
            uint step = overTime / 25;

            SearchInMessages.Animate("2", new Animation(o =>
            {
                AbsoluteLayout.SetLayoutBounds(SearchInMessages, new Rectangle(SearchButton.ContentLayout.Width - 65, o, 35, 35));
            }, 5, -45));

            var searchFrameAppearAnimation = new Animation(v =>
            {
                AbsoluteLayout.SetLayoutBounds(SearchLayout, new Rectangle(0, 0, pageWidth, v));
            }, 50, 0);

            searchFrameAppearAnimation.Commit(
                SearchLayout, HIDE_ANIMATION, step, overTime, Easing.Linear, (v, c) => {

                    onFinish?.Invoke();

                }, () => false
            );

            AbsoluteLayout.SetLayoutBounds(listView, new Rectangle(0, 55, 1, 0.9));
            listView.Header = null;

            if (AbsoluteLayout.GetLayoutBounds(listView).Top != 0)
            {
                var SearchLayoutAppearAnimation = new Animation(v =>
                {
                    AbsoluteLayout.SetLayoutBounds(listView, new Rectangle(0, v, 1, 0.9));

                }, 55, 0);

                SearchLayoutAppearAnimation.Commit(listView, VIEW_IN_ANIMATION, step, overTime, Easing.Linear, null, () => false);//*/
            }
            
        }

        Frame Container;
        Label lblFailed;
        private void WSearchText(string searchedText)
        {

            if (string.IsNullOrEmpty(searchedText)) (SearchButton.ContentLayout.Parent as ContentPage).Title = "Диалоги";
            else
            {
                // AbsoluteLayout.SetLayoutBounds(listView, new Rectangle(0, 55, 1, 0.9));
                listView.Header = new Frame
                {
                    HeightRequest = 55,
                    Content = new Label { Text = $"({itemSource.Count})" },
                    Padding = new Thickness(SearchButton.ContentLayout.Width - 85, 12, 0, 0)
                };
                (SearchButton.ContentLayout.Parent as ContentPage).Title = $"Диалоги ({searchEntry.Text})";

            }

            listView.ItemsSource = itemSource.Where(item =>
            {

                try
                {
                    var r = item.ToString().ToLower().Contains(searchedText.ToLower());
                    return r;
                }
                catch (Exception ex) { Console.Write(ex.Message); }

                return false;

            }).ToList();


            ShowHintBtnOnEmpty();

        }

        private void ShowHintBtnOnEmpty()
        {
            // if (lblFailed != null) lblFailed.IsVisible = false;            

            var searchCount = (listView.ItemsSource as IList<T>).Count;

            if (searchCount == 0 && string.IsNullOrEmpty(searchEntry.Text) == false)
            {

                // делаем видимой кнопку с предложением начать поиск внутри сообщений

                if (Container == null)  //  SearchButton.ContentLayout.Children.Contains(Container) == false
                {

                    const string messSearchBtnText = "Искать в сообщениях";
                    var y = SearchButton.ContentLayout.Height - 80; // / 2;

                    var buttonSearchInMessages = new Button
                    {
                        MinimumWidthRequest = SearchButton.ContentLayout.Width / 2 * 1.5,
                        MinimumHeightRequest = 70,
                        Text = messSearchBtnText,
                        Opacity = 0.8,
                        Margin = new Thickness(-5),
                        TextColor = Color.DarkSlateGray,
                        BackgroundColor = Color.Default
                    };
                    SearchButton.ContentLayout.Children.Add(Container = new Frame
                    {
                        Content = new Frame
                        {
                            Content = buttonSearchInMessages,
                            CornerRadius = 10,
                            Padding = new Thickness(-1),
                            Margin = new Thickness(3),
                            HasShadow = false,
                            BackgroundColor = Color.White
                        },
                        Scale = 0,
                        CornerRadius = 10,
                        IsClippedToBounds = true,
                        HasShadow = false,
                        Padding = new Thickness(-1),
                        Margin = new Thickness(0),
                        BackgroundColor = Color.DarkGray,
                        Opacity = 0.5
                    },
                        new Rectangle(0.5, y, SearchButton.ContentLayout.Width - 40, AbsoluteLayout.AutoSize),
                        AbsoluteLayoutFlags.XProportional
                    );
                    

                    buttonSearchInMessages.Clicked += async (object sender, EventArgs e) =>
                    {
                        
                        int cnt = InsideMessagesFinder_Tapped(sender, e);
                        // if (Container != null) Container.IsVisible = false;
                      
                        if (cnt == 0)
                        {
                            if (lblFailed.IsVisible == false)
                            {
                                lblFailed.IsVisible = true;
                                lblFailed.Scale = 0;
                                lblFailed?.ScaleTo(1);
                            }
                        }
                        else lblFailed.IsVisible = false;


                        // await Container.ScaleTo(0.9f);

                        // Container.IsVisible = false;
                        
                        if (SearchInMessages.Scale == 1) buttonSearchInMessages.Text = "Искать в адресах";
                        else
                            buttonSearchInMessages.Text = "Искать в сообщениях";

                        FormattedString searchedText = new FormattedString();
                        searchedText.Spans.Add(new Span { Text = "По " });
                        searchedText.Spans.Add(new Span
                        {
                            Text = searchEntry.Text.Substring(0, Math.Min(searchEntry.Text.Length - 1, 12)),
                            FontAttributes = FontAttributes.Bold
                        });
                        searchedText.Spans.Add(new Span { Text = " ничего не найдено :-(!" });
                        lblFailed.FormattedText = searchedText;

                        // var searchedText = searchEntry.Text.Substring(0, Math.Min(searchEntry.Text.Length - 1, 12));
                        // lblFailed.Text = $"По `{searchedText}` ничего не найдено :(";

                        lblFailed.Scale = 0;
                        lblFailed?.ScaleTo(1);
                        // Container.ScaleTo(1);

                    };

                }
                if (SearchInMessages.Scale == 1)
                {
                    if (Container.IsVisible == false) Container.Scale = 0;
                    Container.IsVisible = true;
                    Container.ScaleTo(1);
                }
                else if (lblFailed.IsVisible == false)
                {                                        
                    lblFailed.IsVisible = true;
                    lblFailed.Scale = 0;
                    lblFailed?.ScaleTo(1);                    
                }
                lblFailed.Text = "Ничего не найдено :-(";
            }
            else if (Container != null)
            {
                Container.IsVisible = false;
                lblFailed.IsVisible = false;
                (searchEntry.Parent.Parent as Frame).BackgroundColor = Color.Blue;
                SearchInMessages.ScaleTo(1);
            }
        }

        public static SearchPanel<T> Initialize(ContentPage page, View subBtn = null) => new SearchPanel<T>(page, subBtn);

        public bool StateByUnFocused = false; // SearchButton.StateByFocus using instead

        private void SearchEntry_Focused(object sender, FocusEventArgs e)
        {

            if (e.IsFocused)
            {
                // для чего это условие?:                
                // if (itemSource != null && listView.ItemsSource != itemSource) listView.ItemsSource = itemSource;  

                bottomView.IsVisible = false;

                // I can do it by click:
                SearchButton.StateUpdate();
            }
            else // close all
            {
                var offTime = 150;
               
                SearchButton.State = SearchPanelState.Hidden;
                if (string.IsNullOrEmpty(searchEntry.Text) == false)
                {
                    // SearchButton.State |= SearchPanelState.InSearch;
                }

                StateByUnFocused = true;
                Utils.CallAfter(offTime, () => {

                    // SearchButton.StateUpdate(hideIntent);                  

                    if (animeted) SmoothHide(animateLong, () =>
                        {                            
                            SearchLayout.IsVisible = false;
                            SearchButton.Icon = new FileImageSource { File = SearchToolbarButton.Icons[SearchButton.State] };
                            StateByUnFocused = false;
                        });
                    else
                    {
                        SearchLayout.IsVisible = false;

                        if (listView.Parent is AbsoluteLayout)
                        {
                            AbsoluteLayout.SetLayoutBounds(listView, new Rectangle(0, 0, 1, 0.9));
                        }
                    }

                });

                Utils.CallAfter(offTime + 150, () => bottomView.IsVisible = true);

            }

            


            /*
            if (SearchButton.State == (SearchPanelState.Hidden | SearchPanelState.InSearch))
            {

                (SearchButton.ContentLayout.Parent as ContentPage).Title = "Диалоги";
                searchEntry.Text = "";

                var animate_in = new Animation(v => listView.Scale = v, 1, 0.9);        // анимация scale для listView
                listView.Animate("list_in", animate_in, finished: (v, b) =>
                {
                    var animate_out = new Animation(_v => listView.Scale = _v, 0.9, 1);
                    listView.Animate("list_out", animate_out, finished: (_v, _b) => { });
                });
            }
            //*/
        }

    }


}
