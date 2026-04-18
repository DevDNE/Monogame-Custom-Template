using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace MonoGame.GameFramework.Audio;
public class SoundManager
{
    private Dictionary<string, SoundEffect> soundEffects = new Dictionary<string, SoundEffect>();
    private Dictionary<string, Song> songs = new Dictionary<string, Song>();
    private ContentManager content;

    public void LoadContent(ContentManager content)
    {
        this.content = content;
    }

    public void LoadSoundEffect(string name)
    {
        SoundEffect soundEffect = content.Load<SoundEffect>(name);
        soundEffects[name] = soundEffect;
    }

    public void PlaySoundEffect(string name)
    {
        if (soundEffects.ContainsKey(name))
        {
            soundEffects[name].Play();
        }
    }

    public void LoadSong(string name)
    {
        Song song = content.Load<Song>(name);
        songs[name] = song;
    }

    public void PlaySong(string name)
    {
        if (songs.ContainsKey(name))
        {
            MediaPlayer.Play(songs[name]);
        }
    }

    public void PauseSong() => MediaPlayer.Pause();
    public void StopSong() => MediaPlayer.Stop();
}
