using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class SoundPlay : MonoBehaviour {

    public bool play;
    public bool stop;

    AudioSource source;
    
    void Start()
    {
        source = GetComponent<AudioSource>();
        source.Play();
        //audio.Play(44100);
    }

    public void Update()
    {
        if (play)
        {
            source.Play();
            //source.Play(44100);
        }
        else if (stop)
        {
            source.Stop();
        }
    }
}
