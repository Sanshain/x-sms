using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace XxmsApp.Droid
{
    [Activity(Theme = "@style/SplashTheme", NoHistory = true, MainLauncher = true)]  
    public class SplashActivity : Activity                      // AppCompatActivity
    {
        

        protected override void OnCreate(Bundle savedInstanceState) // , PersistableBundle persistentState
        {
            base.OnCreate(savedInstanceState); // persistentState
            this.SetContentView(Resource.Layout.LPreloader);

            var logo = FindViewById(Resource.Id.logoText) as TextView;
            AnimatoOnAppearance(logo, () =>
            {
                var bar = FindViewById(Resource.Id.progressBar) as ProgressBar;
                bar.Visibility = ViewStates.Visible;
            });
            

            /* TextView text = new TextView(this) { Text = "123" };
            text.LayoutParameters = new ViewGroup.LayoutParams(
                LinearLayout.LayoutParams.WrapContent,
                LinearLayout.LayoutParams.WrapContent); //*/
        }


        private static void AnimatoOnAppearance(TextView logo, Action onEnd = null)
        {
            logo.ScaleX = 0;
            logo.ScaleY = 0;

            var duration = 500;
            ObjectAnimator scaleDownX = ObjectAnimator.OfFloat(logo, "ScaleX", 1f);
            ObjectAnimator scaleDownY = ObjectAnimator.OfFloat(logo, "ScaleY", 1f);
            
            scaleDownX.SetDuration(duration);
            scaleDownY.SetDuration(duration);

            AnimatorSet scaleDown = new AnimatorSet();
            // scaleDown.SetDuration(duration);
            scaleDown.Play(scaleDownX).With(scaleDownY);
            scaleDown.AnimationEnd += (s, e) => onEnd?.Invoke();

            scaleDown.Start();
        }


        /// <summary>
        /// Launches the startup task
        /// </summary>
        protected override void OnResume()
        {
            base.OnResume();
            Task startupWork = new Task(() => { SimulateStartup(); });
            startupWork.Start();
        }

        /// <summary>
        /// Simulates background work that happens behind the splash screen
        /// </summary>
        async void SimulateStartup()
        {            
            await Task.Delay(100);  

            StartActivity(new Intent(Application.Context, typeof(MainActivity)));
        }
    }
}