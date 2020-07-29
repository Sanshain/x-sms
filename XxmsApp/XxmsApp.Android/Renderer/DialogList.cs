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
using XxmsApp.Piece;

[assembly: ExportRenderer(typeof(MainList), typeof(XxmsApp.Droid.Renderer.UpdatebleListRenderer))]
namespace XxmsApp.Droid.Renderer
{

    class UpdatebleListRenderer : Xamarin.Forms.Platform.Android.ListViewRenderer
    {
        public UpdatebleListRenderer(Context context) : base(context) { }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.ListView> e)
        {
            base.OnElementChanged(e);

            // e.OldElement?.L(el => Control.ScrollChange -= ScrollChanged);
            // e.NewElement?.L(el => Control.ScrollStateChanged += Control_ScrollStateChanged;);
        }     

    }
}