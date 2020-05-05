using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Linq;


using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Content;
using Android.Graphics;
using static Android.Support.V7.Widget.ActionMenuView;
using Android.Support.V7.View.Menu;
using Android.Support.V7.Widget;
using Android.Animation;

using XxmsApp;



using XxmsApp.Views.Droid;


[assembly: ExportRenderer(typeof(NavPage), typeof(NavPageRenderer))]
namespace XxmsApp.Views.Droid
{

    public class NavPageRenderer : Xamarin.Forms.Platform.Android.AppCompat.NavigationPageRenderer
    {

        ObjectAnimator objectAnimator;
        
        public NavPageRenderer(Context context) : base(context) { }

        public override void OnViewAdded(Android.Views.View child)
        {
            base.OnViewAdded(child);            

            if (child is Android.Support.V7.Widget.Toolbar)
            {                
                var toolBar = child as Android.Support.V7.Widget.Toolbar;
                toolBar.ChildViewAdded += ToolBar_ChildViewAdded;
            }
        }

        private void ToolBar_ChildViewAdded(object sender, ChildViewAddedEventArgs e)
        {
            if (e.Child is Android.Support.V7.Widget.ActionMenuView)
            {
                var mnuWrapper = (e.Child as Android.Support.V7.Widget.ActionMenuView);
                mnuWrapper.ChildViewAdded += MnuWrapper_ChildViewAdded;

                // mnuWrapper.SetBackgroundColor(global::Android.Graphics.Color.Orange);
            }
        }        

        private void MnuWrapper_ChildViewAdded(object sender, ChildViewAddedEventArgs e)
        {
            var item = e.Child;                                 // ActionMenuPresenter            
            item.Click += Item_Click;

        }

        private void Item_Click(object sender, EventArgs e)
        {            
            
            if (sender is ActionMenuItemView item)
            {

                var element = (Element as NavigationPage).RootPage;
                var btn = (element.ToolbarItems.First() as SearchToolbarButton);

                btn.ItemClicked();

                objectAnimator = ObjectAnimator.OfFloat(item, "Alpha", item.Alpha == 0 ? 1 : 0);               
                objectAnimator.SetDuration(300);
                objectAnimator.Start();
                objectAnimator.AnimationEnd += (object s, EventArgs ev) =>
                {

                    // btn.Icon = new FileImageSource { File = SearchToolbarButton.Icons[btn.State] };

                    var image = SearchToolbarButton.Icons[btn.State].Split('.')[0];
                    var d = item.Context.GetDrawable(image);
                    item.SetIcon(d);

                    var a = ObjectAnimator.OfFloat(item, "Alpha", 1);   // item.Alpha == 0 ? 1 : 0
                    a.SetDuration(1000);
                    a.Start();
                };
            }
        }

    }

}
//*/
