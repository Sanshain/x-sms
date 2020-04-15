using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace XxmsApp
{
    public static class Utils
    {



        //public static Constraint Let(this double arg) => Constraint.Constant(arg);
        public static Constraint Let(this double? arg) => arg != null ? Constraint.Constant(arg.Value) : null;
        public static Constraint Let(this Func<RelativeLayout, double> func)// => Constraint.RelativeToParent(func);
        {
            return func != null ? Constraint.RelativeToParent(func) : null;
        }
        


        /// <summary>
        /// Append view element to RelativeLayout (more convenient to use). Its recommended
        /// </summary>
        /// <param name="self">RelativeLayout.IRelativeList<View> Children</param>
        /// <param name="view">view for additional to RelativeLayout</param>
        /// <param name="x">x constant</param>
        /// <param name="y">y constant</param>
        /// <param name="W">w - func</param>
        /// <param name="H">h - func</param>
        public static View AddAsRelative(this RelativeLayout.IRelativeList<View> self,
            View view,
            double x = 0,
            double y = 0,
            Func<RelativeLayout, double> W = null, 
            Func<RelativeLayout, double> H = null)
        {
            self.Add(view,
                Constraint.Constant(x),
                Constraint.Constant(x),
                W != null ? Constraint.RelativeToParent(W) : null,
                H != null ? Constraint.RelativeToParent(H) : null);

            return view;
        }



        public static View AddAsRelative(this RelativeLayout.IRelativeList<View> self,
            View view,
            Func<RelativeLayout, double> X = null,
            Func<RelativeLayout, double> Y = null,
            Func<RelativeLayout, double> W = null,
            Func<RelativeLayout, double> H = null)
        {
            self.Add(view, X.Let(), Y.Let(), W.Let(), H.Let());

            return view;
        }



        /// <summary>
        /// Add range for StackLayout
        /// </summary>
        /// <param name="self"></param>
        /// <param name="views"></param>
        public static void AddRange(this IList<View> self, View[] views)
        {
            foreach (var view in views)
            {
                self.Add(view);
            }
        }


        public static void Extend(this IList<View> self, params View[] views)
        {
            foreach (var view in views)
            {
                self.Add(view);
            }
        }

        public static StackLayout AddChilds(this StackLayout self, params View[] views)
        {
            foreach (var view in views)
            {
                self.Children.Add(view);
            }

            return self;
        }

        

    }



    public class SearchPanel
    {

        public ToolbarItem SearchButton { get; private set; } = null;
        public StackLayout SearchLayout { get; private set; } = null;

        private View bottomView = null;


        public SearchPanel(ContentPage page, View subView = null)
        {
            AbsoluteLayout rootLayout = page.Content as AbsoluteLayout;
            bottomView = subView ?? rootLayout.Children.Last();

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
                else
                {
                    SearchLayout = new StackLayout
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

                    searchEntry.Completed += (object s, EventArgs ev) =>
                    {
                        SearchLayout.IsVisible = false;


                    };

                    searchEntry.Focused += SearchEntry_Focused;
                    searchEntry.Unfocused += SearchEntry_Focused;

                    SearchLayout.Children.Add(searchEntry);
                    rootLayout.Children.Add(SearchLayout, new Rectangle(0, 0, page.Width, 50), AbsoluteLayoutFlags.None);

                    searchEntry.Focus();

                }

            };

        }

        public static SearchPanel Initialize(ContentPage page, View subBtn = null)
        {
            return new SearchPanel(page, subBtn);
        }

        private void SearchEntry_Focused(object sender, FocusEventArgs ev)
        {
            if (ev.IsFocused)
            {
                (sender as Entry).Text = "";
                bottomView.IsVisible = false;
            }
            else
            {
                SearchLayout.IsVisible = false;

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
