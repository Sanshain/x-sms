using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

namespace XxmsApp.Droid
{


    [Activity(Label = "PreferenceActivity")]
    public class RingtonPicker : Activity // : PreferenceActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            this.SetContentView(Resource.Layout.preferencesettings);
            var content = this.FindViewById<FrameLayout>(Resource.Id.frameLayout1);
            if ((content.Parent is LinearLayout root))
            {
                root.ChildViewAdded += Root_ChildViewAdded;
            }

            this.FragmentManager.BeginTransaction().Add(
                Resource.Id.frameLayout1,
                new SettingsFragment()).Commit();

        }

        private void Root_ChildViewAdded(object sender, ViewGroup.ChildViewAddedEventArgs e)
        {
            // throw new NotImplementedException();
        }

        private void SetAnswer(String message)
        {

            Intent data = new Intent();
            data.PutExtra("key", "myvalue");
            this.SetResult(Result.Ok, data);            
            this.Finish();

            // this.FinishActivity(0);

        }


        public class SettingsFragment : PreferenceFragment
        {

            public override void OnCreate(Bundle savedInstanceState)
            {
                base.OnCreate(savedInstanceState);

                AddPreferencesFromResource(Resource.Xml.preferences);
            }
        }
    }

}

