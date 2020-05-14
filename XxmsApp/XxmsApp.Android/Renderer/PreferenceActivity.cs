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
using Android.Support.V7.Preferences;
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

            this.FragmentManager.BeginTransaction().Add(
                Resource.Id.frameLayout1,
                new SettingsFragment()).Commit();



            // dynamic append:
            var content = this.FindViewById<FrameLayout>(Resource.Id.frameLayout1);
            Button b = new Button(this)
            {
                Text = "Button added dynamically!",
                LayoutParameters = new ViewGroup.LayoutParams(
                    ViewGroup.LayoutParams.MatchParent,
                    ViewGroup.LayoutParams.WrapContent)
            };            
            (content.Parent as LinearLayout)?.AddView(b);                                                    // b.Id = 523; // Resource.Id.frameLayout1;



            // event add control for building child tree:
            if ((content.Parent is LinearLayout root))
            {
                root.ChildViewAdded += Root_ChildViewAdded;
            }

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
    }

    public class SettingsFragment : PreferenceFragment
    {

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            AddPreferencesFromResource(Resource.Xml.preferences);
        }
    }
    
    
    public class SettingsPickerFragment : Android.Support.V7.Preferences.PreferenceFragmentCompat
    {
        public override void OnCreatePreferences(Bundle savedInstanceState, string rootKey)
        {
            try
            {
                SetPreferencesFromResource(Resource.Xml.prefcompat, rootKey);
            }
            catch(Exception ex)
            {
                var er = ex.Message;
            }
        }

        const int REQUEST_CODE_ALERT_RINGTONE = 0;



        public override bool OnPreferenceTreeClick(Android.Support.V7.Preferences.Preference preference)
        {
            const string KEY_RINGTONE_PREFERENCE = "alerts_ringtone";
            
            if (preference.Key.Equals(KEY_RINGTONE_PREFERENCE))
            {
                Intent intent = new Intent(Android.Media.RingtoneManager.ActionRingtonePicker);
                intent.PutExtra(Android.Media.RingtoneManager.ExtraRingtoneType, (int)Android.Media.RingtoneType.Notification);
                intent.PutExtra(Android.Media.RingtoneManager.ExtraRingtoneType, true);
                intent.PutExtra(Android.Media.RingtoneManager.ExtraRingtoneShowSilent, true);
                intent.PutExtra(Android.Media.RingtoneManager.ExtraRingtoneDefaultUri, Android.Provider.Settings.System.DefaultNotificationUri);

                StartActivityForResult(intent, REQUEST_CODE_ALERT_RINGTONE);

                return true;
            }
            else
            {
                return base.OnPreferenceTreeClick(preference);
            }
        }

        
        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            int REQUEST_CODE_ALERT_RINGTONE = 1;
            if (requestCode == REQUEST_CODE_ALERT_RINGTONE && data != null)
            {
                var ringtone = data.GetParcelableExtra(Android.Media.RingtoneManager.ExtraRingtonePickedUri);
                if (ringtone != null)
                {                    
                    // setRingtonPreferenceValue(ringtone.toString()); // TODO

                }
                else
                {
                    // "Silent" was selected
                    // setRingtonPreferenceValue(""); // TODO
                }
            }
            else
            {
                base.OnActivityResult(requestCode, resultCode, data);
            }
        }//*/

    }
    //*/
    
    [Activity(Label = "PreferenceAppCompatActivity", Theme = "@style/SettingsTheme")]
    public class MelodyPicker : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            this.SetContentView(Resource.Layout.preferencesettings);

            SupportFragmentManager.BeginTransaction().Replace(Resource.Id.frameLayout1, new SettingsPickerFragment()).Commit();            
        }
    }
    //*/
}

