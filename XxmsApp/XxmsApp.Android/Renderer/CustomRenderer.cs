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
using XxmsApp.Views;
using XxmsApp.Droid;
using XxmsApp.Views.Droid;


[assembly: ExportRenderer(typeof(NavPage), typeof(NavPageRenderer))]
[assembly: ExportRenderer(typeof(HeaderFrame), typeof(HeaderFrameRenderer))]
namespace XxmsApp.Views.Droid
{




    public class HeaderFrameRenderer : Xamarin.Forms.Platform.Android.AppCompat.FrameRenderer
    {
        static bool initialize = false;

        public HeaderFrameRenderer(Context context) : base(context) { }

        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
            if (initialize == false)
            {

                initialize = true;
                if (Control is Android.Widget.FrameLayout frame)
                {
                    // frame.AddView()
                    
                    if (Context is FormsAppCompatActivity activity)
                    {
                        var fragment = new SettingsFragment();
                        var transaction = activity.FragmentManager.BeginTransaction();
                        transaction.Add(Control.Id, fragment).Commit();

                    }

                }
                // this.SetBackgroundColor(Android.Graphics.Color.Red);
            }            
        }

    }







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
            item.FocusChange += Item_FocusChange;
        }

        private void Item_FocusChange(object sender, FocusChangeEventArgs e)
        {
            if (e.HasFocus)
            {

            }
        }

        private void Item_Click(object sender, EventArgs e)
        {            
            
            if (sender is ActionMenuItemView item)
            {
                var duration = 125;
                bool clicked = false;

                /*
                var navpgItms = (Element as NavigationPage).ToolbarItems.ToArray();
                var s1 = (Element as NavigationPage).RootPage.ToolbarItems.ToArray();
                var r = (Element as NavigationPage).CurrentPage.ToolbarItems.ToArray();
                bool b = s1.FirstOrDefault() == r.FirstOrDefault();
                //*/


                var navPage = Element as NavigationPage;
                var btn = navPage.CurrentPage.ToolbarItems.First() as SearchToolbarButton;          // RootPage
                
                if (btn.State == (SearchPanelState.Hidden | SearchPanelState.InSearch) || navPage.CurrentPage != navPage.RootPage)
                {
                    btn.ItemClicked();                                                              // occurs click event on X.Forms project
                    clicked = true;
                }//*/

                objectAnimator = ObjectAnimator.OfFloat(item, "Alpha", 0.3f);
                ObjectAnimator scaleDownX = ObjectAnimator.OfFloat(item, "ScaleX", 0.7f);
                ObjectAnimator scaleDownY = ObjectAnimator.OfFloat(item, "ScaleY", 0.7f);                
                scaleDownX.SetDuration(duration);
                scaleDownY.SetDuration(duration);
                AnimatorSet scaleDown = new AnimatorSet();
                scaleDown.Play(scaleDownX).With(scaleDownY).With(objectAnimator);
                scaleDown.Start();
                
                scaleDown.AnimationEnd += (object s, EventArgs ev) =>
                {
                    // btn.Icon = new FileImageSource { File = SearchToolbarButton.Icons[btn.State] };                    


                    if (clicked == false) btn.ItemClicked();


                    var image = SearchToolbarButton.Icons[btn.State].Split('.')[0];
                    var d = item.Context.GetDrawable(image);
                    item.SetIcon(d);

                    var a = ObjectAnimator.OfFloat(item, "Alpha", 1);   // item.Alpha == 0 ? 1 : 0
                    ObjectAnimator scaleDownX1 = ObjectAnimator.OfFloat(item, "ScaleX", 1);
                    ObjectAnimator scaleDownY1 = ObjectAnimator.OfFloat(item, "ScaleY", 1);
                    scaleDownX1.SetDuration(duration * 2);
                    scaleDownY1.SetDuration(duration * 2);
                    AnimatorSet scaleDown1 = new AnimatorSet();
                    scaleDown1.Play(scaleDownX1).With(scaleDownY1).With(a);
                    scaleDown1.Start();
                    scaleDown1.AnimationEnd += delegate { };

                };
            }
        }

    }

}
//*/
