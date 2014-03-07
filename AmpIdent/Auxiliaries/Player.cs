using System;
using System.Media;
using System.Windows.Controls;

namespace AmpIdent.Auxiliaries
{
    class Player
    {
        private static SoundPlayer _sp;
        private static Boolean _playing;

        public static void Play(Button sender, string path)
        {
            var button = sender;

            if (!_playing)
            {
                _sp = new SoundPlayer();
                _sp.Stop();
                _sp.SoundLocation = path;
                _sp.LoadAsync();
                _sp.Play();
                _playing = true;
                button.Content = "Stop";
            }
            else
            {
                _sp.Stop();
                _playing = false;
                button.Content = "Play";
            }
        }
    }
}
