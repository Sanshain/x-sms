﻿using System;

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
                
                var btn = ((Element as NavigationPage).RootPage.ToolbarItems.First() as SearchToolbarButton);

                

                /*
                objectAnimator = ObjectAnimator.OfFloat(item, "Alpha", item.Alpha == 0 ? 1 : 0);
                objectAnimator.SetDuration(duration);
                objectAnimator.Start();
                objectAnimator.AnimationEnd += (object s, EventArgs ev) =>
                {

                    var image = SearchToolbarButton.Icons[btn.State].Split('.')[0];
                    var d = item.Context.GetDrawable(image);
                    item.SetIcon(d);

                    var a = ObjectAnimator.OfFloat(item, "Alpha", 1);   // item.Alpha == 0 ? 1 : 0
                    a.SetDuration(duration);
                    a.Start();
                };//*/


                objectAnimator = ObjectAnimator.OfFloat(item, "Alpha", item.Alpha == 0 ? 1 : 0);

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


                    btn.ItemClicked();
                    /*
                    bool clicked = false;
                    if (btn.State.HasFlag(SearchPanelState.Hidden))
                    {
                        btn.ItemClicked();                  // occurs click event on X.Forms project
                        clicked = true;
                    }//*/

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
