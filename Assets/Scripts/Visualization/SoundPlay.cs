using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class SoundPlay : MonoBehaviour {

    public bool play;
    public bool stop;

    AudioSource audio;
    
    void Start()
    {
        audio = GetComponent<AudioSource>();
        audio.Play();
        audio.Play(44100);
    }

    public void Update()
    {
        if (play)
        {
            audio.Play();
            audio.Play(44100);
        }
        else if (stop)
        {
            audio.Stop();
        }
    }
}
