using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Xamarin.Forms;
using XxmsApp.Api;
using XxmsApp.Api.Droid;

[assembly: Dependency(typeof(XxmsApp.Droid.Dependencies.Player))]
namespace XxmsApp.Droid.Dependencies
{
    public class Player : IPlayer
    {
        private static Player instance = null;
        internal static Player Instance
        {
            get => instance ?? (instance = new Player());
            set => instance = value;
        }

        internal static Android.Media.Ringtone currentSound = null;
        internal static Android.Media.MediaPlayer currentMelody = null;

        public Player() : base() => Instance = this;

        public (string, string, string) GetDefaultSound()
        {
            var context = Android.App.Application.Context;
            Android.Media.Ringtone ringtone = null;

            var uri = Android.Media.RingtoneManager.GetDefaultUri(Android.Media.RingtoneType.Notification);
            var path = uri != null ? string.Join(":", uri?.Scheme, uri?.SchemeSpecificPart) : null;
            if (uri is Android.Net.Uri)
            {
                ringtone = Android.Media.RingtoneManager.GetRingtone(context, uri);
            }

            var title = ringtone?.GetTitle(context);
            if (title != null)
            {
                var m = new Regex(@"\((\w+)\.").Match(title);
                if (m.Groups.Count > 1) title = m.Groups[1].Value;
            }

            return (title, path, "Notification");
        }

        public List<(string, string, string)> GetStockSounds()
        {

            var sounds = new List<(string, string, string)>();

            Android.Media.RingtoneManager ringtoneMgr = null;

            Android.Media.RingtoneType[] types =
            {
                Android.Media.RingtoneType.Alarm,
                Android.Media.RingtoneType.Notification,
                Android.Media.RingtoneType.Ringtone
            };

            foreach (var ringtoneType in types)
            {
                // Android.Media.RingtoneType.All

                (ringtoneMgr = new Android.Media.RingtoneManager(XxmsApp.Droid.MainActivity.Instance)).SetType(ringtoneType);

                var cursor = ringtoneMgr.Cursor;//*/

                while (!cursor.IsAfterLast && cursor.MoveToNext())
                {
                    var title = cursor.GetString(cursor.GetColumnIndex("title"));
                    var _uri = ringtoneMgr.GetRingtoneUri(cursor.Position);
                    var path = string.Join(":", _uri.Scheme, _uri.SchemeSpecificPart);

                    sounds.Add((title, path, ringtoneType.ToString()));
                }

                // cursor.MoveToPosition(0);

            }


            /*
            var names = cursor.GetColumnNames();
            var cc = cursor.ColumnCount;
            var fields = cursor.Extras.KeySet().ToList();
            //*/

            return sounds;

        }

        public void SelectExternalSound(Action<SoundMusic> onselect)
        {

            XxmsApp.Droid.MainActivity.Instance.ReceiveActivityResult = (Android.Net.Uri uri, object subj) =>
            {
                onselect?.Invoke(new SoundMusic(uri.Path, string.Join(":", uri.Scheme, uri.EncodedSchemeSpecificPart)));
            };

            Intent audio_picker_intent = new Intent(Intent.ActionGetContent);
            audio_picker_intent.SetType("audio/*");
            XxmsApp.Droid.MainActivity.Instance.StartActivityForResult(audio_picker_intent, OnResult.IsAudio.ToInt());


            /*
            Intent audio_picker_intent = new Intent(
                    Intent.ActionPick,
                    Android.Provider.MediaStore.Audio.Media.ExternalContentUri
                );                   
            XxmsApp.Droid.MainActivity.Instance.StartActivityForResult(audio_picker_intent, REQ_PICK_AUDIO);//*/
        }



        /// <summary>
        /// Stop current ringtone playing if playing was started by RingtoneManager,
        /// Unless of you need pass OnFinish action for playing stop
        /// </summary>
        public void StopSound()
        {
            currentSound?.Stop();
            currentMelody?.Stop();
        }

        public int SoundPlay(XxmsApp.Sound sound, Action<string> onFinish, Action<Exception> OnError)
        {
            return SoundPlay(
                sound.RingtoneType == typeof(SoundMusic).Name
                    ? sound.Path
                    : sound.Name,
                sound.RingtoneType,
                onFinish,
                OnError);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">url или имя русурса для воспроизведения</param>
        /// <param name="soundType">указывается, если в параметр path передается имя</param>
        /// <param name="onFinish">метод, кторый будет вызван по завершении воспроизведения</param>
        /// <returns>вернет продолжительность воспроизведения, если указан soundType, иначе - -1</returns>
        public int SoundPlay(string path, string soundType, Action<string> onFinish, Action<Exception> OnError)
        {
            var context = Android.App.Application.Context;

            if (string.IsNullOrEmpty(soundType) == false)
            {

                string sound = soundType == typeof(SoundMusic).Name
                    ? path
                    : $@"/system/media/audio/{soundType.ToLower()}s/{path}.ogg";

                StopSound();                                                   // currentMelody?.Stop();
                var player = currentMelody = new Android.Media.MediaPlayer();
                player.Reset();

                try
                {
                    var soundUri = Android.Net.Uri.Parse(sound);                // player.SetDataSource(sound); - just for asci
                    player.SetDataSource(context, soundUri);                    // player.SetDataSource(context, Urim);
                    player.Prepare();
                    // var duration = player.Duration;

                    player.Start();
                    player.Completion += (object sender, EventArgs e) => onFinish?.Invoke(sound);

                    return 1; // duration;   
                }
                catch (Exception ex)
                {
                    var er = ex.Message;
                    OnError?.Invoke(ex);
                    return -2;
                }

            }

            if (currentSound != null) currentSound.Stop();
            var uri = Android.Net.Uri.Parse(path);
            currentSound = Android.Media.RingtoneManager.GetRingtone(context, uri);
            currentSound.Play();

            return -1;

        }



    }
}