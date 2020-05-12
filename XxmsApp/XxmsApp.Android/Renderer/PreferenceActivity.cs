using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace XxmsApp.Droid
{
    [Activity(Label = "PreferenceActivity")]
    public class RingtonPicker : PreferenceActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);            
            base.AddPreferencesFromResource(Resource.Xml.preferences);            
        }
    }

}