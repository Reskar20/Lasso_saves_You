using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public AudioSource introSong, loopSong;
    public float volume = .25f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        introSong.volume = volume;
        loopSong.volume = volume;
        introSong.Play();
        loopSong.PlayScheduled(AudioSettings.dspTime + introSong.clip.length);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
