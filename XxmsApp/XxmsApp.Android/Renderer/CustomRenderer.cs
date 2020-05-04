
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

using Android.Support.V7.Widget;
using XxmsApp;
using XxmsApp.Views.Droid;
using Android.Animation;
using Android.Support.V7.View.Menu;

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
                objectAnimator = ObjectAnimator.OfFloat(item, "Alpha", item.Alpha == 0 ? 1 : 0);               
                objectAnimator.SetDuration(300);
                objectAnimator.Start();
                objectAnimator.AnimationEnd += (object s, EventArgs ev) =>
                {
                    var btn = (Element.ToolbarItems.First() as SearchToolbarButton);
                    btn.Icon = new FileImageSource { File = SearchToolbarButton.Icons[State] };
                    // Element.ToolbarItems[0].Icon = 
                    // item.SetIcon(item.Context.GetDrawable(""));
                    var a = ObjectAnimator.OfFloat(item, "Alpha", item.Alpha == 0 ? 1 : 0);
                    a.SetDuration(300);
                    a.Start();
                };
            }
        }

    }

}
